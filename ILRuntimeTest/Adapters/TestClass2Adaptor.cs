using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntimeTest.TestFramework
{   
    public class TestClass2Adaptor : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mVMethod1 = new CrossBindingMethodInfo("VMethod1");
        static CrossBindingFunctionInfo<bool> mVMethod2 = new CrossBindingFunctionInfo<bool>("VMethod2");
        static CrossBindingMethodInfo mAMethod1 = new CrossBindingMethodInfo("AbMethod1");
        static CrossBindingFunctionInfo<int, float> mAMethod2 = new CrossBindingFunctionInfo<int, float>("AbMethod2");
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

            
            public override void VMethod1()
            {
                if (mVMethod1.CheckShouldInvokeBase(instance))
                    base.VMethod1();
                else
                    mVMethod1.Invoke(instance);
            }

            public override Boolean VMethod2()
            {
                if (mVMethod2.CheckShouldInvokeBase(instance))
                    return base.VMethod2();
                else
                    return mVMethod2.Invoke(instance);
            }
            IMethod mVMethod3;
            bool invoking;
            public override void VMethod3(ref int arg)
            {
                if(mVMethod3 == null)
                {
                    mVMethod3 = instance.Type.GetMethod("VMethod3");
                }
                if (mVMethod3 != null && !invoking)
                {
                    invoking = true;
                    using(var ctx = appdomain.BeginInvoke(mVMethod3))
                    {
                        ctx.PushInteger(arg);//byref
                        ctx.PushObject(instance);
                        ctx.PushReference(0);
                        ctx.Invoke();
                        arg = ctx.ReadInteger(0);
                    }
                    invoking = false;
                }
                else
                    base.VMethod3(ref arg);
            }

            protected override void AbMethod1()
            {
                mAMethod1.Invoke(instance);
            }

            public override Single AbMethod2(Int32 arg1)
            {
                return mAMethod2.Invoke(instance, arg1);
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