---
title: 寄存器模式
type: guide
order: 101
---

## 寄存器模式

寄存器模式是ILRuntime2.0版引入的专用于优化大规模数值计算的执行模式。该模式通过ILRuntime自己的JIT Compiler将原始DLL的MSIL指令集转换成一个自定义的基于寄存器的指令集，再进行解译执行。由于该JIT编译的结果是ILRuntime自己设计的虚拟指令集，并不是真实硬件指令集，因此可以毫无问题的在iOS等平台上执行

### 使用方法

开启寄存器模式，主要有2个途径：

```csharp
//第一种方式是在AppDomain的构造函数的参数中，指定全局希望使用的JIT模式
appdomain = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntimeJITFlags.JITOnDemand);

//第二种方式为在指定的类或者方法上指定用于该类或者方法的JIT模式
[ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
class foo
{
    [ILRuntimeJIT(ILRuntimeJITFlags.NoJIT)]
    public void bar()
    {
        //该方法的内容将不会开启寄存器模式
    }
	
    public void bar2()
    {
        //该方法的内容将按照所在类指定的模式，也就是立即JIT模式运行
    }
}
```

ILRuntime的JIT模式有以下几种：

- JITOnDemand，按需JIT模式，使用该模式在默认的情况下会按照原始的方式运行，当该方法被反复执行时，会被标记为需要被JIT，并在后台线程完成JIT编译后切换到寄存器模式运行
- JITImmediately，立即JIT模式，使用该模式时，当方法被调用的瞬间即会被执行JIT编译，在第一次执行时即使用寄存器模式运行。JIT会在当前线程发生，因此如果方法过于复杂在第一次执行时可能会有较大的初始化时间
- NoJIT， 禁用JIT模式，该方法在执行时会始终以传统方式执行
- ForceInline， 强制内联模式，该模式只对方法的Attribute生效，标注该模式的方法在被调用时将会无视方法体内容大小，强制被内联

### 寄存器模式的性能特点

当ILRuntime运行在寄存器模式时，主要会有以下性能特征：

- 数值计算性能会大幅提升，包括for循环等需要数值计算的控制流
- 由于小方法会被内联，所以getter/setter等的调用开销，for循环里调用其他热更内方法的性能也会有所提升
- 如果一个方法既没有数值计算，又没有频繁调用热更内小方法或者访问property， 主要由调用系统或UnityAPI组成，则不会产生任何优化，一些情况下可能性能还低于传统模式

### 使用建议

ILRuntime推荐的使用模式有2种：

- AppDomain构造函数时不指定JIT模式，即默认使用传统模式执行，在遇到就要优化的密集计算型方法时，对该方法指定JITImmediately模式
- 直接在AppDomain构造函数处指定JITOnDemand模式

第一种用法对现有实现影响最小，仅在需要优化处开启，可以比较精准的控制执行效果。如果并不知道在什么时候应该使用何种模式，也可以直接使用JITOnDemand模式，让ILRuntime自行决定运行模式，在大多数情况下是能达到不错的性能平衡的。
