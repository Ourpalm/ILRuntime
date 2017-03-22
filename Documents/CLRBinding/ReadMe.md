CLR绑定
===============
通常情况下，如果要从热更DLL中调用Unity主工程或者Unity的接口，是需要通过反射接口来调用的，包括市面上不少其他热更方案，也是通过这种方式来对CLR方接口进行调用的。

但是这种方式有着明显的弊端，最突出的一点就是通过反射来调用接口调用效率会比直接调用低很多，再加上反射传递函数参数时需要使用**object[]**数组，这样不可避免的每次调用都会产生不少GC Alloc。众所周知GC Alloc高意味着在Unity中执行会存在较大的性能问题。

ILRuntime通过CLR方法绑定机制，可以选择性的对经常使用的CLR接口进行直接调用，从而尽可能的消除反射调用开销以及额外的GC Alloc

使用方法
---------
CLR绑定借助了ILRuntime的CLR重定向机制来实现，因为实质上也是将对CLR方法的反射调用重定向到我们自己定义的方法里面来。但是手动编写CLR重定向方法是个工作量非常巨大的事，而且要求对ILRuntime底层机制非常了解（比如如何装拆箱基础类型，怎么处理Ref/Out引用等等），因此ILRuntime提供了一个代码生成工具来自动生成CLR绑定代码。

CLR绑定代码的自动生成工具使用方法如下：
```C#
[MenuItem("ILRuntime/Generate CLR Binding Code")]
static void GenerateCLRBinding()
{
	List<Type> types = new List<Type>();
	//在List中添加你想进行CLR绑定的类型
	types.Add(typeof(int));
	types.Add(typeof(float));
	types.Add(typeof(long));
	types.Add(typeof(object));
	types.Add(typeof(string));
	types.Add(typeof(Console));
	types.Add(typeof(Array));
	types.Add(typeof(Dictionary<string, int>));
	//所有ILRuntime中的类型，实际上在C#运行时中都是ILRuntime.Runtime.Intepreter.ILTypeInstance的实例，
	//因此List<A> List<B>，如果A与B都是ILRuntime中的类型，只需要添加List<ILRuntime.Runtime.Intepreter.ILTypeInstance>即可
	types.Add(typeof(Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, int>));
	//第二个参数为自动生成的代码保存在何处
	ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, "Assets/ILRuntime/Generated");
}
```

在CLR绑定代码生成之后，需要将这些绑定代码注册到AppDomain中才能使CLR绑定生效，但是一定要记得将CLR绑定的注册写在CLR重定向的注册后面，因为同一个方法只能被重定向一次，只有先注册的那个才能生效。

注册方法如下：
```C#
ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
```