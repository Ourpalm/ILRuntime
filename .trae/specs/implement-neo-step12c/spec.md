# ILRuntime Neo Step 12c — Ldloca / Ldarga + Ldfld/Stfld inline fold

## Why
Step 12b 通过 `PropagateByRefReferentOffsets` pass 保证了 `Ldloca V; Ldfld/Stfld f` 未 fold 场景下的**正确性**,`Operand4 = -1 - RefOffset` 走 Ref-Slot receiver 分派。但这条路径每次要:先执行 `Ldloca` 产出 8 字节 Ref Slot、再 Ldfld/Stfld 读回 Ref Slot、走三态分派 + 帧内地址还原。与 Legacy 模式对应指令(直接 struct 字段访问)相比,**指令数 + mStack 读操作显著偏多**。Step 12c 作为纯优化 pass 消除这条中间产物,把配对指令折叠成 `Operand4 > 0`(inline direct)一条指令。

## What Changes
- **新增单一 pre-emit JIT pass `FoldLdlocaFieldAccess`**:插入位置在 [`Optimizer.EliminateConstantLoad`](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L293) 之后、[`CleanupRegister`](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L423) 之前(emit 循环在 L309-L414,fold 必须在 emit 之前才能享受现成的 `CanRemove` 机制)。作用在 `block.FinalInstructions`(register-index 形式,slot 元数据尚未分配)
- **识别 pattern**:同一 basic block 内 `Ldloca V, R` / `Ldarga P, R`(producer)紧邻消费指令 `Op R, ...`(consumer ∈ `Ldfld_* / Stfld_* / Initobj`,**不含 `Ldflda`** — Ldflda 是二态编码,fold 语义为"产生新的 byref 链",归 Step 17),且 `R` 在块内仅被 consumer 引用一次
- **命中改写**:
  - `consumer.Register2 = producer.Register2`(Ldfld/Ldfld_Value:receiver 在 Register2)
    或 `consumer.Register1 = producer.Register2`(Stfld/Stfld_Value/Initobj:receiver 在 Register1)
  - `consumer.Operand4 = 1`(Ldfld/Stfld 需要 inline direct 候选标志与 Translate 阶段 [L1341](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1341)/[L1390](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1390) 一致;Initobj 无需修改 Operand4,lowering [Optimizer.Neo.cs L314-L317](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L314-L317) 只看 receiver IsRef)
  - `block.CanRemove.Add(producerIdx)` — emit 循环 [L339-L344](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L339-L344) 自动过滤该指令并 remap addr / symbols / 跳转 / 内联偏移
- **无需 post-pass 回填**:consumer 现在 `Register2` 指向源 struct V,V 是 non-byref;`LowerNeoOffsets` 时 [Optimizer.Neo.cs L452-L457](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L452-L457) 已有分支:`localInfos[r2].IsRef` → Ref-Slot 编码,否则 `Operand4 == 1` → `localInfos[r2].RefOffset + 1` inline direct。**fold 只需保证 consumer 指向 non-byref slot 即可,lowering 天然完成 Operand4 最终编码**
- **无需担心下游 pass 兼容性**:
  - `CleanupRegister`([Optimizer.RegisterCleanup.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.RegisterCleanup.cs)):V/P 仍被 consumer.Register2 引用,不会被误删;byref R 无引用,会被 cleanup 掉
  - `TypeSpecializeAndRenameNeoRegisters`:consumer.Register2 = V 后走 rename map 与 V 同步,不产生 register 类型冲突
  - `AllocateLocalStackSpaces`:byref R 已被 CleanupRegister 消除,不占 slot
  - `PropagateByRefReferentOffsets`:fold 后没有 byref producer 剩余(Ldloca 已删),不产生 stale 元数据

