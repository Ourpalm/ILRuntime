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

        public static string Test01()
        {
            TestEnum a = TestEnum.Enum4;
            Console.WriteLine("a=" + a);

            return a.ToString();
        }
    }
}
