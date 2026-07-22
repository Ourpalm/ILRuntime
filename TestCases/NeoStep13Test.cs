using System;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    // Step 13 tests exercise the CLR value-type Box / Unbox / Initobj / field-access
    // paths and the StructStorage classification (Inline vs Boxed). Test types are
    // taken from ILRuntimeTest.TestFramework — TestVector3 has a ValueTypeBinder
    // registered (Inline), TestStructA is a bare long field (Inline).

    public class NeoStep13Test
    {
        // Initobj on an Inline CLR struct with binder: 12 bytes zero-cleared in-frame.
        public static void NeoStep13InitobjTestVector3()
        {
            TestVector3 v = default(TestVector3);
            AssertEqual("NeoStep13InitobjTestVector3 X", 0.0f, v.X);
            AssertEqual("NeoStep13InitobjTestVector3 Y", 0.0f, v.Y);
            AssertEqual("NeoStep13InitobjTestVector3 Z", 0.0f, v.Z);
        }

        // Initobj on an Inline pure-data CLR struct.
        public static void NeoStep13InitobjTestStructA()
        {
            TestStructA a = default(TestStructA);
            AssertEqualLong("NeoStep13InitobjTestStructA value", 0L, a.value);
        }

        // Field access on an Inline pure-data CLR struct: read/write single primitive field.
        public static void NeoStep13FieldAccessTestStructA()
        {
            TestStructA a;
            a.value = 0x1122334455667788L;
            AssertEqualLong("NeoStep13FieldAccessTestStructA", 0x1122334455667788L, a.value);
        }

        // Field access on an Inline binder-registered CLR struct: read/write primitive fields
        // through Ldfld_R4 with Operand4 = 1 inline direct against frame bytes.
        public static void NeoStep13FieldAccessTestVector3()
        {
            TestVector3 v;
            v.X = 1.5f;
            v.Y = 2.5f;
            v.Z = 3.5f;
            AssertEqual("NeoStep13FieldAccessTestVector3 X", 1.5f, v.X);
            AssertEqual("NeoStep13FieldAccessTestVector3 Y", 2.5f, v.Y);
            AssertEqual("NeoStep13FieldAccessTestVector3 Z", 3.5f, v.Z);
        }

        // Box / Unbox a TestStructA (Inline pure-data). Round-trip via `object` should
        // preserve the value using the framework-uniform reflection Box/Unbox path.
        public static void NeoStep13BoxUnboxTestStructA()
        {
            TestStructA a;
            a.value = 0x1122334455667788L;
            object boxed = a;
            TestStructA back = (TestStructA)boxed;
            AssertEqualLong("NeoStep13BoxUnboxTestStructA", 0x1122334455667788L, back.value);
        }

        // Nested CLR Inline struct: TestVectorStruct wraps TestVectorStruct2 (which wraps
        // TestVectorStruct3) + TestVector3. Exercises recursive Inline field-offset
        // computation across CLR types and inline-direct Ldfld/Stfld traversal.
        public static void NeoStep13NestedInlineStruct()
        {
            TestVectorStruct s = default(TestVectorStruct);
            s.A = 100;
            s.B.A.A = 1.0f;
            s.B.A.B = 2.0f;
            AssertEqual("NeoStep13NestedInlineStruct A", 100, s.A);
            AssertEqual("NeoStep13NestedInlineStruct B.A.A", 1.0f, s.B.A.A);
            AssertEqual("NeoStep13NestedInlineStruct B.A.B", 2.0f, s.B.A.B);
        }

        // Boxed CLR struct field write/read via Ldloca + Stfld + Ldfld. TestVector3NoBinding
        // overrides ToString() so HasInstanceMethods() == true → StructStorage.Boxed: its slot
        // is a 4-byte mStack index + ref slot, NOT flat bytes; the struct value lives as a boxed
        // object in mStack. Exercises Ldloca producing a heap-style receiver into the boxed
        // instance and Stfld/Ldfld routing field writes/reads through reflection.
        public static void NeoStep13BoxedStructFieldAccess()
        {
            TestVector3NoBinding v = default(TestVector3NoBinding);
            v.x = 11.5f;
            v.y = 22.5f;
            v.z = 33.5f;
            AssertEqual("NeoStep13BoxedStructFieldAccess x", 11.5f, v.x);
            AssertEqual("NeoStep13BoxedStructFieldAccess y", 22.5f, v.y);
            AssertEqual("NeoStep13BoxedStructFieldAccess z", 33.5f, v.z);
        }

        // Instance method call on a CLR Inline value type. ECMA-335 III.3.19 requires `this`
        // to be a managed pointer (T&), so the caller emits `ldloca` producing an 8-byte Ref
        // Slot while the callee param must be classified as byref by the Neo Call ABI.
        // Regression guard: without the byref lift in AllocateNeoCallParamSlot's HasThis
        // branch, the `srcInfo.IsRef != dstInfo.IsRef` check throws InvalidProgramException
        // at JIT prewarm and blocks any struct instance-method invocation.
        //
        // Known failing today (kept intentionally): the JIT-prewarm regression is fixed
        // (Normalize's method body is now reached), but the assertions below currently fail
        // because Normalize's field writes through `this` (a CLR value-type byref receiver)
        // do not propagate back to the caller's frame. Whichever later step owns
        // "CLR value-type instance method mutation write-back through Ref Slot receiver"
        // must re-run this test and turn it green — do NOT weaken the assertions.
        public static void NeoStep13ValueTypeInstanceMethodThis()
        {
            TestVector3 v = default(TestVector3);
            v.X = 3f;
            v.Normalize();
            AssertEqual("NeoStep13ValueTypeInstanceMethodThis X", 1.0f, v.X);
            AssertEqual("NeoStep13ValueTypeInstanceMethodThis Y", 0.0f, v.Y);
            AssertEqual("NeoStep13ValueTypeInstanceMethodThis Z", 0.0f, v.Z);
        }

        // ---------------------------------------------------------------
        // Assertion helpers
        // ---------------------------------------------------------------

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

        private static void AssertEqualLong(string scenario, long expected, long actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }
    }
}
