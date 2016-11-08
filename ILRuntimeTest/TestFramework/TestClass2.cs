using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using ILRuntime.Other;

namespace ILRuntimeTest.TestFramework
{
    [NeedAdaptor]
    abstract class TestClass2
    {
        public void Method1()
        {

        }

        public virtual void VMethod1()
        {

        }

        protected virtual bool VMethod2()
        {
            return false;
        }

        protected abstract void AbMethod1();

        public abstract float AbMethod2(int arg1);

    }


}
