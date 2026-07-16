# Neo 模式 Step B 实施 Spec — Stack Register Slot Layout 修复

## Why

`object-model-neo-design.md` §4.3/§4.4 明确规定：

> **在 Neo 中不存在"栈寄存器"与"局部变量"的区分。** 所有变量（用户声明的 locals + 编译器生成的临时值）统一为帧 byte 区域中有确定偏移和大小的 slot。

> - `int` 变量 = 4 bytes
> - `Vector3` 值类型 = 12 bytes
> - `object` 引用 = 4 bytes（mStack index）
>
> 所有类型都按实际大小占用空间。

但从 Step 3-4 (`9adfab15`) 起，[JITCompiler.AllocateLocalStackSpaces](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1181-L1205) 对 stack register（编译器生成的 tmp register）依然沿用 Legacy `object-model-design.md` §198 的"取全局值类型 upper-bound + `maxRefCount=1`"分配方案：

```csharp
int maxSize = 8, maxRefCount = 1;
foreach (var i in valueTypes) { ... }  // 全局上界
for (int i = 0; i < frame.StackRegisterCount; i++)
{
    slot.Size = maxSize;
    slot.RefCount = maxRefCount;
    offset += maxSize;
    refOffset += maxRefCount;
    localInfo[baseRegStart + i] = slot;
}
```

**遗漏责任链**（参见前置调查结论）：

| Commit | 阶段 | 责任 |
|---|---|---|
| `91cc26ab` (2025-12-16) | 老 Neo 尝试 | 首次引入 upper-bound 分配，无 Neo spec 约束 |
| `9adfab15` (Step 3-4) | **主责** | 把 params/locals 改成按类型分配，**漏改** stack register |
| `2247eff7 / ae47fbc4` (Step 6) | 次责 | 用 Move lowering `min(src, dst)` 打补丁绕过 CopyBlock 越界，未追根 |
| `f6b4ef14` (Step 7-8) | 顺延固化 | 新增 `RefCount` 字段，`stack register` 仍写 `maxRefCount` |
| Step 9 / 10 / 11 | 未涉及 | 但 Step 10 handoff 已经默认了这一偏离 |

**已发生的可观察后果**：

1. **Step 6 workaround**：[Optimizer.Neo.cs Move case L94-118](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L94-L118) 用 `min(srcSz, dstSz)` 修正 CopyBlock 大小。若 slot 按实际类型 sizing，`srcSz == dstSz`，min 不再必要。
2. **Step 11 崩溃**：`NeoStep11MultipleInterfaces` 中 `move r5, r1` 在 `r1` 被 `callvirt` 复用返回 int 之后，Move lowering 通过 IL 层类型追踪（`registerTypes[r1]`）判定 `Operand=1`（refMove），触发 mStack OOR。真正的修复应该是让 Move lowering 直接从 slot layout（`RefCount > 0`）判定 refMove，而不是依赖脆弱的 IL 类型追踪。
3. **无效引用槽预留**：每个 stack register 至少预留 `maxRefCount` 个 mStack 引用槽，即使该 tmp 从头到尾只承载 int / bool。TotalRefSize 被虚增，Callvirt/Ret 引用返回 slot 的 offset 也在被虚增的空间里，浪费 mStack 和帧空间。

## What Changes

### B-1. Stack register 按实际承载类型分配 slot

对每个 stack register（`localInfos[baseRegStart + i]`），根据其**实际承载类型**独立分配 `Offset / Size / RefOffset / RefCount`：

- primitive (i4/u4/i8/u8/r4/r8/…): `Size = sizeof(T)`, `RefCount = 0`
- 引用类型（class / interface / string / delegate / object / CLR value type stored as ref）：`Size = 4`, `RefCount = 1`（存 mStack index）
- ILType 值类型（含 `Vector3` 等 struct）：`Size = il.TotalPrimitiveSize`, `RefCount = il.TotalReferenceCount`

