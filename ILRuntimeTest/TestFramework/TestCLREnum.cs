namespace ILRuntimeTest.TestFramework
{
    public enum TestCLREnum
    {
        Test1 = 0,
        Test2 = 1,
        Test3 = 2
    }

    public class TestCLREnumClass
    {
        public static TestCLREnum Test = TestCLREnum.Test2;
        public static TestCLREnum Test2 => TestCLREnum.Test3;
    }
}