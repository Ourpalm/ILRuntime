using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Other;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntimeTest.TestFramework
{
    //public class ClassInheritanceAdaptor : CrossBindingAdaptor
    //{
    //    public override Type BaseCLRType
    //    {
    //        get
    //        {
    //            return typeof(ClassInheritanceTest);
    //        }
    //    }

    //    public override Type AdaptorType
    //    {
    //        get
    //        {
    //            return typeof(Adaptor);
    //        }
    //    }

    //    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    //    {
    //        return new Adaptor(appdomain, instance);
    //    }

    //    internal class Adaptor : ClassInheritanceTest, CrossBindingAdaptorType
    //    {
    //        ILTypeInstance instance;
    //        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
    //        IMethod mTestAbstract;
    //        IMethod mTestVirtual;
    //        bool isTestVirtualInvoking = false;

    //        public Adaptor()
    //        {

    //        }

    //        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    //        {
    //            this.appdomain = appdomain;
    //            this.instance = instance;
    //        }

    //        public ILTypeInstance ILInstance { get { return instance; } }

    //        public override void TestAbstract()
    //        {
    //            if(mTestAbstract == null)
    //            {
    //                mTestAbstract = instance.Type.GetMethod("TestAbstract", 0);
    //            }
    //            if (mTestAbstract != null)
    //                appdomain.Invoke(mTestAbstract, instance); 
    //        }

    //        public override void TestVirtual()
    //        {
    //            if (mTestVirtual == null)
    //            {
    //                mTestVirtual = instance.Type.GetMethod("TestVirtual", 0);
    //            }
    //            if (mTestVirtual != null && !isTestVirtualInvoking)
    //            {
    //                isTestVirtualInvoking = true;
    //                appdomain.Invoke(mTestVirtual, instance);
    //                isTestVirtualInvoking = false;
    //            }
    //            else
    //                base.TestVirtual();
    //        }

    //        public override string ToString()
    //        {
    //            IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
    //            m = instance.Type.GetVirtualMethod(m);
    //            if (m == null || m is ILMethod)
    //            {
    //                return instance.ToString();
    //            }
    //            else
    //                return instance.Type.FullName;
    //        }
    //    }
    //}
    //public class InterfaceTestAdaptor : CrossBindingAdaptor
    //{
    //    public override Type BaseCLRType
    //    {
    //        get
    //        {
    //            return typeof(InterfaceTest);
    //        }
    //    }

    //    public override Type AdaptorType
    //    {
    //        get
    //        {
    //            return typeof(ClassInheritanceAdaptor.Adaptor);
    //        }
    //    }

    //    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    //    {
    //        return new ClassInheritanceAdaptor.Adaptor(appdomain, instance);
    //    }        
    //}
    public abstract class ClassInheritanceTest2<T>
    {
        public virtual void TestVirtual()
        {
            Console.WriteLine("This is from Base Class");
        }
    }
    [NeedAdaptor]
    public abstract class ClassInheritanceTest : InterfaceTest
    {
        protected int testVal = 100;
        public int TestVal2 = 200;
        public static IDisposable staticField;

        public ClassInheritanceTest()
        {

        }

        public ClassInheritanceTest(int a, int b)
        {
            testVal = 444;
            TestVal2 = 555;
        }

        public abstract void TestAbstract();

        public virtual void TestVirtual()
        {
            Console.WriteLine("This is from Base Class");
        }

        public void TestField()
        {
            Console.WriteLine("testVal = " + testVal);
            Console.WriteLine("testVal2 = " + TestVal2);
        }

        public static void Test3(InterfaceTest ins)
        {
            Console.WriteLine("Testing invoking instance from CLR...");
            ins.TestAbstract();
            ins.TestVirtual();
            ((ClassInheritanceTest)ins).testVal = 233;
            ins.TestField();
        }

        public static void TestLongRef(ref long i)
        {
            Console.WriteLine(string.Format("TestLongRef:{0}", i));
        }
    }

    [NeedAdaptor]
    public interface InterfaceTest
    {
        void TestVirtual();
        void TestAbstract();
        void TestField();
    }
}
