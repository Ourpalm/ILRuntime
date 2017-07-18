---
title: LitJson集成
type: guide
order: 160
---

## LitJson集成

Json序列化是开发中非常经常需要用到的功能，考虑到其通用性，因此ILRuntime对LitJson这个序列化库进行了集成

### 初始化

在使用LitJson前，需要对LitJson进行注册，注册方法很简单，只需要在ILRuntime初始化阶段，在注册CLR绑定之前，执行下面这行代码即可：

```csharp
    LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
```

### 使用

LitJson的使用非常简单，将一个对象转换成json字符串，只需要下面这行代码即可

```csharp
    string json = JsonMapper.ToJson(obj);
```

将json字符串反序列化成对象也同样只需要一行代码

```csharp
    JsonTestClass obj = JsonMapper.ToObject<JsonTestClass>(json);
```

其他具体使用方法请参考LitJson库的文档即可