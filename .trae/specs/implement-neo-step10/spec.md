# Neo Step 10 VTable 构建与虚方法分派 Spec

## Why
Neo 模式已经具备基础 CLR 互操作和非虚调用能力，但虚方法仍需要稳定的运行时分派模型。Step 10 通过为 IL 类型构建 VTable，并在 `callvirt` 中使用 slot 索引完成分派，避免 Legacy 路径中的方法查找和栈模型混用。

## What Changes
- 为 `ILType` 增加 Neo VTable 存储，按虚方法 slot 保存最终 `IMethod`。
- 在类型加载或 Neo JIT 阶段构建 VTable，支持基类继承、override 复用 slot、本类新增 virtual 分配新 slot。
- 扩展 Neo Register VM 指令，支持 `Callvirt_IL`、`Callvirt_CLR` 和通用 `Callvirt` 三类虚调用路径。
- 继承 Legacy `InitStackCodeBody` 与 Register VM `JITCompiler` 的 devirtualize 规则：非 abstract、非 virtual、非 interface 声明类型的 ILMethod `callvirt` SHALL 直接 lowering 为 `Call`。
- 扩展 Neo JIT lowering，将 `callvirt` 编译为携带 vtable slot、this offset、返回目标和参数区信息的 Register 指令。
- 增加 Step 10 测试用例，覆盖简单继承、多层继承、CLR 虚方法调用和 IL/CLR 混合对象变量场景。

## Impact
- Affected specs: Neo Step 8/8b 调用约定与引用类型 newobj；Neo Step 9 CLR 方法调用；后续 Step 11 接口分派。
- Affected code: `ILRuntime/CLR/TypeSystem/ILType.cs`, `ILRuntime/CLR/TypeSystem/IType.cs`, `ILRuntime/CLR/Method/ILMethod.cs`, `ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs`, `ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs`, `ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs`, `ILRuntime/Runtime/Intepreter/OpCodes/*`, `TestCases/*`。

## ADDED Requirements
### Requirement: Neo VTable 元数据
The system SHALL build and store a Neo VTable for IL reference types.

#### Scenario: 基类虚方法 slot
- **WHEN** IL 类型继承一个包含虚方法的基类
- **THEN** 基类虚方法 SHALL 占用稳定的 slot，派生类 VTable SHALL 继承这些 slot

#### Scenario: override 复用 slot
- **WHEN** 派生类 override 基类虚方法
- **THEN** override 方法 SHALL 写入被 override 方法原有 slot，不得新增 slot

#### Scenario: 本类新增虚方法
- **WHEN** 类型声明新的 virtual 方法且不是 override
- **THEN** 该方法 SHALL 分配到当前 VTable 末尾的新 slot

#### Scenario: CLR 基类或 CLR 方法引用
- **WHEN** VTable slot 对应 CLR 方法或 CLR 基类方法
- **THEN** VTable SHALL 能保存可分派的 `CLRMethod` 或等价 `IMethod` 引用

#### Scenario: 过渡签名 key
- **WHEN** Step 10 JIT 版本构建 VTable 时需要匹配 override slot
- **THEN** 可以使用 `IMethod.SignatureString` 懒加载缓存作为过渡 key，避免同一个 `IMethod` 反复字符串拼接

#### Scenario: 后续结构化签名比较
- **WHEN** 后续实现接口分派、AOT VTableTemplate 或完整泛型/byref 场景
- **THEN** SHALL 参考 Legacy `ILType.GetVirtualMethod()` / `GetMethod(... exactMatch: true)` 的成熟逻辑，使用结构化签名比较器或元数据 identity 覆盖泛型类型参数、泛型实例方法、`ref` / `out` 参数和显式接口实现，不得长期依赖字符串拼接

### Requirement: Neo callvirt 分派
The system SHALL dispatch virtual method calls in Neo mode using VTable slot lookup or CLR direct invocation.

#### Scenario: IL 对象虚调用
- **WHEN** `callvirt` 目标对象为 `ILTypeInstance`
- **THEN** `Callvirt_IL` SHALL read `this` from Neo reference slot, fetch `instance.Type.VTable[slotIndex]`, and invoke the actual IL method through `ExecuteNeo`

#### Scenario: CLR 对象虚调用
- **WHEN** 编译期确定 `this` 是 CLR 类型
- **THEN** `Callvirt_CLR` SHALL directly invoke the resolved `CLRMethod` through Step 9 的 Neo CLR 调用路径

#### Scenario: 通用对象虚调用
- **WHEN** 编译期无法确定 `this` 是 IL 对象还是 CLR 对象
- **THEN** 通用 `Callvirt` SHALL inspect the runtime object from `mStack` and dispatch to either IL VTable or CLR invocation

#### Scenario: null this
- **WHEN** `callvirt` 的 `this` reference slot 指向 null
- **THEN** 运行时 SHALL 抛出明确的 null reference 异常，不得继续访问 VTable

### Requirement: Neo JIT lowering for callvirt
The system SHALL lower supported `callvirt` instructions into Neo Register VM instructions with explicit dispatch metadata.

#### Scenario: Legacy 等价 devirtualize
- **WHEN** `callvirt` 解析到 ILMethod，且目标方法不是 abstract、不是 virtual、声明类型不是 interface
- **THEN** Neo JIT SHALL lower it to direct `Call`，与 Legacy `InitStackCodeBody` 和 Register VM `JITCompiler` 保持一致

#### Scenario: 编译期确定 IL 类型
- **WHEN** `callvirt` 的静态 this 类型为 IL 类型且目标为 IL 虚方法
- **THEN** JIT SHALL emit `Callvirt_IL` with vtable slot index

#### Scenario: 编译期确定 CLR 类型
- **WHEN** `callvirt` 的静态 this 类型为 CLR 类型
- **THEN** JIT SHALL emit `Callvirt_CLR` with resolved CLR method identity

#### Scenario: 无法静态确定
- **WHEN** `callvirt` 目标可能为 IL 或 CLR 对象
- **THEN** JIT SHALL emit 通用 `Callvirt` with both IL slot and CLR fallback method metadata when available

### Requirement: 可选精确目标优化边界
The system MAY evaluate additional devirtualization for statically final targets, but Step 10 SHALL NOT rely on this optimization for correctness.

#### Scenario: sealed class 或 final override
- **WHEN** `callvirt` 目标方法在语义上不可被派生类型重写，例如声明类型 sealed 或方法 final
- **THEN** 实现 MAY lower to direct `Call` only if it can preserve existing ILRuntime null 行为 and has targeted regression tests

#### Scenario: 不确定可重写性
- **WHEN** 目标方法的实际分派结果可能被派生类覆盖，或涉及接口、泛型约束、constrained callvirt
- **THEN** JIT SHALL keep virtual dispatch and MUST NOT devirtualize

## MODIFIED Requirements
### Requirement: Neo 调用约定
Neo virtual calls SHALL reuse the Step 8 calling convention. Caller SHALL prepare callee arguments in the Neo byte frame, and return values SHALL be written to caller-provided primitive/ref destinations without using Legacy `StackObject[]`.

### Requirement: CLR 方法调用
Virtual calls to CLR methods SHALL reuse Step 9 `CLRMethod` Neo invocation and `CLRRedirectionDelegateNeo` paths. Step 10 SHALL not introduce a separate CLR argument ABI.

## REMOVED Requirements
无。
