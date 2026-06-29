# Neo 模式 Step 3-4 实施 Spec

## Why
Step 1-2 已经完成宏体系搭建和 `ILTypeInstance` 对象模型重构。下一阶段需要：(1) 让静态字段也走新的 Object Model（Step 3），消除 `ILTypeStaticInstance` 在 Neo 模式下的空实现；(2) 把 `ExecuteNeo` 从 `StackObject*` 帧推进到 `byte*` 紧凑帧布局（Step 4），实现 Neo 帧的入/退帧骨架及最简 `Ret` 路径，为后续指令实现提供基础设施。

## What Changes

### Step 3 — 静态字段适配
- `ILType` 在 Neo 模式下新增静态字段布局信息：`StaticTotalPrimitiveSize`、`StaticTotalReferenceCount`、`staticFieldOffsets[]`（与实例字段并行的 `ILTypeFieldOffset` 数组），在 `InitializeFields` 中按自然对齐计算静态字段的 primitive offset 和 reference offset。
- `ILTypeStaticInstance` 构造函数在 `ENABLE_NEO_MODE` 下：
  - 按 `StaticTotalPrimitiveSize` 分配 `byte[] fields`
  - 按 `StaticTotalReferenceCount` 分配 `AutoList managedObjs`
  - 处理 `InitialValue`（含 `[FieldOffset]` 初始化数据）写入到 `managedObjs` 的引用槽
- `ILEnumTypeInstance` 在 `ENABLE_NEO_MODE` 下基于 `byte[] fields` + `enumType` 的 underlying size 重新实现 `Clone()` 和 `ToString()`。
- `ILType.GetStaticFieldOffset(int idx)` 暴露给后续 `ldsfld`/`stsfld` 指令使用。
- **本步骤不实现新的 `Ldsfld_Neo`/`Stsfld_Neo` 指令**——Neo 解释器目前还没有这些指令的 case handler。仅保证 `StaticInstance` 对象正确构造、字段数据可通过反射/索引器访问，为 Step 7+ 之后实现 ldsfld/stsfld 提供数据基础。

### Step 4 — Neo 帧布局 byte* 语义
- 修改 `ExecuteNeo` 签名：
  ```csharp
  internal unsafe byte* ExecuteNeo(ILMethod method, byte* esp, byte* retDst, int retRefBase, out bool unhandledException)
  ```
  - 移除原 `StackObject*` 参数和返回值
  - `retDst` 为 caller 帧内目标 byte 地址（void 方法传 null）
  - `retRefBase` 为 caller 在 mStack 中为返回引用预留的起始 index（void 或无引用字段时传 0）
- 帧初始化流程：
  - `frameBase = esp`
  - `esp += method.FrameSize`（即 `CompiledFrame.TotalStructSize`，编译期保存在 `ILMethod` 上）
  - `Unsafe.InitBlock(frameBase + ParamPrimitiveSize, 0, LocalsPrimitiveSize)` 清零 locals 区
  - 为引用类型 local 的 byte 位置预先写入 `-1`（null 约定）
  - `frameRefBase = mStack.Count; mStack.Count += method.TotalRefSize`
- 退帧：`mStack.Count = frameRefBase`，`return frameBase`（即调用前的 esp）
- 栈溢出检查：`esp + frameSize <= stackEnd`，溢出抛 `StackOverflowException`
- Frames 栈维护（DebugService 使用）—— 在帧入/退处复用现有 `StackFrame` 机制（保持 IsRegister=true，但语义占位即可）
- 实现 `Ret` 指令的最简 Neo 版本：
  - 有返回值：`Unsafe.CopyBlock(retDst, frameBase + srcOffset, retPrimitiveSize)`，并将 `retRefCount` 个 mStack 引用从 `frameRefBase + srcRefOffset` 拷贝到 `retRefBase`
  - 无返回值（void）：不写
- 异常处理保留现有 `HandleException` 调用路径，但 esp 类型需要 `byte*`/`StackObject*` 适配——本步骤仅保证异常向上抛出的路径可编译通过即可（HandleException 的完整 Neo 适配留给 Step 14）。
- 在 `ILMethod` / `CompiledFrame` 上暴露 `FrameSize`、`ParamPrimitiveSize`、`LocalsPrimitiveSize`、`TotalRefSize`、`ReturnPrimitiveSize`、`ReturnRefCount` 等新属性。
- 调用 `ExecuteNeo` 的现有调用点（如 `AppDomain.Invoke` / `ILMethod` 派发等）需要适配新签名；本步骤如发现调用点过多，允许保留旧签名作为 thin wrapper（内部走非托管帧）— 但仍要满足"Neo 模式下 ILIntepreter 能正确 Enter/Exit 一个空方法"的验证目标。

### 互斥与兼容
- Neo 模式下 `ILTypeStaticInstance` 的所有索引器读写、`StackObject[]`-风格 API（`PushToStack`、`AssignFromStack`、`CopyToRegister` 等）允许保持 Step 2 的"`throw NotSupportedException`"占位状态，因为 Step 3 仅要求 Neo 解释器能在静态字段数据上工作，不要求与 Legacy 接口完全等价。

## Impact

- **Affected specs**: Neo 编译模式（条件编译路径） / Neo 帧布局 / 静态字段访问
- **Affected code**:
  - [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) — 静态字段 offset 计算 + `Static*` 新属性
  - [ILTypeInstance.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/ILTypeInstance.cs) — `ILTypeStaticInstance`、`ILEnumTypeInstance` 的 Neo 路径实现
  - [ILMethod.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/ILMethod.cs) — 暴露 `FrameSize`/`ParamPrimitiveSize`/`LocalsPrimitiveSize`/`TotalRefSize`/`ReturnPrimitiveSize`/`ReturnRefCount`
  - [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) — `CompiledFrame` 字段补全（参数 primitive size、locals primitive size 等元数据）
  - [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) — `ExecuteNeo` 签名重构 + 帧入/退 + Ret 实现
  - [RuntimeStack.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Stack/RuntimeStack.cs) — 暴露 nativePointer 的 `byte*` 视图 / 栈溢出边界 (`stackEnd`)（如需）
  - `ExecuteNeo` 调用点适配（若有）

