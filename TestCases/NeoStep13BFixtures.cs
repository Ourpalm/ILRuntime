using ILRuntimeTest.TestFramework;

namespace TestCases
{
    // IL value type fixture for Task 1. The fields are intentionally declarations
    // only; access and layout assertions belong to the later Step 13B tasks.
    public struct NeoStep13BClrFieldLayout
    {
        public NeoStep13BInlinePrimitive Primitive;
        public NeoStep13BInlineWithReference WithReference;
        public int Tail;
    }

    // Boxed CLR value type nested in an IL value type. The CLR struct has
    // instance methods, so it must remain represented by an object index and
    // a managed reference slot rather than being flattened into frame bytes.
    public struct NeoStep13BClrBoxedFieldLayout
    {
        public TestVector3NoBinding Boxed;
        public int Tail;
    }

    // IL holder used to exercise ILType staticFieldOffsets for CLR Inline fields.
    public class NeoStep13BStaticHolder
    {
        public static NeoStep13BInlinePrimitive Primitive;
        public static NeoStep13BInlineWithReference WithReference;
    }
}
