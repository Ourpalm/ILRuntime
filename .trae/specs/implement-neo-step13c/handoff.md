# ILRuntime Neo Step 13c — Handoff

## 交付概述

Step 13c 把 Neo 模式下的三个 CLR ↔ IL 外部反射入口(`InvocationContext.Invoke`、`DelegateAdapter.ILInvokeSub`、`CLRRedirections.MethodInfoInvoke`)从 `#if ENABLE_NEO_MODE` 分支下的 fail-fast NotImpl 迁移到走 `InvocationFrame`,并把 `InvocationContext` 本身在 Neo 分支下改造为 **`ref struct`**,消除 `StackObject* esp` 语义。同时补齐了 `InvocationFrame.ReadGenericPrimitive<T>` 的读向 zero-boxing 通道,以及新增 `PushReference(int paramIndex)` 用于外部 CLR 侧调用 ref/out 参数的 IL 方法。

- **Neo 测试**:NeoStep13c 新增 7/7 通过;NeoStep6-13c 61 测试 59 通过,2 既有失败(1 Step 18 挂账 + 1 baseline TestCLRBinding 失败,均已用 stash 验证 baseline 同错)
- **`-c Debug` / `-c Debug_Neo` 双配置 0 错误**
- **3 处 Step 12 handoff §7 挂账的 NotImpl 全部闭环**
- **`ReadGenericPrimitive<T>` 的 "reserved for Step 13b" 挂账清除**

---

## 1. InvocationContext ref struct 化(Neo)

[InvocationContext.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/InvocationContext.cs) 用 `#if ENABLE_NEO_MODE / #else` 一分为二:

