ILRuntime中使用委托
===============
如果只在热更新的DLL项目中使用的委托，是不需要任何额外操作的，就跟在通常的C#里那样使用即可

如果你需要将委托实例传给ILRuntime外部使用，那则根据情况，你需要额外添加适配器或者转换器。
需要注意的是，一些编译器功能也会生成将委托传出给外部使用的代码，例如：
* Linq当中where xxxx == xxx，会需要将xxx == xxx这个作为lambda表达式传给Linq.Where这个外部方法使用
* OrderBy()方法，原因同上

如果在运行时发现缺少注册某个指定类型的委托适配器或者转换器时，ILRuntime会抛出相应的异常，根据提示添加注册即可。

委托适配器（DelegateAdapter）
----------
如果将委托实例传出给ILRuntime外部使用，那就意味着需要将委托实例转换成真正的CLR（C#运行时）委托实例，这个过程需要动态创建CLR的委托实例。由于IL2CPP之类的AOT编译技术无法在运行时生成新的类型，所以在创建委托实例的时候ILRuntime选择了显式注册的方式，以保证问题不被隐藏到上线后才发现。

同一个参数组合的委托，只需要注册一次即可，例如：
```C#
delegate void SomeDelegate(int a, float b);

Action<int, float> act;
```
这两个委托都只需要注册一个适配器即可。 注册方法如下
```C#
appDomain.DelegateManager.RegisterMethodDelegate<int, float>();
```

如果是带返回类型的委托，例如：
```C#
delegate bool SomeFunction(int a, float b);

Func<int, float, bool> act;
```
需要按照以下方式注册
```C#
appDomain.DelegateManager.RegisterFunctionDelegate<int, float, bool>();
```

委托转换器（DelegateConvertor）
----------
ILRuntime内部是使用Action,以及Func这两个系统自带委托类型来生成的委托实例，所以如果你需要将一个不是Action或者Func类型的委托实例传到ILRuntime外部使用的话，除了委托适配器，还需要额外写一个转换器，将Action和Func转换成你真正需要的那个委托类型。

比如上面例子中的SomeFunction类型的委托，其所需的Convertor应如下实现：
```C#
app.DelegateManager.RegisterDelegateConvertor<SomeFunction>((action) =>
{
    return new SomeFunction((a, b) =>
    {
       return ((Func<int, float, bool>)action)(a, b);
    });
});
```

建议
=========
为了避免不必要的麻烦，以及后期热更出现问题，建议项目遵循以下几点：
* 尽量避免不必要的跨域委托调用
* 尽量使用Action以及Func这两个系统内置万用委托类型