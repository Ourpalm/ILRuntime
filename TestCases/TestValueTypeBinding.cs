using System;

namespace TestCases
{
    public class TestValueTypeBinding
    {
        public static void Test00()
        {
            var a = ILRuntimeTest.TestFramework.TestVector3.One;
            a.X += 100;

            Console.WriteLine(a.ToString());
        }
    }
}