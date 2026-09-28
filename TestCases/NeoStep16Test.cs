using System;

namespace TestCases
{
    public class NeoStep16Reference { public int Value; }
    public struct NeoStep16Value { public int Number; public string Text; }
    public enum NeoStep16Enum { None, Ready }
    public enum NeoStep16ShortEnum : short { Negative = -1234, Positive = 2345 }

    public static class NeoStep16Test
    {
        public static void NeoStep16PrimitiveArray()
        {
            int[] values = new int[3];
            values[0] = 42;
            values[1] = values[0] + 1;
            if (values.Length != 3 || values[1] != 43) throw new Exception("int array failed");
            float[] floats = new float[1];
            floats[0] = 1.5f;
            if (floats[0] != 1.5f) throw new Exception("float array failed");
            byte[] bytes = new byte[1];
            bytes[0] = 255;
            if (bytes[0] != 255) throw new Exception("byte array failed");
        }

        public static void NeoStep16ReferenceArray()
        {
            var values = new NeoStep16Reference[2];
            values[0] = new NeoStep16Reference();
            values[0].Value = 19;
            if (values[0].Value != 19 || values[1] != null) throw new Exception("IL reference array failed");
            string[] strings = new string[1];
            strings[0] = "ok";
            if (strings[0] != "ok") throw new Exception("CLR reference array failed");
        }

        public static void NeoStep16ValueArray()
        {
            var values = new NeoStep16Value[2];
            var item = new NeoStep16Value { Number = 7, Text = "first" };
            values[0] = item;
            item.Number = 8;
            if (values[0].Number != 7 || values[0].Text != "first")
                throw new Exception("IL struct array copy failed");
        }

        public static void NeoStep16Bounds()
        {
            int[] values = new int[1];
            try { int ignored = values[1]; throw new Exception("load did not fail"); }
            catch (IndexOutOfRangeException) { }
            try { values[1] = 1; throw new Exception("store did not fail"); }
            catch (IndexOutOfRangeException) { }
        }

        public static void NeoStep16Covariance()
        {
            object[] values = new string[1];
            try { values[0] = new object(); throw new Exception("covariant store did not fail"); }
            catch (ArrayTypeMismatchException) { }
        }

        public static void NeoStep16ArrayReference()
        {
            int[] numbers = new int[1];
            ref int number = ref numbers[0];
            number = 71;
            if (number != 71 || numbers[0] != 71)
                throw new Exception("int array reference failed");
            string[] strings = new string[1];
            ref string text = ref strings[0];
            text = "changed";
            if (text != "changed" || strings[0] != "changed")
                throw new Exception("reference array reference failed");
        }

        public static void NeoStep16NullArray()
        {
            int[] values = null;
            try { int ignored = values[0]; throw new Exception("null load did not fail"); }
            catch (NullReferenceException) { }
            try { values[0] = 1; throw new Exception("null store did not fail"); }
            catch (NullReferenceException) { }
        }

        public static void NeoStep16ArrayReferenceChecks()
        {
            int[] values = new int[1];
            try { ref int ignored = ref values[1]; throw new Exception("ref bounds did not fail"); }
            catch (IndexOutOfRangeException) { }
            object[] covariant = new string[1];
            try { ref object ignored = ref covariant[0]; throw new Exception("ref covariance did not fail"); }
            catch (ArrayTypeMismatchException) { }
        }

        public static void NeoStep16ClrValueArrayField()
        {
            var values = new ValueTuple<int, int>[1];
            var initial = new ValueTuple<int, int>();
            initial.Item2 = 9;
            values[0] = initial;
            values[0].Item1 = 23;
            var copy = values[0];
            if (copy.Item1 != 23 || copy.Item2 != 9)
                throw new Exception("CLR struct array copy or field failed");
        }

        public static void NeoStep16JaggedAndILCovariance()
        {
            int[][] rows = new int[1][];
            rows[0] = new int[1];
            rows[0][0] = 37;
            if (rows[0][0] != 37) throw new Exception("jagged array failed");

            object[] objects = new NeoStep16Reference[1];
            try { objects[0] = "wrong"; throw new Exception("IL covariant store did not fail"); }
            catch (ArrayTypeMismatchException) { }
        }

