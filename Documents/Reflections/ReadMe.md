ILRuntime中的反射
===============
用C#开发项目，很多时候会需要使用反射来实现某些功能。但是在脚本中使用反射其实是一个非常困难的事情。因为这需要把ILRuntime中的类型转换成一个真实的C#运行时类型，并把它们映射起来

默认情况下，System.Reflection命名空间中的方法，并不可能得知ILRuntime中定义的类型，因此无法通过Type.GetType等接口取得热更DLL里面的类型。而且ILRuntime里的类型也并不是一个System.Type。

为了解决这个问题，ILRuntime额外实现了几个用于反射的辅助类：ILRuntimeType，ILRuntimeMethodInfo，ILRuntimeFieldInfo等，来模拟系统的类型来提供部分反射功能

通过反射获取Type
----------------
在**热更DLL**当中，直接调用Type.GetType("TypeName")或者typeof(TypeName)均可以得到有效System.Type类型实例
```C#
//在热更DLL中，以下两种方式均可以
Type t = typeof(TypeName);
Type t2 = Type.GetType("TypeName");
```

在**Unity主工程中**，无法通过Type.GetType来取得热更DLL内部定义的类，而只能通过以下方式得到System.Type实例：

```C#
IType type = appdomain.LoadedTypes["TypeName"];
Type t = type.ReflectedType;
```

通过反射创建实例
---------------
在**热更DLL**当中，可以直接通过Activator来创建实例：

```C#
Type t = Type.GetType("TypeName");//或者typeof(TypeName)
//以下两种方式均可以
object instance = Activator.CreateInstance(t);
object instance = Activator.CreateInstance<TypeName>();
```

在**Unity主工程中**，无法通过Activator来创建热更DLL内类型的实例，必须通过AppDomain来创建实例：
```C#
object instance = appdomain.Instantiate("TypeName");
```

通过反射调用方法
---------------
在**热更DLL**当中，通过反射调用方法跟通常C#用法没有任何区别

```C#
Type type = typeof(TypeName);
object instance = Activator.CreateInstance(type);
MethodInfo mi = type.GetMethod("foo");
mi.Invoke(instance, null);
```

在**Unity主工程中**，可以通过C#通常用法来调用，也可以通过ILRuntime自己的接口来调用，两个方式是等效的：
```C#
IType t = appdomain.LoadedTypes["TypeName"];
Type type = t.ReflectedType;

object instance = appdomain.Instantiate("TypeName");

//系统反射接口
MethodInfo mi = type.GetMethod("foo");
mi.Invoke(instance, null);

//ILRuntime的接口
IMethod m = t.GetMethod("foo", 0);
appdomain.Invoke(m, instance, null);
```

通过反射获取和设置Field的值
----------
在热更DLL和Unity主工程中获取和设置Field的值跟通常C#用法没有区别
```C#
Type t;
FieldInfo fi = t.GetField("field");
object val = fi.GetValue(instance);
fi.SetValue(instance, val);
```

通过反射获取Attribute标注
---------
在热更DLL和Unity主工程中获取Attribute标注跟通常C#用法没有区别
```C#
Type t;
FieldInfo fi = t.GetField("field");
object[] attributeArr = fi.GetCustomAttributes(typeof(SomeAttribute), false);
```

限制和注意事项
============

* 在Unity主工程中不能通过new T()的方式来创建热更工程中的类型实例