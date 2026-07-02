# Neo Step 11 接口方法分派 Spec

## Why
Step 10 已完成 Neo 模式下的 VTable 与虚方法分派，但接口调用不能直接使用类虚方法 slot。Step 11 需要为接口建立独立 slot 编号到类 VTable 的 offset 映射，使 `callvirt` 到接口方法时仍能走 Neo 的 O(1) 分派路径。

## What Changes
- 为 `ILType` 增加接口分派所需的 `InterfaceEntry[] interfaceMap` 或等价紧凑结构。
- 按接口独立维护 0-based method slot，并在类型加载或 Neo JIT 阶段计算接口到类 VTable 起始 slot 的映射。
- 增加或特化 `Callvirt_Interface` / `Callvirt_Interface_Neo` 指令，使接口调用通过 `interface offset + interface method slot` 定位实际方法。
- 扩展 Neo JIT lowering，使接口变量、接口约束下的 `callvirt` 能生成接口分派指令，而普通 IL/CLR 虚调用仍沿用 Step 10 路径。
- 增加 Step 11 测试用例，覆盖 IL 接口、CLR 接口、多接口和接口继承链。

## Impact
- Affected specs: Neo Step 10 VTable 构建 + 虚方法分派；Neo Step 11 接口方法分派。
- Affected code: `ILRuntime/CLR/TypeSystem/ILType.cs`, `ILRuntime/CLR/TypeSystem/IType.cs`（如需暴露查询能力）, `ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs`, `ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs`, `ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs`, `ILRuntime/Runtime/Intepreter/OpCodes/*`, `TestCases/*`。

## ADDED Requirements
### Requirement: 接口 offset 映射
The system SHALL maintain a Neo interface dispatch map for each IL type that implements one or more interfaces.

#### Scenario: IL 类实现 IL 接口
- **WHEN** IL 类实现 IL 接口并在 Neo 模式下完成类型加载或 JIT
- **THEN** 该类型能够通过接口类型索引查询到接口方法在类 VTable 中的起始 slot

#### Scenario: 多接口实现
- **WHEN** 一个 IL 类同时实现多个接口
- **THEN** 每个接口 SHALL 拥有独立的 offset 映射，接口内 slot SHALL 不互相污染

#### Scenario: 接口继承链
- **WHEN** 接口继承自其他接口
- **THEN** 实现类 SHALL 能够为父接口和子接口方法建立可查询的 offset 映射

### Requirement: 接口 callvirt 指令
The system SHALL dispatch interface method calls in Neo mode by resolving the interface offset and indexing the target type VTable.

#### Scenario: 通过接口变量调用 IL 实现
- **WHEN** `IInterface x = new Impl(); x.Foo()` 在 Neo 模式下执行
- **THEN** `Callvirt_Interface` SHALL resolve `Impl` 的实际实现方法并执行 `ExecuteNeo`

#### Scenario: IL 类实现 CLR 接口
- **WHEN** IL 类实现 CLR 接口（例如 `IDisposable`）并通过接口变量调用方法
- **THEN** Neo 接口分派 SHALL 定位 IL 实现方法；需要 CrossBindingAdapter 的 CLR 暴露路径不在本步骤扩展

#### Scenario: 接口分派失败
- **WHEN** 对象运行时类型没有实现目标接口或缺少目标接口方法 slot
- **THEN** 运行时 SHALL 抛出明确异常，不应发生 ip 越界、空引用误判或落入 Legacy 栈模型

### Requirement: Neo JIT lowering
The system SHALL lower interface `callvirt` into a Neo interface dispatch instruction when the declaring type is an interface.

#### Scenario: 普通虚调用保持 Step 10 路径
- **WHEN** `callvirt` 目标不是接口方法
- **THEN** lowering SHALL continue to use Step 10 的 `Callvirt_IL` / `Callvirt_CLR` / 通用 `Callvirt` 路径

#### Scenario: 接口方法 lowering
- **WHEN** `callvirt` 目标方法声明在接口类型上
- **THEN** lowering SHALL encode interface type identity and interface method slot into `OpCodeR` operands

## MODIFIED Requirements
### Requirement: Neo VTable 分派
Neo VTable SHALL support both class virtual dispatch and interface dispatch. Interface dispatch SHALL not reuse raw interface slot as class VTable slot directly; it MUST first resolve the implementing type's interface offset.

## REMOVED Requirements
无。
