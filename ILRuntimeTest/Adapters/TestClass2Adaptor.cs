using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntimeTest.TestFramework
{   
    public class TestClass2Adaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(TestClass2);
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

        internal class Adaptor : TestClass2, CrossBindingAdaptorType
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

            
            IMethod mVMethod1;
            bool isVMethod1Invoking = false;

            public override void VMethod1()
            {
                if (mVMethod1 == null)
                {
                    mVMethod1 = instance.Type.GetMethod("VMethod1", 0);
                }
                if (mVMethod1 != null && !isVMethod1Invoking)
                {
                    isVMethod1Invoking = true;
                    appdomain.Invoke(mVMethod1, instance );
                    isVMethod1Invoking = false;
                }
                else
                    base.VMethod1();
            }

            IMethod mVMethod2;
            bool isVMethod2Invoking = false;

            protected override Boolean VMethod2()
            {
                if (mVMethod2 == null)
                {
                    mVMethod2 = instance.Type.GetMethod("VMethod2", 0);
                }

                if (mVMethod2 != null && !isVMethod2Invoking)
                {
                    isVMethod2Invoking = true;
                    var res = (Boolean)appdomain.Invoke(mVMethod2, instance );
                    isVMethod2Invoking = false;
                    return res;
                }
                else
                    return (Boolean)base.VMethod2();
            }

            IMethod mAbMethod1;
            protected override void AbMethod1()
            {
                if(mAbMethod1 == null)
                {
                    mAbMethod1 = instance.Type.GetMethod("AbMethod1", 0);
                }
                if (mAbMethod1 != null)
                    appdomain.Invoke(mAbMethod1, instance );
            }

            IMethod mAbMethod2;
            public override Single AbMethod2(Int32 arg1)
            {
                if(mAbMethod2 == null)
                {
                    mAbMethod2 = instance.Type.GetMethod("AbMethod2", 1);
                }

                if (mAbMethod2 != null)
                {
                    using(var ctx = appdomain.BeginInvoke(mAbMethod2))
                    {
                        ctx.PushObject(instance);
                        ctx.PushInteger(arg1);
                        ctx.Invoke();
                        return ctx.ReadFloat();
                    }                    
                }
                
                return 0;
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

    public class IDisposableClassInheritanceAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(IDisposable);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(IDisposableAdaptor);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new IDisposableAdaptor(appdomain, instance);
        }

        public class IDisposableAdaptor : IDisposable, CrossBindingAdaptorType
        {
            private ILTypeInstance instance;
            private ILRuntime.Runtime.Enviorment.AppDomain appDomain;

            private IMethod iDisposable;
            private readonly object[] param0 = new object[0];

            public IDisposableAdaptor()
            {
            }

            public IDisposableAdaptor(ILRuntime.Runtime.Enviorment.AppDomain appDomain, ILTypeInstance instance)
            {
                this.appDomain = appDomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance
            {
                get
                {
                    return instance;
                }
            }

            public void Dispose()
            {
                if (this.iDisposable == null)
                {
                    this.iDisposable = instance.Type.GetMethod("Dispose");
                }
                this.appDomain.Invoke(this.iDisposable, instance, this.param0);
            }

            public override string ToString()
            {
                IMethod m = this.appDomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }

                return instance.Type.FullName;
            }


        }
    }

}