---
title: ILRuntime中跨域继承
type: guide
order: 120
---

## ILRuntime中跨域继承

如果你想在热更DLL项目当中`继承一个Unity主工程里的类`，或者`实现一个主工程里的接口`，你需要在Unity主工程中实现一个继承适配器。
方法如下：

```csharp
public class TestClass2Adapter : CrossBindingAdaptor
    {
	    //定义访问方法的方法信息
        static CrossBindingMethodInfo mVMethod1_0 = new CrossBindingMethodInfo("VMethod1");
        static CrossBindingFunctionInfo<System.Boolean> mVMethod2_1 = new CrossBindingFunctionInfo<System.Boolean>("VMethod2");
        static CrossBindingMethodInfo mAbMethod1_3 = new CrossBindingMethodInfo("AbMethod1");
        static CrossBindingFunctionInfo<System.Int32, System.Single> mAbMethod2_4 = new CrossBindingFunctionInfo<System.Int32, System.Single>("AbMethod2");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(ILRuntimeTest.TestFramework.TestClass2);//这里是你想继承的类型
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

            //必须要提供一个无参数的构造函数
            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            //下面将所有虚函数都重载一遍，并中转到热更内
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
```

因为跨域继承必须要注册适配器。 如果是热更DLL里面继承热更里面的类型，不需要任何注册。
```csharp
        appdomain.RegisterCrossBindingAdaptor(new ClassInheritanceAdaptor());
```
