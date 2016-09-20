using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class EnumTest
    {
        enum TestEnum : long
        {
            Enum1,
            Enum2,
            Enum4 = 0x123456789,
        }

        enum TestEnum2 : ulong
        {
            Enum1,
            Enum2,
            Enum3234 = 0x123456789,
        }

        enum TestEnum3 : byte
        {
            Enum1bbb,
            Enum2bbb,
        }
        static TestEnum b = TestEnum.Enum2;

        public static string Test01()
        {
            TestEnum a = TestEnum.Enum4;
            Console.WriteLine("a=" + a);

            return a.ToString();
        }

        public static string Test02()
        {
            TestEnum a = (TestEnum)1;
            Console.WriteLine("a=" + a);
            return a.ToString();
        }

        public static bool Test03()
        {
            switch (b)
            {
                case TestEnum.Enum1:
                    return false;
                case TestEnum.Enum2:
                    return true;
                default:
                    return false;
            }
        }

        public static string Test04()
        {
            return Test04Sub(TestEnum.Enum4);
        }

        static string Test04Sub(TestEnum a)
        {
            return a.ToString();
        }

        public static void Test05()
        {
            TestEnum a = TestEnum.Enum4;
            TestEnum2 b = (TestEnum2)a;
            Console.WriteLine("b=" + b);
            TestEnum3 c = (TestEnum3)EnumTest.b;
            Console.WriteLine("c=" + c);
        }
    }
}
