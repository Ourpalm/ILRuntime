using System;

namespace TestCases
{
    public class TestCLREnum
    {
        public static string Test00()
        {
            var str = ILRuntimeTest.TestFramework.TestCLREnum.Test1.ToString();
            Console.WriteLine("Test10 : " + str);
            if (str == "Test1")
            {
                return str;
            }
            throw new Exception("Test Fail");
        }

        public static string Test01()
        {
            var str = ILRuntimeTest.TestFramework.TestCLREnum.Test1 + 1;
            Console.WriteLine(str);
            if (str.ToString() == "Test2")
            {
                return str.ToString();
            }
            throw new Exception("Test Fail");
        }
    }
}