using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.Other;

namespace ILRuntimeTest.TestFramework
{
    [NeedAdaptor]
    public class TestClass3
    {

    }

    [NeedAdaptor]
    public class TestClass4
    {
        protected int a;
        protected int b;

        public virtual void KKK()
        {
            a = 1;
            b = 2;
        }
    }

    public struct TestStruct
    {
        public int value;
        public static void DoTest(ref TestStruct a)
        {
            a.value = 11111;
        }

        public static void DoTest(ref int a)
        {
            a = 22222;
        }
    }
}
