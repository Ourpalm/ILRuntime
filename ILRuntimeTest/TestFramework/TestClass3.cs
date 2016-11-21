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
}
