using System;
using System.Runtime.CompilerServices;

namespace ILRuntime.CLR.TypeSystem
{
    internal sealed class RawObjectPayload
    {
        public byte Data;
    }

    internal static unsafe class MemoryLayoutHelpers
    {
        public static int AlignUp(int offset, int alignment)
        {
            return (offset + alignment - 1) & ~(alignment - 1);
        }

        public static int GetPrimitiveSizeFromClrType(Type t)
        {
            if (t == typeof(int) || t == typeof(uint) || t == typeof(float)) return 4;
            if (t == typeof(short) || t == typeof(ushort) || t == typeof(char)) return 2;
            if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte)) return 1;
            if (t == typeof(long) || t == typeof(ulong) || t == typeof(double) || t == typeof(IntPtr) || t == typeof(UIntPtr)) return 8;
            if (t.IsEnum)
            {
                var ut = t.GetEnumUnderlyingType();
                return GetPrimitiveSizeFromClrType(ut);
            }
            return 4;
        }

        public static int GetPrimitiveAlignmentFromClrType(Type t)
        {
            if (t == typeof(int) || t == typeof(uint) || t == typeof(float)) return 4;
            if (t == typeof(short) || t == typeof(ushort) || t == typeof(char)) return 2;
            if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte)) return 1;
            if (t == typeof(long) || t == typeof(ulong) || t == typeof(double) || t == typeof(IntPtr) || t == typeof(UIntPtr)) return 8;
            if (t.IsEnum)
            {
                var ut = t.GetEnumUnderlyingType();
                return GetPrimitiveAlignmentFromClrType(ut);
            }
            return 4;
        }

        public static void CopyPrimitiveToNeoFrame(ref byte src, byte* dst, Type fieldType)
        {
            if (fieldType == typeof(int)) { *(int*)dst = Unsafe.ReadUnaligned<int>(ref src); return; }
            if (fieldType.IsEnum)
            {
                CopyPrimitiveToNeoFrame(ref src, dst, fieldType.GetEnumUnderlyingType());
                return;
            }
            if (fieldType == typeof(float)) { *(float*)dst = Unsafe.ReadUnaligned<float>(ref src); return; }
            if (fieldType == typeof(long)) { *(long*)dst = Unsafe.ReadUnaligned<long>(ref src); return; }
            if (fieldType == typeof(double)) { *(double*)dst = Unsafe.ReadUnaligned<double>(ref src); return; }
            if (fieldType == typeof(bool)) { *(int*)dst = Unsafe.ReadUnaligned<byte>(ref src) != 0 ? 1 : 0; return; }
            if (fieldType == typeof(byte)) { *(int*)dst = Unsafe.ReadUnaligned<byte>(ref src); return; }
            if (fieldType == typeof(short)) { *(int*)dst = Unsafe.ReadUnaligned<short>(ref src); return; }
            if (fieldType == typeof(uint)) { *(uint*)dst = Unsafe.ReadUnaligned<uint>(ref src); return; }
            if (fieldType == typeof(ushort)) { *(int*)dst = Unsafe.ReadUnaligned<ushort>(ref src); return; }
            if (fieldType == typeof(char)) { *(int*)dst = Unsafe.ReadUnaligned<ushort>(ref src); return; }
            if (fieldType == typeof(sbyte)) { *(int*)dst = Unsafe.ReadUnaligned<sbyte>(ref src); return; }
            if (fieldType == typeof(ulong)) { *(ulong*)dst = Unsafe.ReadUnaligned<ulong>(ref src); return; }
            if (fieldType == typeof(IntPtr)) { *(long*)dst = Unsafe.ReadUnaligned<long>(ref src); return; }
            if (fieldType == typeof(UIntPtr)) { *(ulong*)dst = Unsafe.ReadUnaligned<ulong>(ref src); return; }
            throw new NotSupportedException($"Neo primitive read: unsupported type {fieldType.FullName}");
        }

        public static void CopyPrimitiveFromNeoFrame(byte* src, ref byte dst, Type fieldType)
        {
            if (fieldType == typeof(int) || fieldType == typeof(float) || fieldType == typeof(uint))
            {
                Unsafe.CopyBlock(ref dst, ref *src, 4);
                return;
            }
            if (fieldType.IsEnum)
            {
                CopyPrimitiveFromNeoFrame(src, ref dst, fieldType.GetEnumUnderlyingType());
                return;
            }
            if (fieldType == typeof(long) || fieldType == typeof(double) ||
                fieldType == typeof(ulong) || fieldType == typeof(IntPtr) ||
                fieldType == typeof(UIntPtr))
            {
                Unsafe.CopyBlock(ref dst, ref *src, 8);
                return;
            }
            if (fieldType == typeof(bool) || fieldType == typeof(byte) || fieldType == typeof(sbyte))
            {
                Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<int>(ref *src));
                return;
            }
            if (fieldType == typeof(short) || fieldType == typeof(ushort) || fieldType == typeof(char))
            {
                Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<short>(ref *src));
                return;
            }
            throw new NotSupportedException($"Neo primitive write: unsupported type {fieldType.FullName}");
        }
    }
}
