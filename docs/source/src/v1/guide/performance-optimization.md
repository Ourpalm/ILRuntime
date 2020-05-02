---
title: ILRuntime的性能优化
type: guide
order: 1010
---

## ILRuntime的性能优化建议

### Release vs Debug
ILRuntime的性能跟编译模式和Unity发布选项有着非常大的关系，要想ILRuntime发挥最高性能，需要确保以下两点：

- 热更用的DLL编译的时候一定要选择Release模式，或者开启代码优化选项，Release模式会比Debug模式的性能高至少2倍
- 关闭Development Build选项来发布Unity项目。在Editor中或者开启Development Build选项发布会开启ILRuntime的Debug框架，以提供调用堆栈行号以及调试服务，这些都会额外耗用不少性能，因此正式发布的时候可以不加载pdb文件，以节省更多内存

### CLR绑定
默认情况下，ILRuntime中调用Unity主工程的方法，ILRuntime会通过反射对目标方法进行调用，这个过程会因为装箱，拆箱等操作，产生大量的GC Alloc和额外开销，因此我们需要借助CLR绑定功能，将我们需要的函数调用进行静态绑定，这样在进行调用的时候就不会出现GC Alloc和额外开销了。

>在Unity的示例工程中，有关于CLR绑定使用的例子，
通过ILRuntime菜单里的Generate CLRBinding code选项可以自动生成所需要的绑定代码

### 值类型

由于值类型的特殊和ILRuntime的实现原理，使用ILRuntime外部定义的值类型（例如UnityEngine.Vector3）在默认情况下会造成额外的装箱拆箱开销，以及相对应的GC Alloc内存分配。

为了解决这个问题，ILRuntime在1.3.0版中增加了值类型绑定（ValueTypeBinding）机制，通过对这些值类型添加绑定器，可以大幅增加值类型的执行效率，以及避免GC Alloc内存分配。具体用法请参考ILRuntime的Unity3D示例工程或者ILRuntime的TestCases测试用例工程。

### 接口调用建议
为了调用方便，ILRuntime的很多接口使用了params可变参数，但是有可能会无意间忽视这个操作带来的GCAlloc，例如下面的操作：
```csharp
appdomain.Invoke("MyGame.Main", "Initialize", null);
appdomain.Invoke("MyGame.Main", "Start", null, 100, 200);
```

这两个操作在调用的时候，会分别生成一个`object[0]`和`object[2]`，从而产生GC Alloc，这一点很容易被忽略。所以如果你需要在Update等性能关键的地方调用热更DLL中的方法，应该按照以下方式缓存这个参数数组：
```csharp
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

如果需要传递的参数或返回值中包含int, float等基础类型，那使用上面的方法依然无法消除GC Alloc，为了更高效率的调用，ILRuntime提供了InvocationContext这种调用方式，需要按照如下方式调用
```csharp
int result = 0;
using(var ctx = appdomain.BeginInvoke(m))
{
    //依次将参数压入栈，如果为成员方法，第一个参数固定为对象实例
    ctx.PushObject(this);
	ctx.PushInteger(123);
	//开始调用
	ctx.Invoke();
	//调用完毕后使用对应的Read方法获取返回值
	result = ctx.ReadInteger();
}
```