## Non-Goals(本 step 不做,归后续)
- **同一 byref 寄存器多点消费的 fold**:MSIL 层 `ldloca` 一对一消费为绝对主流,共享用例罕见。这类走 Ref-Slot 兜底路径(Step 12b 已保证正确性)
- **跨基本块 fold**:风险大,ROI 低
- **Ldflda 消费者**:Ldflda 是"产生新的 byref"而非"消费终点",Operand4 是二态编码(0=heap / 1=Ref Slot 源),不含 inline direct 位。fold 语义变化太大,归 Step 17 与 structRefOffset 传播一起处理
- **Ldflda 链折叠**(嵌套 `struct.a.b.c`):依赖 Step 17
- **Ldsflda fold**:静态字段走 heap 路径,不适用 inline direct

## Impact
- **Affected specs**:Step 12b(handoff §7 "转移到下游 step" 已标记 fold 为 Step 12c);Step 17 会在扩展 Ldflda structRefOffset 传播后回顾能否扩展 fold 范围
- **Affected code**:
  - [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) L293 之后新增 `Optimizer.FoldLdlocaFieldAccess(blocks)` 调用
  - 新增文件 `Optimizer.FoldLdlocaFieldAccess.cs`(或加到 Optimizer 相关 partial class),约 60-100 行
- **不涉及 emit 循环 / CleanupRegister / TypeSpecialize / AllocateLocalStackSpaces / PropagateByRefReferentOffsets / LowerNeoOffsets 任何一处**——所有下游 pass 天然兼容

## ADDED Requirements

### Requirement: FoldLdlocaFieldAccess JIT pass
The system SHALL provide a JIT pass that folds `Ldloca V, R` / `Ldarga V, R` producer instructions with their sole consumer instruction when the consumer is `Ldfld_*` / `Stfld_*` / `Ldflda` / `Initobj` and reads receiver register `R` exactly once within the same basic block.

#### Scenario: Ldloca + Ldfld fold
- **WHEN** JIT sees within one basic block: `Ldloca V, R` immediately followed by `Ldfld_I4 R, dst, fieldOffset` and `R` has no other uses in the block
- **THEN** the pass writes `Operand4 = localInfos[V].RefOffset + 1` on the `Ldfld_I4`,adds the `Ldloca` index to `CodeBasicBlock.CanRemove`,and lets emit-final drop it

#### Scenario: Ldarga + Stfld fold
- **WHEN** JIT sees `Ldarga P, R` immediately followed by `Stfld_I4 R, src, fieldOffset` with `R` uniquely consumed
- **THEN** same fold semantics apply,receiver struct RefOffset taken from the parameter slot

#### Scenario: Fold-declined fallback
- **WHEN** the `Ldloca V, R` producer is not immediately followed by a supported consumer,OR `R` has multiple uses within the block,OR the consumer is any opcode outside the whitelist
- **THEN** the pass leaves both instructions unchanged;runtime falls back to Step 12b Ref-Slot receiver dispatch (`Operand4 = -1 - RefOffset`)

### Requirement: Debug symbol integrity after fold
The system SHALL preserve `CodeBasicBlock.InstructionMapping`(final instruction index → CIL `Instruction`) consistency after `Ldloca` instructions are removed by fold. Removed indices are dropped from the mapping;subsequent indices are shifted left by the number of preceding removals within the same block.

#### Scenario: Breakpoint hits the correct CIL instruction
- **WHEN** the user places a breakpoint on a source line whose CIL contains a `ldloca` that got folded
- **THEN** the breakpoint hits the folded consumer instruction (Ldfld/Stfld),whose `InstructionMapping` entry points to the original CIL `Instruction` recorded before fold

#### Scenario: Exception stack trace line numbers survive fold
- **WHEN** a method with fold-hit instructions throws an exception
- **THEN** the stack trace line numbers derived from `InstructionMapping` reflect the original CIL positions,not shifted post-fold offsets

## MODIFIED Requirements
None. `Operand4 > 0` inline direct behavior was fully implemented in Step 12b;this step only produces the encoding more often through fold.

## REMOVED Requirements
None.
