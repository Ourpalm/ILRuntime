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
        public static Action<BaseClassTest> GenericDelegateTest;
        public static Func<int, float, short, double> DelegatePerformanceTest;
        public static Action<TestCLREnum> EnumDelegateTest;
        public static Func<TestCLREnum> EnumDelegateTest2;

        public static void TestEnumDelegate()
        {
            EnumDelegateTest(TestCLREnum.Test1);
            EnumDelegateTest(TestCLREnum.Test2);
            EnumDelegateTest(TestCLREnum.Test3);
        }

        public static void TestEnumDelegate2()
        {
            var e = EnumDelegateTest2();
            switch (e)
            {
                case TestCLREnum.Test2:
                    Console.WriteLine("Test2");
                    break;
                default:
                    throw new Exception("Shouldn't be here");
            }
        }
    }

    public class GenericClassTest<T> : BaseClassTest
    {

    }
    public class BaseClassTest
    {
        public bool testField;

        public static void DoTest()
        {
            GenericClassTest<int> a = new GenericClassTest<int>();
            if (DelegateTest.GenericDelegateTest != null)
                DelegateTest.GenericDelegateTest(a);
        }
    }
}
