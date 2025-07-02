using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Intepreter.OpCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ILRuntime.Hybrid
{
    public class AssemblyPatchInfo
    {
        const int AssemblyPatchMagic = 0x58883551;
        const short Version = 1;
        public string Name { get; internal set; }
        public string BaseHash { get;internal set; }

        public TypePatchInfo[] Types { get; internal set; }
        public TypeReferencePatchInfo[] TypeReferences { get; internal set; }
        public MethodReferencePatchInfo[] MethodReferences { get; internal set; }
        public FieldReferencePatchInfo[] FieldReferences { get; internal set; }
        public string[] StringReferences { get; internal set; }
        public int[][] JumpTables { get; internal set; }

        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(AssemblyPatchMagic);
            bw.Write(Version);
            bw.Write(Name);
            bw.Write(Convert.FromBase64String(BaseHash));
            bw.Write(TypeReferences.Length);
            for (int i = 0; i < TypeReferences.Length; i++)
            {
                TypeReferences[i].WriteToStream(bw);
            }
            bw.Write(MethodReferences.Length);
            for (int i = 0; i < MethodReferences.Length; i++)
            {
                MethodReferences[i].WriteToStream(bw);
            }
            bw.Write(FieldReferences.Length);
            for (int i = 0; i < FieldReferences.Length; i++)
            {
                FieldReferences[i].WriteToStream(bw);
            }
            bw.Write(StringReferences.Length);
            for (int i = 0; i < StringReferences.Length; i++)
            {
                bw.Write(StringReferences[i]);
            }
            bw.Write(JumpTables.Length);
            for (int i = 0; i < JumpTables.Length; i++)
            {
                var arr = JumpTables[i];
                bw.Write(arr.Length);
                for(int j = 0; j < arr.Length; j++)
                {
                    bw.Write(arr[j]);
                }
            }
            bw.Write(Types.Length);
            for(int i = 0; i < Types.Length; i++)
            {
                var type = Types[i];
                type.WriteToStream(bw);
            }
        }

        public static AssemblyPatchInfo FromStream(BinaryReader br)
        {
            if (br.ReadInt32() != AssemblyPatchMagic)
                throw new NotSupportedException("Wrong Patch File Magic");
            if (br.ReadInt16() != Version)
                throw new NotSupportedException("Incorrect Patch File Version");
            AssemblyPatchInfo info = new AssemblyPatchInfo();
            info.Name = br.ReadString();
            info.BaseHash = Convert.ToBase64String(br.ReadBytes(16));
            int cnt = br.ReadInt32();
            info.TypeReferences = new TypeReferencePatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.TypeReferences[i] = TypeReferencePatchInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.MethodReferences = new MethodReferencePatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.MethodReferences[i] = MethodReferencePatchInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.FieldReferences = new FieldReferencePatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.FieldReferences[i] = FieldReferencePatchInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.StringReferences = new string[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.StringReferences[i] = br.ReadString();
            }
            cnt = br.ReadInt32();
            info.JumpTables = new int[cnt][];
            for (int i = 0; i < cnt; i++)
            {
                int eCnt = br.ReadInt32();
                int[] arr = new int[eCnt];
                for (int j = 0; j < eCnt; j++)
                {
                    arr[j] = br.ReadInt32();
                }
                info.JumpTables[i] = arr;
            }
            cnt = br.ReadInt32();
            info.Types = new TypePatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.Types[i] = TypePatchInfo.FromStream(br);
            }
            return info;
        }
    }

    public class TypePatchInfo
    {
        internal TypeDefinition Definition { get; set; }

        public string Name { get; internal set; }
        public string Namespace { get; internal set; }

        public int Index { get; internal set; }

        public bool IsNew { get; internal set; }
        public bool IsGenericDefinition { get; internal set; }

        public FieldPatchInfo[] Fields { get; internal set; }

        public MethodPatchInfo[] Methods { get; internal set; }

        public string[] GenericParameters { get; internal set; }

        public TypeReferencePatchInfo BaseType { get; internal set; }

        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Namespace);
            bw.Write(Index);
            bw.Write(IsNew);
            bw.Write(IsGenericDefinition);
            bw.Write(Fields.Length);
            for (int i = 0; i < Fields.Length; i++)
            {
                Fields[i].WriteToStream(bw);
            }
            bw.Write(Methods.Length);
            for (int i = 0; i < Methods.Length; i++)
            {
                Methods[i].WriteToStream(bw);
            }
            if (IsGenericDefinition)
            {
                bw.Write(GenericParameters.Length);
                for (int i = 0; i < GenericParameters.Length; i++)
                {
                    bw.Write(GenericParameters[i]);
                }
            }
            bw.Write(BaseType != null);
            if (BaseType != null)
            {
                BaseType.WriteToStream(bw);
            }
        }

        public static TypePatchInfo FromStream(BinaryReader br)
        {
            TypePatchInfo info = new TypePatchInfo();
            info.Name = br.ReadString();
            info.Namespace = br.ReadString();
            info.Index = br.ReadInt32();
            info.IsNew = br.ReadBoolean();
            info.IsGenericDefinition = br.ReadBoolean();
            int cnt = br.ReadInt32();
            info.Fields = new FieldPatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.Fields[i] = FieldPatchInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.Methods = new MethodPatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.Methods[i] = MethodPatchInfo.FromStream(br);
            }
            if (info.IsGenericDefinition)
            {
                cnt = br.ReadInt32();
                info.GenericParameters = new string[cnt];
                for(int  i = 0; i < cnt;i++)
                {
                    info.GenericParameters[i] = br.ReadString();
                }
            }
            if(br.ReadBoolean())
            {
                info.BaseType = TypeReferencePatchInfo.FromStream(br);
            }
            return info;
        }
    }
    public struct FieldPatchInfo
    {
        internal FieldDefinition Definition { get; set; }
        public string Name { get; internal set; }
        public bool IsStatic {  get; internal set; }
        public TypeReferencePatchInfo FieldType { get; internal set; }

        public byte[] InitialValues { get; internal set; }

        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(IsStatic);
            FieldType.WriteToStream(bw);
            bw.Write(InitialValues != null ? InitialValues.Length : 0);
            if (InitialValues != null)
            {
                bw.Write(InitialValues);
            }
        }

        public static FieldPatchInfo FromStream(BinaryReader br)
        {
            FieldPatchInfo info = new FieldPatchInfo();
            info.Name = br.ReadString();
            info.IsStatic= br.ReadBoolean();
            info.FieldType = TypeReferencePatchInfo.FromStream(br);
            int cnt = br.ReadInt32();
            if(cnt >0)
            {
                info.InitialValues = br.ReadBytes(cnt);
            }
            return info;
        }
    }

    public struct FieldReferencePatchInfo
    {
        public TypeReferencePatchInfo DeclaringType { get; internal set; }
        public TypeReferencePatchInfo FieldType { get; internal set; }
        public string Name { get; internal set; }
        public bool IsStatic { get; internal set; }
        public bool IsInternal { get; internal set; }
        public int FieldIndex { get; internal set; }

        public static FieldReferencePatchInfo Create(FieldReference fr, HashSet<MemberReference> internalRefs, Dictionary<FieldDefinition, int> fieldIdxMapping)
        {
            FieldReferencePatchInfo result = new FieldReferencePatchInfo();
            result.Name = fr.Name;
            result.IsStatic = fr.Resolve().IsStatic;
            result.DeclaringType = TypeReferencePatchInfo.Create(fr.DeclaringType, internalRefs);
            result.FieldType = TypeReferencePatchInfo.Create(fr.FieldType, internalRefs);
            result.IsInternal = internalRefs.Contains(fr) || internalRefs.Contains(fr.Resolve());
            if (fieldIdxMapping.TryGetValue(fr.Resolve(), out var index))
                result.FieldIndex = index;
            return result;
        }
        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(IsStatic);
            bw.Write(IsInternal);
            bw.Write(FieldIndex);
            FieldType.WriteToStream(bw);
            DeclaringType.WriteToStream(bw);
        }

        public static FieldReferencePatchInfo FromStream(BinaryReader br)
        {
            FieldReferencePatchInfo info = new FieldReferencePatchInfo();
            info.Name = br.ReadString();
            info.IsStatic = br.ReadBoolean();
            info.IsInternal = br.ReadBoolean();
            info.FieldIndex = br.ReadInt32();
            info.FieldType = TypeReferencePatchInfo.FromStream(br);
            info.DeclaringType = TypeReferencePatchInfo.FromStream(br);
            return info;
        }
    }

    public class MethodReferencePatchInfo
    {
        public string Name { get; internal set; }
        public TypeReferencePatchInfo[] Parameters { get; internal set; }
        public MethodReferencePatchInfo GenericDefinition { get; internal set; }
        public TypeReferencePatchInfo DeclaringType { get; internal set; }

        public KeyValuePair<string, TypeReferencePatchInfo>[] GenericArguments { get; internal set; }
        public bool IsGenericInstance { get; internal set; }
        public bool IsInternal { get; internal set; }

        public bool IsStatic { get; internal set; }

        public static MethodReferencePatchInfo Create(IMethod method, ModuleDefinition module, HashSet<MemberReference> internalRefs)
        {
            if(method is ILMethod ilMethod)
            {
                return Create(ilMethod.MethodReference, internalRefs);
            }
            else
            {
                CLRMethod clr = (CLRMethod)method;
                if (clr.DeclearingType.IsGenericInstance)
                {
                    var dtr = clr.DeclearingType.ToTypeReference(module);
                    MethodReference mr = new MethodReference(clr.Name, clr.ReturnType.ToTypeReference(module), dtr);
                    for(int i = 0; i < clr.ParameterCount; i++)
                    {
                        mr.Parameters.Add(new ParameterDefinition(clr.Parameters[i].ToTypeReference(module)));
                    }
                    var res = Create(mr, internalRefs);
                    res.IsStatic = clr.IsStatic;
                    return res;
                }
                else
                {
                    if (clr.IsConstructor)
                        return Create(module.ImportReference(clr.ConstructorInfo), internalRefs);
                    else
                        return Create(module.ImportReference(clr.MethodInfo), internalRefs);
                }
            }
        }

        public static MethodReferencePatchInfo Create(MethodReference mr, HashSet<MemberReference> internalRefs)
        {
            MethodReferencePatchInfo result = new MethodReferencePatchInfo();
            if (mr.IsGenericInstance)
            {
                GenericInstanceMethod gim = mr as GenericInstanceMethod;

                result.IsGenericInstance = true;
                result.GenericDefinition = Create(gim.ElementMethod, internalRefs);
                result.GenericArguments = new KeyValuePair<string, TypeReferencePatchInfo>[gim.GenericArguments.Count];
                for (int i = 0; i < gim.GenericArguments.Count; i++)
                {
                    result.GenericArguments[i] = new KeyValuePair<string, TypeReferencePatchInfo>(gim.ElementMethod.GenericParameters[i].Name, TypeReferencePatchInfo.Create(gim.GenericArguments[i], internalRefs));
                }
            }
            else
            {
                var mdef = mr.Resolve();
                result.DeclaringType = TypeReferencePatchInfo.Create(mr.DeclaringType, internalRefs);
                result.Name = mr.Name;
                result.Parameters = new TypeReferencePatchInfo[mr.Parameters.Count];
                result.IsInternal = internalRefs.Contains(mr) || (mdef != null && internalRefs.Contains(mdef));
                result.IsStatic = mdef != null ? mdef.IsStatic : false;
                for (int i = 0; i < mr.Parameters.Count; i++)
                {
                    result.Parameters[i] = TypeReferencePatchInfo.Create(mr.Parameters[i].ParameterType, internalRefs);
                }
            }

            return result;
        }

        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(IsGenericInstance);
            bw.Write(IsInternal);
            bw.Write(IsStatic);
            if (IsGenericInstance)
            {
                GenericDefinition.WriteToStream(bw);
                bw.Write(GenericArguments.Length);
                for (int i = 0; i < GenericArguments.Length; i++)
                {
                    var arg = GenericArguments[i];
                    bw.Write(arg.Key);
                    arg.Value.WriteToStream(bw);
                }
            }
            else
            {
                DeclaringType.WriteToStream(bw);
                bw.Write(Name);
                bw.Write(Parameters.Length);
                for (int i = 0; i < Parameters.Length; i++)
                {
                    Parameters[i].WriteToStream(bw);
                }
            }
        }

        public static MethodReferencePatchInfo FromStream(BinaryReader br)
        {
            MethodReferencePatchInfo info = new MethodReferencePatchInfo();
            info.IsGenericInstance = br.ReadBoolean();
            info.IsInternal = br.ReadBoolean();
            info.IsStatic = br.ReadBoolean();
            if (info.IsGenericInstance)
            {
                info.GenericDefinition = FromStream(br);
                int cnt = br.ReadInt32();
                info.GenericArguments = new KeyValuePair<string, TypeReferencePatchInfo>[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    info.GenericArguments[i] = new KeyValuePair<string, TypeReferencePatchInfo>(br.ReadString(), TypeReferencePatchInfo.FromStream(br));
                }
            }
            else
            {
                info.DeclaringType = TypeReferencePatchInfo.FromStream(br);
                info.Name = br.ReadString();
                int cnt = br.ReadInt32();
                info.Parameters = new TypeReferencePatchInfo[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    info.Parameters[i] = TypeReferencePatchInfo.FromStream(br);
                }
            }
            
            return info;
        }
    }

    public struct MethodPatchInfo
    {
        internal MethodDefinition Definition { get; set; }
        public string Name { get; set; }
        public TypeReferencePatchInfo[] Parameters { get; internal set; }

        public TypeReferencePatchInfo[] GenericParameters { get; internal set; }
        public TypeReferencePatchInfo ReturnType { get; internal set; }

        public bool IsNew { get; internal set; }
        public bool IsStatic { get; internal set; }
        public int Index { get; internal set; }

        public bool IsRegisterVM { get; internal set; }

        public TypeReferencePatchInfo[] LocalVariables { get; internal set; }
        internal OpCode[] CodeBody { get; set; }

        internal OpCodeR[] CodeBodyRegister { get; set; }
        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Index);
            bw.Write(IsNew);
            bw.Write(IsStatic);
            bw.Write(IsRegisterVM);
            ReturnType.WriteToStream(bw);
            bw.Write(GenericParameters.Length);
            for (int i = 0; i < GenericParameters.Length; i++)
            {
                GenericParameters[i].WriteToStream(bw);
            }
            bw.Write(Parameters.Length);
            for (int i = 0; i < Parameters.Length; i++)
            {
                Parameters[i].WriteToStream(bw);
            }
            bw.Write(LocalVariables.Length);
            for (int i = 0; i < LocalVariables.Length; i++)
            {
                LocalVariables[i].WriteToStream(bw);
            }
            bw.Write(CodeBody.Length);
            for (int i = 0; i < CodeBody.Length; i++)
            {
                var opcode = CodeBody[i];
                bw.Write((short)opcode.Code);
                bw.Write(opcode.TokenInteger);
                bw.Write(opcode.TokenLong);
            }
        }

        public static MethodPatchInfo FromStream(BinaryReader br)
        {
            MethodPatchInfo info = new MethodPatchInfo();
            info.Name = br.ReadString();
            info.Index = br.ReadInt32();
            info.IsNew = br.ReadBoolean();
            info.IsStatic = br.ReadBoolean();
            info.IsRegisterVM = br.ReadBoolean();
            info.ReturnType = TypeReferencePatchInfo.FromStream(br);
            int cnt = br.ReadInt32();
            info.GenericParameters = new TypeReferencePatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.GenericParameters[i] = TypeReferencePatchInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.Parameters = new TypeReferencePatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.Parameters[i] = TypeReferencePatchInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.LocalVariables = new TypeReferencePatchInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.LocalVariables[i] = TypeReferencePatchInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.CodeBody = new OpCode[cnt];
            for (int i = 0; i < cnt; i++)
            {
                OpCode opcode = new OpCode()
                {
                    Code = (OpCodeEnum)br.ReadInt16(),
                    TokenInteger = br.ReadInt32(),
                    TokenLong = br.ReadInt64(),
                };
                info.CodeBody[i] = opcode;
            }
            return info;
        }
    }

    public class TypeReferencePatchInfo
    {
        public string Name { get; internal set; }
        public TypeReferencePatchInfo ElementType { get; internal set; }
        public KeyValuePair<string, TypeReferencePatchInfo>[] GenericArguments { get; internal set; }
        public bool IsArray { get; internal set; }
        public bool IsByReference { get; internal set; }
        public bool IsGenericParameter { get; internal set; }
        public bool IsGenericInstance { get; internal set; }
        public bool IsInternal { get; internal set; }

        public static TypeReferencePatchInfo Create(IType type, ModuleDefinition module, HashSet<MemberReference> internalRefs)
        {
            if(type is ILType iltype)
            {
                return Create(iltype.TypeReference, internalRefs);
            }
            else if(type is ILGenericParameterType gpt)
            {
                return Create(gpt.TypeReference, internalRefs);
            }
            else
            {
                if (type.IsGenericInstance)
                {
                    TypeReferencePatchInfo res = new TypeReferencePatchInfo();
                    res.IsGenericInstance = true;
                    var gd = ((CLRType)type).TypeForCLR.GetGenericTypeDefinition();
                    res.ElementType = Create(module.ImportReference(gd), internalRefs);
                    res.GenericArguments = new KeyValuePair<string, TypeReferencePatchInfo>[type.GenericArguments.Length];
                    for (int i = 0; i < type.GenericArguments.Length; i++)
                    {
                        res.GenericArguments[i] = new KeyValuePair<string, TypeReferencePatchInfo>(type.GenericArguments[i].Key, Create(type.GenericArguments[i].Value, module, internalRefs));
                    }
                    return res;
                }
                else
                    return Create(module.ImportReference(((CLRType)type).TypeForCLR), internalRefs);
            }
        }

        public static TypeReferencePatchInfo Create(TypeReference def, HashSet<MemberReference> internalRefs)
        {
            TypeReferencePatchInfo result = new TypeReferencePatchInfo();
            if (def.IsByReference)
            {
                result.IsByReference = true;
                result.ElementType = Create(def.GetElementType(), internalRefs);
            }
            else if (def.IsArray)
            {
                result.IsArray = true;
                result.ElementType = Create(def.GetElementType(), internalRefs);
            }
            else if (def.IsGenericParameter)
            {
                result.IsGenericParameter = true;
                result.Name = def.Name;
            }
            else if (def.IsGenericInstance)
            {
                GenericInstanceType git = def as GenericInstanceType;
                result.IsGenericInstance = true;
                result.ElementType = Create(git.ElementType, internalRefs);
                result.GenericArguments = new KeyValuePair<string, TypeReferencePatchInfo>[git.GenericArguments.Count];
                for (int i = 0; i < git.GenericArguments.Count; i++)
                {
                    result.GenericArguments[i] = new KeyValuePair<string, TypeReferencePatchInfo>(git.ElementType.GenericParameters[i].Name, Create(git.GenericArguments[i], internalRefs));
                }
            }
            else
            {
                if (def.FullName == typeof(ILGenericParameterType).FullName)
                {
                    result.IsGenericParameter = true;
                    result.Name = "T";
                }
                else
                {
                    result.Name = def.GetSafeFullNames();
                    result.IsInternal = internalRefs != null && (internalRefs.Contains(def) || internalRefs.Contains(def.Resolve()));
                }
            }
            return result;
        }
        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(IsByReference);
            bw.Write(IsArray);
            bw.Write(IsGenericParameter);
            bw.Write(IsGenericInstance);
            bw.Write(IsInternal);
            if (IsByReference)
            {
                ElementType.WriteToStream(bw);
            }
            else if (IsArray)
            {
                ElementType.WriteToStream(bw);
            }
            else if (IsGenericParameter)
            {
                bw.Write(Name);
            }
            else if (IsGenericInstance)
            {
                ElementType.WriteToStream(bw);
                bw.Write(GenericArguments.Length);
                for(int i = 0;i< GenericArguments.Length;i++)
                {
                    var arg = GenericArguments[i];
                    bw.Write(arg.Key);
                    arg.Value.WriteToStream(bw);
                }
            }
            else
            {
                bw.Write(Name);
            }
        }

        public static TypeReferencePatchInfo FromStream(BinaryReader br)
        {
            TypeReferencePatchInfo info = new TypeReferencePatchInfo();
            info.IsByReference = br.ReadBoolean();
            info.IsArray = br.ReadBoolean();
            info.IsGenericParameter = br.ReadBoolean();
            info.IsGenericInstance = br.ReadBoolean();
            info.IsInternal = br.ReadBoolean();
            if (info.IsByReference)
            {
                info.ElementType = FromStream(br);
            }
            else if (info.IsArray)
            {
                info.ElementType = FromStream(br);
            }
            else if (info.IsGenericParameter)
            {
                info.Name = br.ReadString();
            }
            else if (info.IsGenericInstance)
            {
                info.ElementType = FromStream(br);
                int cnt = br.ReadInt32();
                info.GenericArguments = new KeyValuePair<string, TypeReferencePatchInfo>[cnt];
                for (int i = 0; i < info.GenericArguments.Length; i++)
                {
                    info.GenericArguments[i] = new KeyValuePair<string, TypeReferencePatchInfo>(br.ReadString(), FromStream(br));
                }
            }
            else
            {
                info.Name = br.ReadString();
            }
            return info;
        }
    }
    public class AssemblyHashInfo
    {
        const int AssemblyHashMagic = 0x58883553;
        const short Version = 1;
        public string Name { get; set; }
        public string Hash { get; set; }
        public TypeHashInfo[] Types { get; set; }

        public int Index { get; set; }

        public int TypeCount { get; set; }
        public int MethodCount { get; set; }

        public AssemblyDefinition Assembly { get; private set; }

        public static AssemblyHashInfo BuildHashInfo(AssemblyDefinition asm, IPatchSettings settings)
        {
            AssemblyHashInfo info = new AssemblyHashInfo();
            info.Name = asm.FullName;
            info.Assembly = asm;
            int typeIdx = 0, methodIdx = 0, fieldIdx = 1;//Field Starts with 1 to distinguish fields without index

            var mainModule = asm.MainModule;
            mainModule.FixClosureNameConsistency(settings);
            if (mainModule.HasTypes)
            {
                List<TypeHashInfo> types = new List<TypeHashInfo>();
                foreach (var i in mainModule.Types)
                {
                    AddTypeInfo(i, types, ref typeIdx, ref fieldIdx, ref methodIdx, false, settings);
                }
                info.Types = types.ToArray();
            }

            info.TypeCount = typeIdx;
            info.MethodCount = methodIdx;
            MD5 md5 = MD5.Create();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            foreach (var i in info.Types)
            {
                bw.Write(i.Index);
                bw.Write(i.Hash);
                foreach (var j in i.Methods)
                {
                    bw.Write(j.Index);
                    bw.Write(j.FullHash);
                }
            }
            info.Hash = Convert.ToBase64String(md5.ComputeHash(ms.GetBuffer(), 0, (int)ms.Position));
            md5.Dispose();
            return info;
        }

        static void AddTypeInfo(TypeDefinition type, List<TypeHashInfo> types, ref int typeIdx, ref int fieldIdx, ref int methodIdx, bool forceInclude, IPatchSettings settings)
        {
            bool shouldIncludeBySetting = settings !=null? settings.ShouldTypeIncludeInPatch(type) : false;
            bool isIgnore = false;
            bool shoudInclude = type.ShouldIncludeInPatch(out isIgnore) || forceInclude || shouldIncludeBySetting;
            shoudInclude &= !type.IsDelegate() && !type.IsInterface && !type.IsEnum && !isIgnore;
            if (shoudInclude)
                types.Add(TypeHashInfo.BuildHashInfo(type, ref typeIdx, ref fieldIdx, ref methodIdx, settings));
            if (!isIgnore)
            {
                foreach (var t in type.NestedTypes)
                {
                    AddTypeInfo(t, types, ref typeIdx, ref fieldIdx, ref methodIdx, shoudInclude, settings);
                }
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(AssemblyHashMagic);
            bw.Write(Version);
            bw.Write(Name);
            bw.Write(Convert.FromBase64String(Hash));
            bw.Write(Index);
            bw.Write(TypeCount);
            bw.Write(MethodCount);
            bw.Write(Types.Length);
            foreach (var i in Types)
            {
                i.WriteToStream(bw);
            }
        }

        public static AssemblyHashInfo FromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            if (br.ReadInt32() != AssemblyHashMagic)
                throw new NotSupportedException("Wrong Hash File Magic");
            if (br.ReadInt16() != Version)
                throw new NotSupportedException("Incorrect Hash File Version");
            AssemblyHashInfo info = new AssemblyHashInfo();
            info.Name = br.ReadString();
            info.Hash = Convert.ToBase64String(br.ReadBytes(16));
            info.Index = br.ReadInt32();
            info.TypeCount = br.ReadInt32();
            info.MethodCount = br.ReadInt32();
            int cnt = br.ReadInt32();
            info.Types = new TypeHashInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.Types[i] = TypeHashInfo.FromStream(br);
            }
            return info;
        }
    }

    public struct TypeHashInfo
    {
        public TypeDefinition Definition { get; set; }
        public string Hash { get; set; }

        public int Index { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }

        public string AliasName { get; set; }
        public FieldHashInfo[] Fields { get; set; }

        public MethodHashInfo[] Methods { get; set; }

        static void GetBaseTypeFields(TypeDefinition type, ref List<FieldReference> fields, IPatchSettings settings, Dictionary<TypeReference, TypeReference> genericArguments = null)
        {
            if (type.BaseType == null)
                return;
            var baseType = type.BaseType.Resolve();
            bool isGenericInstance = false;
            if (type.BaseType.IsGenericInstance)
            {
                isGenericInstance = true;
                if (genericArguments == null)
                {
                    genericArguments = new Dictionary<TypeReference, TypeReference>();
                }
                var gi = type.BaseType as GenericInstanceType;
                var gp = gi.Resolve().GenericParameters;
                var ga = gi.GenericArguments;
                for (int i = 0; i < gp.Count; i++)
                {
                    genericArguments[gp[i]] = ga[i];
                }
            }
            if (baseType == null || (baseType.ShouldIncludeInPatch(out var isIgnore) && !isIgnore))
                return;
            if (settings != null && settings.ShouldTypeIncludeInPatch(baseType))
                return;
            if (type.IsDelegate() && type.IsInterface && !type.IsEnum)
                return;
            if (baseType.Fields.Count > 0)
            {
                if(fields == null)
                    fields = new List<FieldReference>();
                foreach (var i in baseType.Fields)
                {
                    if (i.IsPublic || i.IsFamily || i.IsAssembly || i.IsFamilyOrAssembly || i.IsFamilyAndAssembly)
                    {
                        if (isGenericInstance)
                        {
                            fields.Add(new FieldReference(i.Name, i.FieldType, type.BaseType));
                        }
                        else
                            fields.Add(i);
                    }
                }
            }
            GetBaseTypeFields(baseType, ref fields, settings, genericArguments);
        }
        

        public static TypeHashInfo BuildHashInfo(TypeDefinition type, ref int typeIdx, ref int fieldIdx, ref int methodIdx, IPatchSettings settings)
        {
            MD5 md5 = MD5.Create();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            TypeHashInfo info = new TypeHashInfo();
            info.Definition = type;

            type.GetTypeNames(out var namesp, out var name);
            info.Name = name;
            info.Namespace = namesp;
            info.Index = typeIdx++;
            List<FieldReference> baseFields = null;
            GetBaseTypeFields(type, ref baseFields, settings);
            int baseFieldCnt = baseFields != null ? baseFields.Count : 0;
            info.Fields = new FieldHashInfo[type.Fields.Count + baseFieldCnt];
            for (int i = 0; i < type.Fields.Count; i++)
            {
                var f = type.Fields[i];
                info.Fields[i] = new FieldHashInfo()
                {
                    Definition = f,
                    Name = f.Name,
                    Hash = f.ComputeFieldHash(bw, md5),
                    Index = fieldIdx++,
                };
            }
            for (int i = 0; i < baseFieldCnt; i++)
            {
                var f = baseFields[i];
                info.Fields[i + type.Fields.Count] = new FieldHashInfo()
                {
                    Definition = f,
                    Name = f.Name,
                    IsForeign = true,
                    Hash = f.ComputeFieldHash(bw, md5),
                    Index = fieldIdx++,
                };
            }
            info.Hash = type.ComputeFieldsHash(bw, md5);
            List<MethodHashInfo> methods = new List<MethodHashInfo>();
            for (int i = 0; i < type.Methods.Count; i++)
            {
                var m = type.Methods[i];
                if (m.ShouldIncludeInPatch())
                {
                    methods.Add(new MethodHashInfo()
                    {
                        Definition = m,
                        DeclaringType = m.DeclaringType.FullName,
                        Name = m.Name,
                        Hash = m.ComputeHash(bw, md5, false),
                        FullHash = m.ComputeHash(bw, md5, true),
                        Index = methodIdx++,
                    });
                }
            }
            info.Methods = methods.ToArray();
            md5.Dispose();
            return info;
        }

        public FieldHashInfo FindField(string fieldName)
        {
            return Fields.Where((f) => f.Name == fieldName).FirstOrDefault();
        }

        public MethodHashInfo FindMethod(string methodHash)
        {
            return Methods.Where((m) => m.Hash == methodHash).FirstOrDefault();
        }

        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Namespace);
            bool hasAliasName = !string.IsNullOrEmpty(AliasName);
            bw.Write(hasAliasName);
            if (hasAliasName)
            {
                bw.Write(AliasName);
            }
            bw.Write(Index);
            bw.Write(Convert.FromBase64String(Hash));
            bw.Write(Fields.Length);
            foreach (var i in Fields)
            {
                i.WriteToStream(bw);
            }
            bw.Write(Methods.Length);
            foreach (var m in Methods)
            {
                m.WriteToStream(bw);
            }
        }

        public static TypeHashInfo FromStream(BinaryReader br)
        {
            var info = new TypeHashInfo();
            info.Name = br.ReadString();
            info.Namespace = br.ReadString();
            if (br.ReadBoolean())
                info.AliasName = br.ReadString();
            info.Index = br.ReadInt32();
            info.Hash = Convert.ToBase64String(br.ReadBytes(16));
            int cnt = br.ReadInt32();
            info.Fields = new FieldHashInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.Fields[i] = FieldHashInfo.FromStream(br);
            }
            cnt = br.ReadInt32();
            info.Methods = new MethodHashInfo[cnt];
            for (int i = 0; i < cnt; i++)
            {
                info.Methods[i] = MethodHashInfo.FromStream(br);
            }
            return info;
        }
    }

    public struct FieldHashInfo
    {
        public FieldReference Definition { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IsForeign { get; set; }

        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Convert.FromBase64String(Hash));
            bw.Write(Index);
            bw.Write(IsForeign);
        }

        public static FieldHashInfo FromStream(BinaryReader br)
        {
            var info = new FieldHashInfo();
            info.Name = br.ReadString();
            info.Hash = Convert.ToBase64String(br.ReadBytes(16));
            info.Index = br.ReadInt32();
            info.IsForeign = br.ReadBoolean();
            return info;
        }
    }

    public struct MethodHashInfo
    {
        public int Index { get; set; }
        public MethodDefinition Definition { get; set; }
        public string DeclaringType { get; set; }

        public string Name { get; set; }
        public string Hash { get; set; }
        public string FullHash { get; set; }

        public void WriteToStream(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Index);
            bw.Write(DeclaringType);
            bw.Write(Convert.FromBase64String(Hash));
            bw.Write(Convert.FromBase64String(FullHash));
        }
        public static MethodHashInfo FromStream(BinaryReader br)
        {
            var info = new MethodHashInfo();
            info.Name = br.ReadString();
            info.Index = br.ReadInt32();
            info.DeclaringType = br.ReadString();
            info.Hash = Convert.ToBase64String(br.ReadBytes(16));
            info.FullHash = Convert.ToBase64String(br.ReadBytes(16));
            return info;
        }
    }
}
