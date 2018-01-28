using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class EnumTest
    {
        public enum TestEnum : long
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

        public static string Test06()
        {
            System.IO.FileMode a = System.IO.FileMode.Create;

            Console.WriteLine("a=" + a);
            return a.ToString();
        }

        public static string Test07()
        {
            if (File.Exists("test.txt"))
            {
                File.Delete("test.txt");
            }

            using (System.IO.FileStream fs = new System.IO.FileStream("test.txt", System.IO.FileMode.Create))
            {
                fs.WriteByte(100);
            }

            using (System.IO.FileStream fs = new System.IO.FileStream("test.txt", System.IO.FileMode.Open))
            {
                return fs.ReadByte().ToString();
            }
        }

        public static void Test08()
        {
            object o = TestEnum.Enum4;

            Console.WriteLine((TestEnum)o == TestEnum.Enum4);
        }

        public static void Test09()
        {
            Dictionary<TestEnum, int> dic = new Dictionary<TestEnum, int>();
            dic[TestEnum.Enum2] = 123;

            int res;
            if (dic.TryGetValue(TestEnum.Enum2, out res))
            {
                Console.WriteLine(res);
            }
        }

        public static void Test10()
        {
            object e = TestEnum3.Enum2bbb;
            byte b = (byte)e; //InvalidCastException
            Console.WriteLine(b);
        }
    }
}
