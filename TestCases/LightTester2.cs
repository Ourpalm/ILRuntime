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

        public static void UnitTest_SwitchInline()
        {
            if (TestSwitchInline(1) != 5)
                throw new Exception();
            if (TestSwitchInline(2) != 9)
                throw new Exception();
            if (TestSwitchInline(253) != 100)
                throw new Exception();
        }

        static int TestSwitchInline(int value)
        {
            switch (value)
            {
                case 1:
                    return 5;
                case 2:
                    return 9;
                default:
                    return 100;
            }
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
        public static bool static_field = true;
        public static bool static_Property => static_field;
        public static int UnitTest_2006()
        {
            int a = 2;
            int n = 0;
            try
            {
                if (static_Property) //属性
                    n += a;
                return n;
            }
            catch (Exception)
            {
                n = -1;
            }
            return n;
        }
        public static void UnitTest_TestInline01()
        {
            object obj = new object();
            UnitTest_TestInline01Sub(obj);
            if (obj == null)
                throw new Exception();
        }

        static void UnitTest_TestInline01Sub(object obj)
        {
            Console.WriteLine(obj);
            obj = null;
            if (obj != null)
                throw new Exception();
        }

        public static void UnitTest_TestFCP()
        {
            var color = ToColor("#FF00FF00");
            if (Math.Abs(color.x - 1) > 0.001f || Math.Abs(color.y - 1) > 0.001f || Math.Abs(color.z - 0) > 0.001f)
                throw new Exception();
        }

        static ILRuntimeTest.TestFramework.TestVector3NoBinding ToColor(string str)
        {
            string strNew = str.StartsWith("#") ? str.Remove(0, 1) : str;
            string str1 = strNew.Substring(0, 2);
            string str2 = strNew.Substring(2, 2);
            string str3 = strNew.Substring(4, 2);
            float num1 = (float)Convert.ToInt64(str1, 16) / 255;
            float num2 = (float)Convert.ToInt64(str2, 16) / 255;
            float num3 = (float)Convert.ToInt64(str3, 16) / 255;
            if (strNew.Length > 7)
            {
                string str4 = strNew.Substring(6, 2);
                float num4 = (float)Convert.ToInt64(str4, 16) / 255;
                ILRuntimeTest.TestFramework.TestVector3NoBinding color1 = new ILRuntimeTest.TestFramework.TestVector3NoBinding(num1, num3, num4);
                return color1;
            }
            ILRuntimeTest.TestFramework.TestVector3NoBinding color = new ILRuntimeTest.TestFramework.TestVector3NoBinding(num1, num2, num3);
            return color;
        }
        static Dictionary<int, int>[] CachePosSnDic = { null, null, null };
        static Dictionary<int, bool> tabHasItemDic = new Dictionary<int, bool>();
        public static void UnitTest_TestFCP2()
        {
            var array = UnitTest_TestFCP2Sub();
            int count = array.Count;
            for(int i=0;i< count; ++i)
            {
                for(int j = 0; j < CachePosSnDic.Length; j++)
                {
                    if (CachePosSnDic[j] == null)
                        CachePosSnDic[j] = new Dictionary<int, int>();
                    CachePosSnDic[j].Add(array[i].TestVal2, 0);
                }
            }

            var tabarray = UnitTest_TestFCP2Sub2();
            int tabCount = tabarray.Count;
            for(int i = 0; i < tabCount; ++i)
            {
                tabHasItemDic.Add(tabarray[i].TestVal2, false);
            }
        }

        class TestCls
        {
            public int TestVal2;
        }
        static List<TestCls> UnitTest_TestFCP2Sub()
        {
            var res = new List<TestCls>();
            res.Add(new TestCls()
            {
                TestVal2 = 1
            });
            res.Add(new TestCls()
            {
                TestVal2 = 2
            });
            res.Add(new TestCls()
            {
                TestVal2 = 3
            });
            res.Add(new TestCls()
            {
                TestVal2 = 4
            });
            res.Add(new TestCls()
            {
                TestVal2 = 5
            });
            return res;
        }

        static List<TestCls> UnitTest_TestFCP2Sub2()
        {
            return new List<TestCls>();
        }

        public static void UnitTest_TestFCP3()
        {
            for(int i = 0; i < 100; i++)
            {
                try
                {
                    throw new Exception("test");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{ex.Message}\r\n{ex.Data["StackTrace"]}");
                }
            }
        }
    }
}
