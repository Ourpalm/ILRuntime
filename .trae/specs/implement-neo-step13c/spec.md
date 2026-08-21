# ILRuntime Neo Step 13c — CLR ↔ IL 外部入口迁移到 InvocationFrame Spec

## Why

Step 12/13 完成后,Neo 侧 `ExecuteNeo` 主执行循环已经用 `InvocationFrame` 承载 flat-bytes 参数区,但**三个外部反射入口**仍然在 `#if ENABLE_NEO_MODE` 分支下 fail-fast:

- [InvocationContext.Invoke L457-L460](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/InvocationContext.cs#L457-L460)
- [DelegateAdapter.ILInvokeSub L942-L946](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/DelegateAdapter.cs#L942-L946)
- [CLRRedirections.MethodInfoInvoke L898-L901](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/CLRRedirections.cs#L898-L901)

Legacy 版本用 `StackObject* esp` + `AutoList mStack.Add(null)` 手写封送,与 Neo 的 flat-bytes 帧不兼容。此外
[ILIntepreter.InvocationFrame.cs L254-L263](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs#L254-L263) 的 `ReadGenericPrimitive<T>` 目前经过一次 boxing (`ReadNeoPrimitive` 返回 `object` 再强转 `T`),没有接 `PrimitiveConverter<T>` 的 zero-boxing 通道;reference-T 分支还挂着 `"reserved for Step 13b"` NotImpl。

本步骤把三条入口全部改走 `InvocationFrame`,补齐读向 zero-boxing 通道,并在 Neo 分支下让 `InvocationContext` 本身变为 `ref struct` 承载 `InvocationFrame`,彻底移除 `StackObject* esp` 语义。同时把 `PushReference(int index)` 语义正式定义为 emit **Step 12b Ref Slot `(-1, ParamInfos[index].Offset)`**,复用现有 `Stind/Ldind` 回写能力。

## What Changes

- **Neo 分支下 `InvocationContext` 变 `ref struct`**,内部字段替换为 `InvocationFrame frame`;Legacy 分支保持普通 `struct`,通过 `#if ENABLE_NEO_MODE / #else` 二选一编译。
- `AppDomain.BeginInvoke(IMethod)` 的 Neo 返回路径统一构造 `InvocationContext(ref)` 并内部 `InvocationFrame.Begin`;`Dispose()` 释放 `frame` 后归还 interpreter,保持与 Legacy 一致的 API 面。
- `PushInteger` / `PushInteger<T>` / `PushLong[/<T>]` / `PushFloat[/<T>]` / `PushDouble[/<T>]` / `PushBool` / `PushObject` / `PushValueType<T>` / `PushParameter<T>` 全部一行转发到 `InvocationFrame.PushXxx`;`PrimitiveConverter<T>` zero-boxing 通道保留。
- `PushReference(int index)` 改为 emit **Ref Slot `(-1, ParamInfos[index].Offset)`** 写入下一个参数槽;callee 通过 `Stind/Ldind` 回写后 caller `Read*(index)` 读到最新值。
- `Invoke()` 调用 `frame.Execute(out unhandledException)`;`ReadResult<T>` / `ReadInteger[/<T>]` / `ReadLong[/<T>]` / `ReadFloat[/<T>]` / `ReadDouble[/<T>]` / `ReadObject<T>` / `ReadValueType<T>` 全部路由到 `frame.ReadXxx`。
- **`ReadGenericPrimitive<T>` 补齐 `PrimitiveConverter<T>.CheckAndInvokeFromInteger/Long/Float/Double`**;新增对称 `WriteGenericPrimitive<T>` helper 或让 `WriteInt32<T>` 等直接消费 `PrimitiveConverter<T>`(现有代码已是,但需检视),移除 reference-T 分支的 "reserved for Step 13b" NotImpl(实际语义:reference T 不走该 helper,由 `WriteObject`/`ReadObject` 处理,若误进则明确 `NotSupportedException` 而非"待实现")。
- `DelegateAdapter.ILInvokeSub` Neo 路径:每次调用 `AppDomain.BeginInvoke(method)` 拿 `InvocationContext`,`HasThis` → `frame.PushObject(instance)`;遍历 caller 的 `ebp` 参数区(Legacy 里用 `Minus(ebp, i)`,Neo 侧改为从 caller 的 `InvocationContext` 上下文 read 参数,或直接透传 CLR 委托的 `object[] args`)——**注意**:`ILInvokeSub` 被 `Delegate.Invoke/BeginInvoke wrappers` 调用,caller 上下文一定是 CLR 侧(有 `object[] args`),因此可以复用 `MethodInfoInvoke` 相同的 CLR→IL marshalling 逻辑。多播委托的 `next` 递归改为迭代:每个 delegate 一个独立 frame,只保留最后一个 frame 的返回值。
- `CLRRedirections.MethodInfoInvoke` Neo 路径:从 `esp` 反 pop 出 `p[] / obj / instance` 后,若 target 是 `ILRuntimeMethodInfo` → `BeginInvoke(ilMethod)` 构造 frame,`HasThis` push `obj`,遍历 `p[]` 逐个 `PushObject`,`Invoke` 后用 `PushObject(ret, isBox:true)` 把返回值送回 caller CLR 栈;非 IL target 走原 CLR `MethodInfo.Invoke` 反射。
- **审计** [Step 12 handoff §7](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12/handoff.md) 中挂 "Step 13" tag 的所有 NotImpl,确认全部被本 step 覆盖或明确迁移到后续 step,不留隐藏尾巴。

## Non-Goals

- **不实现** CLR → IL ref/out 参数的**跨帧生命周期检查**(帧内 Ref Slot 传出后 caller 帧还活着的静态断言等):归 Step 17。
- **不实现** 委托 `ldftn/ldvirtftn/DelegateManager` 侧的 Neo 特化:归 Step 19。
- **不迁移** CLRBinding 代码生成器(Unsafe.Unbox in-place 消除 WriteBackInstance):归 Step 15。
- 不改动 `PrimitiveConverter<T>` 现有 API 面(仅补充消费点,不删除也不新增字段)。
- 不改 Legacy 分支的任何 API 或行为;所有变更严格限制在 `#if ENABLE_NEO_MODE` 分支下。

## Impact

- Affected code:
  - [ILRuntime/Runtime/Enviorment/InvocationContext.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/InvocationContext.cs) — Neo 分支重写为 ref struct,所有 Push/Read 转发
  - [ILRuntime/Runtime/Enviorment/AppDomain.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs) — `BeginInvoke` 的 Neo 分支构造 ref struct
  - [ILRuntime/Runtime/Intepreter/DelegateAdapter.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/DelegateAdapter.cs) — `ILInvokeSub` Neo 分支落地
  - [ILRuntime/Runtime/Enviorment/CLRRedirections.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/CLRRedirections.cs) — `MethodInfoInvoke` Neo 分支落地
  - [ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs) — `PushReference(int)` 新增,泛型读向补齐 `PrimitiveConverter<T>`
  - [TestCases/NeoStep13cTest.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep13cTest.cs) — 新增

- Legacy 分支代码路径**零改动**:同一份源文件通过 `#if ENABLE_NEO_MODE / #else` 二选一编译。

## API Contract

### `InvocationContext` (Neo, `ref struct`)

```
ref struct InvocationContext  // Neo 分支; ref struct 不能实现 IDisposable, 通过 pattern-based Dispose 支持 using
{
    private InvocationFrame frame;
    private AppDomain domain;
    private ILIntepreter intp;
    private ILMethod method;
    private int paramCnt;
    private bool invocated;
    private bool hasReturn;

    // 与 Legacy 完全对称的公有 API:
    public void PushInteger(int);  public void PushInteger(long);
    public void PushInteger<T>(T); public void PushLong<T>(T); ...
    public void PushFloat(float);  public void PushDouble(double); public void PushBool(bool);
    public void PushObject(object);  public void PushObject<T>(T);
    public void PushValueType<T>(T); public void PushParameter<T>(T);
    public void PushReference(int index);     // ★ 新语义:emit Ref Slot (-1, ParamInfos[index].Offset)
    public void Invoke();
    public T ReadResult<T>();  public int ReadInteger(int idx = 0);  public long ReadLong(int idx = 0);
    public T ReadInteger<T>(int idx = 0); ... // zero-boxing enum↔int
    public float ReadFloat(int idx = 0); public double ReadDouble(int idx = 0); public bool ReadBool(int idx = 0);
    public T ReadValueType<T>(int idx = 0);   public T ReadObject<T>(int idx = 0);
    public void Dispose();
}
```

- **参数索引语义**:`PushXxx` 内部维护 `paramCnt` 游标,与 `InvocationFrame.nextParamIdx` 一致;`ReadXxx(int idx = 0)` 表示 **caller 视角的参数 index**(即调用后想读回第 `idx` 号参数的当前值,例如 ref/out 参数修改后);`idx == -1` 或不传保持 Legacy 语义读**返回值**(实际读 `retDst`)。

### `InvocationFrame.PushReference(int paramIndex)` (新增)

```
public void PushReference(int paramIndex)
{
    // 断言:参数 nextParamIdx 的 slot 为 8 字节 Ref Slot(在 JIT 层分配已确定)
    // 断言:paramIndex 指向已 Push 的前序参数
    var srcOffset = method.CompiledFrame.ParamInfos[paramIndex].Offset;
    var dst = frameBase + method.CompiledFrame.ParamInfos[nextParamIdx].Offset;
    *(int*)dst = -1;              // objIndex == -1 表示帧内非托管
    *(int*)(dst + 4) = srcOffset; // frame-relative offset
    nextParamIdx++;
}
```

### `ReadGenericPrimitive<T>` zero-boxing 补齐

```
private T ReadGenericPrimitive<T>()
{
    var invocationType = InvocationContext.GetInvocationType<T>();
    switch (invocationType)
    {
        case InvocationTypes.Integer: return PrimitiveConverter<T>.CheckAndInvokeFromInteger(*(int*)retDst);
        case InvocationTypes.Long:    return PrimitiveConverter<T>.CheckAndInvokeFromLong(*(long*)retDst);
        case InvocationTypes.Float:   return PrimitiveConverter<T>.CheckAndInvokeFromFloat(*(float*)retDst);
        case InvocationTypes.Double:  return PrimitiveConverter<T>.CheckAndInvokeFromDouble(*(double*)retDst);
        // Enum without registered converter → fallback boxing via ReadNeoPrimitive + (T) cast
        default: throw new NotSupportedException(...);  // reference/valuetype T 不走此路径
    }
}
```

## ADDED Requirements

### Requirement: Neo InvocationContext ref struct

Neo 分支下 `InvocationContext` SHALL be a `ref struct` containing an `InvocationFrame` field. All existing public members SHALL be preserved with unchanged signatures so caller code compiles without change under both `ENABLE_NEO_MODE` and Legacy.

#### Scenario: External caller invokes IL method with mixed primitive/enum/object parameters

- **WHEN** a CLR caller obtains `InvocationContext` via `appdomain.BeginInvoke(method)`, pushes an `int`, an `enum` value via `PushInteger<TEnum>`, an `ILTypeInstance` via `PushObject`, and a CLR value type with binder via `PushValueType<T>`
- **THEN** all four arguments land at the correct `ParamInfos[i].Offset` in `frameBase`, mStack references are seated in the callee ref region, and `Invoke` executes without throwing NotImpl

#### Scenario: Zero-boxing enum roundtrip

- **WHEN** the caller pushes and reads an `enum` value through `PushInteger<TEnum>` / `ReadInteger<TEnum>` with a registered `PrimitiveConverter<TEnum>`
- **THEN** no boxing occurs on either write or read path (verified by absence of `object` intermediate in both `WriteInt32<T>` and `ReadGenericPrimitive<T>` code paths)

### Requirement: PushReference emits Ref Slot

`InvocationContext.PushReference(int index)` in Neo mode SHALL push an 8-byte Ref Slot with `objectIndex == -1` and `offset == ParamInfos[index].Offset`, consuming exactly one parameter slot (with Ref Slot layout `size=8, refCount=0` as defined by Step 12b).

#### Scenario: External caller invokes IL method with ref parameter

- **WHEN** the caller invokes `void Increment(ref int x)` by pushing an `int` value at param 0, then `PushReference(0)` at param 1
- **THEN** the callee's `Stind_I4` on param 1 writes back to param 0's frame offset; the caller sees the updated value via `ReadInteger(0)` after `Invoke`

#### Scenario: External caller invokes IL method with out parameter

- **WHEN** the caller pushes an uninitialized `int` slot then `PushReference(0)`
- **THEN** the callee's assignment through the out parameter is observable via `ReadInteger(0)`; no NotSupportedException is thrown

### Requirement: DelegateAdapter.ILInvokeSub Neo path

`DelegateAdapter.ILInvokeSub` in Neo mode SHALL use `AppDomain.BeginInvoke(method)` to obtain an `InvocationContext`, push `this` (when `HasThis`) and each argument via the standard `Push*` API, invoke, and (for single-cast) return the result via a single boxed CLR object. Multicast SHALL iterate over the `next` chain, invoking each delegate with a fresh `InvocationContext` and returning the last delegate's result.

#### Scenario: CLR-side calls single-cast delegate to IL method

- **WHEN** a CLR-side delegate variable typed `Func<int, int>` bound to an IL method is invoked with argument `42`
- **THEN** the IL method executes and returns via the frame; the CLR caller observes the boxed return value

#### Scenario: Multicast delegate invocation

- **WHEN** two IL methods are combined into a multicast `Action<int>` and invoked
- **THEN** both methods execute in order, each with its own frame; no cross-frame parameter aliasing occurs

### Requirement: CLRRedirections.MethodInfoInvoke Neo path

`CLRRedirections.MethodInfoInvoke` in Neo mode SHALL, when the target is `ILRuntimeMethodInfo`, obtain an `InvocationContext` from `AppDomain.BeginInvoke(ilMethod)`, push `this` (when applicable) and each entry of the `object[] args` array via `PushObject`, invoke, and push the result back onto the CLR-side `esp` for the caller redirection frame. Non-IL targets SHALL continue to use raw CLR `MethodInfo.Invoke` reflection.

#### Scenario: reflection to IL method via MethodInfo.Invoke

- **WHEN** CLR code obtains `MethodInfo` for an IL method via `Type.GetMethod` and calls `.Invoke(instance, new object[] { 1, "hello" })`
- **THEN** the IL method receives both arguments in its frame and returns a boxed CLR object visible on the caller esp

#### Scenario: reflection to CLR method preserves original behavior

- **WHEN** the target method is a CLR type's method (not `ILRuntimeMethodInfo`)
- **THEN** `MethodInfoInvoke` falls back to standard CLR reflection with unchanged behavior compared to Legacy

### Requirement: Read-side generic primitive stub landed

`InvocationFrame.ReadGenericPrimitive<T>` SHALL dispatch through `PrimitiveConverter<T>.CheckAndInvokeFromInteger/Long/Float/Double` for `InvocationTypes.Integer/Long/Float/Double`, matching the write-side `WriteInt32<T>/WriteInt64<T>/WriteSingle<T>/WriteDouble<T>` symmetry. The previous `"reserved for Step 13b"` NotImplementedException SHALL be replaced by:
- primitive/enum-with-converter → zero-boxing path
- reference-type T → `NotSupportedException` with clear message ("reference-type T is not supported by generic primitive stub; use ReadObject<T> instead")

#### Scenario: enum with registered converter roundtrip

- **WHEN** `PrimitiveConverter<MyEnum>` is registered with `ToInteger`/`FromInteger` lambdas and the caller uses `ReadInteger<MyEnum>()` after an IL method returns an enum
- **THEN** no `object` boxing is observed in `ReadGenericPrimitive<T>`; the result is obtained via direct `*(int*)retDst` + converter delegate invocation

### Requirement: Step 12 handoff §7 audit

All `NotImplementedException` messages tagged "Step 13" in Neo code paths listed in [Step 12 handoff §7](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12/handoff.md) SHALL be either removed (this step) or explicitly re-tagged to a later step with justification. No hidden "Step 13" NotImpl SHALL remain after Step 13c.

#### Scenario: Grep audit passes

- **WHEN** the reviewer runs `rg "Step 13" ILRuntime/` after Step 13c completion
- **THEN** every remaining hit is either a comment/reference (e.g., "as of Step 13") or explicitly re-tagged to Step 14/15/17/18 with reason

## MODIFIED Requirements

### Requirement: InvocationContext BeginInvoke lifecycle

The existing `AppDomain.BeginInvoke` requirement is modified so that under `ENABLE_NEO_MODE` the returned `InvocationContext` is a `ref struct` wrapping an `InvocationFrame` whose lifetime is coextensive with the `Dispose` call. Legacy behavior is unchanged.

## REMOVED Requirements

None (all Legacy behavior preserved).

## Verification

- Neo NeoStep13cTest new suite (see tasks.md) covering:
  1. `InvocationContext.Invoke` with primitive / enum / object / IL value type / CLR value type-with-binder / CLR value type-without-binder / reference parameters; all return types
  2. `ref int` and `out int` parameter via `PushReference` writeback
  3. Single-cast and multi-cast `Delegate.Invoke` to IL methods
  4. `MethodInfo.Invoke` reflection to IL methods
  5. IL enum return value read via `ReadResult<TEnum>` matches Legacy behavior (boxed underlying primitive, not `ILEnumTypeInstance`)
- NeoStep6-13B all-green regression, Legacy 493/493 no regression
- `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` and `dotnet build -c Debug ILRuntime/ILRuntime.csproj` both 0 errors
