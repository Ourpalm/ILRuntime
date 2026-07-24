namespace ILRuntimeTest.TestFramework
{
    // Pure data CLR value type: it must be classified as Inline without a binder.
    public struct NeoStep13BInlinePrimitive
    {
        public long Value;
        public static long StaticValue;
    }

    // Public fields and no instance methods keep this CLR value type Inline while
    // still exercising the primitive/reference split.
    public struct NeoStep13BInlineWithReference
    {
        public int Prefix;
        public string Reference;
        public long Suffix;

        public static int StaticPrefix;
        public static string StaticReference;
        public static long StaticSuffix;
    }
}
