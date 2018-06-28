using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class DelegateTest
    {
        static TestDelegate testDele;

        public static void DelegateTest01()
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += IntTest;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += IntTest2;

            DelegateTestCls cls = new DelegateTestCls(1000);
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += cls.IntTest;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += cls.IntTest2;

            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest(123);
        }

        public static void DelegateTest02()
        {
            Action<int> a = null;
            a += IntTest;
            a += IntTest2;

            DelegateTestCls cls = new DelegateTestCls(1000);
            a += cls.IntTest;
            a += cls.IntTest2;
            a += (i) =>
            {
                Console.WriteLine("lambda a=" + i);
            };
            a(123);
        }

        public static int DelegateTest03()
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2 += IntTest3;

            DelegateTestCls cls = new DelegateTestCls(1000);
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2 += cls.IntTest3;

            int val = ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2(123);
            return val;
        }

        delegate int TestDelegate(int b);


        public static int DelegateTest04()
        {
            Func<int, int> a = null;
            a += IntTest3;

            DelegateTestCls cls = new DelegateTestCls(1000);
            a += cls.IntTest3;
            a += (i) =>
            {
                Console.WriteLine("lambda a=" + i);
                return i + 300;
            };
            int val = a(123);

            return val;
        }

        public static int DelegateTest05()
        {
            testDele += IntTest3;

            DelegateTestCls cls = new DelegateTestCls(1000);
            testDele += cls.IntTest3;

            int val = testDele(123);
            return val;
        }

        public static void DelegateTest06()
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest = null;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += IntTest;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest -= IntTest;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += IntTest2;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest(1000);
        }

        public static void DelegateTest07()
        {
            Test07Sub(IntTest);
            Test07Sub2(IntTest);
        }

        public static void DelegateTest08()
        {
            testDele = null;
            testDele += IntTest3;
            testDele -= IntTest3;
            testDele += IntTest3;
            Console.WriteLine(testDele(1000));
        }

        public static void DelegateTest09()
        {
            Test09Sub(IntTest3);
            Test09Sub2(IntTest3);
        }
        public static void DelegateTest10()
        {
            Action<int, string, string> a = (b, c, d) =>
            {
                Action act = () =>
                {
                    Console.WriteLine(string.Format("{0},{1},{2}", b, c, d));
                };
                act();
            };

            a(1, "2", "3");
        }

        public static void DelegateTest11()
        {
            ILRuntimeTest.TestFramework.DelegateTest.GenericDelegateTest = CallBack;
            ILRuntimeTest.TestFramework.BaseClassTest.DoTest();
        }

        public static void DelegateTest12()
        {
            testDele += IntTest3;
            Dictionary<int, TestDelegate> dic = new Dictionary<int, TestDelegate>();
            dic[0] = testDele;
            dic[1] = IntTest3;
            Console.WriteLine(dic[0](1000));
            Console.WriteLine(dic[1](1000));

        }

        public static void DelegateTest13()
        {
            Action<int> a = null;
            a += IntTest;
            a += IntTest2;

            a -= IntTest;
            a -= IntTest2;

            Console.WriteLine(a == null);
        }

        public static void DelegateTest14()
        {
            Action<string> a = null;
            a += TestString;
            a += TestString2;
            a += TestString3;
            a += TestString4;

            a("ffff");
        }

        public static void DelegateTest15()
        {
            DelegateTestCls cls = new DelegateTestCls(123);
            DelegateTest15Sub(cls.IntTest);
        }

        static void DelegateTest15Sub(Action<int> test)
        {
            test(555);
        }

        public static void DelegateTest16()
        {
            System.Action a = () => { };
            System.Action<int> a_2 = (i) => { };
            int a3 = 2;
            Console.WriteLine(typeof(System.Action) == a.GetType());  //false, should be true
            Console.WriteLine(typeof(int) == a3.GetType());  //false, should be true
        }

        public static void DelegateTest17()
        {
            Action a = () =>{ };
            Console.WriteLine(a.GetHashCode());
        }

        public static void DelegateTest18()
        {
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest = DelegateTest18Sub;
            ILRuntimeTest.TestFramework.DelegateTest.TestEnumDelegate();
        }

        static void DelegateTest18Sub(ILRuntimeTest.TestFramework.TestCLREnum e)
        {
            switch (e)
            {
                case ILRuntimeTest.TestFramework.TestCLREnum.Test1:
                    Console.WriteLine("Test1");
                    break;
                case ILRuntimeTest.TestFramework.TestCLREnum.Test2:
                    Console.WriteLine("Test2");
                    break;
                case ILRuntimeTest.TestFramework.TestCLREnum.Test3:
                    Console.WriteLine("Test3");
                    break;
                default:
                    throw new Exception("Should not be here");
            }
        }
        public static void DelegateTest19()
        {
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest2 = DelegateTest19Sub;
            ILRuntimeTest.TestFramework.DelegateTest.TestEnumDelegate2();
        }

        static ILRuntimeTest.TestFramework.TestCLREnum DelegateTest19Sub()
        {
            return ILRuntimeTest.TestFramework.TestCLREnum.Test2;
        }

        static void TestString(string a)
        {
            Console.WriteLine("test1:" + a);
        }

        static void TestString2(string a)
        {
            Console.WriteLine("test2:" + a);
        }

        static void TestString3(string a)
        {
            Console.WriteLine("test3:" + a);
        }

        static void TestString4(string a)
        {
            Console.WriteLine("test4:" + a);
        }

        static void CallBack(ILRuntimeTest.TestFramework.BaseClassTest a)
        {
            a.testField = true;
            Console.WriteLine(a.testField);
        }

        static void Test07Sub(ILRuntimeTest.TestFramework.IntDelegate action)
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest = action;
        }

        static void Test07Sub2(ILRuntimeTest.TestFramework.IntDelegate action)
        {
            Console.WriteLine("Test Result=" + (ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest == action));
        }

        static void Test09Sub(TestDelegate action)
        {
            testDele = action;
        }

        static void Test09Sub2(TestDelegate action)
        {
            Console.WriteLine("Test Result=" + (testDele == action));
        }
        static void IntTest(int a)
        {
            Console.WriteLine("dele a=" + a);
        }

        static void IntTest2(int a)
        {
            Console.WriteLine("dele2 a=" + a);
        }

        static int IntTest3(int a)
        {
            Console.WriteLine("dele3 a=" + a);
            return a + 100;
        }

        class DelegateTestCls : DelegateTestClsBase
        {
            public DelegateTestCls(int b)
            {
                this.b = b;
            }
            public override void IntTest(int a)
            {
                base.IntTest(a);
                Console.WriteLine("dele3 a=" + (a + b));
            }


            public int IntTest3(int a)
            {
                Console.WriteLine("dele5 a=" + a);
                return a + 200;
            }
        }

        class DelegateTestClsBase
        {
            protected int b;
            public virtual void IntTest(int a)
            {
                Console.WriteLine("dele3base a=" + (a + b));
            }
            public virtual void IntTest2(int a)
            {
                Console.WriteLine("dele4 a=" + (a + b));
            }
        }
    }
}
