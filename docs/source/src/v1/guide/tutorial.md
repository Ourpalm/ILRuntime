---
title: 从零开始
type: guide
order: 100
---

## 从零开始

欢迎您选择ILRuntime ， 根据下面教程您可以快速的开始。

### 起步

#### **在Unity2018以上版本中开始使用ILRuntime**

ILRuntime1.6版新增了Package Manager发布，使用Unity2018以上版本可以直接通过Package Manager安装，具体方法如下

> 如果你使用的是`中国特别版`Unity，那直接打开Package Manager即可找到ILRuntime
> 如果你使用的是`国际版`Unity，或者无法在PackageManager中找到ILRuntime，则需要按照以下步骤设置项目

首先需要在项目的Packages/manifest.json中，添加ILRuntime的源信息，在这个文件的dependencies节点前增加以下代码
```json
  "scopedRegistries": [
    {
      "name": "ILRuntime",
      "url": "https://registry.npmjs.org",
      "scopes": [
        "com.ourpalm"
      ]
    }
  ],
```
然后通过Unity的`Window->Package Manager`菜单，打开Package Manager，将上部标签页选项选择为All Packages，Advanced里勾上Show Preview Packages，等待Unity加载完包信息，应该就能在左侧列表中找到ILRuntime，点击安装即可

部分Unity版本可以无法直接在列表中刷出ILRuntime，如果左边列表找不着，那就在项目的manifest.json中的dependencies段的开头，增加如下代码手动将ILRuntime添加进项目
```json
    "com.ourpalm.ilruntime": "1.6.0",
```

ILRuntime包安装完毕后，在Package Manager中选中ILRuntime， 右边详细页面中有Samples，点击右方的`Import to project`可以将ILRuntime的示例Demo直接导入当前工程。

>示例导入工程后有可能因为没开启unsafe导致编译报错，可以在PlayerSettings中勾选Allow unsafe code解决编译问题。

在Assets\Samples\ILRuntime\1.6\Demo\HotFix_Project~目录中打开热更DLL的vs工程，直接编译，然后就可以正常运行ILRuntime的Demo示例了

> 如果在进行以上配置后依然无法找到ILRuntime，可以按照下面`Unity3D的示例工程`的步骤手动安装ILRuntime

#### **Unity3D的示例工程**

对于Unity2018以前的版本，你可以手动在[这里](https://github.com/Ourpalm/ILRuntimeU3D)下载到最新的Unity实例工程，该示例是在Unity2019下制作的。

里面有2个工程，其中ILRuntimeDemo是Unity的主工程。实例都在这个工程当中的各个示例场景中，这个工程是在Unity2019下完成的，如果你使用的是其他版本的Unity，可能需要自己做一些修改。

HotFix_Project是热更DLL工程，存在于`Assets\Samples\ILRuntime\1.6\Demo\HotFix_Project~`目录中，请用VS2015之类的C# IDE打开和进行编译，在编译前请确保至少打开过一次Unity的主工程，如果编译依然说找不到UnityEngine等dll，请手动重新指认一下

#### **在Unity中使用Github的master分支**

如果你希望在Unity中使用ILRuntime的最新master版本
你需要将下列源码目录复制Unity工程的Assets目录：

- `Dependencies`
- `ILRuntime`

> 需要注意的是，需要删除这些目录里面的`bin`、`obj`、`Properties子目录`，以及`.csproj文件`。此外，由于ILRuntime使用了`unsafe`代码来优化执行效率，所以你需要在Unity中开启`unsafe`模式：

- Unity2017以上的版本请在PlayerSettings中勾选Allow unsafe mode
- 在`Assets`目录里建立一个名为`smcs.rsp`的文本文件
- 在`smcs.rsp`文件中加入 `-unsafe`

>- 如果你使用的是`Unity5.4`及以前的版本，并且使用的编译设置是`.Net 2.0`而不是`.Net 2.0 Subset`的话，你需要将上述说明中的`smcs.rsp`文件名改成`gmcs.rsp`。
>- 如果你使用的是`Unity5.5 `以上的版本，你需要将上述说明中的`smcs.rsp`文件名改成`mcs.rsp`

#### **从Visual Studio开始**

如果你希望在VisualStudio的C#项目中使用ILRuntime， 你只需要引用编译好的`ILRuntime.dll`，`ILRuntim.Mono.Cecil.dll`以及`ILRuntime.Mono.Cecil.Pdb.dll`即可。

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
        Debug.LogError(www.error);
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
        Debug.LogError(www.error);
    byte[] pdb = www.bytes;
    System.IO.MemoryStream fs = new MemoryStream(dll);
    System.IO.MemoryStream p = new MemoryStream(pdb);
    appdomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());    
    
    OnILRuntimeInitialized();
}

void OnILRuntimeInitialized()
{
    appdomain.Invoke("Hotfix.Game", "Initialize", null, null);
}
```

这个例子为了演示方便，直接从StreamingAssets目录里读取了脚本DLL文件以及调试符号PDB文件， 实际发布的时候，如果要热更，肯定是将DLL和PDB文件打包到Assetbundle中进行动态加载的，这个不是ILRuntime的范畴，故不具体演示了。


### 调试插件

ILRuntime提供了一个支持Visual Studio 2015、Visual Studio 2017和Visual Studio 2019的调试插件，用来源码级调试你的热更脚本。

你可以在[这里](https://github.com/Ourpalm/ILRuntime/releases)下载到最新的Visual Studio调试插件。

**使用方法如下：**

- 安装ILRuntime调试插件，并重新启动VS2015或VS2017、VS2019
- 运行Unity工程，并保证执行过appdomain.DebugService.StartDebugService(56000);来启动调试服务器,一定要确保dll和pdb都加载完毕再调用此接口。
- 用VisualStudio打开热更DLL项目
- 点击菜单中的Debug->Attach to ILRuntime按钮。注意,不是“附加Unity调试程序”
- 在弹出来的窗口中填入被调试的主机的IP地址以及调试服务器的端口
- 点击Attach按钮后，即可像UnityVS一样下断点调试

**注意事项：**

- 如果使用VS2015的话需要`Visual Studio 2015 Update3`以上版本
