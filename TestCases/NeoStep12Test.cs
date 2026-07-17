using System;

namespace TestCases
{
    // 3 primitive fields, no refs. Exercises basic struct field layout and
    // Ldfld_*/Stfld_* against a stack-slot value type (Operand4=1 path).
    public struct NeoStep12Vec3
    {
        public float X;
        public float Y;
        public float Z;
    }

    // Mixed alignment: byte(1) + long(8) + int(4). Natural alignment should
    // pad byte→8, place long at 8, int at 16; total ceil to 8 = 24.
    public struct NeoStep12Aligned
    {
        public byte A;
        public long B;
        public int C;
    }

    // Nested struct: outer contains inner Vec3 + int tail.
    public struct NeoStep12Nested
    {
        public NeoStep12Vec3 Inner;
        public int Tail;
    }

    // Struct with a managed reference field (string). Primitives layout still
    // needs a 4-byte slot for the ref index, plus 1 ref-count in ManagedObjects.
    public struct NeoStep12WithRef
    {
        public int Id;
        public string Name;
        public float Val;
    }

    // Boxed/small primitive fields exercise sub-int (byte, bool, char) writes.
    public struct NeoStep12BoolByte
    {
        public bool Flag;
        public byte Small;
        public char Ch;
        public int Tail;
    }

    // Struct-this method target: instance method returning a computed field
    // combination, tests struct-this ABI (Task 12).
    public struct NeoStep12ThisTarget
    {
        public int A;
        public int B;

        public int Sum()
        {
            return A + B;
        }

        public void Bump(int by)
        {
            A += by;
            B += by;
        }
    }

    // Static cctor: verifies Task 13 (cctor restore).
    public static class NeoStep12StaticHolder
    {
        public static int X;

        static NeoStep12StaticHolder()
        {
            X = 42;
        }
    }

    public class NeoStep12Test
    {
        public static void NeoStep12BasicFieldAccess()
        {
            NeoStep12Vec3 v;
            v.X = 1.5f;
            v.Y = 2.5f;
            v.Z = 3.5f;
            AssertEqual("NeoStep12BasicFieldAccess X", 1.5f, v.X);
            AssertEqual("NeoStep12BasicFieldAccess Y", 2.5f, v.Y);
            AssertEqual("NeoStep12BasicFieldAccess Z", 3.5f, v.Z);
        }

        public static void NeoStep12Alignment()
        {
            NeoStep12Aligned a;
            a.A = 7;
            a.B = 0x1122334455667788L;
            a.C = -12345;
            AssertEqual("NeoStep12Alignment A", 7, a.A);
            AssertEqual("NeoStep12Alignment B", 0x1122334455667788L, a.B);
            AssertEqual("NeoStep12Alignment C", -12345, a.C);
        }

        public static void NeoStep12NestedStruct()
        {
            NeoStep12Nested n;
            n.Inner.X = 10.0f;
            n.Inner.Y = 20.0f;
            n.Inner.Z = 30.0f;
            n.Tail = 42;
            AssertEqual("NeoStep12NestedStruct Inner.X", 10.0f, n.Inner.X);
            AssertEqual("NeoStep12NestedStruct Inner.Y", 20.0f, n.Inner.Y);
            AssertEqual("NeoStep12NestedStruct Inner.Z", 30.0f, n.Inner.Z);
            AssertEqual("NeoStep12NestedStruct Tail", 42, n.Tail);
        }

        public static void NeoStep12StructWithRef()
        {
            NeoStep12WithRef r;
            r.Id = 99;
            r.Name = "hello";
            r.Val = 1.25f;
            AssertEqual("NeoStep12StructWithRef Id", 99, r.Id);
            AssertEqual("NeoStep12StructWithRef Name", "hello", r.Name);
            AssertEqual("NeoStep12StructWithRef Val", 1.25f, r.Val);
        }

        public static void NeoStep12StructAssignment()
        {
            NeoStep12Vec3 src;
            src.X = 1.0f;
            src.Y = 2.0f;
            src.Z = 3.0f;

            NeoStep12Vec3 dst = src;
            AssertEqual("NeoStep12StructAssignment dst.X", 1.0f, dst.X);
            AssertEqual("NeoStep12StructAssignment dst.Y", 2.0f, dst.Y);
            AssertEqual("NeoStep12StructAssignment dst.Z", 3.0f, dst.Z);

            // Mutate src; dst must be untouched (value semantics).
            src.X = 100.0f;
            AssertEqual("NeoStep12StructAssignment isolation", 1.0f, dst.X);
        }

        public static void NeoStep12StructMethodCall()
        {
            NeoStep12ThisTarget t;
            t.A = 10;
            t.B = 32;
            int sum = t.Sum();
            AssertEqual("NeoStep12StructMethodCall sum", 42, sum);

            t.Bump(5);
            AssertEqual("NeoStep12StructMethodCall bumped A", 15, t.A);
            AssertEqual("NeoStep12StructMethodCall bumped B", 37, t.B);
        }

        public static void NeoStep12StructParam()
        {
            NeoStep12Vec3 v;
            v.X = 4.0f;
            v.Y = 5.0f;
            v.Z = 6.0f;
            float dot = ComputeDot(v, v);
            AssertEqual("NeoStep12StructParam dot", 4.0f * 4.0f + 5.0f * 5.0f + 6.0f * 6.0f, dot);
        }

        private static float ComputeDot(NeoStep12Vec3 a, NeoStep12Vec3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static void NeoStep12StaticCctor()
        {
            AssertEqual("NeoStep12StaticCctor X", 42, NeoStep12StaticHolder.X);
        }

        public static void NeoStep12InitobjDefault()
        {
            NeoStep12WithRef r = default(NeoStep12WithRef);
            AssertEqual("NeoStep12InitobjDefault Id", 0, r.Id);
            AssertEqualObj("NeoStep12InitobjDefault Name", null, r.Name);
            AssertEqual("NeoStep12InitobjDefault Val", 0.0f, r.Val);
        }

        public static void NeoStep12BoolByteField()
        {
            NeoStep12BoolByte b;
            b.Flag = true;
            b.Small = 0xAB;
            b.Ch = 'Z';
            b.Tail = 999;
            AssertEqual("NeoStep12BoolByteField Flag", true, b.Flag);
            AssertEqual("NeoStep12BoolByteField Small", (int)0xAB, (int)b.Small);
            AssertEqual("NeoStep12BoolByteField Ch", (int)'Z', (int)b.Ch);
            AssertEqual("NeoStep12BoolByteField Tail", 999, b.Tail);

            // Verify sub-int writes don't spill into neighbors.
            b.Flag = false;
            AssertEqual("NeoStep12BoolByteField Small preserved", (int)0xAB, (int)b.Small);
            AssertEqual("NeoStep12BoolByteField Tail preserved", 999, b.Tail);
        }

        private static void AssertEqual(string scenario, int expected, int actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }

        private static void AssertEqual(string scenario, long expected, long actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }

        private static void AssertEqual(string scenario, float expected, float actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }

        private static void AssertEqual(string scenario, bool expected, bool actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }

        private static void AssertEqual(string scenario, string expected, string actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }

        private static void AssertEqualObj(string scenario, object expected, object actual)
        {
            if (!object.Equals(expected, actual))
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }
    }
}
