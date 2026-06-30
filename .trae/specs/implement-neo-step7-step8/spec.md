# Neo Step7-Step8 Spec

## Why
Neo 模式已经完成 Step1-6，当前解释器只能覆盖基础帧、常量、算术、比较、分支与基础 lowering。为了继续推进对象模型，需要补齐引用类型 slot 生命周期、堆对象字段访问、IL 方法间调用，以及 IL 引用类型对象创建。

## What Changes
- 实现 Step7：Neo 帧中引用类型 slot 的 mStack 生命周期、`Ldnull`、`Ldstr`、引用 `Move`、堆 IL 对象字段 `Ldfld_*` / `Stfld_*` 非 Inline 访问。
- 恢复 Step6 smoke 中临时跳过的静态构造函数调用，并通过字段访问链验证 cctor。
- 实现 Step8：Caller-Write + Direct-Return 的 IL 方法间调用约定。
- 实现 Step8b：IL 引用类型 `Newobj`，构造函数按 Step8 调用约定执行。
- 测试集中放在 Step8/8b 完成后执行，不在 Step7 中途运行。

## Impact
- Affected specs: Neo Step7、Step8、Step8b
- Affected code: `ILRuntime/Runtime/Intepreter/ILIntepreter.Neo.cs`、`ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs`、`ILRuntime/Runtime/Intepreter/OpCodes/OpCodeREnum.cs`、`ILRuntime/Runtime/Intepreter/OpCodes/OpCode.cs`、`ILRuntime/CLR/TypeSystem/ILType.cs`、`TestCases`

## ADDED Requirements

### Requirement: Neo 引用类型 slot 生命周期
The system SHALL store reference locals/temps as mStack indexes in the Neo frame byte area, using `-1` as null.

#### Scenario: 引用 local 初始化
- **WHEN** Neo 方法进入帧并初始化 locals 区
- **THEN** 引用类型 local/temp 的帧内 int 值为 `-1`
- **AND** `mStack` 为当前方法的引用 slot 批量预留 `TotalRefSize` 个槽

#### Scenario: 方法退出释放引用槽
- **WHEN** Neo 方法正常返回
- **THEN** `mStack.Count` 恢复到该帧入口的 `frameRefBase`

### Requirement: Neo 引用常量与引用 Move
The system SHALL support `Ldnull`、`Ldstr` and reference slot copying in Neo mode.

#### Scenario: null 引用
- **WHEN** 方法执行 `Ldnull`
- **THEN** 目标帧 slot 写入 `-1`

#### Scenario: string 常量
- **WHEN** 方法执行 `Ldstr`
- **THEN** 字符串对象写入目标 ref slot 对应的 mStack 槽
- **AND** 目标帧 slot 写入该 mStack 绝对 index

#### Scenario: 引用 Move
- **WHEN** 方法把一个引用 slot 复制到另一个引用 slot
- **THEN** 目标 mStack 槽持有相同对象引用
- **AND** 目标帧 slot 写入目标 mStack 绝对 index 或保留 `-1`

### Requirement: Neo 堆 IL 对象字段访问
The system SHALL support non-inline `Ldfld_*` / `Stfld_*` for IL heap objects by reading object indexes from the frame and accessing `ILTypeInstance.Primitives` / `ManagedObjects`.

#### Scenario: 读写 primitive 字段
- **WHEN** Neo 方法对 IL 引用类型实例执行 `Stfld_I4` 后再执行 `Ldfld_I4`
- **THEN** 读出的 primitive 值与写入值一致

#### Scenario: 读写引用字段
- **WHEN** Neo 方法对 IL 引用类型实例执行引用类型字段读写
- **THEN** 字段 primitive 区保存 mStack index
- **AND** 对应 managed reference 被保存在对象的 `ManagedObjects` 中

### Requirement: cctor 恢复
The system SHALL restore static constructor invocation in Neo mode after field access handlers are implemented.

#### Scenario: 静态字段初始化
- **WHEN** Neo 模式首次访问带静态构造或静态字段初始化的 IL 类型
- **THEN** `appdomain.Invoke(staticConstructor)` 被执行
- **AND** `class C { static int X = 42; }` 的静态字段读取结果为 42

### Requirement: Neo IL 方法调用
The system SHALL implement Caller-Write + Direct-Return for IL-to-IL calls.

#### Scenario: primitive 参数和返回值
- **WHEN** caller 调用 `int Bar(int a, int b)`
- **THEN** caller 将参数写入 callee 的 `esp + paramOffset`
- **AND** callee `Ret` 直接写入 caller 指定的 `retDst`

#### Scenario: reference 参数和返回值
- **WHEN** caller 传递或接收引用类型值
- **THEN** 帧内只传递 mStack index
- **AND** 返回引用写入 caller 的目标 ref slot，不被 callee 退帧截断

#### Scenario: 递归调用
- **WHEN** Neo 方法递归调用自身
- **THEN** 每层调用使用独立帧和独立引用 slot 区域
- **AND** 返回后 `esp` 与 `mStack.Count` 均恢复正确

### Requirement: Neo IL 引用类型 newobj
The system SHALL support `Newobj` for IL reference types without requiring VTable dispatch.

#### Scenario: 默认构造函数
- **WHEN** Neo 方法执行 `new MyILClass()`
- **THEN** 分配 `ILTypeInstance`
- **AND** 对象 index 写入 caller 预分配的目标 ref slot
- **AND** 构造函数按 `call` 约定执行

#### Scenario: 带参数构造函数
- **WHEN** Neo 方法执行 `new MyILClass(42, "hello")`
- **THEN** `this` 作为 param0 写入 callee 帧
- **AND** 其余参数按签名写入后续 param offset
- **AND** 构造函数中的字段赋值可被后续字段读取验证

## MODIFIED Requirements

### Requirement: Step6 smoke 回归
Step6 smoke 测试不再依赖跳过 cctor 的临时条件编译。实现 Step7-8 后，`NeoStep6Test` 应在恢复 cctor 的前提下继续通过。

### Requirement: 测试执行时机
本变更的端到端测试 SHALL wait until Step8/8b implementation is complete. Step7 implementation tasks SHALL rely on build/static validation only, not partial test execution.

## REMOVED Requirements

### Requirement: Neo 模式跳过静态构造函数
**Reason**: Step7 完成字段访问后，跳过 cctor 的临时绕过不再需要。
**Migration**: 删除 `ILType.cs` 中 Neo 模式跳过 `appdomain.Invoke(staticConstructor)` 的条件编译分支，恢复统一调用路径。
