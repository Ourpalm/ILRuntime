ILRuntime的性能优化建议
===============

Release vs Debug
---------
ILRuntime的性能跟编译模式和Unity发布选项有着非常大的关系，要想ILRuntime发挥最高性能，需要确保以下两点：

* 热更用的DLL编译的时候一定要选择Release模式，或者开启代码优化选项，Release模式会比Debug模式的性能高至少2倍
* 关闭Development Build选项来发布Unity项目。在Editor中或者开启Development Build选项发布会开启ILRuntime的Debug框架，以提供调用堆栈行号以及调试服务，这些都会额外耗用不少性能，因此正式发布的时候可以不加载pdb文件，以节省更多内存

值类型
----------
由于值类型的特殊和ILRuntime的实现原理，目前没有办法做到直接在栈上为所有类型申请内存，因此依然只有将值类型进行装箱，然后在通过深层拷贝来模拟值类型的行为。

因此在ILRuntime中值类型的运行效率会低于引用类型，并且在赋值时可能还会产生额外的GC Alloc，因此在热更DLL当中应该尽量避免大量使用值类型

接口调用建议
-----------
为了调用方便，ILRuntime的很多接口使用了params可变参数，但是有可能会无意间忽视这个操作带来的GCAlloc，例如下面的操作：
```C#
appdomain.Invoke("MyGame.Main", "Initialize", null);
appdomain.Invoke("MyGame.Main", "Start", null, 100, 200);
```

这两个操作在调用的时候，会分别生成一个`object[0]`和`object[2]`，从而产生GC Alloc，这一点很容易被忽略。所以如果你需要在Update等性能关键的地方调用热更DLL中的方法，应该按照以下方式缓存这个参数数组：
```C#
object[] param0 = new object[0];
object[] param2 = new object[2];
IMethod m, m2;

void Start()
{
    m = appdomain.LoadedTypes["MyGame.SomeUI"].GetMethod("Update", 0);
	m2 = appdomain.LoadedTypes["MyGame.SomeUI"].GetMethod("SomethingAfterUpdate", 2);
}

void Update()
{
    appdomain.Invoke(m, null, param0);
	param2[0] = this;
	param2[1] = appdomain;
	appdomain.Invoke(m2, null, param2);
}
```

通过缓存IMethod实例以及参数列表数组，可以做到这个Update操作不会产生任何额外的GC Alloc，并且以最高的性能来执行