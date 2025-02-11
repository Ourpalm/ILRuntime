using ILRuntime.Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ILRuntime.Hybrid
{
    public class AssemblyHashInfo
    {
        public string Name { get; set; }
        public TypeHashInfo[] Types { get; set; }

        public static AssemblyHashInfo BuildHashInfo(AssemblyDefinition asm)
        {
            AssemblyHashInfo info = new AssemblyHashInfo();
            info.Name = asm.FullName;

            var mainModule = asm.MainModule;
            if (mainModule.HasTypes)
            {
                List<TypeHashInfo> types = new List<TypeHashInfo>();
                foreach (var i in mainModule.Types)
                {
                    AddTypeInfo(i, types);
                }
                info.Types = types.ToArray();
            }

            return info;
        }

        static void AddTypeInfo(TypeDefinition type, List<TypeHashInfo> types)
        {
            types.Add(TypeHashInfo.BuildHashInfo(type));
            foreach (var t in type.NestedTypes)
            {
                AddTypeInfo(t, types);
            }
        }
    }

    public struct TypeHashInfo
    {
        public TypeDefinition Definition { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public HashTuple[] Fields { get; set; }

        public MethodHashInfo[] Methods { get; set; }

        public static TypeHashInfo BuildHashInfo(TypeDefinition type)
        {
            MD5 md5 = MD5.Create();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            TypeHashInfo info = new TypeHashInfo();
            info.Name = type.FullName;
            info.Hash = type.ComputeFieldsHash(bw, md5);
            info.Fields = new HashTuple[type.Fields.Count];
            for(int i = 0; i < info.Fields.Length; i++)
            {
                var f = type.Fields[i];
                info.Fields[i] = new HashTuple()
                {
                    Definition = f,
                    Name = f.Name,
                    Hash = f.ComputeFieldHash(bw, md5)
                };
            }
            info.Methods = new MethodHashInfo[type.Methods.Count];
            for(int i = 0; i < type.Methods.Count; i++)
            {
                var m = type.Methods[i];
                info.Methods[i] = new MethodHashInfo()
                {
                    Definition = m,
                    FullName = m.FullName,
                    Hash = m.ComputeHash(bw, md5)
                };
            }
            return info;
        }
    }

    public struct HashTuple
    {
        public MemberReference Definition { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
    }

    public struct MethodHashInfo
    {
        public MethodDefinition Definition { get; set; }
        public string FullName { get; set; }
        public string Hash { get; set; }
    }
}
