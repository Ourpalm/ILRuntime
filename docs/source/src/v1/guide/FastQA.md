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

- Unity2018.3以上需要用最新ilruntime。
- V1.4已过时，示例工程内的版本已过时，当前最新版本为[master]分支。

## 打包

### 用mono打包的时候，ILRuntime相关的功能都正常，改用IL2Cpp之后，注册的所有委托都不执行了？

- 注册委托只能写在主工程里，不能写在热更工程里。
- 检查是否做了[CLR自动分析绑定]。

### ExecutionEngineException: Attempting to call method 'Scene::GetModule' for which no ahead of time (AOT) code was generated.电脑运行没问题，安卓上报这个错 

- 检查是否做了[CLR自动分析绑定]。
- CLRBindings.Initialize(appdomain); 要调用这一句，放在最后.是否漏了这句。
  



## 疑难杂症


