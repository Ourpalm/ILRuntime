using System;

namespace ILRuntime.CLR.TypeSystem
{
    internal static class MemoryLayoutHelpers
    {
        public static int AlignUp(int offset, int alignment)
        {
            return (offset + alignment - 1) & ~(alignment - 1);
        }

        public static int GetPrimitiveSizeFromClrType(Type t)
        {
            if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte)) return 1;
            if (t == typeof(short) || t == typeof(ushort) || t == typeof(char)) return 2;
            if (t == typeof(int) || t == typeof(uint) || t == typeof(float)) return 4;
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
            if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte)) return 1;
            if (t == typeof(short) || t == typeof(ushort) || t == typeof(char)) return 2;
            if (t == typeof(int) || t == typeof(uint) || t == typeof(float)) return 4;
            if (t == typeof(long) || t == typeof(ulong) || t == typeof(double) || t == typeof(IntPtr) || t == typeof(UIntPtr)) return 8;
            if (t.IsEnum)
            {
                var ut = t.GetEnumUnderlyingType();
                return GetPrimitiveAlignmentFromClrType(ut);
            }
            return 4;
        }
    }
}