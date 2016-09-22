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
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest(123);
        }

        static void IntTest(int a)
        {
            Console.WriteLine("dele a=" + a);
        }

        static void IntTest2(int a)
        {
            Console.WriteLine("dele a=" + a);
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
                Console.WriteLine("dele a=" + (a + b));
            }

            public void IntTest2(int a)
            {
                Console.WriteLine("dele a=" + (a + b));
            }
        }
    }
}
