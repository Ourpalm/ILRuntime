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

        class DelegateTestCls
        {
            int b;
            public DelegateTestCls(int b)
            {
                this.b = b;
            }
            public void IntTest(int a)
            {
                Console.WriteLine("dele3 a=" + (a + b));
            }

            public void IntTest2(int a)
            {
                Console.WriteLine("dele4 a=" + (a + b));
            }
            public int IntTest3(int a)
            {
                Console.WriteLine("dele5 a=" + a);
                return a + 200;
            }
        }
    }
}
