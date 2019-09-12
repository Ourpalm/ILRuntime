using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class ExpTest_20
    {
        public static void UnitTest_2001()
        {
            TestSwitch(1);
            TestSwitch(2);
            TestSwitch(253);            
        }

        static void TestSwitch(int value)
        {
            switch (value)
            {
                case 0:
                    Console.WriteLine("case 0");
                    break;
                case 1:
                    Console.WriteLine("case 1");
                    break;
                case 43:
                    Console.WriteLine("case XX");
                    break;
                case 2:
                    Console.WriteLine("case 2");
                    break;
                case 7:
                    Console.WriteLine("case XX");
                    break;
                case 8:
                    Console.WriteLine("case XX");
                    break;
                case 12:
                    Console.WriteLine("case XX");
                    break;
                
                default:
                    Console.WriteLine("case default");
                    break;
            }
        }

        public static int UnitTest_2002()
        {
            bool cond = true;
            int cnt = 0;
            while (cond)
            {
                cnt++;
                if (cnt > 100)
                    cond = false;
            }

            return cnt;
        }

        public static int UnitTest_2003()
        {
            bool cond = true;
            int cnt = 0;
            do
            {
                cnt++;
                if (cnt > 100)
                    cond = false;
            }
            while (cond);
            return cnt;
        }

        public static void UnitTest_2004()
        {
            Console.WriteLine("UnitTest_2004");
            bool[] bb = new bool[10];
            ArrayTest1(bb);
            Console.WriteLine(bb[1]);
        }

        static void ArrayTest1(bool[] ab)
        {
            ab[1] = true;
        }

        public static void UnitTest_2005()
        {
            string str = "12345";
            char c = str[1];
            Console.WriteLine(c);
        }
    }
}
