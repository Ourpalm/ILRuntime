using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime.Intepreter.OpCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ILRuntime.Hybrid
{ 
    public class PatchGenerator
    {
        AssemblyHashInfo asmInfo;
        AssemblyHashInfo patchAsmInfo;
        AssemblyPatchInfo patchInfo;
        TypePatchInfo privateImplementationType;
        Dictionary<string, FieldPatchInfo> privateImplementationFields;
        TypeReference byteArrayType;
        IAssemblyResolver resolver;

        public bool HasPatch { get; private set; }

        public AssemblyPatchInfo PatchInfo { get { return patchInfo; } }
        public PatchGenerator(Stream srcHash, Stream patchedAsm, IAssemblyResolver resolver = null, IPatchSettings settings = null)
        {
            asmInfo = AssemblyHashInfo.FromStream(srcHash);
            this.resolver = resolver;
            LoadAssemblyHashInfo(patchedAsm, resolver, settings);
        }

        public PatchGenerator(AssemblyHashInfo hashInfo, Stream patchedAsm, IAssemblyResolver resolver = null, IPatchSettings settings = null)
        {
            asmInfo = hashInfo;
            this.resolver = resolver;
            LoadAssemblyHashInfo(patchedAsm, resolver, settings);
        }

        void LoadAssemblyHashInfo(Stream patchAsm, IAssemblyResolver resolver, IPatchSettings settings)
        {
            if (resolver == null)
                resolver = new DefaultAssemblyResolver();
            ReaderParameters readerParameters = new ReaderParameters
            {
                AssemblyResolver = resolver,
            };
            var def = AssemblyDefinition.ReadAssembly(patchAsm, readerParameters);
            patchAsmInfo = AssemblyHashInfo.BuildHashInfo(def, settings);

            privateImplementationType = new TypePatchInfo()
            {
                Namespace = "ILRuntime.Hybrid",
                Name = $"<PrivateImplementationDetails>_{patchAsmInfo.Hash}",
                IsNew = true,
                Methods = new MethodPatchInfo[0],
            };
            privateImplementationFields = new Dictionary<string, FieldPatchInfo>();
            byteArrayType = patchAsmInfo.Assembly.MainModule.ImportReference(typeof(byte[]));
        }

        bool CheckIsRootClass(TypeDefinition td, out TypeReference baseType, HashSet<MemberReference> patchingSet)
        {
            baseType = td.BaseType;
            while (baseType != null)
            {
                if (patchingSet.Contains(baseType))
                    break;
                var bt = baseType.Resolve();
                baseType = bt.BaseType;
            }

            if (baseType != null)
            {
                return !patchingSet.Contains(baseType);
            }
            return true;
        }

        public void Analyze()
        {
            HashSet<MemberReference> internalRefs = new HashSet<MemberReference>();
            Dictionary<FieldDefinition, int> fieldIdxMapping = new Dictionary<FieldDefinition, int>();
            Dictionary<string, TypeHashInfo> originalMapping = new Dictionary<string, TypeHashInfo>();
            foreach (var i in asmInfo.Types)
            {
                originalMapping[i.Name] = i;
            }
            List<TypePatchInfo> types = new List<TypePatchInfo>();

            foreach(var i in patchAsmInfo.Types)
            {
                List<FieldPatchInfo> fieldPatch = null;
                List<MethodPatchInfo> methodPatch = null;
                if(originalMapping.TryGetValue(i.Name, out var oriType))
                {
                    if(i.Hash != oriType.Hash)
                    {
                        foreach(var f in i.Fields)
                        {
                            var oriField = oriType.FindField(f.Name);
                            if(!f.IsForeign && oriField.Hash != f.Hash)//Foreign fields cannot be patched
                            {
                                if (fieldPatch == null)
                                    fieldPatch = new List<FieldPatchInfo>();
                                fieldPatch.Add(new FieldPatchInfo()
                                {
                                    Name = f.Name,
                                    Definition = f.Definition.Resolve(),
                                });
                                internalRefs.Add(f.Definition);
                                internalRefs.Add(f.Definition.Resolve());
                            }
                            else
                            {
                                fieldIdxMapping[f.Definition.Resolve()] = oriField.Index;
                            }
                        }
                    }
                    else
                    {
                        foreach (var f in i.Fields)
                        {
                            var oriField = oriType.FindField(f.Name);
                            if (!string.IsNullOrEmpty(oriField.Hash))
                                fieldIdxMapping[f.Definition.Resolve()] = oriField.Index;
                        }
                    }
                    foreach(var m in i.Methods)
                    {
                        var oriMethod = oriType.FindMethod(m.Hash);
                        if (oriMethod.FullHash != m.FullHash)
                        {
                            if (methodPatch == null)
                                methodPatch = new List<MethodPatchInfo>();
                            methodPatch.Add(new MethodPatchInfo()
                            {
                                Index = oriMethod.Index,
                                IsNew = string.IsNullOrEmpty(oriMethod.FullHash),
                                Name = m.Name,
                                Definition = m.Definition
                            });
                            internalRefs.Add(m.Definition);
                        }
                    }
                }

                if (fieldPatch != null || methodPatch != null)
                {
                    TypePatchInfo info = new TypePatchInfo();
                    info.Name = i.Name;
                    info.Namespace = i.Namespace;
                    info.Index = i.Index;
                    info.Definition = i.Definition;
                    info.IsNew = string.IsNullOrEmpty(oriType.Hash);
                    info.Fields = fieldPatch != null ? fieldPatch.ToArray() : new FieldPatchInfo[0];
                    info.Methods = methodPatch != null ? methodPatch.ToArray() : new MethodPatchInfo[0];
                    if(i.Definition.HasGenericParameters)
                    {
                        info.IsGenericDefinition = true;
                        info.GenericParameters = new string[i.Definition.GenericParameters.Count];
                        for (int j = 0; j < info.GenericParameters.Length; j++)
                        {
                            info.GenericParameters[j] = i.Definition.GenericParameters[j].Name;
                        }
                    }
                    types.Add(info);
                    internalRefs.Add(i.Definition);
                }
            }

            if (types.Count > 0)
            {
                foreach (var t in types)
                {
                    TypeReference declaredType = t.Definition;
                    if(!CheckIsRootClass(t.Definition, out var baseType, internalRefs))
                    {
                        t.BaseType = TypeReferencePatchInfo.Create(baseType, internalRefs);
                    }
                    if (declaredType.HasGenericParameters)
                    {
                        declaredType = declaredType.MakeGenericInstanceType(declaredType.GenericParameters.ToArray());
                    }
                    //postponed initialization, so internal references can be resolved correctly
                    for (int i = 0; i < t.Fields.Length; i++)
                    {
                        ref var f = ref t.Fields[i];
                        f.IsStatic = f.Definition.IsStatic;
                        f.FieldType = TypeReferencePatchInfo.Create(f.Definition.FieldType, internalRefs);
                    }
                    for (int i = 0; i < t.Methods.Length; i++)
                    {
                        ref var m = ref t.Methods[i];
                        List<TypeReferencePatchInfo> parameters = new List<TypeReferencePatchInfo>();
                        List<TypeReferencePatchInfo> variables = new List<TypeReferencePatchInfo>();
                        if (!m.Definition.IsStatic && !t.IsNew)
                        {
                            var pt = TypeReferencePatchInfo.Create(declaredType, internalRefs);
                            parameters.Add(pt);
                        }
                        m.IsStatic = !t.IsNew || m.Definition.IsStatic;
                        foreach (var p in m.Definition.Parameters)
                        {
                            TypeReferencePatchInfo param = TypeReferencePatchInfo.Create(p.ParameterType, internalRefs);
                            parameters.Add(param);
                        }
                        if (t.Definition.HasGenericParameters)
                        {
                            m.GenericParameters = new TypeReferencePatchInfo[t.Definition.GenericParameters.Count];
                            for (int j = 0; j < m.GenericParameters.Length; j++)
                            {
                                m.GenericParameters[j] = TypeReferencePatchInfo.Create(t.Definition.GenericParameters[j], internalRefs);
                            }
                        }
                        else
                            m.GenericParameters = new TypeReferencePatchInfo[0];
                        m.Parameters = parameters.ToArray();
                        foreach (var v in m.Definition.Body.Variables)
                        {
                            TypeReferencePatchInfo vars = TypeReferencePatchInfo.Create(v.VariableType, internalRefs);
                            variables.Add(vars);
                        }
                        m.LocalVariables = variables.ToArray();
                        m.ReturnType = TypeReferencePatchInfo.Create(m.Definition.ReturnType, internalRefs);
                    }
                }
                patchInfo = new AssemblyPatchInfo();
                patchInfo.Name = asmInfo.Name;
                patchInfo.BaseHash = asmInfo.Hash;

                BuildMethodBody(types, internalRefs, fieldIdxMapping);

                if (privateImplementationFields.Count > 0)
                {
                    privateImplementationType.Fields = privateImplementationFields.Values.ToArray();
                    types.Add(privateImplementationType);
                }

                patchInfo.Types = types.ToArray();
                HasPatch = true;
            }
        }

        void PrepareStaticFieldOpcode(Runtime.Enviorment.AppDomain app, ref OpCode opcode, Dictionary<long, int> staticFieldIdxMapping, List<FieldReferencePatchInfo> fields, HashSet<MemberReference> internalRefs, Dictionary<FieldDefinition, int> fieldIdxMapping)
        {
            var t = app.GetType((int)(opcode.TokenLong >> 32));
            if (t == null) throw new Exception();

            if (!staticFieldIdxMapping.TryGetValue(opcode.TokenLong, out var fIdx))
            {
                FieldReference fr = null;
                if (t is ILType ilType)
                {
                    fr = ilType.StaticFieldReferences[(int)opcode.TokenLong];
                }
                else
                {
                    fr = patchAsmInfo.Assembly.MainModule.ImportReference(((CLRType)t).Fields[(int)opcode.TokenLong]);
                }
                if (fr.DeclaringType.Name.Contains("<PrivateImplementationDetails>"))
                {
                    if(!privateImplementationFields.TryGetValue(fr.Name, out var fieldInfo))
                    {
                        fieldInfo = new FieldPatchInfo()
                        {
                            FieldType = TypeReferencePatchInfo.Create(byteArrayType, internalRefs),                            
                            IsStatic =true,
                            Name = fr.Name,
                            InitialValues = fr.Resolve().InitialValue
                        };
                        privateImplementationFields.Add(fr.Name, fieldInfo);
                    }
                    FieldReferencePatchInfo frpi = new FieldReferencePatchInfo()
                    {
                        DeclaringType = new TypeReferencePatchInfo()
                        {
                            Name = $"{privateImplementationType.Namespace}.{privateImplementationType.Name}",
                            IsInternal = true,
                        },
                        FieldType = TypeReferencePatchInfo.Create(byteArrayType, internalRefs),
                        IsStatic = true,
                        IsInternal = true,
                        Name = fr.Name
                    };
                    fIdx = fields.Count;
                    staticFieldIdxMapping[opcode.TokenLong] = fIdx;
                    fields.Add(frpi);
                }
                else
                {
                    fIdx = fields.Count;
                    staticFieldIdxMapping[opcode.TokenLong] = fIdx;
                    fields.Add(FieldReferencePatchInfo.Create(fr, internalRefs, fieldIdxMapping));
                }
            }
            opcode.TokenLong = fIdx;
        }

        void BuildMethodBody(List<TypePatchInfo> typeInfos, HashSet<MemberReference> internalRefs, Dictionary<FieldDefinition, int> patchFieldIdxMapping)
        {
            List<TypeReferencePatchInfo> types = new List<TypeReferencePatchInfo>();
            Dictionary<IType, int> typeIdxMapping = new Dictionary<IType, int>();
            List<MethodReferencePatchInfo> methods = new List<MethodReferencePatchInfo>();
            Dictionary<IMethod, int> methodIdxMapping = new Dictionary<IMethod, int>();
            List<string> strings = new List<string>();
            Dictionary<long, int> stringIdxMapping = new Dictionary<long, int>();
            List<FieldReferencePatchInfo> fields = new List<FieldReferencePatchInfo>();
            Dictionary<long, int> fieldIdxMapping = new Dictionary<long, int>();
            Dictionary<long, int> staticFieldIdxMapping = new Dictionary<long, int>();
            Dictionary<int, int> jumptableIdxMapping = new Dictionary<int, int>();
            List<int[]> jumptables = new List<int[]>();
            Runtime.Enviorment.AppDomain app = new Runtime.Enviorment.AppDomain();
            app.SuppressStaticConstructor = true;
            foreach (var i in patchAsmInfo.Assembly.MainModule.AssemblyReferences)
            {
                var asm = resolver?.Resolve(i);
                if (asm != null)
                    app.InitializeFromModule(asm.MainModule);
            }
            app.InitializeFromModule(patchAsmInfo.Assembly.MainModule);

            foreach (var i in typeInfos)
            {
                var type = app.GetType(i.Definition, null, null) as ILType;
                if (type == null)
                    throw new Exception();
                for (int j = 0; j < i.Methods.Length; j++)
                {
                    ref var mi = ref i.Methods[j];
                    var method = type.GetMethod(mi.Definition) as ILMethod;
                    if (method == null)
                        throw new Exception();
                    var body = method.Body;
                    mi.CodeBody = new OpCode[body.Length];
                    for (int idx = 0; idx < body.Length; idx++)
                    {
                        var opcode = body[idx];
                        switch (opcode.Code)
                        {
                            case OpCodeEnum.Call:
                            case OpCodeEnum.Newobj:
                            case OpCodeEnum.Ldftn:
                            case OpCodeEnum.Ldvirtftn:
                            case OpCodeEnum.Callvirt:
                                {
                                    var m = app.GetMethod(opcode.TokenInteger);
                                    if(m == null)
                                    {
                                        int exToken = (int)(((ulong)opcode.TokenLong & 0xFFFFFFFF00000000) >> 32);
                                        if (exToken > 0)
                                        {
                                            var ex = app.GetException(exToken);
                                            if (ex != null)
                                            {
                                                throw ex;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (opcode.Code == OpCodeEnum.Call && method.IsConstructor && m.IsConstructor && (m.DeclearingType == type.BaseType || m.DeclearingType == type))
                                        {
                                            //calling other base type's constructor inside constructor cannot be patched, therefore should also be ignored in the patch
                                            opcode.TokenInteger = 0;
                                            int paramCnt = m.ParameterCount;
                                            if (!m.IsStatic)
                                                paramCnt++;
                                            opcode.TokenLong = paramCnt;
                                        }
                                        else
                                        {
                                            if (!methodIdxMapping.TryGetValue(m, out var mIdx))
                                            {
                                                mIdx = methods.Count;
                                                methods.Add(MethodReferencePatchInfo.Create(m, patchAsmInfo.Assembly.MainModule, internalRefs));
                                                methodIdxMapping[m] = mIdx;
                                            }//irrelevant methods has TokenInteger==0, to distinguish that, we start from 1
                                            opcode.TokenInteger = mIdx + 1;
                                        }                                        
                                    }
                                }
                                break;
                            case OpCodeEnum.Box:
                            case OpCodeEnum.Unbox_Any:
                            case OpCodeEnum.Unbox:
                            case OpCodeEnum.Initobj:
                            case OpCodeEnum.Isinst:
                            case OpCodeEnum.Newarr:
                            case OpCodeEnum.Stobj:
                            case OpCodeEnum.Ldobj:
                            case OpCodeEnum.Castclass:
                                {
                                    var t = app.GetType(opcode.TokenInteger);
                                    if(!typeIdxMapping.TryGetValue(t, out var tIdx))
                                    {
                                        tIdx = types.Count;
                                        types.Add(TypeReferencePatchInfo.Create(t, patchAsmInfo.Assembly.MainModule, internalRefs));
                                        typeIdxMapping[t] = tIdx;
                                    }
                                    opcode.TokenInteger = tIdx;
                                }
                                break;
                            case OpCodeEnum.Constrained:
                                {
                                    var t = app.GetType(opcode.TokenInteger);
                                    if (t == null)
                                        throw new Exception();
                                    if (!typeIdxMapping.TryGetValue(t, out var tIdx))
                                    {
                                        tIdx = types.Count;
                                        types.Add(TypeReferencePatchInfo.Create(t, patchAsmInfo.Assembly.MainModule, internalRefs));
                                        typeIdxMapping[t] = tIdx;
                                    }
                                    var m = app.GetMethod((int)opcode.TokenLong);
                                    if (m == null)
                                        throw new Exception();
                                    if (!methodIdxMapping.TryGetValue(m, out var mIdx))
                                    {
                                        mIdx = methods.Count;
                                        methods.Add(MethodReferencePatchInfo.Create(m, patchAsmInfo.Assembly.MainModule, internalRefs));
                                        methodIdxMapping[m] = mIdx;
                                    }
                                    opcode.TokenInteger = tIdx;
                                    //irrelevant methods has TokenInteger==0, to distinguish that, we start from 1
                                    opcode.TokenLong = mIdx + 1;
                                }
                                break;
                            case OpCodeEnum.Stfld:
                            case OpCodeEnum.Ldfld:
                            case OpCodeEnum.Ldflda:
                                {
                                    var t = app.GetType((int)(opcode.TokenLong >> 32));
                                    if (t == null) throw new Exception();

                                    if (!fieldIdxMapping.TryGetValue(opcode.TokenLong, out var fIdx))
                                    {
                                        FieldReference fr = null;
                                        if (t is ILType ilType)
                                        {
                                            ilType.GetField((int)opcode.TokenLong, out fr);
                                        }
                                        else
                                        {
                                            fr = patchAsmInfo.Assembly.MainModule.ImportReference(((CLRType)t).Fields[(int)opcode.TokenLong]);
                                        }
                                        fIdx = fields.Count;
                                        fieldIdxMapping[opcode.TokenLong] = fIdx;
                                        fields.Add(FieldReferencePatchInfo.Create(fr, internalRefs, patchFieldIdxMapping));
                                    }
                                    opcode.TokenLong = fIdx;
                                }
                                break;
                            case OpCodeEnum.Stsfld:
                            case OpCodeEnum.Ldsfld:
                            case OpCodeEnum.Ldsflda:
                                PrepareStaticFieldOpcode(app, ref opcode, staticFieldIdxMapping, fields, internalRefs, patchFieldIdxMapping);
                                break;
                            case OpCodeEnum.Ldstr:
                                {
                                    var s = app.GetString(opcode.TokenLong);
                                    if (s == null) throw new Exception();
                                    if(!stringIdxMapping.TryGetValue(opcode.TokenLong, out var sIdx))
                                    {
                                        sIdx = strings.Count;
                                        stringIdxMapping[opcode.TokenLong] = sIdx;
                                        strings.Add(s);
                                    }
                                    opcode.TokenLong = sIdx;
                                }
                                break;
                            case OpCodeEnum.Ldtoken:
                                {
                                    switch (opcode.TokenInteger)
                                    {
                                        case 0:
                                            PrepareStaticFieldOpcode(app, ref opcode, staticFieldIdxMapping, fields, internalRefs, patchFieldIdxMapping);
                                            break;
                                        case 1:
                                            var t = app.GetType((int)opcode.TokenLong);
                                            if (!typeIdxMapping.TryGetValue(t, out var tIdx))
                                            {
                                                tIdx = types.Count;
                                                types.Add(TypeReferencePatchInfo.Create(t, patchAsmInfo.Assembly.MainModule, internalRefs));
                                                typeIdxMapping[t] = tIdx;
                                            }
                                            opcode.TokenLong = tIdx;
                                            break;
                                        default:
                                            throw new Exception();
                                    }
                                }
                                break;
                            case OpCodeEnum.Switch:
                                {
                                    if (!jumptableIdxMapping.TryGetValue(opcode.TokenInteger, out var sIdx))
                                    {
                                        sIdx = jumptables.Count;
                                        jumptableIdxMapping[opcode.TokenInteger] = sIdx;
                                        jumptables.Add(method.JumpTables[opcode.TokenInteger]);
                                    }
                                    opcode.TokenInteger = sIdx;
                                }
                                break;
                            default:
                                break;
                        }

                        mi.CodeBody[idx] = opcode;
                    }
                }
            }

            patchInfo.TypeReferences = types.ToArray();
            patchInfo.MethodReferences = methods.ToArray();
            patchInfo.FieldReferences = fields.ToArray();
            patchInfo.StringReferences = strings.ToArray();
            patchInfo.JumpTables = jumptables.ToArray();

            app.Dispose();
        }

        public void SavePatch(Stream stream)
        {
            if (patchInfo == null || !HasPatch)
                throw new NotSupportedException("There is nothing to patch");

            BinaryWriter bw = new BinaryWriter(stream);
            patchInfo.WriteToStream(bw);
        }
    }
}
