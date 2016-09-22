using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntimeTest.TestFramework
{
    public delegate void IntDelegate(int a);
    public class DelegateTest
    {
        public static IntDelegate IntDelegateTest;
    }
}
