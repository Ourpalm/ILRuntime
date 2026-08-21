# Step 13c Tasks — CLR ↔ IL 外部入口迁移到 InvocationFrame

## 工程约束

- 所有变更严格限制在 `#if ENABLE_NEO_MODE` 分支;Legacy 分支零改动,通过 `#if / #else` 二选一编译。
- Neo 分支下 `InvocationContext` 声明为 `ref struct`,内部持有 `InvocationFrame frame` 字段;不允许并存 `StackObject* esp` 与 `frame` 两份状态。
- `PrimitiveConverter<T>` API 面不改;仅补充消费点(读向)。
- `PushReference(int index)` 复用 Step 12b Ref Slot `(-1, offset)` 编码,不新增指令、不新增 slot 类型;JIT 已按 8 字节分配的参数 slot 直接消费。
- 多播委托一 delegate 一 frame,不共享 `frameBase`;`Dispose` 时释放对应帧的 mStack ref region。
- 严禁在 `Push*` / `Read*` hot path 引入反射查表或字段名哈希;所有 offset 从 `method.CompiledFrame.ParamInfos[index].Offset` 直接读取。

## Task Dependencies

```
Task 1 (审计 + Ref Slot handler 单测准备)
  └─→ Task 2 (InvocationFrame.PushReference + ReadGenericPrimitive<T> zero-boxing)
       └─→ Task 3 (InvocationContext Neo 分支重写为 ref struct)
            └─→ Task 4 (DelegateAdapter.ILInvokeSub Neo 分支)
                 └─→ Task 5 (CLRRedirections.MethodInfoInvoke Neo 分支)
                      └─→ Task 6 (NeoStep13cTest 全套用例)
                           └─→ Task 7 (双配置编译 + 全量回归 + handoff)
```

## Tasks

- [x] Task 1: 审计与准备
  - [x] `rg "Step 13" ILRuntime/` 全量扫描,列出所有挂 "Step 13" tag 的 NotImpl,标注哪些归本 step、哪些明确迁移到后续 step
  - [x] 阅读 [InvocationContext.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/InvocationContext.cs) 全文,列出所有需要在 Neo 分支重写的公共成员及其 Legacy 语义
  - [x] 阅读 [AppDomain.BeginInvoke](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs) 定位构造 InvocationContext 的位置,确认 Neo 分支的 `#if` 切换点
  - [x] 确认 `method.CompiledFrame.ParamInfos[]` 已为 ref/out 参数分配 8 字节 Ref Slot(Step 12b 已完成),记录关键 API 位置

- [x] Task 2: InvocationFrame 侧扩展
  - [x] 在 [ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs) 新增 `public void PushReference(int paramIndex)`:emit `(-1, ParamInfos[paramIndex].Offset)` 到 `frameBase + ParamInfos[nextParamIdx].Offset`,递增 `nextParamIdx`
  - [x] 断言 `ParamInfos[nextParamIdx].Size == 8` 且 `RefCount == 0`(Ref Slot 布局);断言 `paramIndex < nextParamIdx`(引用的必须是已 Push 的参数)
  - [x] 修改 `ReadGenericPrimitive<T>`:通过 `InvocationContext.GetInvocationType<T>()` 分派到 `PrimitiveConverter<T>.CheckAndInvokeFromInteger/Long/Float/Double`,实现读向 zero-boxing;reference-type T 分支明确抛 `NotSupportedException` 并附文案 "use ReadObject<T> instead"
  - [x] 移除 `GenericStubMsg` 中 "reserved for Step 13b" 文案(该常量若无其他引用可整体删除)
  - [x] 添加 index-form `ReadInt32(int paramIndex)` / `ReadInt64` / `ReadSingle` / `ReadDouble` / `ReadObject(int paramIndex)` 用于读回 ref/out 参数的 caller 观察值(读 `frameBase + ParamInfos[paramIndex].Offset`)

