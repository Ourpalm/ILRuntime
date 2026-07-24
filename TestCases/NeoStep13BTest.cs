using System;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public class NeoStep13BTest
    {
        public static void NeoStep13BInlineNestedNaturalAlignmentAndTail()
        {
            NeoStep13BClrFieldLayout value = default(NeoStep13BClrFieldLayout);
            value.Primitive.Value = 0x1122334455667788L;
            value.WithReference.Prefix = 17;
            value.WithReference.Reference = "inline-reference";
            value.WithReference.Suffix = 0x2233445566778899L;
            value.Tail = 0x13579BDF;

            AssertEqualLong("NeoStep13BInlineNestedNaturalAlignmentAndTail primitive",
                0x1122334455667788L, value.Primitive.Value);
            AssertEqual("NeoStep13BInlineNestedNaturalAlignmentAndTail prefix",
                17, value.WithReference.Prefix);
            AssertEqual("NeoStep13BInlineNestedNaturalAlignmentAndTail reference",
                "inline-reference", value.WithReference.Reference);
            AssertEqualLong("NeoStep13BInlineNestedNaturalAlignmentAndTail suffix",
                0x2233445566778899L, value.WithReference.Suffix);
            AssertEqual("NeoStep13BInlineNestedNaturalAlignmentAndTail tail",
                0x13579BDF, value.Tail);
        }

        public static void NeoStep13BInlineReferenceNullAndSlotIsolation()
        {
            NeoStep13BInlineWithReference value = default(NeoStep13BInlineWithReference);
            value.Prefix = 0x10203040;
            value.Reference = "first";
            value.Suffix = 0x0102030405060708L;

            AssertEqual("NeoStep13BInlineReferenceNullAndSlotIsolation non-null prefix",
                0x10203040, value.Prefix);
            AssertEqual("NeoStep13BInlineReferenceNullAndSlotIsolation non-null reference",
                "first", value.Reference);
            AssertEqualLong("NeoStep13BInlineReferenceNullAndSlotIsolation non-null suffix",
                0x0102030405060708L, value.Suffix);

            value.Reference = null;
            AssertEqual("NeoStep13BInlineReferenceNullAndSlotIsolation null reference",
                null, value.Reference);
            AssertEqual("NeoStep13BInlineReferenceNullAndSlotIsolation null prefix",
                0x10203040, value.Prefix);
            AssertEqualLong("NeoStep13BInlineReferenceNullAndSlotIsolation null suffix",
                0x0102030405060708L, value.Suffix);

            value.Reference = "second";
            AssertEqual("NeoStep13BInlineReferenceNullAndSlotIsolation restored reference",
                "second", value.Reference);
            AssertEqual("NeoStep13BInlineReferenceNullAndSlotIsolation restored prefix",
                0x10203040, value.Prefix);
            AssertEqualLong("NeoStep13BInlineReferenceNullAndSlotIsolation restored suffix",
                0x0102030405060708L, value.Suffix);
        }

        public static void NeoStep13BBoxedNestedIndexRefSlot()
        {
            NeoStep13BClrBoxedFieldLayout value = default(NeoStep13BClrBoxedFieldLayout);
            TestVector3NoBinding boxed = default(TestVector3NoBinding);
            boxed.x = 11.5f;
            boxed.y = 22.5f;
            boxed.z = 33.5f;
            value.Boxed = boxed;
            value.Tail = 707;

            AssertEqual("NeoStep13BBoxedNestedIndexRefSlot x", 11.5f, value.Boxed.x);
            AssertEqual("NeoStep13BBoxedNestedIndexRefSlot y", 22.5f, value.Boxed.y);
            AssertEqual("NeoStep13BBoxedNestedIndexRefSlot z", 33.5f, value.Boxed.z);
            AssertEqual("NeoStep13BBoxedNestedIndexRefSlot tail", 707, value.Tail);
        }

        public static void NeoStep13BBoxUnboxMoveVtRoundTrip()
        {
            NeoStep13BClrFieldLayout source = default(NeoStep13BClrFieldLayout);
            source.Primitive.Value = 0x0102030405060708L;
            source.WithReference.Prefix = 31;
            source.WithReference.Reference = "round-trip";
            source.WithReference.Suffix = 0x1112131415161718L;
            source.Tail = 909;

            NeoStep13BClrFieldLayout moved = source;
            object boxed = moved;
            NeoStep13BClrFieldLayout unboxed = (NeoStep13BClrFieldLayout)boxed;

            AssertEqualLong("NeoStep13BBoxUnboxMoveVtRoundTrip primitive",
                0x0102030405060708L, unboxed.Primitive.Value);
            AssertEqual("NeoStep13BBoxUnboxMoveVtRoundTrip prefix", 31, unboxed.WithReference.Prefix);
            AssertEqual("NeoStep13BBoxUnboxMoveVtRoundTrip reference", "round-trip", unboxed.WithReference.Reference);
            AssertEqualLong("NeoStep13BBoxUnboxMoveVtRoundTrip suffix",
                0x1112131415161718L, unboxed.WithReference.Suffix);
            AssertEqual("NeoStep13BBoxUnboxMoveVtRoundTrip tail", 909, unboxed.Tail);

            source.Tail = -1;
            source.WithReference.Reference = "source-only";
            AssertEqual("NeoStep13BBoxUnboxMoveVtRoundTrip moved isolation", 909, moved.Tail);
            AssertEqual("NeoStep13BBoxUnboxMoveVtRoundTrip unboxed isolation",
                "round-trip", unboxed.WithReference.Reference);
        }

        public static void NeoStep13BStaticInstanceSymmetry()
        {
            NeoStep13BInlinePrimitive instancePrimitive = default(NeoStep13BInlinePrimitive);
            NeoStep13BInlineWithReference instanceReference = default(NeoStep13BInlineWithReference);
            instancePrimitive.Value = 0x1122334455667788L;
            instanceReference.Prefix = 51;
            instanceReference.Reference = "instance-reference";
            instanceReference.Suffix = 0x4142434445464748L;

            NeoStep13BInlinePrimitive staticPrimitive = default(NeoStep13BInlinePrimitive);
            NeoStep13BInlineWithReference staticReference = default(NeoStep13BInlineWithReference);
            staticPrimitive.Value = 0x5566778899AABBCCL;
            staticReference.Prefix = 41;
            staticReference.Reference = "static-reference";
            staticReference.Suffix = 0x3132333435363738L;

            NeoStep13BStaticHolder.Primitive = staticPrimitive;
            NeoStep13BStaticHolder.WithReference = staticReference;

            NeoStep13BInlinePrimitive staticPrimitiveRead = NeoStep13BStaticHolder.Primitive;
            NeoStep13BInlineWithReference staticReferenceRead = NeoStep13BStaticHolder.WithReference;

            AssertEqualLong("NeoStep13BStaticInstanceSymmetry static primitive",
                0x5566778899AABBCCL, staticPrimitiveRead.Value);
            AssertEqual("NeoStep13BStaticInstanceSymmetry static prefix",
                41, staticReferenceRead.Prefix);
            AssertEqual("NeoStep13BStaticInstanceSymmetry static reference",
                "static-reference", staticReferenceRead.Reference);
            AssertEqualLong("NeoStep13BStaticInstanceSymmetry static suffix",
                0x3132333435363738L, staticReferenceRead.Suffix);

            AssertEqualLong("NeoStep13BStaticInstanceSymmetry instance primitive",
                0x1122334455667788L, instancePrimitive.Value);
            AssertEqual("NeoStep13BStaticInstanceSymmetry instance prefix",
                51, instanceReference.Prefix);
            AssertEqual("NeoStep13BStaticInstanceSymmetry instance reference",
                "instance-reference", instanceReference.Reference);
            AssertEqualLong("NeoStep13BStaticInstanceSymmetry instance suffix",
                0x4142434445464748L, instanceReference.Suffix);
        }

        private static void AssertEqual(string scenario, int expected, int actual)
        {
            if (actual != expected)
            {
                throw new Exception(scenario);
            }
        }

        private static void AssertEqual(string scenario, float expected, float actual)
        {
            if (actual != expected)
            {
                throw new Exception(scenario);
            }
        }

        private static void AssertEqual(string scenario, string expected, string actual)
        {
            if (actual != expected)
            {
                throw new Exception(scenario);
            }
        }

        private static void AssertEqualLong(string scenario, long expected, long actual)
        {
            if (actual != expected)
            {
                throw new Exception(scenario);
            }
        }
    }
}