        public static void NeoStep16EnumArrays()
        {
            DayOfWeek[] clr = new DayOfWeek[1];
            clr[0] = DayOfWeek.Monday;
            if (clr[0] != DayOfWeek.Monday) throw new Exception("CLR enum array failed");
            NeoStep16Enum[] il = new NeoStep16Enum[1];
            il[0] = NeoStep16Enum.Ready;
            if (il[0] != NeoStep16Enum.Ready) throw new Exception("IL enum array failed");
        }

        public static void NeoStep16UnsignedArrayReference()
        {
            ulong[] values = new ulong[1];
            ref ulong cell = ref values[0];
            cell = ulong.MaxValue;
            if (cell != ulong.MaxValue || values[0] != ulong.MaxValue)
                throw new Exception("unsigned array reference lost high bit");
        }

        public static void NeoStep16AllPrimitiveArrayStorage()
        {
            bool[] booleans = { true };
            char[] characters = { '\ufffe' };
            sbyte[] signedBytes = { -101 };
            byte[] bytes = { 240 };
            short[] signedShorts = { -30000 };
            ushort[] shorts = { 60000 };
            int[] signedInts = { -123456789 };
            uint[] ints = { 4000000000U };
            long[] signedLongs = { -1234567890123L };
            ulong[] longs = { ulong.MaxValue };
            float[] singles = { 2.5f };
            double[] doubles = { 3.75 };

            if (!booleans[0] || characters[0] != '\ufffe' || signedBytes[0] != -101 ||
                bytes[0] != 240 || signedShorts[0] != -30000 || shorts[0] != 60000 ||
                signedInts[0] != -123456789 || ints[0] != 4000000000U ||
                signedLongs[0] != -1234567890123L || longs[0] != ulong.MaxValue ||
                singles[0] != 2.5f || doubles[0] != 3.75)
                throw new Exception("primitive array load failed");

            booleans[0] = false;
            characters[0] = 'z';
            signedBytes[0] = -1;
            bytes[0] = 255;
            signedShorts[0] = -1;
            shorts[0] = 65535;
            signedInts[0] = -1;
            ints[0] = uint.MaxValue;
            signedLongs[0] = -1;
            longs[0] = ulong.MaxValue - 1;
            singles[0] = -1.5f;
            doubles[0] = -2.25;
            if (booleans[0] || characters[0] != 'z' || signedBytes[0] != -1 ||
                bytes[0] != 255 || signedShorts[0] != -1 || shorts[0] != 65535 ||
                signedInts[0] != -1 || ints[0] != uint.MaxValue || signedLongs[0] != -1 ||
                longs[0] != ulong.MaxValue - 1 || singles[0] != -1.5f ||
                doubles[0] != -2.25)
                throw new Exception("primitive array store failed");
        }

        public static void NeoStep16TypedArrayReferences()
        {
            byte[] bytes = new byte[1];
            ref byte b = ref bytes[0];
            b = 201;
            if (b != 201 || bytes[0] != 201) throw new Exception("byte ref failed");

            char[] chars = new char[1];
            ref char c = ref chars[0];
            c = '\ufffe';
            if (c != '\ufffe' || chars[0] != '\ufffe') throw new Exception("char ref failed");

            ushort[] shorts = new ushort[1];
            ref ushort s = ref shorts[0];
            s = 60000;
            if (s != 60000 || shorts[0] != 60000) throw new Exception("ushort ref failed");

            uint[] ints = new uint[1];
            ref uint i = ref ints[0];
            i = uint.MaxValue;
            if (i != uint.MaxValue || ints[0] != uint.MaxValue)
                throw new Exception("uint ref failed");

            double[] doubles = new double[1];
            ref double d = ref doubles[0];
            d = 9.25;
            if (d != 9.25 || doubles[0] != 9.25)
                throw new Exception("double ref failed");
        }

        public static void NeoStep16ShortEnumArrayFallback()
        {
            NeoStep16ShortEnum[] values = new NeoStep16ShortEnum[2];
            values[0] = NeoStep16ShortEnum.Negative;
            values[1] = NeoStep16ShortEnum.Positive;
            if (values[0] != NeoStep16ShortEnum.Negative ||
                values[1] != NeoStep16ShortEnum.Positive)
                throw new Exception("short enum array fallback failed");
        }
    }
}
