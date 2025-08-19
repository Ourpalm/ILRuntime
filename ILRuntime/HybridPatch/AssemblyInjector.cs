using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ILRuntime.Hybrid
{
    public class AssemblyInjector
    {
        Stream srcAsm;
        ModuleDefinition module;
        FieldDefinition appdomainField, typeArrField, methodArrField, appliedPatchHash;
        MethodDefinition applyPatchMethod;
        AssemblyDefinition asm;
        AssemblyHashInfo asmInfo;
        ReflectionReferences reflection;
        List<MethodDefinition> initMethods = new List<MethodDefinition>();
        HashSet<TypeReference> patchingSet = new HashSet<TypeReference>();
        List<KeyValuePair<TypeDefinition, Instruction>> baseCalls = new List<KeyValuePair<TypeDefinition, Instruction>>();

        const string ExtraFieldObjectFieldName = "___@extraFields";
        const string ExtraFieldTypeFieldName = "___@extra_field_type";
        const string InitializedPatchHashFieldName = "___@initialized_patch_hash";
        const string InitializeGenericInstanceMethodName = "___@InitializeGenericInstance";
        const string InstantiateMethodName = "___@InstantiateExtraFieldObject";

        internal const string PatchTypeGetPatchableTypesMethodName = "GetPatchableTypes";
        internal const string PatchApplyPatchMethodName = "ApplyPatch";
        AssemblyInjector(Stream srcAsm, Stream pdbStream, IAssemblyResolver resolver, IPatchSettings settings)
        {
            var a = new DefaultAssemblyResolver();
            if (resolver == null)
                resolver = new DefaultAssemblyResolver();
            bool hasSymbol = pdbStream != null;
            srcAsm = CheckAndCopyStream(srcAsm);
            if (hasSymbol)
                pdbStream = CheckAndCopyStream(pdbStream);
            ReaderParameters readerParameters = new ReaderParameters
            {
                ReadSymbols = hasSymbol,
                SymbolReaderProvider = hasSymbol ? new PdbReaderProvider() : null,
                AssemblyResolver = resolver,
                SymbolStream = pdbStream,
                ReadWrite = true
            };
            this.srcAsm = srcAsm;
            asm = AssemblyDefinition.ReadAssembly(srcAsm, readerParameters);
            module = asm.MainModule;
            asmInfo = AssemblyHashInfo.BuildHashInfo(asm, settings);
        }

        Stream CheckAndCopyStream(Stream ori)
        {
            if (ori is FileStream)
            {
                MemoryStream ms = new MemoryStream((int)(ori.Length - ori.Position));
                ori.CopyTo(ms);
                ms.Position = 0;
                return ms;
            }
            else
                return ori;
        }

        public void Inject()
        {
            string attributeName = typeof(ILRuntimePatchAttribute).FullName;
            patchingSet.Clear();
            if (module.HasTypes)
            {
                var patchType = module.GetType("ILRuntime.Hybrid.Patched");
                if (patchType != null)
                    throw new NotSupportedException("Cannot patch a assembly twice");

                reflection = new ReflectionReferences(module);
                InjectPatchType();
                foreach (var i in asmInfo.Types)
                {
                    patchingSet.Add(i.Definition);
                }
                foreach (var i in asmInfo.Types)
                {
                    InjectType(i);
                }
                InjectApplyPatchMethod();
            }
            foreach(var i in baseCalls)
            {
                var ins = i.Value;
                var methodReference = ins.Operand as MethodReference;
                if (methodReference != null && !CheckIsRootClass(i.Key, out var baseType))
                {
                    var td = baseType.Resolve();
                    foreach(var m in td.Methods)
                    {
                        if(m.Name == methodReference.Name)
                        {
                            ins.Operand = m;
                            continue;
                        }
                    }
                }
            }
        }

        void InjectPatchType()
        {
            var patchType = new TypeDefinition("ILRuntime.Hybrid", "Patched", TypeAttributes.Sealed | TypeAttributes.Public, module.TypeSystem.Object);
            CustomAttribute attr = new CustomAttribute(reflection.PatchableAssemblyCtor);
            attr.Properties.Add(new CustomAttributeNamedArgument(nameof(PatchableAssemblyAttribute.Name), new CustomAttributeArgument(module.TypeSystem.String, asmInfo.Name)));
            attr.Properties.Add(new CustomAttributeNamedArgument(nameof(PatchableAssemblyAttribute.AssemblyHash), new CustomAttributeArgument(module.TypeSystem.String, asmInfo.Hash)));
            patchType.CustomAttributes.Add(attr);
            appdomainField = new FieldDefinition("domain", FieldAttributes.Static | FieldAttributes.Assembly, reflection.AppDomainType);
            typeArrField = new FieldDefinition("types", FieldAttributes.Static | FieldAttributes.Assembly, reflection.ILTypeArrayType);
            methodArrField = new FieldDefinition("methods", FieldAttributes.Static | FieldAttributes.Assembly, reflection.ILMethodArrayType);
            appliedPatchHash = new FieldDefinition("appliedPatchHash", FieldAttributes.Static | FieldAttributes.Assembly, module.TypeSystem.Int32);
            var patchableTypesField = new FieldDefinition("patchableTypes", FieldAttributes.Static | FieldAttributes.Assembly, reflection.TypeArrayType);
            patchType.Fields.Add(appdomainField);
            patchType.Fields.Add(typeArrField);
            patchType.Fields.Add(methodArrField);
            patchType.Fields.Add(appliedPatchHash);
            patchType.Fields.Add(patchableTypesField);

            applyPatchMethod = new MethodDefinition(PatchApplyPatchMethodName, MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Boolean);
            var patchArg = new ParameterDefinition("patch", ParameterAttributes.None, reflection.AssemblyPatchType);
            applyPatchMethod.Parameters.Add(patchArg);
            patchArg = new ParameterDefinition("domain", ParameterAttributes.None, reflection.AppDomainType);
            applyPatchMethod.Parameters.Add(patchArg);
            patchType.Methods.Add(applyPatchMethod);

            var getPatchables = new MethodDefinition(PatchTypeGetPatchableTypesMethodName, MethodAttributes.Public | MethodAttributes.Static, reflection.TypeArrayType);
            patchType.Methods.Add(getPatchables);

            getPatchables.Body = new MethodBody(getPatchables);
            var processor = getPatchables.Body.GetILProcessor();

            processor.Append(processor.Create(OpCodes.Ldsfld, patchableTypesField));
            Instruction elseMarker = processor.Create(OpCodes.Nop);
            processor.Append(processor.Create(OpCodes.Brtrue, elseMarker));

            processor.Append(processor.Create(OpCodes.Ldc_I4, asmInfo.TypeCount));
            processor.Append(processor.Create(OpCodes.Newarr, reflection.TypeType));
            processor.Append(processor.Create(OpCodes.Stsfld, patchableTypesField));
            for (int i = 0; i < asmInfo.TypeCount; i++)
            {
                var t = asmInfo.Types[i];
                processor.Append(processor.Create(OpCodes.Ldsfld, patchableTypesField));
                processor.Append(processor.Create(OpCodes.Ldc_I4, i));
                processor.Append(processor.Create(OpCodes.Ldtoken, t.Definition));
                processor.Append(processor.Create(OpCodes.Call, reflection.GetTypeFromHandle));
                processor.Append(processor.Create(OpCodes.Stelem_Any, reflection.TypeType));
            }
            processor.Append(elseMarker);
            processor.Append(processor.Create(OpCodes.Ldsfld, patchableTypesField));
            processor.Append(processor.Create(OpCodes.Ret));

            module.Types.Add(patchType);
        }

        void InjectApplyPatchMethod()
        {
            applyPatchMethod.Body = new MethodBody(applyPatchMethod);
            var processor = applyPatchMethod.Body.GetILProcessor();
            processor.Append(processor.Create(OpCodes.Ldarg_1));
            processor.Append(processor.Create(OpCodes.Stsfld, appdomainField));
            processor.Append(processor.Create(OpCodes.Ldc_I4, asmInfo.TypeCount));
            processor.Append(processor.Create(OpCodes.Newarr, reflection.ILTypeType));
            processor.Append(processor.Create(OpCodes.Stsfld, typeArrField));
            processor.Append(processor.Create(OpCodes.Ldc_I4, asmInfo.MethodCount));
            processor.Append(processor.Create(OpCodes.Newarr, reflection.ILMethodType));
            processor.Append(processor.Create(OpCodes.Stsfld, methodArrField));

            processor.Append(processor.Create(OpCodes.Ldarg_0));
            processor.Append(processor.Create(OpCodes.Ldsfld, typeArrField));
            processor.Append(processor.Create(OpCodes.Ldsfld, methodArrField));
            processor.Append(processor.Create(OpCodes.Call, reflection.PatchApplyPatchMethod));
            processor.Append(processor.Create(OpCodes.Ldarg_0));
            processor.Append(processor.Create(OpCodes.Callvirt, reflection.ObjectGetHashCodeMethod));
            processor.Append(processor.Create(OpCodes.Stsfld, appliedPatchHash));
            //var nameProp = new PropertyDefinition("Name",)
            foreach (var i in initMethods)
            {
                processor.Append(processor.Create(OpCodes.Call, i));
            }
            processor.Append(processor.Create(OpCodes.Ldc_I4_1));
            processor.Append(processor.Create(OpCodes.Ret));
        }

        public void WriteAssemblyToFile(Stream stream, Stream pdbStream = null)
        {
            bool hasSymbol = pdbStream != null;
            WriterParameters writerParameters = new WriterParameters
            {
                SymbolStream = pdbStream,
                WriteSymbols = hasSymbol,
                SymbolWriterProvider = hasSymbol ? new PdbWriterProvider() : null
            };
            asm.Write(stream, writerParameters);
        }

        public void WritePatchInfoToFile(Stream stream)
        {
            asmInfo.WriteToStream(stream);
        }

        MethodDefinition InjectSetFieldValue(TypeHashInfo info, TypeDefinition type, TypeReference declaringType, bool isStatic, bool isRoot)
        {
            MethodAttributes ma = isStatic ? MethodAttributes.Private | MethodAttributes.Static :
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (!isStatic && isRoot)
                ma |= MethodAttributes.NewSlot;
            string methodName = isStatic ? "ILRuntimeSetStaticFieldValue" : nameof(IPatchableType.ILRuntimeSetFieldValue);

            var method = new MethodDefinition(methodName, ma, module.TypeSystem.Void);
            method.Parameters.Add(new ParameterDefinition("fieldIdx", ParameterAttributes.None, module.TypeSystem.Int32));
            method.Parameters.Add(new ParameterDefinition("intp", ParameterAttributes.None, reflection.ILInterpreterType));
            method.Parameters.Add(new ParameterDefinition("esp", ParameterAttributes.None, reflection.StackObjectRefType));
            method.Parameters.Add(new ParameterDefinition("mStack", ParameterAttributes.None, reflection.ObjectIListType));
            type.Methods.Add(method);
            method.Body = new MethodBody(method);
            var processor = method.Body.GetILProcessor();
            if (info.Fields.Length > 0)
            {
                Instruction[] targets = new Instruction[info.Fields.Length];
                Instruction switchEnd = processor.Create(OpCodes.Nop);
                if(isStatic)
                    processor.Append(processor.Create(OpCodes.Ldarg_0));
                else
                    processor.Append(processor.Create(OpCodes.Ldarg_1));
                processor.Append(processor.Create(OpCodes.Ldc_I4, info.Fields[0].Index));
                processor.Append(processor.Create(OpCodes.Sub));
                processor.Append(processor.Create(OpCodes.Switch, targets));
                processor.Append(processor.Create(OpCodes.Br, switchEnd));
                for (int i = 0; i < targets.Length; i++)
                {
                    var field = info.Fields[i].Definition;
                    var fd = field.Resolve();
                    if (fd != null)
                    {
                        if (fd.IsStatic != isStatic || fd.HasConstant)
                        {
                            targets[i] = switchEnd;
                            continue;
                        }
                    }
                    TypeReference fieldDeclaringType = declaringType;
                    if (info.Fields[i].IsForeign)
                    {
                        TypeReference btr = type;
                        do
                        {
                            var bt = btr.Resolve();
                            btr = bt != null ? bt.BaseType : null;
                        }
                        while (btr != null && !btr.CheckTypeReferenceEqual(field.DeclaringType));
                        if (btr != null)
                            fieldDeclaringType = btr;
                    }
                    var fieldType = field.FieldType.IsGenericParameter ? field.FieldType : module.ImportReference(field.FieldType);
                    fieldDeclaringType = module.ImportReference(fieldDeclaringType);
                    FieldReference fr = new FieldReference(field.Name, fieldType, fieldDeclaringType);
                    Instruction begin = null;
                    if (field.FieldType.IsPrimitive)
                    {
                        if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Int32))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt32) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Int16) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt16) ||
                            field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Byte) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.SByte) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Char))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                            }
                            if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt32))
                            {
                                processor.Append(processor.Create(OpCodes.Conv_U4));
                            }
                            else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Int16))
                            {
                                processor.Append(processor.Create(OpCodes.Conv_I2));
                            }
                            else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt16))
                            {
                                processor.Append(processor.Create(OpCodes.Conv_U2));
                            }
                            else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Byte))
                            {
                                processor.Append(processor.Create(OpCodes.Conv_U1));
                            }
                            else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.SByte))
                            {
                                processor.Append(processor.Create(OpCodes.Conv_I1));
                            }
                            if(isStatic)
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));
                            else
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Int64))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt64Method));
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));

                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt64Method));
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt64))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt64Method));
                                processor.Append(processor.Create(OpCodes.Conv_U8));
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt64Method));
                                processor.Append(processor.Create(OpCodes.Conv_U8));
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.IntPtr))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt64Method));
                                processor.Append(processor.Create(OpCodes.Conv_I));
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt64Method));
                                processor.Append(processor.Create(OpCodes.Conv_I));
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Single))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveFloatMethod));
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));
                                processor.Append(processor.Create(OpCodes.Ret));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveFloatMethod));
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                                processor.Append(processor.Create(OpCodes.Ret));
                            }
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Double))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveDoubleMethod));
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));
                                processor.Append(processor.Create(OpCodes.Ret));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveDoubleMethod));
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                                processor.Append(processor.Create(OpCodes.Ret));
                            }
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Boolean))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_1);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                                processor.Append(processor.Create(OpCodes.Ldc_I4_1));
                                processor.Append(processor.Create(OpCodes.Ceq));
                                processor.Append(processor.Create(OpCodes.Stsfld, fr));
                                processor.Append(processor.Create(OpCodes.Ret));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_0);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_2));
                                processor.Append(processor.Create(OpCodes.Ldarg_3));
                                processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                                processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                                processor.Append(processor.Create(OpCodes.Ldc_I4_1));
                                processor.Append(processor.Create(OpCodes.Ceq));
                                processor.Append(processor.Create(OpCodes.Stfld, fr));
                                processor.Append(processor.Create(OpCodes.Ret));
                            }
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else if (!field.FieldType.IsGenericParameter && field.FieldType.Resolve().IsEnum)
                    {
                        if (isStatic)
                        {
                            begin = processor.Create(OpCodes.Ldarg_1);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldarg_2));
                            processor.Append(processor.Create(OpCodes.Ldarg_3));
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                            processor.Append(processor.Create(OpCodes.Stsfld, fr));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else
                        {
                            begin = processor.Create(OpCodes.Ldarg_0);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldarg_2));
                            processor.Append(processor.Create(OpCodes.Ldarg_3));
                            processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterRetrieveInt32Method));
                            processor.Append(processor.Create(OpCodes.Stfld, fr));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                    }
                    else
                    {
                        if (fieldType.IsGenericParameter && fieldDeclaringType is GenericInstanceType git)
                        {
                            var gp = git.Resolve().GenericParameters;
                            var ga = git.GenericArguments;
                            for (int idx = 0; idx < gp.Count; idx++)
                            {
                                if (gp[idx] == fieldType)
                                {
                                    fieldType = ga[idx];
                                    break;
                                }
                            }
                        }
                        if (isStatic)
                        {
                            begin = processor.Create(OpCodes.Ldarg_1);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldarg_2));
                            processor.Append(processor.Create(OpCodes.Ldarg_3));
                            processor.Append(processor.Create(OpCodes.Call, reflection.GetRetrieveObjectMethod(fieldType)));
                            processor.Append(processor.Create(OpCodes.Stsfld, fr));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else
                        {
                            begin = processor.Create(OpCodes.Ldarg_0);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldarg_2));
                            processor.Append(processor.Create(OpCodes.Ldarg_3));
                            processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                            processor.Append(processor.Create(OpCodes.Call, reflection.GetRetrieveObjectMethod(fieldType)));
                            processor.Append(processor.Create(OpCodes.Stfld, fr));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                    }
                    targets[i] = begin;
                }
                processor.Append(switchEnd);
                if (!isStatic && !isRoot)
                {
                    processor.Append(processor.Create(OpCodes.Ldarg_0));
                    processor.Append(processor.Create(OpCodes.Ldarg_1));
                    processor.Append(processor.Create(OpCodes.Ldarg_2));
                    processor.Append(processor.Create(OpCodes.Ldarg_3));
                    processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                    var callIns = processor.Create(OpCodes.Call, method);
                    baseCalls.Add(new KeyValuePair<TypeDefinition, Instruction>(type, callIns));
                    processor.Append(callIns);
                }
            }
            processor.Append(processor.Create(OpCodes.Ret));

            return method;
        }

        MethodDefinition InjectReadFieldValue(TypeHashInfo info, TypeDefinition type, TypeReference declaringType, bool isStatic, bool isRoot)
        {
            MethodAttributes ma = isStatic ? MethodAttributes.Private | MethodAttributes.Static : 
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (!isStatic && isRoot)
                ma |= MethodAttributes.NewSlot;
            string methodName = isStatic ? "ILRuntimeGetStaticFieldValue" : nameof(IPatchableType.ILRuntimeGetFieldValue);
            var method = new MethodDefinition(methodName, ma, reflection.StackObjectRefType);
            method.Parameters.Add(new ParameterDefinition("fieldIdx", ParameterAttributes.None, module.TypeSystem.Int32));
            method.Parameters.Add(new ParameterDefinition("intp", ParameterAttributes.None, reflection.ILInterpreterType));
            method.Parameters.Add(new ParameterDefinition("esp", ParameterAttributes.None, reflection.StackObjectRefType));
            method.Parameters.Add(new ParameterDefinition("mStack", ParameterAttributes.None, reflection.ObjectIListType));
            type.Methods.Add(method);
            method.Body = new MethodBody(method);
            var processor = method.Body.GetILProcessor();
            if (info.Fields.Length > 0)
            {
                Instruction[] targets = new Instruction[info.Fields.Length];
                Instruction switchEnd = processor.Create(OpCodes.Nop);
                if(isStatic)
                    processor.Append(processor.Create(OpCodes.Ldarg_0));
                else
                    processor.Append(processor.Create(OpCodes.Ldarg_1));
                processor.Append(processor.Create(OpCodes.Ldc_I4, info.Fields[0].Index));
                processor.Append(processor.Create(OpCodes.Sub));
                processor.Append(processor.Create(OpCodes.Switch, targets));
                processor.Append(processor.Create(OpCodes.Br, switchEnd));
                for (int i = 0; i < targets.Length; i++)
                {
                    var field = info.Fields[i].Definition;
                    var fd = field.Resolve();
                    if (fd != null)
                    {
                        if (fd.IsStatic != isStatic || fd.HasConstant)
                        {
                            targets[i] = switchEnd;
                            continue;
                        }
                    }
                    Instruction begin = null;
                    TypeReference fieldDeclaringType = declaringType;
                    if (info.Fields[i].IsForeign)
                    {
                        TypeReference btr = type;
                        do
                        {
                            var bt = btr.Resolve();
                            btr = bt != null ? bt.BaseType : null;
                        }
                        while (btr != null && !btr.CheckTypeReferenceEqual(field.DeclaringType));
                        if (btr != null)
                            fieldDeclaringType = btr;
                    }
                    TypeReference fieldType = field.FieldType.IsGenericParameter ? field.FieldType : module.ImportReference(field.FieldType);
                    fieldDeclaringType = module.ImportReference(fieldDeclaringType);
                    FieldReference fr = new FieldReference(field.Name, fieldType, fieldDeclaringType);
                    if (field.FieldType.IsPrimitive)
                    {
                        if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Int32))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_2);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_3);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_0));
                                processor.Append(processor.Create(OpCodes.Ldfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterPushInt32Method));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt32) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Int16) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt16) ||
                            field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Byte) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.SByte) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Char))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_2);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_3);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_0));
                                processor.Append(processor.Create(OpCodes.Ldfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Conv_I4));
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterPushInt32Method));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Int64))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_2);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_3);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_0));
                                processor.Append(processor.Create(OpCodes.Ldfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterPushInt64Method));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.UInt64) || field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.IntPtr))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_2);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_3);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_0));
                                processor.Append(processor.Create(OpCodes.Ldfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Conv_I8));
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterPushInt64Method));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Single))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_2);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_3);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_0));
                                processor.Append(processor.Create(OpCodes.Ldfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterPushFloatMethod));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Double))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_2);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_3);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_0));
                                processor.Append(processor.Create(OpCodes.Ldfld, fr));
                            }
                            processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterPushDoubleMethod));
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else if (field.FieldType.CheckTypeReferenceEqual(module.TypeSystem.Boolean))
                        {
                            if (isStatic)
                            {
                                begin = processor.Create(OpCodes.Ldarg_2);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                            }
                            else
                            {
                                begin = processor.Create(OpCodes.Ldarg_3);
                                processor.Append(begin);
                                processor.Append(processor.Create(OpCodes.Ldarg_0));
                                processor.Append(processor.Create(OpCodes.Ldfld, fr));
                            }
                            var marker = processor.Create(OpCodes.Ldc_I4_1);
                            var callIns = processor.Create(OpCodes.Call, reflection.InterpreterPushInt32Method);
                            processor.Append(processor.Create(OpCodes.Ldc_I4_0));
                            processor.Append(processor.Create(OpCodes.Cgt_Un));
                            processor.Append(callIns);
                            processor.Append(processor.Create(OpCodes.Ret));
                        }
                        else
                            throw new NotImplementedException();
                    }
                    else if (!field.FieldType.IsGenericParameter && field.FieldType.Resolve().IsEnum)
                    {
                        if (isStatic)
                        {
                            begin = processor.Create(OpCodes.Ldarg_2);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                        }
                        else
                        {
                            begin = processor.Create(OpCodes.Ldarg_3);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldarg_0));
                            processor.Append(processor.Create(OpCodes.Ldfld, fr));
                        }
                        processor.Append(processor.Create(OpCodes.Conv_I4));
                        processor.Append(processor.Create(OpCodes.Call, reflection.InterpreterPushInt32Method));
                        processor.Append(processor.Create(OpCodes.Ret));
                    }
                    else
                    {
                        if (isStatic)
                        {
                            begin = processor.Create(OpCodes.Ldarg_1);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldarg_2));
                            processor.Append(processor.Create(OpCodes.Ldarg_3));
                            processor.Append(processor.Create(OpCodes.Ldsfld, fr));
                        }
                        else
                        {
                            begin = processor.Create(OpCodes.Ldarg_2);
                            processor.Append(begin);
                            processor.Append(processor.Create(OpCodes.Ldarg_3));
                            processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                            processor.Append(processor.Create(OpCodes.Ldarg_0));
                            processor.Append(processor.Create(OpCodes.Ldfld, fr));
                        }
                        if (fieldType.IsGenericParameter && fieldDeclaringType is GenericInstanceType git)
                        {
                            var gp = git.Resolve().GenericParameters;
                            var ga = git.GenericArguments;
                            for (int idx = 0; idx < gp.Count; idx++)
                            {
                                if (gp[idx] == fieldType)
                                {
                                    fieldType = ga[idx];
                                    break;
                                }
                            }
                        }
                        processor.Append(processor.Create(OpCodes.Call, reflection.GetPushObjectMethod(fieldType)));
                        processor.Append(processor.Create(OpCodes.Ret));
                    }
                    targets[i] = begin;
                }

                processor.Append(switchEnd);

                if (!isStatic && !isRoot)
                {
                    processor.Append(processor.Create(OpCodes.Ldarg_0));
                    processor.Append(processor.Create(OpCodes.Ldarg_1));
                    processor.Append(processor.Create(OpCodes.Ldarg_2));
                    processor.Append(processor.Create(OpCodes.Ldarg_3));
                    processor.Append(processor.Create(OpCodes.Ldarg_S, (byte)4));
                    var callIns = processor.Create(OpCodes.Call, method);
                    baseCalls.Add(new KeyValuePair<TypeDefinition, Instruction>(type, callIns));
                    processor.Append(callIns);
                    processor.Append(processor.Create(OpCodes.Starg_S, (byte)3));
                }
            }
            if(isStatic)
                processor.Append(processor.Create(OpCodes.Ldarg_2));
            else
                processor.Append(processor.Create(OpCodes.Ldarg_3));
            processor.Append(processor.Create(OpCodes.Ret));

            return method;
        }

        bool CheckHasStatic(TypeHashInfo info)
        {
            foreach (var i in info.Fields)
            {
                var fd = i.Definition.Resolve();
                if (fd !=null && fd.IsStatic)
                    return true;
            }
            return false;
        }

        bool CheckIsRootClass(TypeDefinition td, out TypeReference baseType)
        {
            baseType = td.BaseType;
            while (baseType != null)
            {
                if (patchingSet.Contains(baseType))
                    break;
                var bt = baseType.Resolve();
                baseType = bt.BaseType;
            }

            if(baseType != null)
            {
                return !patchingSet.Contains(baseType);
            }
            return true;
        }

        void InjectIPatchableType(TypeHashInfo info, ref TypeInjectionContext ctx, out MethodDefinition staticGetter, out MethodDefinition staticSetter)
        {
            var type = info.Definition;
            if ((type.Attributes & TypeAttributes.NestedPrivate) == TypeAttributes.NestedPrivate)
            {
                type.Attributes ^= TypeAttributes.NestedPrivate;
                type.Attributes |= TypeAttributes.NestedAssembly;
            }
            else if ((type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
            {
                type.Attributes ^= TypeAttributes.NotPublic;
            }

            bool isRoot = CheckIsRootClass(type, out var baseType);

            if (isRoot)
                type.Interfaces.Add(new InterfaceImplementation(reflection.IPatchableTypeType));
            
            var property = new PropertyDefinition(nameof(IPatchableType.ILRuntimeExtraFieldObject), PropertyAttributes.None, reflection.ILTypeInstanceType);
            MethodAttributes mAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (isRoot)
                mAttr |= MethodAttributes.NewSlot;
            var method = new MethodDefinition($"get_{nameof(IPatchableType.ILRuntimeExtraFieldObject)}", mAttr, reflection.ILTypeInstanceType);
            property.GetMethod = method;
            type.Methods.Add(method);
            type.Properties.Add(property);
            method.Body = new MethodBody(method);
            var processor = method.Body.GetILProcessor();
            processor.Append(processor.Create(OpCodes.Ldarg_0));
            processor.Append(processor.Create(OpCodes.Ldfld, ctx.ExtraFieldsObject));
            processor.Append(processor.Create(OpCodes.Ret));

            bool hasStatic = CheckHasStatic(info);
            staticGetter = null;
            staticSetter = null;

            if (ctx.IsGenericType)
                InjectReadFieldValue(info, type, ctx.DeclaringTypeGenericInstance, false, isRoot);
            else
                InjectReadFieldValue(info, type, ctx.DeclaringType, false, isRoot);
            if (hasStatic)
            {
                if (ctx.IsGenericType)
                    staticGetter = InjectReadFieldValue(info, type, ctx.DeclaringTypeGenericInstance, true, isRoot);
                else
                    staticGetter = InjectReadFieldValue(info, type, ctx.DeclaringType, true, isRoot);
            }
            if (ctx.IsGenericType)
                InjectSetFieldValue(info, type, ctx.DeclaringTypeGenericInstance, false, isRoot);
            else
                InjectSetFieldValue(info, type, ctx.DeclaringType, false, isRoot);

            if (hasStatic)
            {
                if (ctx.IsGenericType)
                    staticSetter = InjectSetFieldValue(info, type, ctx.DeclaringTypeGenericInstance, true, isRoot);
                else
                    staticSetter = InjectSetFieldValue(info, type, ctx.DeclaringType, true, isRoot);
            }
        }
        void InjectType(TypeHashInfo info)
        {
            string extraObjName = $"{ExtraFieldObjectFieldName}_{info.Index}";
            string extraTypeName = $"{ExtraFieldTypeFieldName}_{info.Index}";
            TypeInjectionContext ctx = new TypeInjectionContext();
            ctx.DeclaringType = info.Definition;
            var fd = new FieldDefinition(extraObjName, FieldAttributes.Private, reflection.ILTypeInstanceType);
            ctx.DeclaringType.Fields.Add(fd);
            ctx.ExtraFieldsObject = fd;

            fd = new FieldDefinition(extraTypeName, FieldAttributes.Private | FieldAttributes.Static, reflection.ILTypeType);
            ctx.DeclaringType.Fields.Add(fd);
            ctx.ExtraFieldILType = fd;

            if (ctx.DeclaringType.HasGenericParameters)
            {
                ctx.IsGenericType = true;
                ctx.DeclaringTypeGenericDefinition = new TypeDefinition(ctx.DeclaringType.Namespace, $"{ctx.DeclaringType.Name}__redirects", TypeAttributes.NotPublic, module.TypeSystem.Object);
                module.Types.Add(ctx.DeclaringTypeGenericDefinition);
                ctx.DeclaringTypeGenericInstance = ctx.DeclaringType.MakeGenericInstanceType(ctx.DeclaringType.GenericParameters.ToArray());
                ctx.ExtraFieldsObject = new FieldReference(extraObjName, reflection.ILTypeInstanceType, ctx.DeclaringTypeGenericInstance);
                ctx.ExtraFieldILTypeGenericInstance = new FieldReference(extraTypeName, reflection.ILTypeType, ctx.DeclaringTypeGenericInstance);

                fd = new FieldDefinition(extraTypeName, FieldAttributes.Assembly | FieldAttributes.Static, reflection.ILTypeType);
                ctx.DeclaringTypeGenericDefinition.Fields.Add(fd);
                ctx.ExtraFieldILTypeGenericDefinition = fd;

                fd = new FieldDefinition(InitializedPatchHashFieldName, FieldAttributes.Private | FieldAttributes.Static, module.TypeSystem.Int32);
                ctx.InitializedPatchHash = new FieldReference(InitializedPatchHashFieldName, module.TypeSystem.Int32, ctx.DeclaringTypeGenericInstance);
                ctx.DeclaringType.Fields.Add(fd);
            }

            ctx.MethodRedirections = new List<MethodRedirectionInjectionContext>();

            MethodDefinition initMethod = null;
            if (ctx.IsGenericType)
            {
                initMethod = new MethodDefinition(InitializeGenericInstanceMethodName, MethodAttributes.Private | MethodAttributes.Static, module.TypeSystem.Void);
                ctx.DeclaringType.Methods.Add(initMethod);
                ctx.InitializeGenericInstance = new MethodReference(InitializeGenericInstanceMethodName, module.TypeSystem.Void, ctx.DeclaringTypeGenericInstance);
            }

            MethodDefinition instantiateMethod = null;
            if(CheckIsRootClass(info.Definition, out var baseType))
            {
                ctx.IsRoot = true;
                instantiateMethod = new MethodDefinition(InstantiateMethodName, MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, module.TypeSystem.Void);                
            }
            else
            {
                ctx.IsRoot = false;
                instantiateMethod = new MethodDefinition(InstantiateMethodName, MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, module.TypeSystem.Void);
            }
            ctx.DeclaringType.Methods.Add(instantiateMethod);
            if (ctx.IsGenericType)
            {
                ctx.InstantiateMethod = new MethodReference(InstantiateMethodName, module.TypeSystem.Void, ctx.DeclaringTypeGenericInstance);
                ctx.InstantiateMethod.HasThis = true;
            }
            else
                ctx.InstantiateMethod = instantiateMethod;
            InjectInstantiateMethodBody(ref ctx, instantiateMethod);

            List<KeyValuePair<int, MethodRedirectionInjectionContext>> redirections = new List<KeyValuePair<int, MethodRedirectionInjectionContext>>();
            foreach (var i in info.Methods)
            {
                var m = i.Definition;
                if (m.IsAbstract)
                    continue;
                var mCtx = InjectMethod(ref ctx, i);
                ctx.MethodRedirections.Add(mCtx);
                redirections.Add(new KeyValuePair<int, MethodRedirectionInjectionContext>(i.Index, mCtx));
            }

            if (ctx.IsGenericType)
            {
                InjectGenericInstanceInitialization(ref ctx, initMethod);
            }

            InjectIPatchableType(info, ref ctx, out var staticGetter, out var staticSetter);

            var method = new MethodDefinition("__ApplyPatch", MethodAttributes.Assembly | MethodAttributes.Static, module.TypeSystem.Void);
            if (ctx.IsGenericType)
                ctx.DeclaringTypeGenericDefinition.Methods.Add(method);
            else
                ctx.DeclaringType.Methods.Add(method);
            method.Body = new MethodBody(method);
            var processor = method.Body.GetILProcessor();
            processor.Append(processor.Create(OpCodes.Ldsfld, typeArrField));
            processor.Append(processor.Create(OpCodes.Ldc_I4, info.Index));
            processor.Append(processor.Create(OpCodes.Ldelem_Ref));
            if (ctx.IsGenericType)
                processor.Append(processor.Create(OpCodes.Stsfld, ctx.ExtraFieldILTypeGenericDefinition));
            else
                processor.Append(processor.Create(OpCodes.Stsfld, ctx.ExtraFieldILType));
            foreach (var i in redirections)
            {
                processor.Append(processor.Create(OpCodes.Ldsfld, methodArrField));
                processor.Append(processor.Create(OpCodes.Ldc_I4, i.Key));
                processor.Append(processor.Create(OpCodes.Ldelem_Ref));
                if (ctx.IsGenericType)
                    processor.Append(processor.Create(OpCodes.Stsfld, i.Value.ILMethodGenericDefinition));
                else
                    processor.Append(processor.Create(OpCodes.Stsfld, i.Value.ILMethod));
            }
            if (staticGetter != null && staticSetter != null)
            {
                processor.Append(processor.Create(OpCodes.Ldsfld, appdomainField));
                processor.Append(processor.Create(OpCodes.Ldtoken, info.Definition));
                processor.Append(processor.Create(OpCodes.Call, reflection.GetTypeFromHandle));
                processor.Append(processor.Create(OpCodes.Ldnull));
                processor.Append(processor.Create(OpCodes.Ldftn, staticGetter));
                processor.Append(processor.Create(OpCodes.Newobj, reflection.PatchGetFieldDelegateCtor));
                processor.Append(processor.Create(OpCodes.Ldnull));
                processor.Append(processor.Create(OpCodes.Ldftn, staticSetter));
                processor.Append(processor.Create(OpCodes.Newobj, reflection.PatchSetFieldDelegateCtor));
                processor.Append(processor.Create(OpCodes.Callvirt, reflection.RegisterTypeStaticFieldAccessorMethod));
            }
            processor.Append(processor.Create(OpCodes.Ret));
            initMethods.Add(method);
        }

        void InjectInstantiateMethodBody(ref TypeInjectionContext ctx, MethodDefinition method)
        {
            method.Body = new MethodBody(method);
            var processor = method.Body.GetILProcessor();
            var elseMarker = processor.Create(OpCodes.Ret);
            if (ctx.IsGenericType)
            {
                processor.Append(processor.Create(OpCodes.Call, ctx.InitializeGenericInstance));
                processor.Append(processor.Create(OpCodes.Ldsfld, ctx.ExtraFieldILTypeGenericInstance));
            }
            else
                processor.Append(processor.Create(OpCodes.Ldsfld, ctx.ExtraFieldILType));
            processor.Append(processor.Create(OpCodes.Brfalse_S, elseMarker));
            processor.Append(processor.Create(OpCodes.Ldarg_0));
            if (ctx.IsGenericType)
                processor.Append(processor.Create(OpCodes.Ldsfld, ctx.ExtraFieldILTypeGenericInstance));
            else
                processor.Append(processor.Create(OpCodes.Ldsfld, ctx.ExtraFieldILType));
            processor.Append(processor.Create(OpCodes.Ldc_I4_1));
            processor.Append(processor.Create(OpCodes.Callvirt, reflection.ILTypeInstantiateMethod));
            processor.Append(processor.Create(OpCodes.Stfld, ctx.ExtraFieldsObject));
            processor.Append(elseMarker);
        }

        void InjectGenericInstanceInitialization(ref TypeInjectionContext ctx, MethodDefinition method)
        {
            method.Body = new MethodBody(method);
            VariableDefinition kvArr = new VariableDefinition(reflection.KeyValuePairArrayType);
            VariableDefinition typeArr = new VariableDefinition(reflection.ITypeArrayType);
            VariableDefinition type = new VariableDefinition(reflection.ITypeType);
            method.Body.Variables.Add(kvArr);
            method.Body.Variables.Add(typeArr);
            method.Body.Variables.Add(type);
            var processor = method.Body.GetILProcessor();
            Instruction retIns = processor.Create(OpCodes.Ret);
            processor.Append(processor.Create(OpCodes.Ldsfld, ctx.InitializedPatchHash));
            processor.Append(processor.Create(OpCodes.Ldsfld, appliedPatchHash));
            processor.Append(processor.Create(OpCodes.Beq, retIns));
            processor.Append(processor.Create(OpCodes.Ldsfld, appliedPatchHash));
            processor.Append(processor.Create(OpCodes.Stsfld, ctx.InitializedPatchHash));
            
            Instruction elseMark = processor.Create(OpCodes.Nop);
            processor.Append(processor.Create(OpCodes.Ldsfld, ctx.InitializedPatchHash));
            processor.Append(processor.Create(OpCodes.Brfalse, elseMark));
            int gaCnt = ctx.DeclaringType.GenericParameters.Count;
            processor.Append(processor.Create(OpCodes.Ldc_I4_S, (sbyte)gaCnt));
            processor.Append(processor.Create(OpCodes.Newarr, reflection.KeyValuePairType));
            processor.Append(processor.Create(OpCodes.Stloc, kvArr));
            processor.Append(processor.Create(OpCodes.Ldc_I4_S, (sbyte)gaCnt));
            processor.Append(processor.Create(OpCodes.Newarr, reflection.ITypeType));
            processor.Append(processor.Create(OpCodes.Stloc, typeArr));
            for (int i = 0; i < gaCnt; i++)
            {
                var ga = ctx.DeclaringType.GenericParameters[i];
                processor.Append(processor.Create(OpCodes.Ldloc, kvArr));
                processor.Append(processor.Create(OpCodes.Ldc_I4, i));
                processor.Append(processor.Create(OpCodes.Ldstr, ga.Name));
                processor.Append(processor.Create(OpCodes.Ldsfld, appdomainField));
                processor.Append(processor.Create(OpCodes.Ldtoken, ga));
                processor.Append(processor.Create(OpCodes.Call, reflection.GetTypeFromHandle));
                processor.Append(processor.Create(OpCodes.Call, reflection.AppDomainGetTypeMethod));
                processor.Append(processor.Create(OpCodes.Stloc, type));
                processor.Append(processor.Create(OpCodes.Ldloc, type));
                processor.Append(processor.Create(OpCodes.Newobj, reflection.KeyValuePairCtor));
                processor.Append(processor.Create(OpCodes.Stelem_Any, reflection.KeyValuePairType));
                processor.Append(processor.Create(OpCodes.Ldloc, typeArr));
                processor.Append(processor.Create(OpCodes.Ldc_I4_S, (sbyte)i));
                processor.Append(processor.Create(OpCodes.Ldloc, type));
                processor.Append(processor.Create(OpCodes.Stelem_Any, reflection.ITypeType));
            }

            processor.Append(processor.Create(OpCodes.Ldsfld, ctx.ExtraFieldILTypeGenericDefinition));
            processor.Append(processor.Create(OpCodes.Ldloc, kvArr));
            processor.Append(processor.Create(OpCodes.Callvirt, reflection.ILTypeMakeGenericInstanceMethod));
            processor.Append(processor.Create(OpCodes.Castclass, reflection.ILTypeType));
            processor.Append(processor.Create(OpCodes.Stsfld, ctx.ExtraFieldILTypeGenericInstance));
            foreach (var i in ctx.MethodRedirections)
            {
                processor.Append(processor.Create(OpCodes.Ldsfld, ctx.ExtraFieldILTypeGenericInstance));
                processor.Append(processor.Create(OpCodes.Ldsfld, i.ILMethodGenericDefinition));
                processor.Append(processor.Create(OpCodes.Callvirt, reflection.ILTypeGetMethodByGenericDefinition));
                processor.Append(processor.Create(OpCodes.Stsfld, i.ILMethodGenericInstance));
            }
            processor.Append(processor.Create(OpCodes.Br, retIns));
            processor.Append(elseMark);
            processor.Append(processor.Create(OpCodes.Ldnull));
            processor.Append(processor.Create(OpCodes.Stsfld, ctx.ExtraFieldILTypeGenericInstance));
            foreach (var i in ctx.MethodRedirections)
            {
                processor.Append(processor.Create(OpCodes.Ldnull));
                processor.Append(processor.Create(OpCodes.Stsfld, i.ILMethodGenericInstance));
            }
            //TypeReference kvType = module.ImportReference KeyValuePair<string, IType>
            processor.Append(retIns);
        }

        MethodRedirectionInjectionContext InjectMethod(ref TypeInjectionContext ctx, MethodHashInfo methodInfo)
        {
            MethodDefinition method = methodInfo.Definition;
            if ((method.Attributes & MethodAttributes.Family) != MethodAttributes.Family)
                method.IsPublic = true;//Make sure the methods can be accessed by patch

            MethodRedirectionInjectionContext methodCtx = default(MethodRedirectionInjectionContext);
            methodCtx.ILMethod = new FieldDefinition($"___@redirect_{method.Name}_{methodInfo.Index}", FieldAttributes.Assembly | FieldAttributes.Static, reflection.ILMethodType);
            if (ctx.IsGenericType)
            {
                methodCtx.ILMethodGenericDefinition = new FieldDefinition($"___@redirect_{method.Name}_{methodInfo.Index}", FieldAttributes.Assembly | FieldAttributes.Static, reflection.ILMethodType);
                ctx.DeclaringTypeGenericDefinition.Fields.Add(methodCtx.ILMethodGenericDefinition);
                methodCtx.ILMethodGenericInstance = new FieldReference(methodCtx.ILMethod.Name, reflection.ILMethodType, ctx.DeclaringTypeGenericInstance);
            }
            ctx.DeclaringType.Fields.Add(methodCtx.ILMethod);

            bool isOverride = method.IsVirtual && !method.IsNewSlot;
            FieldReference invokingField = null;
            TypeReference methodDeclaringType = ctx.DeclaringType;
            if (isOverride)
            {
                MethodReference overrideMethod = null;
                do
                {
                    var dt = methodDeclaringType.Resolve();
                    if (dt != null)
                    {
                        methodDeclaringType = dt.BaseType;
                        overrideMethod = methodDeclaringType.GetMethod(method.Name);
                    }
                    else
                        break;
                }
                while (overrideMethod == null);
                methodDeclaringType = module.ImportReference(methodDeclaringType);
                var omd = overrideMethod?.Resolve();
                if (omd != null && omd.IsAbstract)
                {
                    isOverride = false;
                }
                else
                {
                    var fd = new FieldDefinition($"___@invoking_{method.Name}_{methodInfo.Index}", FieldAttributes.Private, module.TypeSystem.Boolean);
                    ctx.DeclaringType.Fields.Add(fd);
                    if (ctx.IsGenericType)
                    {
                        invokingField = new FieldReference(fd.Name, module.TypeSystem.Boolean, ctx.DeclaringTypeGenericInstance);
                    }
                    else
                        invokingField = fd;
                }
            }
            
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
            Instruction elseMarker;
            if (method.IsConstructor && !method.IsStatic)
            {
                //Must append injected code after calling base type's constructor
                bool begin = false;
                foreach (var i in method.Body.Instructions)
                {
                    if (!begin && i.OpCode == OpCodes.Call)
                    {
                        if (i.Operand is MethodReference mr)
                        {
                            if (mr.Resolve().IsConstructor)
                            {
                                begin = true;
                                first = null;
                            }
                        }
                    }
                    else if (begin)
                    {
                        if (first == null)
                        {
                            first = i;
                            break;
                        }
                    }
                }

                if (ctx.IsRoot)
                {
                    processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));                    
                    processor.AppendInstruction(first, processor.Create(OpCodes.Callvirt, ctx.InstantiateMethod));                    
                }
            }
            else if (ctx.IsGenericType)
                processor.AppendInstruction(first, processor.Create(OpCodes.Call, ctx.InitializeGenericInstance));
            if (ctx.IsGenericType)
            {
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, methodCtx.ILMethodGenericInstance));
            }
            else
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, methodCtx.ILMethod));
            elseMarker = processor.Create(OpCodes.Nop);
            processor.AppendInstruction(first, processor.Create(OpCodes.Brfalse, elseMarker));

            if (isOverride)
            {
                var callBaseMarker = processor.Create(OpCodes.Nop);
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldfld, invokingField));
                processor.AppendInstruction(first, processor.Create(OpCodes.Brfalse, callBaseMarker));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
                if (ctx.DeclaringType.IsValueType)
                {
                    if (ctx.IsGenericType)
                    {
                        processor.AppendInstruction(first, processor.Create(OpCodes.Ldobj, ctx.DeclaringTypeGenericInstance));
                        processor.AppendInstruction(first, processor.Create(OpCodes.Box, ctx.DeclaringTypeGenericInstance));
                    }
                    else
                    {
                        processor.AppendInstruction(first, processor.Create(OpCodes.Ldobj, ctx.DeclaringType));
                        processor.AppendInstruction(first, processor.Create(OpCodes.Box, ctx.DeclaringType));
                    }
                }
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg, i + 1));
                }
                MethodReference baseMethod = new MethodReference(method.Name, method.ReturnType, methodDeclaringType);
                baseMethod.HasThis = true;
                foreach (var i in method.Parameters)
                {
                    baseMethod.Parameters.Add(new ParameterDefinition(i.ParameterType));
                }
                processor.AppendInstruction(first, processor.Create(OpCodes.Call, baseMethod));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ret));
                processor.AppendInstruction(first, callBaseMarker); 
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldc_I4_1));
                processor.AppendInstruction(first, processor.Create(OpCodes.Stfld, invokingField));
            }
            processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, appdomainField));
            if (ctx.IsGenericType)
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, methodCtx.ILMethodGenericInstance));
            else
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldsfld, methodCtx.ILMethod));
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
                        if (p.ParameterType.GetElementType().IsValueType && !p.ParameterType.GetElementType().IsPrimitive)
                        {
                            processor.AppendLoadArgument(paramIdx, first);
                            processor.AppendInstruction(first, processor.Create(OpCodes.Initobj, p.ParameterType.GetElementType()));
                        }
                        processor.AppendPushArgument(module, reflection, invokeCtx, p, paramIdx, first);
                    }
                }
            }
            if (!method.IsStatic)
            {
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldloca, invokeCtx));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
                if (method.DeclaringType.IsValueType)
                {
                    processor.AppendInstruction(first, processor.Create(OpCodes.Ldobj, method.DeclaringType));
                    processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.GetPushParameterMethod(method.DeclaringType)));
                }
                else
                {
                    processor.AppendInstruction(first, processor.Create(OpCodes.Ldc_I4_1));
                    processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.PushObjectMethod));
                }
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
            if (isOverride)
            {
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldc_I4_0));
                processor.AppendInstruction(first, processor.Create(OpCodes.Stfld, invokingField));
            }

            if (method.DeclaringType.IsValueType && !method.IsStatic)
            {
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
                processor.AppendInstruction(first, processor.Create(OpCodes.Ldloca, invokeCtx));
                if (refIdxCur > 0)
                    processor.AppendLdc(first, 0 + refIdxCur);
                else
                    processor.AppendLdc(first, 0);
                processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.GetReadResultByIndexMethod(method.DeclaringType)));
                processor.AppendInstruction(first, processor.Create(OpCodes.Stobj, method.DeclaringType));
            }
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

            return methodCtx;
        }
        public static AssemblyInjector CreateInjector(Stream srcAsm, Stream pdbStream = null, IPatchSettings settings = null, IAssemblyResolver resolver = null)
        {
            return new AssemblyInjector(srcAsm, pdbStream, resolver, settings);
        }
    }
}
