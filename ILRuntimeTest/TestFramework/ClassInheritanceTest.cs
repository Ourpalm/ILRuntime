using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntimeTest.TestFramework
{
    public class ClassInheritanceAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(ClassInheritanceTest);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);
            }
        }

        public override object CreateCLRInstance(ILTypeInstance instance)
        {
            return new Adaptor();
        }

        class Adaptor : ClassInheritanceTest
        {
            public override void TestAbstract()
            {
                throw new NotImplementedException();
            }

            public override void TestVirtual()
            {
                base.TestVirtual();
            }
        }
    }

    public abstract class ClassInheritanceTest
    {
        protected int testVal = 100;
        public int TestVal2 = 200;

        public abstract void TestAbstract();

        public virtual void TestVirtual()
        {
            Console.WriteLine("This is from Base Class");
        }

        public void TestField()
        {
            Console.WriteLine("testVal = " + testVal);
        }
    }
}
