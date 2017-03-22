CLR重定向
===============
在开发中，如ILRuntime的反射那篇文档中说的，一些依赖反射的接口是没有办法直接运行的，最典型的就是在Unity主工程中通过new T()创建热更DLL内类型的实例。
细心的朋友一定会好奇，为什么Activator.CreateInstance<Type>();这个明显内部是new T();的接口可以直接调用呢？

ILRuntime为了解决这类问题，引入了CLR重定向机制。 原理就是当IL解译器发现需要调用某个指定CLR方法时，将实际调用重定向到另外一个方法进行挟持，再在这个方法中对ILRuntime的反射的用法进行处理

刚刚提到的Activator.CreateInstance<T>的CLR重定向定义如下：
```C#
        public static StackObject* CreateInstance(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method, bool isNewObj)
        {
		    //获取泛型参数<T>的实际类型
            IType[] genericArguments = method.GenericArguments;
            if (genericArguments != null && genericArguments.Length == 1)
            {
                var t = genericArguments[0];
                if (t is ILType)//如果T是热更DLL里的类型
                {
				    //通过ILRuntime的接口来创建实例
                    return ILIntepreter.PushObject(esp, mStack, ((ILType)t).Instantiate());
                }
                else
                    return ILIntepreter.PushObject(esp, mStack, Activator.CreateInstance(t.TypeForCLR));//通过系统反射接口创建实例
            }
            else
                throw new EntryPointNotFoundException();
        }
```

要让这段代码生效，需要执行相对应的注册方法：
```C#
		foreach (var i in typeof(System.Activator).GetMethods())
		{
		    //找到名字为CreateInstance，并且是泛型方法的方法定义
			if (i.Name == "CreateInstance" && i.IsGenericMethodDefinition)
			{
				appdomain.RegisterCLRMethodRedirection(i, CreateInstance);
			}
		}
```

带参数的方法的重定向
-----------
刚刚的例子当中，由于CreateInstance<T>方法并没有任何参数，所以需要另外一个例子来展示用法，最好的例子就是Unity的Debug.Log接口了，默认情况下，如果在DLL工程中调用该接口，是没有办法显示正确的调用堆栈的，会给开发带来一些麻烦，下面我会展示怎么通过CLR重定向来实现在Debug.Log调用中打印热更DLL中的调用堆栈

```C#
        public unsafe static StackObject* DLog(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
			//只有一个参数，所以返回指针就是当前栈指针ESP - 1
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
			//第一个参数为ESP -1， 第二个参数为ESP - 2，以此类推
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
			//获取参数message的值
            object message = StackObject.ToObject(ptr_of_this_method, __domain, __mStack);
			//需要清理堆栈
            __intp.Free(ptr_of_this_method);
			//如果参数类型是基础类型，例如int，可以直接通过int param = ptr_of_this_method->Value获取值，
			//关于具体原理和其他基础类型如何获取，请参考ILRuntime实现原理的文档。
			
			//通过ILRuntime的Debug接口获取调用热更DLL的堆栈
            string stackTrace = __domain.DebugService.GetStackTrance(__intp);
            Debug.Log(string.Format("{0}\n{1}", format, stackTrace));

            return __ret;
        }
```

然后在通过下面的代码注册重定向即可：
```C#
appdomain.RegisterCLRMethodRedirection(typeof(Debug).GetMethod("Log"), DLog);
```