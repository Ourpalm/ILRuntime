using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class DelegateTest
    {
        public static void DelegateTest01()
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += IntTest;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += IntTest2;

            DelegateTestCls cls = new TestCases.DelegateTest.DelegateTestCls(1000);
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += cls.IntTest;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += cls.IntTest2;

            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest(123);
        }

        public static void DelegateTest02()
        {
            Action<int> a = null;
            a += IntTest;
            a += IntTest2;

            DelegateTestCls cls = new TestCases.DelegateTest.DelegateTestCls(1000);
            a += cls.IntTest;
            a += cls.IntTest2;
            a(123);
        }

        static void IntTest(int a)
        {
            Console.WriteLine("dele a=" + a);
        }

        static void IntTest2(int a)
        {
            Console.WriteLine("dele2 a=" + a);
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
        }
    }
}