**承载类型的确定**：复用现有 `TypeSpecializeNeoOpcodes` / `BuildInitialRegisterTypes` 已经在维护的 `IType[] registerTypes` 推断路径。为了让 stack register 也能被这份类型信息覆盖，必须在 `AllocateLocalStackSpaces` **之前**先跑完寄存器类型推断，把每个 stack register 的实际承载类型确定下来，再按类型分配 slot。

如果某个 stack register 因编译器 dead-code 消除或 lowering 优化未被写入过（`registerTypes[r] == null`），退化为最小 slot（`Size = 4, RefCount = 0`），因为它对运行时无效，只需保证帧偏移可用。

### B-2. `TypeSpecializeNeoOpcodes` 补齐 SetRegisterType

`TypeSpecializeNeoOpcodes` 目前只覆盖了 Ldc_*、Move、算术、Conv、Ldfld_* 等生成结果的 Opcode，遗漏了：

- `Call / Callvirt / Callvirt_IL / Callvirt_CLR / Callvirt_Constrained / Call_Redirect / Call_Redirect_IL`：`Register1 = 返回值 slot`，返回值类型 = `IMethod.ReturnType`
- `Newobj`：`Register1 = 新对象 slot`，类型 = 目标类型
- `Ldelem_* / Ldelema`：类型可推
- `Ldloc / Ldarg`（若产生 register，Move 已覆盖大多数）
- `Box`：结果为引用（`Register1 = 引用 slot`）
- `Unbox / Unbox_Any`：结果类型 = 目标类型
- `Ldsfld_*` 变体：类型固定
- `Ldnull` 已覆盖；`Ldtoken / Ldftn / Ldvirtftn`：结果为引用
- `Isinst / Castclass`：结果类型 = 目标引用类型
- `Ldstr / Ldnull` 已覆盖

**补齐目标**：让 `registerTypes[]` 在 lowering 前对每个"曾被写入过"的 stack register 都有正确类型，作为 slot 分配的输入。

**注意**：这份类型追踪只用于 **分配阶段**（Step B-1）与 refMove 判定回退兜底。真正的 refMove 判定改由 slot layout 完成（Step B-3），不再依赖它作为运行时正确性来源，因此即使有零星漏补也只是 slot 大小估算偏保守，不会引入运行时 bug。

### B-3. Move 指令 refMove 判定回归 slot layout

修改 [Optimizer.Neo.cs Move case L94-118](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L94-L118) 与 [JITCompiler.cs L517-522 Move handler](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L517-L522)：

**JITCompiler 侧**（TypeSpecializeNeoOpcodes 中）：删除 `op.Operand = IsNeoReferenceSlot(srcType) ? 1 : 0;` 的写入，不再依赖 IL 类型追踪；仅保留 `SetRegisterType(registerTypes, op.Register1, srcType)` 用于 stack register 类型推断（B-1 的输入）。

**Optimizer.Neo 侧**（LowerNeoOffsets Move case）：

```csharp
case OpCodeREnum.Move:
{
    int srcReg = op.Register2;
    int dstReg = op.Register1;
    int srcSz  = (srcReg >= 0 && srcReg < localInfos.Length) ? localInfos[srcReg].Size : 0;
    int dstSz  = (dstReg >= 0 && dstReg < localInfos.Length) ? localInfos[dstReg].Size : 0;
    int srcRef = (srcReg >= 0 && srcReg < localInfos.Length) ? localInfos[srcReg].RefCount : 0;
    int dstRef = (dstReg >= 0 && dstReg < localInfos.Length) ? localInfos[dstReg].RefCount : 0;
    // 每个 slot 都按实际承载类型分配 (Step B-1)，所以 src.Size == dst.Size,
    // src.RefCount == dst.RefCount。若 srcRef > 0 则是引用 slot。
    bool isRefMove = srcRef > 0;
    int sz = srcSz;
    LowerR1R2(ref op, localInfos);
    op.Operand = isRefMove ? 1 : 0;
    op.Operand2 = sz;
    op.Operand3 = localInfos[dstReg].RefOffset;
}
break;
```

