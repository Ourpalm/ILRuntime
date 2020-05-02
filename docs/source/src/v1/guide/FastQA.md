---
title: 常见问题解答
type: guide
order: 1030
---

<!--参考链接-->  
[master]:https://github.com/Ourpalm/ILRuntime

[CLR自动分析绑定]:https://ourpalm.github.io/ILRuntime/public/v1/guide/bind.html
<!--参考链接-->  

## 版本

> Unity2018.3以上需要用最新ILruntime。
> 最新的发布版本为V1.6，Unity2018以上可通过Package Manager方式安装
> V1.4已过时，示例工程内的版本已过时，当前最新版本为[master]分支。

## 编辑器内出现错误

### 运行出现报错ObjectDisposedException: Cannot access a closed Stream

> 加载dll的流被关闭了。新版要求流不能关闭，也不能用using写法。新的用法请参见Demo示例

### 运行时出现Cannot find Adaptor for: xxxxxxxxx
> 热更当中跨域继承了xxxxxxx类型，但是没有注册对应的适配器

## 打包后出现问题

### 用mono打包的时候，ILRuntime相关的功能都正常，改用IL2Cpp之后，注册的所有委托都不执行了？

>- 注册委托只能写在主工程里，不能写在热更工程里。
>- 检查是否做了[CLR自动分析绑定]。

### ExecutionEngineException: Attempting to call method 'Scene::GetModule' for which no ahead of time (AOT) code was generated.电脑运行没问题，安卓上报这个错 

>- 检查是否做了[CLR自动分析绑定]。
>- CLRBindings.Initialize(appdomain); 要调用这一句，放在最后.是否漏了这句。

### 真机上调试或运行时出现随机闪退
>- 请确认XCode的编译选项中是否使用了Debug，由于iPhone的线程栈空间很小，调用层级稍微深一点就会出现爆栈，因此请使用Release选项编译XCode工程

## 使用ILRuntime后产生的性能问题

### 使用ILRuntime后产生的GC Alloc比用之前大和很多啊，为什么
>- 请先确保发包之前做过[CLR自动分析绑定]，请一定要注意是自动分析，而不是手动给type列表进行绑定
>- 如果热更里的脚本使用了比较多的值类型局部变量如Vector3等，请确定是否注册了对应的`值类型绑定适配器`
>- 是否在热更代码中使用了`foreach`，由于原理限制，在热更中使用foreach无法避免产生GC Alloc，请使用支持for循环的数据结构，或者用List等支持for遍历的结构辅助Dictionary等无法for遍历的结构
>- 在`编辑器`和`Development Build`的真机包，由于需要支持断点调试和报错打印行号，会有每个方法调用20字节的固定内存分配开销，如果需要分析实际GC Alloc，可在PlayerSettings中定义`DISABLE_ILRUNTIME_DEBUG`宏

### 使用ILRunitme后某个方法调用比之前慢了很多啊
>- 请先确保发包之前做过[CLR自动分析绑定]，请一定要注意是自动分析，而不是手动给type列表进行绑定， 没有CLR绑定的方法调用会慢几十倍
>- 请确保热更DLL是使用Release方式编译的，Debug编译的DLL会慢3-5倍
>- 请发布非Development Build真机包后再测试耗时，编辑器中和Development Build执行速度会比最终Release包慢几倍
>- 请确保注册了常使用的值类型的值类型绑定
>- 由于热更都是解译执行，所以执行效率跟直接执行天然就有20-100倍的差距，因此不大适合需要进行大量遍历计算的操作，可将耗时的工具方法放入主工程辅助

## 疑难杂症



