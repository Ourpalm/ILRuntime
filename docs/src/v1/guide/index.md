---
title: 介绍
type: guide
order: 0
---

## ILRuntime

[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Ourpalm/ILRuntime/blob/master/LICENSE.TXT) [![GitHub version](https://badge.fury.io/gh/Ourpalm%2FILRuntime.svg)](https://badge.fury.io/gh/Ourpalm%2FILRuntime) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/Ourpalm/ILRuntime/pulls)

ILRuntime项目为基于C#的平台（例如Unity）提供了一个纯C#实现的，快速、方便并且可靠的IL运行时，使得能够在不支持JIT的硬件环境（如iOS）能够实现代码的热更新

### ILRuntime的优势

同市面上的其他热更方案相比，ILRuntime主要有以下优点：

- 无缝访问C#工程的现成代码，无需额外抽象脚本API
- 直接使用VS2015进行开发，ILRuntime的解译引擎支持.Net 4.6编译的DLL
- 执行效率是L#的10-20倍
- 选择性的CLR绑定使跨域调用更快速，绑定后跨域调用的性能能达到slua的2倍左右（从脚本调用GameObject之类的接口）
- 支持跨域继承
- 完整的泛型支持
- 拥有Visual Studio 2015的调试插件，可以实现真机源码级调试(WIP)