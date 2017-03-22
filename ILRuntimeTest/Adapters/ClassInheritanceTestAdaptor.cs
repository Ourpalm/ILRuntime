using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntimeTest.TestFramework
{   
    public class ClassInheritanceTestAdaptor : CrossBindingAdaptor
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

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

		internal class Adaptor : ClassInheritanceTest, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adaptor()
            {

            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            
            IMethod mTestAbstract;
            public override void TestAbstract()
            {
                if(mTestAbstract == null)
                {
                    mTestAbstract = instance.Type.GetMethod("TestAbstract", 0);
                }
                if (mTestAbstract != null)
                    appdomain.Invoke(mTestAbstract, instance );
            }

            IMethod mTestVirtual;
            bool isTestVirtualInvoking = false;

            public override void TestVirtual()
            {
                if (mTestVirtual == null)
                {
                    mTestVirtual = instance.Type.GetMethod("TestVirtual", 0);
                }
                if (mTestVirtual != null && !isTestVirtualInvoking)
                {
                    isTestVirtualInvoking = true;
                    appdomain.Invoke(mTestVirtual, instance );
                    isTestVirtualInvoking = false;
                }
                else
                    base.TestVirtual();
            }

            
            
            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }

    public class ClassInheritanceTest2Adaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(ClassInheritanceTest2<Adaptor>);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        internal class Adaptor : ClassInheritanceTest2<Adaptor>, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adaptor()
            {

            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            IMethod mTestVirtual;
            bool isTestVirtualInvoking = false;

            public override void TestVirtual()
            {
                if (mTestVirtual == null)
                {
                    mTestVirtual = instance.Type.GetMethod("TestVirtual", 0);
                }
                if (mTestVirtual != null && !isTestVirtualInvoking)
                {
                    isTestVirtualInvoking = true;
                    appdomain.Invoke(mTestVirtual, instance);
                    isTestVirtualInvoking = false;
                }
                else
                    base.TestVirtual();
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }

    public class ClassInheritanceTest2Adaptor2 : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(ClassInheritanceTest2<int>);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        internal class Adaptor : ClassInheritanceTest2<int>, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adaptor()
            {

            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            IMethod mTestVirtual;
            bool isTestVirtualInvoking = false;

            public override void TestVirtual()
            {
                if (mTestVirtual == null)
                {
                    mTestVirtual = instance.Type.GetMethod("TestVirtual", 0);
                }
                if (mTestVirtual != null && !isTestVirtualInvoking)
                {
                    isTestVirtualInvoking = true;
                    appdomain.Invoke(mTestVirtual, instance);
                    isTestVirtualInvoking = false;
                }
                else
                    base.TestVirtual();
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }


    public class InterfaceTestAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(InterfaceTest);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(ClassInheritanceTestAdaptor.Adaptor);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new ClassInheritanceTestAdaptor.Adaptor(appdomain, instance);
        }
    }
}