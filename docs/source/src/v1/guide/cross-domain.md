---
title: ILRuntime中跨域继承
type: guide
order: 120
---

## ILRuntime中跨域继承

如果你想在热更DLL项目当中`继承一个Unity主工程里的类`，或者`实现一个主工程里的接口`，你需要在Unity主工程中实现一个继承适配器。
方法如下：

```csharp
    //你想在DLL中继承的那个类
    public abstract class ClassInheritanceTest
    {
        public abstract void TestAbstract();
        public virtual void TestVirtual(ClassInheritanceTest a)
        {
            
        }
    }

    //这个类就是继承适配器类
    public class ClassInheritanceAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
			    //如果你是想一个类实现多个Unity主工程的接口，这里需要return null;
                return typeof(ClassInheritanceTest);//这是你想继承的那个类
            }
        }
		
        public override Type[] BaseCLRTypes
        {
            get
            {
                //跨域继承只能有1个Adapter，因此应该尽量避免一个类同时实现多个外部接口，
                //ILRuntime虽然支持同时实现多个接口，但是一定要小心这种用法，使用不当很容易造成不可预期的问题
                //日常开发如果需要实现多个DLL外部接口，请在Unity这边先做一个基类实现那些个接口，然后继承那个基类
				//如需一个Adapter实现多个接口，请用下面这行
                //return new Type[] { typeof(IEnumerator<object>), typeof(IEnumerator), typeof(IDisposable) };
				return null;
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);//这是实际的适配器类
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);//创建一个新的实例
        }

        //实际的适配器类需要继承你想继承的那个类，并且实现CrossBindingAdaptorType接口
        class Adaptor : ClassInheritanceTest,CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;
            IMethod mTestAbstract;
			bool mTestAbstractGot;
            IMethod mTestVirtual;
			bool mTestVirtualGot;
            bool isTestVirtualInvoking = false;
			//缓存这个数组来避免调用时的GC Alloc
			object[] param1 = new object[1];

            public Adaptor()
            {

            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }
            
			//你需要重写所有你希望在热更脚本里面重写的方法，并且将控制权转到脚本里去
            public override void TestAbstract()
            {
                if(!mTestAbstractGot)
                {
                    mTestAbstract = instance.Type.GetMethod("TestAbstract", 0);
					mTestAbstractGot = true;
                }
                if (mTestAbstract != null)
                    appdomain.Invoke(mTestAbstract, instance, null);//没有参数建议显式传递null为参数列表，否则会自动new object[0]导致GC Alloc
            }

            public override void TestVirtual(ClassInheritanceTest a)
            {
                if (!mTestVirtualGot)
                {
                    mTestVirtual = instance.Type.GetMethod("TestVirtual", 1);
					mTestVirtualGot = true;
                }
				//对于虚函数而言，必须设定一个标识位来确定是否当前已经在调用中，否则如果脚本类中调用base.TestVirtual()就会造成无限循环，最终导致爆栈
                if (mTestVirtual != null && !isTestVirtualInvoking)
                {
                    isTestVirtualInvoking = true;
					param1[0] = a;
                    appdomain.Invoke(mTestVirtual, instance, this.param1);
                    isTestVirtualInvoking = false;
                }
                else
                    base.TestVirtual(a);
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
```

因为跨域继承必须要注册适配器。 如果是热更DLL里面继承热更里面的类型，不需要任何注册。
```csharp
        appdomain.RegisterCrossBindingAdaptor(new ClassInheritanceAdaptor());
```
