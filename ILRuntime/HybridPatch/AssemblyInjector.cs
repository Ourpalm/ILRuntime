using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Runtime;
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
        AssemblyDefinition asm;
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
                var patchedType = module.GetType("ILRuntime.Hybrid.Patched");
                if (patchedType != null)
                    throw new NotSupportedException("Cannot patch a assembly twice");

                module.Types.Add(new TypeDefinition("ILRuntime.Hybrid", "Patched", TypeAttributes.Sealed | TypeAttributes.NotPublic));
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
            TypeReference deleType = null;
            MethodReference invokeMethod;
            if (method.ReturnType != module.TypeSystem.Void)
            {
                (deleType, invokeMethod) = CreateFunctionRedirectDelegate(method);
            }
            else
            {
                (deleType, invokeMethod) = CreateMethodRedirectDelegate(method);
            }
            redirectDef = new FieldDefinition($"___@redirect_{method.Name}_{mIdx}", FieldAttributes.Private | FieldAttributes.Static, deleType);
            declaringType.Fields.Add(redirectDef);

            var processor = method.Body.GetILProcessor();
            Instruction first = null;
            if (method.Body.Instructions.Count > 0)
                first = method.Body.Instructions[0];

            AppendInstruction(processor, first, processor.Create(OpCodes.Ldsfld, redirectDef));
            AppendInstruction(processor, first, processor.Create(OpCodes.Ldnull));
            AppendInstruction(processor, first, processor.Create(OpCodes.Ceq));
            Instruction elseMarker = processor.Create(OpCodes.Nop);
            AppendInstruction(processor, first, processor.Create(OpCodes.Brtrue_S, elseMarker));
            AppendInstruction(processor, first, processor.Create(OpCodes.Ldsfld, redirectDef));

            if (!method.IsStatic)
            {
                AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_0));
            }
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                int paramIdx = method.IsStatic ? i : i + 1;
                switch (paramIdx)
                {
                    case 0:
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_0));
                        break;
                    case 1:
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_1));
                        break;
                    case 2:
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_2));
                        break;
                    case 3:
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg_3));
                        break;
                    default:
                        AppendInstruction(processor, first, processor.Create(OpCodes.Ldarg, i));
                        break;
                }
            }
            AppendInstruction(processor, first, processor.Create(OpCodes.Callvirt, invokeMethod));
            AppendInstruction(processor, first, processor.Create(OpCodes.Ret));
            AppendInstruction(processor, first, elseMarker);
        }

        void AppendInstruction(ILProcessor processor, Instruction before, Instruction instruction)
        {
            if(before != null)
                processor.InsertBefore(before, instruction);
            else
                processor.Append(instruction);
        }

        (TypeReference, MethodReference) CreateFunctionRedirectDelegate(MethodDefinition method)
        {
            int paramCnt = method.Parameters.Count;
            if (!method.IsStatic)
                paramCnt++;

            if (paramCnt < 5)
            {
                Type gType = null;
                switch (paramCnt)
                {
                    case 0:
                        gType = typeof(Func<>);
                        break;
                    case 1:
                        gType = typeof(Func<,>);
                        break;
                    case 2:
                        gType = typeof(Func<,,>);
                        break;
                    case 3:
                        gType = typeof(Func<,,,>);
                        break;
                    case 4:
                        gType = typeof(Func<,,,,>);
                        break;
                }
                TypeReference gTypeRef = module.ImportReference(gType);
                List<TypeReference> paramTypes = new List<TypeReference>();
                if (!method.IsStatic)
                {
                    paramTypes.Add(method.DeclaringType);
                }
                foreach (var param in method.Parameters)
                {
                    var pType = param.ParameterType.IsByReference ? param.ParameterType.GetElementType() : param.ParameterType;
                    paramTypes.Add(pType);
                }
                paramTypes.Add(method.ReturnType);
                GenericInstanceType resType = gTypeRef.MakeGenericInstanceType(paramTypes.ToArray());
                var invoke = resType.Resolve().Methods.First((m) => m.Name == "Invoke");
                return (resType, module.ImportReference(resType.GetMethod(invoke)));
            }
            else
            {
                return (null, null);
            }
        }

        (TypeReference, MethodReference) CreateMethodRedirectDelegate(MethodDefinition method)
        {
            int paramCnt = method.Parameters.Count;
            if (!method.IsStatic)
                paramCnt++;

            if (paramCnt < 5)
            {
                Type gType = null;
                switch (paramCnt)
                {
                    case 0:
                        gType = typeof(Action);
                        break;
                    case 1:
                        gType = typeof(Action<>);
                        break;
                    case 2:
                        gType = typeof(Action<,>);
                        break;
                    case 3:
                        gType = typeof(Action<,,>);
                        break;
                    case 4:
                        gType = typeof(Action<,,,>);
                        break;

                }
                TypeReference gTypeRef = module.ImportReference(gType);
                List<TypeReference> paramTypes = new List<TypeReference>();
                if (!method.IsStatic)
                {
                    paramTypes.Add(method.DeclaringType);                    
                }
                foreach(var param in method.Parameters)
                {
                    var pType = param.ParameterType.IsByReference ? param.ParameterType.GetElementType() : param.ParameterType;
                    paramTypes.Add(pType);
                }
                GenericInstanceType resType = gTypeRef.MakeGenericInstanceType(paramTypes.ToArray());

                var invoke = resType.Resolve().Methods.First((m) => m.Name == "Invoke");
                
                return (resType, module.ImportReference(resType.GetMethod(invoke)));
            }
            else
            {
                return (null, null);
            }
        }
        public static AssemblyInjector CreateInjector(Stream srcAsm)
        {
            return new AssemblyInjector(srcAsm);
        }
    }
}
