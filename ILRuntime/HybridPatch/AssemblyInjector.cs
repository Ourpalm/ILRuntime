using ILRuntime.CLR.Method;
using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace ILRuntime.Hybrid
{
    public class AssemblyInjector
    {
        Stream srcAsm;
        ModuleDefinition module;
        FieldDefinition appdomainField;
        AssemblyDefinition asm;
        ReflectionReferences reflection;
        AssemblyInjector(Stream srcAsm)
        {
            this.srcAsm = srcAsm;
            asm = AssemblyDefinition.ReadAssembly(srcAsm);
            module = asm.MainModule;            
        }

        public void Inject()
        {
            string attributeName = typeof(ILRuntimePatchAttribute).FullName;
            if (module.HasTypes)
            {
                var patchType = module.GetType("ILRuntime.Hybrid.Patched");
                if (patchType != null)
                    throw new NotSupportedException("Cannot patch a assembly twice");

                reflection = new ReflectionReferences(module);
                InjectPatchType();
                foreach (var type in module.GetTypes())
                {
                    if (type.HasCustomAttributes)
                    {
                        foreach (var attr in type.CustomAttributes)
                        {
                            if (attr.AttributeType.FullName == attributeName)
                            {
                                InjectType(type);
                            }
                        }
                    }
                }                
            }
        }

        void InjectPatchType()
        {
            var patchType = new TypeDefinition("ILRuntime.Hybrid", "Patched", TypeAttributes.Sealed | TypeAttributes.Public);
            appdomainField = new FieldDefinition("domain", FieldAttributes.Static | FieldAttributes.Assembly, reflection.AppDomainType);
            patchType.Fields.Add(appdomainField);
            module.Types.Add(patchType);

        }

        public void WriteToFile(Stream stream)
        {
            asm.Write(stream);
        }


        void InjectType(TypeDefinition type)
        {
            type.Fields.Add(new FieldDefinition("___@extraFields", FieldAttributes.Private, module.TypeSystem.Object));

            if (type.HasMethods)
            {
                int mIdx = 0;
                foreach(var method in type.Methods)
                {
                    if (method.IsAbstract)
                        continue;
                            
                    InjectMethod(type, method, mIdx++);
                }
                if (type.HasNestedTypes)
                {
                    foreach(var t in type.NestedTypes)
                    {
                        InjectType(t);
                    }
                }
            }
        }

        void InjectMethod(TypeDefinition declaringType, MethodDefinition method, int mIdx)
        {
            FieldDefinition redirectDef;
            redirectDef = new FieldDefinition($"___@redirect_{method.Name}_{mIdx}", FieldAttributes.Private | FieldAttributes.Static, reflection.ILMethodType);
            declaringType.Fields.Add(redirectDef);

            VariableDefinition invokeCtx = new VariableDefinition(reflection.InvocationCtxType);
            method.Body.Variables.Add(invokeCtx);
            bool hasReturn = method.ReturnType != module.TypeSystem.Void;

            VariableDefinition returnValue = null;
            if (hasReturn)
            {
                //returnValue = new VariableDefinition(method.ReturnType);
                //method.Body.Variables.Add(returnValue);
            }
            var processor = method.Body.GetILProcessor();
            Instruction first = null;
            if (method.Body.Instructions.Count > 0)
                first = method.Body.Instructions[0];

            int[] refOutIdx = new int[method.Parameters.Count];
            int refIdxCur = 0;
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var p = method.Parameters[i];
                if (p.ParameterType.IsByReference)
                {
                    refOutIdx[i] = refIdxCur++;
                }
                else
                    refOutIdx[i] = -1;
            }

            processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, redirectDef));
            Instruction elseMarker = processor.Create(OpCodes.Nop);
            processor.AppendInstruction(first, processor.Create(OpCodes.Brfalse_S, elseMarker));

            processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, appdomainField));
            processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, redirectDef));
            processor.AppendInstruction(first, processor.Create(OpCodes.Callvirt, reflection.BeginInvokeMethod));
            processor.AppendInstruction(first, processor.Create(OpCodes.Stloc, invokeCtx));
            if (refIdxCur > 0)
            {
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    var p = method.Parameters[i];
                    if (p.ParameterType.IsByReference)
                    {
                        int paramIdx = method.IsStatic ? i : i + 1;
                        processor.AppendPushArgument(module, reflection, invokeCtx, p, paramIdx, first);
                    }
                }
            }
            if (!method.IsStatic)
            {
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldc_I4_1));
                processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.PushObjectMethod));
            }
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var p = method.Parameters[i];
                if (p.ParameterType.IsByReference)
                {
                    processor.AppendInstruction(first, processor.Create(OpCodes.Ldloca, invokeCtx));
                    processor.AppendLdc(first, refOutIdx[i]);
                    processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.PushReferenceMethod));
                    continue;
                }
                int paramIdx = method.IsStatic ? i : i + 1;
                processor.AppendPushArgument(module, reflection, invokeCtx, p, paramIdx, first);
            }

            processor.AppendInstruction(first, processor.Create(OpCodes.Ldloca, invokeCtx));
            processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.InvocationCtxInvokeMethod));
            if (refIdxCur > 0)
            {
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    var p = method.Parameters[i];
                    if (p.ParameterType.IsByReference)
                    {
                        int paramIdx = method.IsStatic ? i : i + 1;
                        processor.AppendStoreArgument(module, reflection, invokeCtx, p, refOutIdx[i], paramIdx, first);
                    }
                }
            }
            if (hasReturn)
            {
                processor.AppendReadReturnValue(module, method.ReturnType, reflection, invokeCtx, first);
            }
            processor.AppendInstruction(first, processor.Create(OpCodes.Ldloca, invokeCtx));
            processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.DisposeMethod));
            processor.AppendInstruction(first, processor.Create(OpCodes.Ret));
            processor.AppendInstruction(first, elseMarker);
        }
        public static AssemblyInjector CreateInjector(Stream srcAsm)
        {
            return new AssemblyInjector(srcAsm);
        }
    }
}
