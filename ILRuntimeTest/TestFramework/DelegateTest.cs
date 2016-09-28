using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntimeTest.TestFramework
{
    public delegate void IntDelegate(int a);
    public delegate int IntDelegate2(int a);
    public class DelegateTest
    {
        public static IntDelegate IntDelegateTest;
        public static IntDelegate2 IntDelegateTest2;
    }
}
