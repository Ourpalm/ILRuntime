ILRuntime
==========
[English Document](ReadMe-EN.md "Click here for English documents")

[![license](https://img.shields.io/badge/license-MIT-blue.png)](https://github.com/Ourpalm/ILRuntime/blob/master/LICENSE.TXT)
[![release](https://img.shields.io/badge/release-v1.1.0-blue.png)](https://github.com/Ourpalm/ILRuntime/releases)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.png)](https://github.com/Ourpalm/ILRuntime/pulls)

ILRuntime项目为基于C#的平台（例如Unity）提供了一个纯C#实现的，快速、方便并且可靠的IL运行时，使得能够在不支持JIT的硬件环境（如iOS）能够实现代码的热更新

ILRuntime的优势
----------
同市面上的其他热更方案相比，ILRuntime主要有以下优点：
* 无缝访问C#工程的现成代码，无需额外抽象脚本API
* 直接使用VS2015进行开发，ILRuntime的解译引擎支持.Net 4.6编译的DLL
* 执行效率是L#的10-20倍
* 选择性的CLR绑定使跨域调用更快速，绑定后跨域调用的性能能达到slua的2倍左右（从脚本调用GameObject之类的接口）
* 支持跨域继承
* 完整的泛型支持
* 拥有Visual Studio 2015的调试插件，可以实现真机源码级调试(WIP)

C# vs Lua
========
目前市面上主流的热更方案，主要分为Lua的实现和用C#的实现，两种实现方式各有各的优缺点。Lua是一个已经非常成熟的解决方案，但是对于Unity项目而言，也有非常明显的缺点。就是如果使用Lua来进行逻辑开发，就势必要求团队当中的人员需要同时对Lua和C#都特别熟悉，或者将团队中的人员分成C#小组和Lua小组。不管哪一种方案，对于中小型团队都是非常痛苦的一件事情。用C#来作为热更语言最大的优势就是项目可以用同一个语言来进行开发，对Unity项目而言，这种方式肯定是开发效率最高的。

Lua的优势在于解决方案足够成熟，之前的C++团队可能比起C#，更加习惯使用Lua来进行逻辑开发。此外借助luajit，在某些情况下的执行效率会非常不错，但是luajit现在维护情况也不容乐观，官方还是推荐使用公版Lua来开发。

快速入门
========
Unity
----------
如果你希望在Unity中使用ILRuntime，推荐的方式是直接使用ILRuntime的源代码，这样ILRuntime可以根据你的发布设置自动进行优化。
你需要将下列源码目录复制Unity工程的Assets目录：
* Mono.Cecil.20
* Mono.Cecil.Pdb
* ILRuntime

需要注意的是，需要删除这些目录里面的bin、obj、Properties子目录，以及.csproj文件。

此外，由于ILRuntime使用了unsafe代码来优化执行效率，所以你需要在Unity中开启unsafe模式：
* 在Assets目录里建立一个名为smcs.rsp的文本文件
* 在smcs.rsp文件中加入"-unsafe"

你可以在[这里](https://github.com/Ourpalm/ILRuntime/releases)下载到Unity的示例工程

Visual Studio
----------
如果你希望在VisualStudio的C#项目中使用ILRuntime， 你只需要引用编译好的ILRuntime.dll，Mono.Cecil.20.dll以及Mono.Cecil.Pdb即可。

用法
----------
使用ILRuntime非常简单，只需要以下这些代码即可运行一个完整的例子：
```C#
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

测试用例
----------
ILRuntime项目提供了一个测试用例工程ILRuntimeTest，用来验证ILRuntime的正常运行，在运行测试用例前，需要手动生成一下TestCases里面的工程，生成DLL文件。

调试插件
----------
ILRuntime提供了一个VisualStudio2015的调试插件，用来源码级调试你的热更脚本。

你可以在[这里](https://github.com/Ourpalm/ILRuntime/releases)下载到最新的VS2015调试插件。

使用方法如下：

* 安装ILRuntime调试插件，并重新启动VS2015
* 运行Unity工程，并保证执行过appdomain.DebugService.StartDebugService(56000);来启动调试服务器
* 用VS2015打开热更DLL项目
* 点击菜单中的Debug->Attach to ILRuntime按钮
* 在弹出来的窗口中填入被调试的主机的IP地址以及调试服务器的端口
* 点击Attach按钮后，即可像UnityVS一样下断点调试

注意事项：

* 调试插件需要Visual Studio 2015 Update3以上版本
* 调试插件目前仍热在开发中，目前断点后仅能查看基础类型的局部变量和函数参数的值

Unity3D的示例工程
==============
你可以在[这里](https://github.com/Ourpalm/ILRuntime/releases)下载到最新的Unity实例工程。

里面有2个工程，其中ILRuntimeDemo是Unity的主工程。实例都在这个工程当中的各个示例场景中，这个工程是在Unity5.4下完成的，如果你使用的是其他版本的Unity，可能需要自己做一些修改。

HotFix_Project是热更DLL工程，请用VS2015之类的C# IDE打开和进行编译，在编译前请确保至少打开过一次Unity的主工程，如果编译依然说说找不到UnityEngine等dll，请手动重新指认一下

文档
==========
* [ILRuntime中使用委托](Documents/Delegates/)
* [ILRuntime中跨域继承](Documents/Inheritance/)
* [ILRuntime中的反射](Documents/Reflections/)
* [CLR重定向机制](Documents/CLRRedirection/)
* [CLR绑定](Documents/CLRBinding/)
* [iOS IL2CPP打包注意事项](Documents/IL2CPP/)
* [ILRuntime的性能优化建议](Documents/Optimization/)
* [ILRuntime的实现原理](Documents/ILIntepreter/)

技术支持
==========

QQ群：512079820