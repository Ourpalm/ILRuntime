using ILRuntime.Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ILRuntime.Hybrid
{
    static class HashingUtility
    {
        public static string ComputeFieldsHash(this TypeDefinition type, BinaryWriter bw, HashAlgorithm hasher)
        {
            MemoryStream ms = bw.BaseStream as MemoryStream;
            ms.Position = 0;
            ms.SetLength(0);
            if(type.HasFields)
            {
                foreach (var i in type.Fields)
                {
                    WriteFieldInfo(i, bw, hasher);
                }
            }
            return Convert.ToBase64String(hasher.ComputeHash(ms.GetBuffer(), 0, (int)ms.Position));
        }

        static void WriteFieldInfo(FieldDefinition field, BinaryWriter bw, HashAlgorithm hasher)
        {
            bw.Write(field.IsStatic);
            bw.Write(field.FieldType.FullName);
            bw.Write(field.Name);
        }

        static void WriteFieldInfo(FieldReference field, BinaryWriter bw, HashAlgorithm hasher)
        {
            var fd = field.Resolve();
            if (fd != null)
                bw.Write(fd.IsStatic);
            else
                bw.Write(false);
            bw.Write(field.FieldType.FullName);
            bw.Write(field.Name);
        }

        public static string ComputeFieldHash(this FieldReference field, BinaryWriter bw, HashAlgorithm hasher)
        {
            MemoryStream ms = bw.BaseStream as MemoryStream;
            ms.Position = 0;

            WriteFieldInfo(field, bw, hasher);
            return Convert.ToBase64String(hasher.ComputeHash(ms.GetBuffer(), 0, (int)ms.Position));
        }

        public static string ComputeHash(this MethodDefinition m, BinaryWriter bw, HashAlgorithm hasher, bool includeBody)
        {
            MemoryStream ms = bw.BaseStream as MemoryStream;
            ms.Position = 0;
            bw.Write(m.IsStatic);
            WriteOperand(m, bw);
            if (!m.IsStatic)
                WriteOperand(m.DeclaringType, bw);
            foreach (var i in m.Parameters)
            {
                WriteOperand(i.ParameterType, bw);
            }
            if (includeBody)
            {
                var body = m.Body;
                if (body != null)
                {
                    foreach (var i in body.Variables)
                    {
                        WriteOperand(i.VariableType, bw);
                    }
                    foreach (var ins in body.Instructions)
                    {
                        WriteOperand(ins, bw);
                    }
                }
            }
            return Convert.ToBase64String(hasher.ComputeHash(ms.GetBuffer(), 0, (int)bw.BaseStream.Position));
        }

        static void WriteOperand(object operand, BinaryWriter bw)
        {
            if (operand != null)
            {
                if (operand is int)
                {
                    bw.Write((int)operand);
                }
                else if (operand is uint)
                {
                    bw.Write((uint)operand);
                }
                else if (operand is long)
                {
                    bw.Write((long)operand);
                }
                else if (operand is ulong)
                {
                    bw.Write((ulong)operand);
                }
                else if (operand is short)
                {
                    bw.Write((short)operand);
                }
                else if (operand is ushort)
                {
                    bw.Write((ushort)operand);
                }
                else if (operand is byte)
                {
                    bw.Write((byte)operand);
                }
                else if (operand is sbyte)
                {
                    bw.Write((sbyte)operand);
                }
                else if (operand is char)
                {
                    bw.Write((char)operand);
                }
                else if (operand is bool)
                {
                    bw.Write((bool)operand);
                }
                else if (operand is float)
                {
                    bw.Write((float)operand);
                }
                else if (operand is double)
                {
                    bw.Write((double)operand);
                }
                else if (operand is string)
                {
                    bw.Write((string)operand);
                }
                else if (operand is MethodReference)
                {
                    MethodReference mr = (MethodReference)operand;
                    bw.Write(mr.FullName);
                }
                else if (operand is TypeReference)
                {
                    TypeReference tr = (TypeReference)operand;
                    bw.Write(tr.IsByReference);
                    bw.Write(tr.IsByReference ? tr.GetElementType().FullName : tr.FullName);
                }
                else if (operand is Mono.Cecil.Cil.Instruction)
                {
                    Mono.Cecil.Cil.Instruction ins = (Mono.Cecil.Cil.Instruction)operand;
                    bw.Write((short)ins.OpCode.Code);
                    bw.Write(ins.OpCode.Value);
                    WriteOperand(ins.Operand, bw);
                }
                else if (operand is ParameterDefinition)
                {
                    bw.Write(((ParameterDefinition)operand).Index);
                }
                else if (operand is FieldReference)
                {
                    FieldReference fr = (FieldReference)operand;
                    WriteOperand(fr.DeclaringType, bw);
                    bw.Write(fr.Name);
                }
                else if (operand is Mono.Cecil.Cil.VariableReference)
                {
                    Mono.Cecil.Cil.VariableReference vr = (Mono.Cecil.Cil.VariableReference)operand;
                    bw.Write(vr.Index);
                }
                else if (operand is Mono.Cecil.Cil.Instruction[])
                {
                    foreach (var i in (Mono.Cecil.Cil.Instruction[])operand)
                    {
                        WriteOperand(i, bw);
                    }
                }
                else
                    throw new NotImplementedException();
            }
        }
    }
}