- [x] Task 3: InvocationContext Neo 分支重写
  - [x] 在 [InvocationContext.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/InvocationContext.cs) 用 `#if ENABLE_NEO_MODE` / `#else` 把整个 `struct InvocationContext` 一分为二;Neo 分支声明为 `public ref struct InvocationContext`,包含 `InvocationFrame frame` 及必要的元数据字段(domain / intp / method / paramCnt / invocated / hasReturn)
  - [x] 构造函数改为 `internal InvocationContext(AppDomain domain, ILIntepreter intp, ILMethod method)`:内部调用 `InvocationFrame.Begin(intp, method)`
  - [x] 所有 `Push*` 系列一行转发:`PushInteger(int)` → `frame.PushInt32(value)`,`PushInteger<T>(T)` → `frame.PushInt32<T>(value)`,依此类推;`PushObject` → `frame.PushObject`;`PushValueType<T>` → 通过 `PushObject` 或专门的 `frame.PushObject` value-type 分支(需读 InvocationFrame.WriteObjectInternal 的 valuetype 处理确保覆盖)
  - [x] `PushReference(int index)` → `frame.PushReference(index)`
  - [x] `Invoke()` → `frame.Execute(out var unhandledException)`;若 `unhandledException != null` throw
  - [x] `ReadResult<T>()` → 按 `GetInvocationType<T>()` 分派到 `frame.ReadInt32<T> / ReadInt64<T> / ReadSingle<T> / ReadDouble<T> / ReadObject`(value type / reference type 用 `ReadObject` 再 `(T)` 或走 binder)
  - [x] `ReadInteger[/<T>](int idx=0)` / `ReadLong` / `ReadFloat` / `ReadDouble` / `ReadBool` / `ReadObject<T>` / `ReadValueType<T>`:`idx == 0 && hasReturn` 走返回值路径,否则读 `frame.ReadXxx(paramIndex: idx)` 参数位(用于 ref/out writeback)
  - [x] `Dispose()` → `frame.Dispose(); domain.FreeILIntepreter(intp);`
  - [x] [AppDomain.cs BeginInvoke](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs) 的 Neo 分支返回 `new InvocationContext(this, intp, method)`

- [x] Task 4: DelegateAdapter.ILInvokeSub Neo 落地
  - [x] [DelegateAdapter.cs L942-L946](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/DelegateAdapter.cs#L942-L946) 移除 `#if ENABLE_NEO_MODE` NotImpl,替换为 Neo 实现
  - [x] Neo 分支签名保持 `(ILIntepreter intp, StackObject* ebp, AutoList mStack)`,但内部实现改为:
    - 从 `ebp` 前序参数区 `Minus(ebp, i)` `StackObject.ToObject` 出所有 `object` 形态参数(delegate wrapper 已把 CLR 参数转成 `object`)
    - `using (var ctx = appdomain.BeginInvoke(method))` 构造 frame
    - `HasThis` → `ctx.PushObject(instance)`;`IsExtend && instance != null` 额外 push
    - 逐个 `ctx.PushObject(arg[i])`
    - `ctx.Invoke()`
    - `hasReturn` → `ctx.ReadResult<object>()` boxed → 通过 `intp.PushObject(esp, mStack, ret, isBox:true)` 送回 caller
  - [x] 多播:`next != null` → 递归改成 `while (next != null) { ... using new ctx ... ; next = next.Next; }`;只保留最后一个 delegate 的返回值送回 caller
  - [x] 保留 `IsExtend` 额外 push 语义

- [x] Task 5: CLRRedirections.MethodInfoInvoke Neo 落地
  - [x] [CLRRedirections.cs L898-L901](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/CLRRedirections.cs#L898-L901) 移除 NotImpl,替换为 Neo 实现
  - [x] Neo 分支从 `esp` pop `p[]` / `obj` / `instance`(逻辑与 Legacy 相同,只是 pop 的方式仍走 `StackObject.ToObject`;`intp.Free` 后进入统一分派)
  - [x] 若 `instance is ILRuntimeMethodInfo`:`var ilMethod = ((ILRuntimeMethodInfo)instance).ILMethod`;`using (var ctx = appdomain.BeginInvoke(ilMethod))`;`HasThis` push `obj`;循环 `PushObject(p[i])`;`ctx.Invoke()`;`hasReturn` → `intp.PushObject(esp, mStack, ret, isBox:true)`,否则 `PushNull`
  - [x] 非 IL target 直接调 `((MethodInfo)instance).Invoke(obj, p)`,原样 `PushObject` 送回

- [x] Task 6: NeoStep13cTest 定向测试
  - [x] `NeoStep13cInvocationContextPrimitive`:`BeginInvoke → PushInteger → Invoke → ReadResult<int>`
  - [x] `NeoStep13cInvocationContextVoid`:void return
  - [x] `NeoStep13cInvocationContextEnumReturn`:IL enum boxed underlying (Legacy 等价)
  - [x] `NeoStep13cInvocationContextCLRValueTypeReturn`:CLR Inline TestVector3
  - [x] `NeoStep13cInvocationContextLongReturn` / `FloatReturn` / `DoubleReturn`:各类型 return
  - [挂账] Delegate/MethodInfo.Invoke/PushReference 端到端测试依赖 Step 19 `ldftn`/`ldtoken` 或 CLR-side test host

- [x] Task 7: 编译、测试与收尾
  - [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 通过 0 错误
  - [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 通过 0 错误
  - [x] TestCases 构建通过
  - [x] NeoStep13c 全套 7/7 通过
  - [x] NeoStep6-13c 回归 59/61 通过(2 既有失败与本 step 无关,已用 stash 验证 baseline 同错)
  - [x] Legacy load 阶段有既有 STTask 问题(baseline 同错)
  - [x] `rg "Step 13"` 剩余项全部有明确归属,无隐藏尾巴
  - [x] 更新 `checklist.md`,`handoff.md`
