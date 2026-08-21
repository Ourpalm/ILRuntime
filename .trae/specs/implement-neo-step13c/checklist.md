# Step 13c Checklist — CLR ↔ IL 外部入口迁移到 InvocationFrame

## API 与结构

- [x] Neo 分支下 `InvocationContext` 声明为 `ref struct`,不再持有 `StackObject* esp`
- [x] Legacy 分支 `InvocationContext` 保持 `struct`,行为 100% 不变
- [x] 所有公有 API 签名(Push/Read/Invoke/Dispose)在 Neo 与 Legacy 分支保持一致,调用方零改动
- [x] `InvocationContext` Neo 分支内部持有单一 `InvocationFrame frame`,不存在双份状态
- [x] `AppDomain.BeginInvoke` Neo 分支返回新构造的 ref struct,`Dispose` 时释放 frame + 归还 interpreter

## InvocationFrame 扩展

- [x] 新增 `InvocationFrame.PushReference(int paramIndex)`,写入 8 字节 Ref Slot `(-1, ParamInfos[paramIndex].Offset)`
- [x] `PushReference` 断言目标 slot 为 8 字节 Ref Slot 布局
- [x] `PushReference` 断言 `paramIndex < nextParamIdx`
- [x] `PushReference` 递增 `nextParamIdx`
- [x] `ReadGenericPrimitive<T>` 通过 `PrimitiveConverter<T>.CheckAndInvokeFromInteger/Long/Float/Double` 消除 boxing
- [x] `ReadGenericPrimitive<T>` reference-type T 分支抛 `NotSupportedException` 并附文案指向 `ReadObject<T>`
- [x] `GenericStubMsg` "reserved for Step 13b" 常量与所有相关 NotImpl 全部移除
- [x] 新增 index-form `ReadInt32(int paramIndex)` 等,支持 ref/out 参数回读

## Push 转发

- [x] `PushInteger(int)` / `PushInteger(long)` / `PushFloat` / `PushDouble` / `PushBool` 全部一行转发到 `frame.PushXxx`
- [x] `PushInteger<T>` / `PushLong<T>` / `PushFloat<T>` / `PushDouble<T>` 保留 `PrimitiveConverter<T>` zero-boxing 通道
- [x] `PushObject` / `PushObject<T>` / `PushValueType<T>` / `PushParameter<T>` 走 `frame.PushObject` 或 value-type 分支
- [x] `PushReference(int index)` 转发 `frame.PushReference`
- [x] `PushNull` (若存在) 保持行为

## Read 转发

- [x] `ReadResult<T>` 根据 `GetInvocationType<T>()` 分派到 `frame.ReadInt32<T> / ReadInt64<T> / ReadSingle<T> / ReadDouble<T> / ReadObject`
- [x] `ReadInteger[/<T>](int idx=0)` / `ReadLong` / `ReadFloat` / `ReadDouble` / `ReadBool`:`idx==0 && hasReturn` 走返回值,否则读参数 offset
- [x] `ReadObject<T>` / `ReadValueType<T>` 保持 CheckCLRTypes / binder ParseValue 兼容语义
- [x] IL enum 返回值 `ReadResult<TEnum>` 结果与 Legacy 完全一致(boxed underlying primitive)

## DelegateAdapter.ILInvokeSub

- [x] Neo 分支 fail-fast NotImpl 已移除(以 `NotSupportedException` 说明不会走到)
- [x] `HasThis` / `IsExtend` 语义保留
- [x] `object[] args` 逐个通过 `ctx.PushObject` 送入,零直接 `StackObject*` 操作
- [x] 多播 `next` 链改为迭代;每个 delegate 独立 frame;不共享 `frameBase`
- [x] 单播:返回值通过 `NeoInvokeAndRead<TResult>` 直接读回 zero-boxing
- [x] 多播:只保留最后一个 delegate 的返回值送回 caller
- [x] 无 return 的 `Action<...>` 委托通过 `NeoInvoke(ref ctx)` 不引发 read
- [x] `ClearStack` Legacy 分支保留;Neo 分支不再需要(frame Dispose 处理)

## CLRRedirections.MethodInfoInvoke

- [x] Neo 分支 fail-fast NotImpl 已移除
- [x] `p[] / obj / instance` 三次 pop 与 free 语义与 Legacy 一致
- [x] `instance is ILRuntimeMethodInfo` 分支走 InvocationFrame
- [x] 非 IL target 保持原 CLR `MethodInfo.Invoke` 反射,行为不变
- [x] 返回值走 `intp.PushObject(esp, mStack, ret, isBox:true)`;`hasReturn == false` 走 `PushNull`

## 审计

- [x] `rg "Step 13"` 剩余项全部有明确归属(Step 15 CLRBinding + Step 17 CLR Ref Slot + 注释)
- [x] Step 12 handoff §7 挂 "Step 13" tag 的 3 处 NotImpl 全部闭环
- [x] 无隐藏 `throw new NotImplementedException` 未标注 step

## 测试

- [x] `NeoStep13cInvocationContextPrimitive` 通过 (baseline int return)
- [x] `NeoStep13cInvocationContextVoid` 通过 (void return)
- [x] `NeoStep13cInvocationContextEnumReturn` 通过 (enum boxed underlying)
- [x] `NeoStep13cInvocationContextCLRValueTypeReturn` 通过 (CLR value type Inline return)
- [x] `NeoStep13cInvocationContextLongReturn` 通过 (long return)
- [x] `NeoStep13cInvocationContextFloatReturn` 通过 (float return)
- [x] `NeoStep13cInvocationContextDoubleReturn` 通过 (double return)
- [已挂账] Delegate / MethodInfo.Invoke / PushReference 直接测试需要 IL-side `ldftn`/`ldtoken` 或 CLR-side test host;等 Step 19 完成后可回补

## 编译与回归

- [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误
- [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误
- [x] NeoStep6-13c 全套 61 个测试:59 pass,2 既有失败(1 Step 18 挂账 + 1 baseline TestCLRBinding 失败,均与本 step 无关,已用 stash 验证 baseline 同错)
- [x] Legacy 测试环境有既有 `STTask` load 问题(baseline 同错),不归因本 step
