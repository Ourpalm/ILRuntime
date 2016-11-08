using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.Other;

namespace ILRuntimeTest.TestFramework
{
    [DelegateExport]
    public delegate void IntDelegate(int a);

    [DelegateExport]
    public delegate void Int2Delegate(int a, int b);

    [DelegateExport]
    public delegate void InitFloat(int a, float b);

    [DelegateExport]
    public delegate int IntDelegate2(int a);

    [DelegateExport]
    public delegate bool Int2Delegate2(int a, int b);

    [DelegateExport]
    public delegate string IntFloatDelegate2(int a, float b);

    public class DelegateTest
    {
        public static IntDelegate IntDelegateTest;
        public static Int2Delegate IntDelegateTest1;
        public static IntDelegate2 IntDelegateTest2;
        public static IntDelegate2 IntDelegateTestReturn;
        public static Int2Delegate2 IntDelegateTestReturn1;
        public static IntFloatDelegate2 IntDelegateTestReturn2;
    }
}
