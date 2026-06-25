---
name: "ilruntime-debug"
description: "Debug ILRuntime ILIntepreter execution errors. Invoke whenever ILIntepreter internal errors occur and root cause is unclear - always check stack state first before diving into interpreter internals."
---

# ILRuntime ILIntepreter 调试指南

## 适用场景

当遇到任何 ILIntepreter 内部错误且无法立即判断原因时，优先使用此技能：

### 必用场景
- `StackOverflowException`（解释器栈损坏，非 C# 原生栈溢出）
- `ArgumentOutOfRangeException` 访问 `mStack` 时越界
- 任何 `ILRuntime.Runtime.Intepreter.ILIntepreter` 栈跟踪中的异常

### 常用场景
- AOP 注入代码执行后返回值读取失败
- 泛型类型/方法注入后行为异常
- 方法调用参数传递错误
- 值类型参数/返回值处理异常
- 栈不平衡导致的各种诡异行为

### 使用原则
1. **优先看栈**：遇到 ILIntepreter 相关错误，第一反应是添加 `DumpStack` 日志查看栈状态
2. **后看源码**：只有在栈信息无法解释问题时，才深入分析 ILIntepreter 和 RuntimeStack 等内部代码
3. **防御性检查**：在关键位置添加边界检查，帮助快速定位问题

---

## 调试流程

### 步骤1：添加栈状态日志

在 `ILRuntime/Runtime/Intepreter/ILIntepreter.cs` 第 282 行（`code = ip->Code;` 之后）添加：

```csharp
System.Console.WriteLine(AppDomain.DebugService.DumpStack(esp, stack).ToString());
```

**注意**：这会在每个 IL 指令执行前打印完整栈状态，日志量极大。建议：
1. 通过测试框架只运行单个测试用例
2. 观察崩溃前的最后几帧栈状态

### 步骤2：分析 DumpStack 输出

输出格式示例：
```
(0x1F4839A0C50) Type:Object Value:0 ValueLow:0 Text:HotfixAOT.GenericStruct`1[System.Int32]
(0x1F4839A0C5C) Type:Integer Value:42 ValueLow:0 Text:42
->(0x1F4839A0C68) Type:Null Value:-1 ValueLow:0 Text:null |Base Method:...
Managed Objects:
(0)HotfixAOT.GenericStruct`1[System.Int32]
```

关注要点：
- `->` 标记当前 ESP 位置
- `Type:` 字段的类型是否符合预期（如 `Integer` vs `Object`）
- `Value:` 字段的值是否越界（特别是 `Object` 类型指向 `mStack` 的索引）
- `Managed Objects:` 列表长度是否与预期一致

### 步骤3：定位问题类型

#### 问题类型A：mStack 被提前清空

**现象**：`StackObject.ToObject failed! ObjectType: Object, Value: 0, mStack.Count: 0`

**原因**：`InvocationContext.Invoke()` 执行完毕后 `PopFrame` 会清理 `mStack`，如果注入代码在 `Invoke()` 后尝试从 `mStack` 读取数据，会导致越界。

**解决方案**：将需要在 `Invoke()` 后读取的值（如值类型 `this`）作为"前置 slot" push 到 `InvocationContext`，类似于 ref 参数的处理方式。

#### 问题类型B：泛型方法参数获取错误

**现象**：`AppDomain.GetType(Type t)` 收到 null key 或错误的类型

**原因**：`MethodBase.GetCurrentMethod().GetGenericArguments()` 在运行时返回的是泛型参数定义（如 `T`），不是实际的泛型实参（如 `int`）。

**解决方案**：用 `ldtoken` + `Type.GetTypeFromHandle` 构建类型数组：

```csharp
// 错误：GetCurrentMethod().GetGenericArguments() 返回泛型参数定义
// 正确：通过 ldtoken + GetTypeFromHandle 获取实际泛型实参
ldc_i4 count
newarr Type
for each generic param:
    dup
    ldc_i4 i
    ldtoken T_i
    call Type.GetTypeFromHandle
    stelem_ref
```

#### 问题类型C：栈不平衡

**现象**：`StackOverflowException` 或 ESP 指向错误的位置

