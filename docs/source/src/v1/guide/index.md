---
title: 介绍
type: guide
order: 0
---

## ILRuntime

[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Ourpalm/ILRuntime/blob/master/LICENSE.TXT) [![release](https://img.shields.io/badge/release-v1.6.0-blue.svg)](https://github.com/Ourpalm/ILRuntime/releases) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/Ourpalm/ILRuntime/pulls)

ILRuntime项目为基于C#的平台（例如Unity）提供了一个`纯C#实现`，`快速`、`方便`且`可靠`的IL运行时，使得能够在不支持JIT的硬件环境（如iOS）能够实现代码的热更新

### ILRuntime的优势

同市面上的其他热更方案相比，ILRuntime主要有以下优点：

- 无缝访问C#工程的现成代码，无需额外抽象脚本API
- 直接使用VS2015进行开发，ILRuntime的解译引擎支持.Net 4.6编译的DLL
- 执行效率是L#的10-20倍
- 选择性的CLR绑定使跨域调用更快速，绑定后跨域调用的性能能达到slua的2倍左右（从脚本调用GameObject之类的接口）
- 支持跨域继承
- 完整的泛型支持
- 拥有Visual Studio的调试插件，可以实现真机源码级调试。支持Visual Studio 2015 Update3 以及Visual Studio 2017和Visual Studio 2019

### C# vs Lua

目前市面上主流的热更方案，主要分为Lua的实现和用C#的实现，两种实现方式各有各的优缺点。

Lua是一个已经非常成熟的解决方案，但是对于Unity项目而言，也有非常明显的缺点。就是如果使用Lua来进行逻辑开发，就势必要求团队当中的人员需要同时对Lua和C#都特别熟悉，或者将团队中的人员分成C#小组和Lua小组。不管哪一种方案，对于中小型团队都是非常痛苦的一件事情。

用C#来作为热更语言最大的优势就是项目可以用同一个语言来进行开发，对Unity项目而言，这种方式肯定是开发效率最高的。

Lua的优势在于解决方案足够成熟，之前的C++团队可能比起C#，更加习惯使用Lua来进行逻辑开发。此外借助luajit，在某些情况下的执行效率会非常不错，但是luajit现在维护情况也不容乐观，官方还是推荐使用公版Lua来开发。

>如果需要测试ILRuntime对比Lua的性能Benchmark，需要确认以下几点：
- ILRuntime加载的dll文件是`Release`模式编译的
- dll中对外部API的调用都进行了`CLR绑定`
- 确保`没有勾选Development Build`的情况下发布成正式真机运行包，而`不是在Editor中直接运行`

>ILRuntime设计上为了在开发时提供更多的调试支持，在Unity Editor中运行会有很多额外的性能开销，
因此在Unity Editor中直接测试并不能代表ILRuntime的实际运行性能。