去掉 `min(src, dst)` 补丁（B-1 后 src/dst size 保证相等；即使被值类型 Move 复用，也应该在 CLR 层就已经是相同 struct 类型）。

**断言**：DEBUG 下若 `srcSz != dstSz` 或 `srcRef != dstRef`，抛异常暴露 slot 分配 bug —— 这正是 Step 6 workaround 掩盖过的真信息。

### B-4. 帧元数据统计口径调整

由于 stack register 不再按 `maxRefCount` 预留 mStack 槽，`TotalRefSize` 会缩小。Callvirt/Ret 侧读取 `TotalRefSize` 作为帧引用区容量的调用点（如 [ILIntepreter.Neo.cs 帧入口](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs)、Ret 引用返回、Callvirt 返回 slot 定位）都应仍然正确，因为它们都通过 `LocalInfos[reg].RefOffset` 定位到当前 slot 自己的槽，绝对偏移由 `frameRefBase + slot.RefOffset` 决定。这一 Spec 只是让 `TotalRefSize` 变小（stack register 不再全部占一个 ref 槽），无 API 破坏。

**回归风险点**：Callvirt 返回引用 slot 的定位。当前 [InitializeCallvirtDispatch](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) 处理路径 / Call handoff 已经通过 `Operand3 = LocalInfos[dstReg].RefOffset` 定位返回 ref slot；只要 B-1 保证返回值 slot 有正确的 `RefCount`（1 或值类型的 refSize），这条链自动正确。B-2 需要确保 Call/Callvirt 变体在 `SetRegisterType` 中记录了返回类型，这样返回值 slot 在 B-1 中会分配到匹配大小的 ref 空间。

### B-5. Step 6 Move workaround 移除

