---
title: 介绍
type: guide
order: 0
---

## ILRuntime

[![license](https://img.shields.io/badge/license-MIT-blue.png)](https://github.com/Ourpalm/ILRuntime/blob/master/LICENSE.TXT) [![GitHub version](https://badge.fury.io/gh/Ourpalm%2FILRuntime.svg)](https://badge.fury.io/gh/Ourpalm%2FILRuntime) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.png)](https://github.com/Ourpalm/ILRuntime/pulls)

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

### 启步

#### **从Unity开始**

如果你希望在Unity中使用ILRuntime，推荐的方式是直接使用ILRuntime的源代码，这样ILRuntime可以根据你的发布设置自动进行优化。

你需要将下列源码目录复制Unity工程的Assets目录：

- Mono.Cecil.20
- Mono.Cecil.Pdb
- ILRuntime

> 需要注意的是，需要删除这些目录里面的`bin`、`obj`、`Properties子目录`，以及`.csproj文件`。此外，由于ILRuntime使用了`unsafe`代码来优化执行效率，所以你需要在Unity中开启`unsafe`模式：

- 在Assets目录里建立一个名为smcs.rsp的文本文件
- 在smcs.rsp文件中加入"-unsafe"

#### **从Visual Studio开始**

如果你希望在VisualStudio的C#项目中使用ILRuntime， 你只需要引用编译好的`ILRuntime.dll`，`Mono.Cecil.20.dll`以及`Mono.Cecil.Pdb`即可。

### 使用之前

请先执行一次测试用例以保证您下载的ILRuntime没有问题。

ILRuntime项目提供了一个测试用例工程ILRuntimeTest，用来验证ILRuntime的正常运行，在运行测试用例前，需要手动生成一下TestCases里面的工程，生成DLL文件。

### 开始使用

使用ILRuntime非常简单，只需要以下这些代码即可运行一个完整的例子：

```csharp
ILRuntime.Runtime.Enviorment.AppDomain appdomain;
void Start()
{
    StartCoroutine(LoadILRuntime());
}

IEnumerator LoadILRuntime()
{
    appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
#if UNITY_ANDROID
    WWW www = new WWW(Application.streamingAssetsPath + "/Hotfix.dll");
#else
    WWW www = new WWW("file:///" + Application.streamingAssetsPath + "/Hotfix.dll");
#endif
    while (!www.isDone)
        yield return null;
    if (!string.IsNullOrEmpty(www.error))
        D.error(www.error);
    byte[] dll = www.bytes;
    www.Dispose();
#if UNITY_ANDROID
    www = new WWW(Application.streamingAssetsPath + "/Hotfix.pdb");
#else
    www = new WWW("file:///" + Application.streamingAssetsPath + "/Hotfix.pdb");
#endif
    while (!www.isDone)
        yield return null;
    if (!string.IsNullOrEmpty(www.error))
        D.error(www.error);
    byte[] pdb = www.bytes;
    using (System.IO.MemoryStream fs = new MemoryStream(dll))
    {
        using (System.IO.MemoryStream p = new MemoryStream(pdb))
        {
            appdomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
        }
    }
    OnILRuntimeInitialized();
}

void OnILRuntimeInitialized()
{
    appdomain.Invoke("Hotfix.Game", "Initialize", null, null);
}
```

这个例子为了演示方便，直接从StreamingAssets目录里读取了脚本DLL文件以及调试符号PDB文件， 实际发布的时候，如果要热更，肯定是将DLL和PDB文件打包到Assetbundle中进行动态加载的，这个不是ILRuntime的范畴，故不具体演示了。


### 调试插件

ILRuntime提供了一个VisualStudio2015的调试插件，用来源码级调试你的热更脚本。

你可以在[这里](https://github.com/Ourpalm/ILRuntime/releases)下载到最新的VS2015调试插件。

使用方法如下：

- 安装ILRuntime调试插件，并重新启动VS2015
- 运行Unity工程，并保证执行过appdomain.DebugService.StartDebugService(56000);来启动调试服务器
- 用VS2015打开热更DLL项目
- 点击菜单中的Debug->Attach to ILRuntime按钮
- 在弹出来的窗口中填入被调试的主机的IP地址以及调试服务器的端口
- 点击Attach按钮后，即可像UnityVS一样下断点调试

注意事项：

- 调试插件需要Visual Studio 2015 Update3以上版本
- 调试插件目前仍热在开发中，目前断点后仅能查看基础类型的局部变量和函数参数的值