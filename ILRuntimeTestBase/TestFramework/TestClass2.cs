using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using ILRuntime.Other;

namespace ILRuntimeTest.TestFramework
{
    [NeedAdaptor]
    public abstract class TestClass2
    {
        static List<TestClass2> objs = new List<TestClass2>();
        public static void Register(TestClass2 obj)
        {
            objs.Add(obj);
        }

        public static TestClass2 Alloc()
        {
            return objs[0];
        }
        public static int Add(int a, int b)
        {
            if (b < 0)
                return a;
            else
                return a + b;
        }
        public void Method1()
        {

        }

        public virtual void VMethod1()
        {

        }

        public virtual bool VMethod2()
        {
            return false;
        }

        public virtual void VMethod3(ref int arg)
        {
            arg++;
        }
        protected abstract void AbMethod1();

        public abstract float AbMethod2(int arg1);

    }


}
