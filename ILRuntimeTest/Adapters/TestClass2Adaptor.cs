using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntimeTest
{
    public class TestClass2Adapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mVMethod1_0 = new CrossBindingMethodInfo("VMethod1");
        static CrossBindingFunctionInfo<System.Boolean> mVMethod2_1 = new CrossBindingFunctionInfo<System.Boolean>("VMethod2");
        class VMethod3_2Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] { typeof(System.Int32).MakeByRefType() };

            public VMethod3_2Info()
                : base("VMethod3")
            {

            }

            protected override Type ReturnType { get { return null; } }

            protected override Type[] Parameters { get { return pTypes; } }
            public void Invoke(ILTypeInstance instance, ref System.Int32 arg)
            {
                EnsureMethod(instance);
                if (method != null)
                {
                    invoking = true;
                    try
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(arg);
                            ctx.PushObject(instance);
                            ctx.PushReference(0);
                            ctx.Invoke();
                            arg = ctx.ReadObject<System.Int32>(0);
                        }
                    }
                    finally
                    {
                        invoking = false;
                    }
                }
            }

            public override void Invoke(ILTypeInstance instance)
            {
                throw new NotSupportedException();
            }
        }
        static VMethod3_2Info mVMethod3_2 = new VMethod3_2Info();
        static CrossBindingMethodInfo mAbMethod1_3 = new CrossBindingMethodInfo("AbMethod1");
        static CrossBindingFunctionInfo<System.Int32, System.Single> mAbMethod2_4 = new CrossBindingFunctionInfo<System.Int32, System.Single>("AbMethod2");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(ILRuntimeTest.TestFramework.TestClass2);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : ILRuntimeTest.TestFramework.TestClass2, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public override void VMethod1()
            {
                if (mVMethod1_0.CheckShouldInvokeBase(this.instance))
                    base.VMethod1();
                else
                    mVMethod1_0.Invoke(this.instance);
            }

            public override System.Boolean VMethod2()
            {
                if (mVMethod2_1.CheckShouldInvokeBase(this.instance))
                    return base.VMethod2();
                else
                    return mVMethod2_1.Invoke(this.instance);
            }

            public override void VMethod3(ref System.Int32 arg)
            {
                if (mVMethod3_2.CheckShouldInvokeBase(this.instance))
                    base.VMethod3(ref arg);
                else
                    mVMethod3_2.Invoke(this.instance, ref arg);
            }

            protected override void AbMethod1()
            {
                mAbMethod1_3.Invoke(this.instance);
            }

            public override System.Single AbMethod2(System.Int32 arg1)
            {
                return mAbMethod2_4.Invoke(this.instance, arg1);
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

    public class IDisposableAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mDispose_0 = new CrossBindingMethodInfo("Dispose");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(System.IDisposable);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : System.IDisposable, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public void Dispose()
            {
                mDispose_0.Invoke(this.instance);
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
}