删除 [Optimizer.Neo.cs L94-98 的注释与 `min(src, dst)` 逻辑](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L94-L118)，替换为 B-3 的 slot-layout 判定实现。移除 [checklist 中 Move lowering bug 记录](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step6/checklist.md#L79) 对应的补丁话术，改为在本 Spec 的完成记录中说明"Step B 已根除该 workaround"。

## Impact

- **Affected specs**: `implement-neo-step3-step4`（补账）、`implement-neo-step6`（移除 workaround）、`implement-neo-step10`（handoff 中"TotalRefSize 覆盖 stack registers"的表述需要软化为"覆盖所有 slot 的 RefCount 之和"）、`implement-neo-step11`（依赖此修复恢复测试通过）
- **Affected code**:
  - [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs)
    - `AllocateLocalStackSpaces`（L1070-L1225）：stack register 循环改为按实际类型分配
    - `TypeSpecializeNeoOpcodes`（L480-L648）：补齐 Call / Callvirt* / Newobj / Ldelem* / Box / Unbox* / Ldsfld* / Ldtoken / Ldftn / Isinst / Castclass 的 `SetRegisterType`
    - `Compile` / `InitCodeBody` 主控流：确保 `TypeSpecializeNeoOpcodes`（含 registerTypes 推断）在 `AllocateLocalStackSpaces` **之前**执行；或者把 registerTypes 的构建拆成一个独立 pass 并 pass 到 `AllocateLocalStackSpaces`
    - Move 指令 `op.Operand` 不再写 refMove 标志（下沉到 lowering pass）
  - [Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs)：Move case 改为基于 slot layout 判定 refMove；DEBUG 断言 src/dst layout 一致
  - 无其它文件

## 设计决策

1. **Slot 分配依赖 stack register 类型推断**：`AllocateLocalStackSpaces` 现在依赖 `registerTypes[]`（B-1 输入）。因此 registerTypes 推断（现放在 `TypeSpecializeNeoOpcodes` 里）必须先于 slot 分配跑完。当前主控流实际顺序为 `Optimizer.CleanupRegister → CodeBody 定型 → TypeSpecializeNeoOpcodes → AllocateLocalStackSpaces`（在 Neo 模式下，`InitCodeBody` 内已按此调用），已满足前提；本 Spec 无需改变主控流顺序，仅补齐 `SetRegisterType` 覆盖。**验证点**：在 Compile 主控流增加断言，确保 `TypeSpecializeNeoOpcodes` 结束后 `registerTypes` 不为 null 时才进入 slot 分配。

2. **不引入独立的"stack register 类型表"**：直接复用 `registerTypes[]`（`IType[]`）作为分配阶段读取的类型来源。为此需要在 `AllocateLocalStackSpaces` 签名中接收该数组（或把它挂到 `CompiledFrame` 上），而非只从 `def.Body.Variables` 推 locals 类型。

3. **未写入过的 stack register 用最小 slot**：`registerTypes[r] == null` 表示这个 tmp 从未被产生结果的 opcode 写入，可能因 lowering / CleanupRegister 未删干净。分配 `Size=4, RefCount=0` 保证帧不越界即可。

4. **删除 `min(src, dst)` 补丁**：B-1 后 slot 类型精确，`src.Size == dst.Size`；若不等，属于编译器 bug 应立即暴露（DEBUG 断言）。

5. **不改 `Operand` 语义**：Move 的 `Operand=1` 表示 refMove 保持不变，仅改判定来源（从 JIT 侧 `IsNeoReferenceSlot(srcType)` 改为 lowering 侧 `srcRef > 0`）。runtime handler 不动。

6. **本 Spec 不改 Legacy**：所有改动全部在 `#if ENABLE_NEO_MODE` 内，Legacy 编译路径零影响。

7. **本 Spec 不包含**：Callvirt/Call 变体本身的重构（Step 12 议题）、Slot 复用（liveness / graph coloring）、值类型完整 lowering（Step 12/13）。

## ADDED Requirements

### Requirement: Neo stack register slot 按承载类型独立分配

The system SHALL, in `JITCompiler.AllocateLocalStackSpaces` under `#if ENABLE_NEO_MODE`, allocate each stack register slot's `Size`, `RefOffset`, `RefCount` based on the actual `IType` written to that register during type inference (`TypeSpecializeNeoOpcodes` maintained `registerTypes[]`), using the same per-type sizing rules as user-declared locals (primitive: `sizeof(T)` / `RefCount=0`; reference: `Size=4` / `RefCount=1`; ILType value type: `Size=TotalPrimitiveSize` / `RefCount=TotalReferenceCount`). Stack registers never written by any result-producing opcode SHALL fall back to `Size=4, RefCount=0`.

#### Scenario: int-only stack register
- **WHEN** 一个 stack register 全生命周期只承载 int (`registerTypes[r] == appdomain.IntType`)
- **THEN** `LocalInfos[r].Size == 4 && LocalInfos[r].RefCount == 0`

#### Scenario: 引用类型 stack register
- **WHEN** 一个 stack register 承载 `object` / `string` / interface / class
- **THEN** `LocalInfos[r].Size == 4 && LocalInfos[r].RefCount == 1`

#### Scenario: ILType 值类型 stack register
- **WHEN** 一个 stack register 承载 `Vector3` (12 bytes primitive) 或含引用字段的 struct
- **THEN** `LocalInfos[r].Size == il.TotalPrimitiveSize && LocalInfos[r].RefCount == il.TotalReferenceCount`

### Requirement: TypeSpecializeNeoOpcodes 覆盖所有产生 dst register 的 opcode

The system SHALL, in `TypeSpecializeNeoOpcodes`, invoke `SetRegisterType(registerTypes, op.Register1, resultType)` for every opcode that writes a result to `Register1`, including: `Call / Callvirt / Callvirt_IL / Callvirt_CLR / Call_Redirect / Call_Redirect_IL / Newobj / Ldelem_* / Ldelema / Box / Unbox / Unbox_Any / Ldsfld_* / Ldtoken / Ldftn / Ldvirtftn / Isinst / Castclass`, in addition to the arithmetic / load-constant / field-load opcodes already covered.

#### Scenario: Call 返回值类型追踪
- **WHEN** `int GetValue()` 通过 Call 指令写入 stack register r
- **THEN** `registerTypes[r] == appdomain.IntType`（因此后续 B-1 分配 `Size=4, RefCount=0`）

#### Scenario: Newobj 引用类型追踪
- **WHEN** `Foo obj = new Foo()` 通过 Newobj 写入 stack register r
- **THEN** `registerTypes[r]` 为 Foo 类型（引用型），B-1 分配 `Size=4, RefCount=1`

### Requirement: Move 指令 refMove 判定基于 slot layout

The system SHALL, in `Optimizer.Neo.cs LowerNeoOffsets` Move case, determine `op.Operand` (refMove flag) from `LocalInfos[srcReg].RefCount > 0`, not from a JIT-time `IL type -> IsNeoReferenceSlot` heuristic. The Move copy size (`op.Operand2`) SHALL equal `LocalInfos[srcReg].Size`. In DEBUG builds the pass SHALL assert `LocalInfos[srcReg].Size == LocalInfos[dstReg].Size` and `LocalInfos[srcReg].RefCount == LocalInfos[dstReg].RefCount`.

#### Scenario: int→int Move
- **WHEN** `move r5, r1` 且 r1/r5 都被分配为 int slot
- **THEN** lowering 后 `op.Operand == 0 && op.Operand2 == 4`

#### Scenario: object→object Move
- **WHEN** `move r5, r1` 且 r1/r5 都被分配为 ref slot
- **THEN** lowering 后 `op.Operand == 1 && op.Operand2 == 4`

#### Scenario: Vector3→Vector3 Move
- **WHEN** `move r5, r1` 且 r1/r5 都被分配为 Vector3 slot (Size=12, RefCount=0)
- **THEN** lowering 后 `op.Operand == 0 && op.Operand2 == 12`

## MODIFIED Requirements

### Requirement: `AllocateLocalStackSpaces` stack register 段

**原实现**（`JITCompiler.cs` L1181-L1205）取所有值类型 upper-bound：

```csharp
int maxSize = 8, maxRefCount = 1;
foreach (var i in valueTypes) { ... }
for (int i = 0; i < frame.StackRegisterCount; i++)
{
    slot.Size = maxSize;
    slot.RefCount = maxRefCount;
    ...
}
```

**新实现**：按 `registerTypes[baseRegStart + i]` 逐 slot 分配（复用 `AllocateSlotForType` 或等价内联逻辑）。删除 `GatherValueTypes` 用于 upper-bound 计算的路径（若 `GatherValueTypes` 无其它调用点，可整段删除）。

### Requirement: Move 指令 lowering 补丁（Step 6 workaround）移除

**原**：`Optimizer.Neo.cs` L94-118 用 `min(srcSz, dstSz)` 修正 wide stack register → narrow local 的 CopyBlock 越界。

**新**：Step B-1 后 src/dst slot 大小相等，直接使用 `srcSz`；`min` 表达式与"防越界"注释一并删除。DEBUG 断言 src/dst layout 一致，任何不等都属编译器 bug。

## REMOVED Requirements

无（本步骤仅添加和修改）。

## 完成验收

- Legacy `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误 0 警告
- Neo `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误 0 警告
- `NeoStep6Test`（Legacy Mode + Neo Mode 各一遍）全绿
- `NeoStep7Test / NeoStep8Test / NeoStep9Test / NeoStep10Test / NeoStep11Test`（Neo Mode）全绿
- `NeoStep11MultipleInterfaces` 单独 + 全组均通过（当前 pending bug 消除）
- 手工抽查 3 个方法（int-only、含 object、含 Vector3）的 `TotalStructSize` 与老实现比对，应严格 ≤ 老实现值