## 设计决策

1. **静态字段 offset 算法与实例字段同构**：直接复用 `InitializeFields` 中现有的 `primitiveOffset`/`referenceOffset` 逐步累加逻辑，仅在 `field.IsStatic == true` 分支镜像一份独立的 `staticPrimitiveOffset`/`staticReferenceOffset`。
2. **`InitialValue` 处理**：包含 `[FieldOffset]` 或 `cctor` 字面量初始化的字段，Legacy 路径下作为 `byte[]` 对象存入 `managedObjs`。Neo 模式下保持相同语义——这类字段视为 reference（mStack 中存放 `byte[]` 对象），不要 marshalling 到 primitive 区。
3. **`FrameSize` 命名**：以 `ILMethod.FrameSize` 暴露 `CompiledFrame.TotalStructSize`，避免外部代码访问 `CompiledFrame` 内部结构。
4. **栈溢出抛异常**：使用 `Runtime.Stack.StackOverflowException`（如不存在则用 `System.StackOverflowException`），保持与 Legacy `ExecuteR` 行为一致。
5. **现有 `ExecuteNeo` 调用点处理策略**：当前 Neo 入口（`ILMethod.Invoke` 路径）暂未真正联通 Neo 模式（Step 1-2 中 Neo 解释器仅是占位），允许通过 `#if ENABLE_NEO_MODE` 在调度入口选择 `ExecuteNeo`，并在该入口处分配/管理 retDst 缓冲（最简：使用一个本方法栈上的 byte 缓冲区）。
6. **本 Spec 不实现的内容**：算术指令、Call 约定、CLR 互操作、值类型完整实现、ldsfld/stsfld 指令本身——这些属于 Step 5+ 的范围。本 Spec 仅保证空方法 / 仅含 `Ret` 的方法能在 Neo 帧上正确 Enter/Exit/Return。

## ADDED Requirements

### Requirement: Neo 静态字段对象模型
The system SHALL allow `ILTypeStaticInstance` in Neo mode to allocate its primitive storage as `byte[]` sized by `ILType.StaticTotalPrimitiveSize`, and its reference storage as `AutoList` sized by `ILType.StaticTotalReferenceCount`.

#### Scenario: 加载含静态字段的类型
- **WHEN** Neo 模式下访问一个含若干静态字段（混合 primitive 与引用）的 IL 类型的 `StaticInstance`
- **THEN** 返回的 `ILTypeStaticInstance` 的 `Primitives.Length == type.StaticTotalPrimitiveSize` 且 `ManagedObjects.Count == type.StaticTotalReferenceCount`

#### Scenario: 静态字段 InitialValue
- **WHEN** 一个静态字段在 IL 元数据中带有 `InitialValue`（如 `static readonly int[] arr = {...}` 的初始化 blob）
- **THEN** 该字段对应的 `ManagedObjects` 槽持有原始 `byte[]`，与 Legacy 行为一致

### Requirement: Neo 帧 byte* 入/退
The system SHALL enter and exit `ExecuteNeo` with a `byte*` frame: on entry the interpreter advances `esp` by `method.FrameSize`, clears the locals primitive region, initializes reference local slots to `-1`, batch-reserves `method.TotalRefSize` slots in `mStack`; on exit it truncates `mStack` back to `frameRefBase` and returns the original `esp` value.

#### Scenario: 进入并立即返回的空方法
- **WHEN** Neo 模式下调用一个 `void Foo() { return; }`
- **THEN** 调用前后 `mStack.Count` 一致，调用前后 `esp`（caller 视角）一致

#### Scenario: 带返回值的最简方法
- **WHEN** Neo 模式下调用 `int Bar() { return 42; }`（编译后只含 `Ldc_I4 → Ret`，或即便 `Ldc_I4` 暂未实现，至少 `int Bar() { return localOrParam; }` 这类只走 `Ret` 的路径）
- **THEN** caller 指定的 `retDst` 处写入 42（int）

### Requirement: ILMethod 暴露帧元数据
The system SHALL expose `FrameSize`, `ParamPrimitiveSize`, `LocalsPrimitiveSize`, `TotalRefSize`, `ReturnPrimitiveSize`, `ReturnRefCount` on `ILMethod` under `#if ENABLE_NEO_MODE`.

#### Scenario: 帧元数据可用
- **WHEN** Neo 模式下对一个已 JIT 的 `ILMethod` 读取 `FrameSize`
- **THEN** 返回 `CompiledFrame.TotalStructSize`

## MODIFIED Requirements

### Requirement: ExecuteNeo 签名
原始签名：`StackObject* ExecuteNeo(ILMethod method, StackObject* esp, out bool unhandledException)`

新签名：
```csharp
internal unsafe byte* ExecuteNeo(ILMethod method, byte* esp, byte* retDst, int retRefBase, out bool unhandledException)
```

返回值含义：返回 caller 视角的原始 `esp`（即本帧 `frameBase`），调用方可直接复用而无需感知本帧大小。

### Requirement: ILTypeStaticInstance 构造
Neo 模式下原 `#if !ENABLE_NEO_MODE` 占位的构造逻辑被替换为基于 `byte[] + AutoList` 的新实现（见 ADDED 部分）。

## REMOVED Requirements

无（本步骤仅添加和修改）。