**原因**：注入的 IL 代码 push/pop 的数量与预期不符，或 `PushReference` 指向的 index 不正确。

**解决方案**：检查 `InvocationContext` 的参数 push 顺序和数量是否与 `method.ParameterCount` 匹配。

---

## 关键代码位置

| 文件 | 行号 | 用途 |
|------|------|------|
| `ILRuntime/Runtime/Intepreter/ILIntepreter.cs` | 282 | 添加 DumpStack 日志 |
| `ILRuntime/Runtime/Stack/RuntimeStack.cs` | 112-172 | `PopFrame` 栈清理逻辑 |
| `ILRuntime/Runtime/Enviorment/InvocationContext.cs` | 448-462 | `Invoke()` 执行入口 |
| `ILRuntime/HybridPatch/AssemblyInjector.cs` | 1229-1413 | AOP 注入代码生成 |

---

## 调试技巧

### 技巧1：限制单测试用例

使用测试框架参数运行单个测试：
```bash
dotnet run --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj -- TestCases.dll HotfixAOT.patch false HotfixTestGenericTestCases.Test03
```

### 技巧2：观察崩溃前的栈变化

重点关注：
1. ESP 位置的变化
2. `Managed Objects` 数量的变化
3. 返回值所在位置的类型和值

### 技巧3：防御性编程

在关键位置添加边界检查：
```csharp
// StackObject.ToObject 中的检查
if (esp->Value < 0 || esp->Value >= mStack.Count)
    throw new Exception($"StackObject.ToObject failed! ObjectType: {esp->ObjectType}, Value: {esp->Value}, mStack.Count: {mStack.Count}");
```

---

## 常见修复模式

### 模式1：值类型 this 回写

对于值类型实例方法，需要将 `this` 像 ref 参数一样处理：

```csharp
// 在 refIdxCur 初始化时分配 slot
int thisRefIdx = -1;
if (method.DeclaringType.IsValueType && !method.IsStatic)
{
    thisRefIdx = refIdxCur++;
}

// 在 ref 参数 push 阶段先 push this 值类型
if (thisRefIdx >= 0)
{
    processor.AppendInstruction(first, processor.Create(OpCodes.Ldloca, invokeCtx));
    processor.AppendInstruction(first, processor.Create(OpCodes.Ldarg_0));
    processor.AppendInstruction(first, processor.Create(OpCodes.Ldobj, vtType));
    processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.GetPushParameterMethod(vtType)));
}

// push this 参数时用 PushReference
if (method.DeclaringType.IsValueType)
{
    processor.AppendLdc(first, thisRefIdx);
    processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.PushReferenceMethod));
}

// 读回 this 时使用 thisRefIdx
processor.AppendLdc(first, thisRefIdx);
processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.GetReadResultByIndexMethod(vtType)));
```

### 模式2：泛型方法参数获取

```csharp
// 错误方式：GetCurrentMethod().GetGenericArguments()
// 正确方式：ldtoken + GetTypeFromHandle
var genericParams = method.GenericParameters;
processor.AppendLdc(first, genericParams.Count);
processor.AppendInstruction(first, processor.Create(OpCodes.Newarr, reflection.TypeType));
for (int i = 0; i < genericParams.Count; i++)
{
    processor.AppendInstruction(first, processor.Create(OpCodes.Dup));
    processor.AppendLdc(first, i);
    processor.AppendInstruction(first, processor.Create(OpCodes.Ldtoken, genericParams[i]));
    processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.GetTypeFromHandle));
    processor.AppendInstruction(first, processor.Create(OpCodes.Stelem_Ref));
}
processor.AppendInstruction(first, processor.Create(OpCodes.Call, reflection.ILMethodMakeGenericInstanceMethod));
```

---

## 注意事项

1. **不要修改 ILRuntime 核心代码**（除非明确确认是框架 Bug），优先检查注入代码
2. **日志清理**：调试完成后移除 `DumpStack` 日志，否则影响性能
3. **栈状态**：`PopFrame` 后 `mStack` 会被清理，不要在 `Invoke()` 后访问已清理的数据
4. **泛型实参**：运行时 `typeof(T)` 会被正确解析为实际类型，而 `GetCurrentMethod().GetGenericArguments()` 返回的是泛型参数定义