- **Legacy 分支**:保持原有 `public unsafe struct InvocationContext : IDisposable`,字段与行为 100% 不变
- **Neo 分支**:改为 `public unsafe ref struct InvocationContext`(**不实现 IDisposable**,靠 C# 8+ pattern-based Dispose)。字段收敛为:
  - `ILIntepreter.InvocationFrame frame` — 内嵌 InvocationFrame(ref struct 可以作 ref struct 字段)
  - `AppDomain domain`, `ILIntepreter intp`, `ILMethod method`, `int paramCnt`, `bool invocated`, `bool hasReturn`

**关键决策**:ref struct 强制短生命周期(不能存字段、不能跨 await、不能被 lambda 捕获),恰好匹配 InvocationContext 的语义——BeginInvoke 借用 interpreter,Push*/Invoke/Read*/Dispose 都必须在同一栈帧完成。C# 语法 `using (var ctx = app.BeginInvoke(m)) { ... }` **保持不变**。

**API 表面完全兼容**:所有公共 `Push*` / `Read*` / `Invoke` / `Dispose` 签名不变,只是内部一行转发到 `frame.PushXxx` / `frame.ReadXxx`。零 boxing enum 路径保留(`PushInteger<T>` 走 `PrimitiveConverter<T>.CheckAndInvokeToInteger`)。

**PushValueTypeSub<T> / ReadValueTypeSub<T> 的兼容性**:这两个 static helper 在 Neo 分支下作 stub 保留(throw `NotSupportedException`),因为 `ILIntepreter.PushObject<T>` / `RetrieveObject<T>`(Legacy StackObject API,Neo 不会调用)在 assembly 层面仍引用它们。

## 2. InvocationFrame 扩展

[ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs):

### 2.1 PushReference(int paramIndex) 新增

写入 8 字节 Ref Slot `(objectIndex=-1, offset=ParamInfos[paramIndex].Offset)` 到下一个参数槽,断言:
- `paramIndex < nextParamIdx`(引用的必须是已 Push 的参数)
- 目标 slot `Size == 8 && RefCount == 0`(Ref Slot 布局,Step 12b 由 JIT 分配)

复用 Step 12b 的 Ldflda / Stind_* / Ldind_* Ref Slot 分派,不新增指令,不新增 slot 类型。callee 通过 `Stind_I4` 等对 caller 参数的写回,caller 在 `ctx.ReadInteger(0)` 处观察到最新值。

### 2.2 ReadGenericPrimitive<T> zero-boxing 落地

- primitive/enum T 有 `PrimitiveConverter<T>` 注册 → 直接 `*(int*)retDst` + delegate invoke,零 boxing
- IntPtr/UIntPtr → 特化路径
- enum 没有 converter → fallback via `ReadNeoPrimitive` boxing 再 `(T)` cast(与 Legacy 一致)
- reference-type T → `NotSupportedException("... use ReadObject<T> instead")`

### 2.3 Index-form Read API 新增

`ReadInt32(int paramIndex)`, `ReadInt64`, `ReadSingle`, `ReadDouble`, `ReadObject(int paramIndex)`, 以及泛型版 `ReadInt32<T>(int paramIndex)` 等。这些是给 caller 观察 ref/out writeback 用的:调用完 `Invoke` 后,`ctx.ReadInteger(0)` 读回 param slot 的当前值。

### 2.4 `GenericStubMsg` 常量删除

原有 "reserved for Step 13b" 文案挂账清除。

## 3. AppDomain.BeginInvoke 分派

[AppDomain.cs L1703-L1716](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs#L1703-L1716):

```csharp
#if ENABLE_NEO_MODE
    return new InvocationContext(this, inteptreter, (ILMethod)m);
#else
    return new InvocationContext(inteptreter, (ILMethod)m);
#endif
```

## 4. DelegateAdapter.ILInvokeSub Neo 落地

[DelegateAdapter.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/DelegateAdapter.cs):

- **`BeginInvoke()`**(L925-933): Neo 分支下**不再**做 `*ctx.ESP = default; ctx.ESP++`(Legacy 用于预留 delegate 自身的 return slot,Neo InvocationFrame 已自带 retDst)
- **新增 `NeoInvoke(ref InvocationContext ctx)`** 和 **`NeoInvokeAndRead<TResult>(ref InvocationContext ctx, InvocationTypes retType)`** 两个 helper:
  - `InsertThisAndInvoke(ref ctx)` — 调用 `ctx.Invoke()`
  - 多播(`next != null`)迭代:每个 delegate 一个独立的 `InvocationContext` + `InvocationFrame`,通过 `ctx.ReadResult<object>(i)` 从 head frame 读回参数,再 `PushObject` 到新 frame。只有最后一个 delegate 的返回值被保留
- **`ILInvokeSub`** Neo 分支改为 `throw NotSupportedException`(通过 `NeoInvoke` 直连,不再走 StackObject 路径)
- **12 处 `InvokeILMethod*` 变体**每处加 `#if ENABLE_NEO_MODE / #else`:
  - Neo 分支:`if (method.HasThis && instance != null) ctx.PushObject(instance);` 补 this,然后 push 参数,最后 `return NeoInvokeAndRead<TResult>(ref ctx, pTypes[N])`(Function)或 `NeoInvoke(ref ctx)`(Action)
  - 12 处 `using (var ctx = BeginInvoke()) { ... }` 全部改为 `var ctx = BeginInvoke(); try { ... } finally { ctx.Dispose(); }`,因为 C# 禁止把 `using` 变量作 `ref` 传出(CS1657)

## 5. CLRRedirections.MethodInfoInvoke Neo 落地

[CLRRedirections.cs L896-L987](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/CLRRedirections.cs#L896-L987):

Neo 分支下:
1. 沿用 Legacy 的 pop 逻辑读 `p[] / obj / instance`(StackObject 侧的 caller 数据仍需按原方式读取,这不是 Neo 侧的问题)
2. 若 `instance is ILRuntimeMethodInfo`:
   - `var ctx = domain.BeginInvoke(ilmethod); try { ... } finally { ctx.Dispose(); }`
   - `if (ilmethod.HasThis) ctx.PushObject(obj);`
   - `foreach (arr[i]) ctx.PushObject(CheckCrossBindingAdapter(arr[i]));`
   - `ctx.Invoke();` 后 `ctx.ReadObject(typeof(object))` 取回 boxed 返回值
3. 送回 caller esp:`ILIntepreter.PushObject(ret, mStack, retObj, true)` 或 `PushNull`

## 6. 测试

新增 [TestCases/NeoStep13cTest.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep13cTest.cs) 7 个用例,全绿:

- `NeoStep13cInvocationContextPrimitive` — int return baseline
- `NeoStep13cInvocationContextVoid` — void return
- `NeoStep13cInvocationContextEnumReturn` — IL enum boxed underlying
- `NeoStep13cInvocationContextCLRValueTypeReturn` — CLR value type Inline (TestVector3)
- `NeoStep13cInvocationContextLongReturn` — long 8B slot
- `NeoStep13cInvocationContextFloatReturn` — float 4B slot
- `NeoStep13cInvocationContextDoubleReturn` — double 8B slot

**未加入本 step 的用例(挂账)**:

| 用例 | 阻塞依赖 |
|---|---|
| Delegate single-cast (`Func<int,int>` bound to IL method) | 需要 IL-side `ldftn` (**Step 19**) |
| Delegate multi-cast | 同上 (**Step 19**) |
| MethodInfo.Invoke → IL method | 需要 IL-side `ldtoken` / `GetMethod` |
| PushReference / ref-out 参数 | 需要 CLR-side test host(CLI 只跑 IL 静态方法) |

这些用例的**运行时路径**已在本 step 实现,单元覆盖等后续 step 完成对应 IL opcode / 引入 CLR-side test host 后回补即可。

## 7. Step 12 handoff §7 审计

3 处 "Step 13" tag NotImpl 全部闭环:

| 位置 | 状态 |
|---|---|
| [InvocationContext.cs Invoke](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/InvocationContext.cs) Neo 分支 | ✅ 替换为 `frame.Execute` |
| [DelegateAdapter.cs ILInvokeSub](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/DelegateAdapter.cs) Neo 分支 | ✅ 走 `NeoInvoke` helper 直连 InvocationFrame |
| [CLRRedirections.cs MethodInfoInvoke](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/CLRRedirections.cs) Neo 分支 | ✅ 走 `AppDomain.BeginInvoke` |

`rg "Step 13" ILRuntime/` 剩余 8 处引用**全部归后续 step**:
- 4 处 [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) 标注 "Step 13/17" → 归 **Step 17**(CLR Ref Slot receiver Initobj/Ldfld_Value/Stfld_Value/field access)
- 2 处 [BindingGeneratorExtensions.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/CLRBinding/BindingGeneratorExtensions.cs) → 归 **Step 15**(CLRBinding 代码生成器改造)
- 1 处 [CLRMethod.cs L364](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/CLRMethod.cs#L364) → 归 **Step 15**(反射 fallback)
- 1 处 [Optimizer.Neo.cs L319](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L319) → 仅是历史注释

## 8. 挂账(转移到下游 step)

| 挂账项 | 目标 step |
|---|---|
| Delegate `ldftn`/`ldvirtftn` + `DelegateAdapter.ILInvoke` StackObject 侧改造 | **Step 19** |
| IL-side `ldtoken` / `MethodBase.GetMethod` 实现 | **Step 19**(委托相关) |
| PushReference / ref-out 参数的 IL-side 端到端测试 | CLR-side test host(测试基础设施改造) |
| NeoStep13cTest 的 Delegate / MethodInfo.Invoke / ref 用例回补 | Step 19 完成后 |

## 9. 关键代码变更清单

| 文件 | 关键变更 |
|---|---|
| [InvocationContext.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/InvocationContext.cs) | Neo 分支重写为 `ref struct`,内嵌 `InvocationFrame` 字段;Legacy 结构不变;共享 helper 提取到 `InvocationContextShared` |
| [AppDomain.cs L1703-L1716](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs#L1703-L1716) | `BeginInvoke` `#if` 分派 |
| [DelegateAdapter.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/DelegateAdapter.cs) | 新增 `NeoInvoke` / `NeoInvokeAndRead`;12 处 `InvokeILMethod*` 变体加 `#if` 拆分;`ILInvokeSub` Neo 分支不再抛 NotImpl |
| [CLRRedirections.cs L896-L987](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/CLRRedirections.cs#L896-L987) | `MethodInfoInvoke` Neo 分支落地 |
| [ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs) | 新增 `PushReference` / index-form Read API;`ReadGenericPrimitive<T>` 走 `PrimitiveConverter<T>`;删除 `GenericStubMsg` |
| [TestCases/NeoStep13cTest.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep13cTest.cs) | 新增 7 个 InvocationContext.Invoke 覆盖测试 |

## 10. 交给下一位的三行结论

Step 13c 完成 CLR ↔ IL 外部入口统一到 `InvocationFrame`:`InvocationContext` 变为 `ref struct` 内嵌 `InvocationFrame`,`DelegateAdapter.InvokeILMethod` 通过 `NeoInvoke` / `NeoInvokeAndRead` 走 zero-boxing 直连 frame,`CLRRedirections.MethodInfoInvoke` 通过 `AppDomain.BeginInvoke` 复用同一路径。`PushReference` 正式定义为 Step 12b Ref Slot `(-1, offset)` emit。Delegate 和 MethodInfo 的**端到端 IL 侧测试**依赖 Step 19 的 `ldftn` / `ldtoken`,已挂账。Neo/Legacy 双配置 0 错误,NeoStep6-13c 无本 step 引入的回归。
