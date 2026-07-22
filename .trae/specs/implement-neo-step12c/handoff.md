# ILRuntime Neo Step 12c — Handoff

## 交付概述
Step 12c 是 Step 12b 的**纯性能优化后继**,消除同帧 struct 字段访问路径上的中间 Ref Slot 产物。目标是把 `Ldloca V; Ldfld/Stfld f`(以及 `Ldarga P; Ldfld/Stfld` / `Ldloca V; Initobj`)折叠成一条 inline direct 指令,砍掉每次访问一次 8 字节 Ref Slot 编码 + 一次三态分派 + 一次帧内地址还原。**正确性保底由 Step 12b 的 `PropagateByRefReferentOffsets` 承担,fold 未命中或未启用时 Ref-Slot 兜底路径依然工作**。

- **Neo 测试:41/41 通过**
- **Legacy 测试:493/493 通过**,useRegister=false 无回归
- **`-c Debug` / `-c Debug_Neo` 双配置 0 错误**
- **JIT dump 验证**:`NeoStep12StructWithRef` 的 `stfld.i4` / `ldfld.i4` 之前的 `ldloca.s` 全部被 `(x)` 标记并从 emit 后指令中消失,receiver 由 byref R 换成源 struct V,`.inline` 后缀不变(inline direct 路径)

---

## 1. Fold pass 定位与工作方式

**位置**: [JITCompiler.cs L294-L300](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L294-L300),在 `Optimizer.EliminateConstantLoad` 之后、emit 循环之前:

```
BCP → FCP → BCP → EliminateConstantLoad → 【FoldLdlocaFieldAccess】 → Emit → CleanupRegister → ...
```

**作用对象**: `CodeBasicBlock.FinalInstructions`(register-index 形式,slot 元数据尚未分配)

**核心逻辑**(简化):
1. 找到 producer:`Ldloca V, R` / `Ldloca_S V, R` / `Ldarga P, R` / `Ldarga_S P, R`
2. **向前 forward scan** 找到"第一个引用 R 的指令" j — 中间可能穿插 `Ldc_I4_S` / `Ldstr` 等常量加载指令(为 Stfld 的 value 参数准备),它们不引用 R
3. 检查 j 是否是 foldable consumer:`Ldfld_* / Stfld_* / Initobj`。**不含 Ldflda**(Ldflda 是二态 Operand4 编码,fold 语义不兼容,归 Step 17)
4. 检查 R 是否只在 consumer 的 receiver 槽被引用一次(其它槽和其它指令中的 R 都是 fold 拒绝条件)
5. 命中:
   - `consumer.Register1 = V`(Stfld/Initobj)或 `consumer.Register2 = V`(Ldfld)
   - `consumer.Operand4 = 1`(Ldfld/Stfld 需要 inline direct 候选标志,与 Translate 阶段 [L1341](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1341)/[L1390](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1390) 一致;Initobj lowering 只看 receiver.IsRef,不看 Operand4,故不修改)
   - `block.CanRemove.Add(producerIdx)` — 后续 emit 循环 [L339-L344](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L339-L344) 自动过滤该指令

**无需 post-pass 回填**:consumer 现在 `Register2/Register1` 指向源 struct V(non-byref);[LowerNeoOffsets](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs) 时 `localInfos[V].IsRef == false` + `Operand4 == 1` 分支直接编码为 `Operand4 = RefOffset + 1`(inline direct)。

## 2. 下游 pass 兼容性 — 无 remap 成本

- **CleanupRegister**: byref R 无引用,被自动消除;V/P 依然被 consumer 引用,保留
- **TypeSpecializeAndRenameNeoRegisters**: consumer.Register2 = V 后走 rename map 与 V 同步,不产生 register 类型冲突
- **AllocateLocalStackSpaces**: byref R 已被 CleanupRegister 消除,不占 slot
- **PropagateByRefReferentOffsets**: fold 后 Ldloca 已删,pass 内 scan 空,不产生 stale byref RefOffset 元数据
- **Emit 循环**: `addr[oriIns.Instruction] = curIndex` 建立 IL→输出 index 映射的地方在 CanRemove 检查前 [L322-L338](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L322-L338),所以被 fold 的 Ldloca 对应的 CIL Instruction 自然绑到"下一条存活指令"(consumer),ExceptionHandler 表 / 断点调试 / Leave 目标全部正确

## 3. 受影响文件

| 文件 | 变更 |
|---|---|
| [Optimizer.FoldLdlocaFieldAccess.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.FoldLdlocaFieldAccess.cs) | 新增 pass,约 220 行 |
| [JITCompiler.cs L294-L300](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L294-L300) | pipeline 插入 fold 调用 |

其他 pass 全部未动。

## 4. 覆盖场景

### 已 fold 场景(inline direct 路径)
| producer | consumer | receiver 位置 | Operand4 变化 |
|---|---|---|---|
| Ldloca / Ldloca_S | Ldfld_I1/U1/I2/U2/I4/U4/I8/R4/R8/Ref/Value/Ldfld | Register2 | 1(候选) |
| Ldloca / Ldloca_S | Stfld_I1/U1/I2/U2/I4/U4/I8/R4/R8/Ref/Value/Stfld | Register1 | 1(候选) |
| Ldloca / Ldloca_S | Initobj | Register1 | 不改(lowering 只看 IsRef) |
| Ldarga / Ldarga_S | 同上 | 同上 | 同上 |

### 未 fold 场景(走 Step 12b Ref-Slot 兜底,正确性由 `PropagateByRefReferentOffsets` 保障)
- **Ldflda consumer**: Ldflda Operand4 是二态编码(0=heap / 1=Ref-Slot 源),没有 inline direct 位;fold 会改变语义。**归 Step 17**(嵌套 struct 字段 byref 链)
- **byref R 有多点消费**:如 `Ldloca V, R; Call Foo(R); Call Bar(R)` — Ref-Slot 兜底
- **byref R 被中间指令使用**:producer 与首次 R 引用之间如果有其它引用 R 的指令,单一使用检查失败
- **跨基本块**:pass 只在 block 内扫描,跨块 byref 传递走 Ref-Slot 兜底

## 5. 验收证据

**JIT dump**(`NeoStep12StructWithRef`,`-c Debug_Neo`,`OUTPUT_JIT_RESULT`):

```
JIT Results / Optimizer Results / Final Results 三段对比:
    Ldloca.s r1, r0                     ← Translate 后仍存在
    stfld.i4.inline r1, r2, primOff=0   ← receiver 是 byref r1

Optimizer Results 里:
    (x)ldloca.s r1, r0                  ← 已加入 CanRemove
    stfld.i4.inline r0, r2, primOff=0   ← receiver 换成源 struct r0

Final Results(emit 后):
    stfld.i4.inline r0, r2, primOff=0   ← Ldloca 完全消失,单条指令
```

Ldfld_Ref / Stfld_Ref / Ldfld_R4 / Stfld_R4 变体同样命中。

## 6. Non-Goals(转移到 Step 17 / 其它 step)

- **Ldflda consumer**: 归 Step 17 与 `structRefOffset` 传播一起处理
- **跨基本块 fold**: 待后续 step 视需求
- **Ldflda 链折叠**(嵌套 `struct.a.b.c`): 归 Step 17
- **Ldsflda fold**: 静态字段走 heap 路径,不适用 inline direct

## 7. 交给下一位的三行结论

Step 12c 通过一个单向 pre-emit fold pass 消除了 Neo 模式下同帧 struct 字段访问的中间 Ref Slot 产物,复用现有 `CanRemove` 机制实现零 remap 成本,pipeline 内没有触碰任何其它 pass。fold pass 是纯优化,不承担正确性责任;正确性完全由 Step 12b `PropagateByRefReferentOffsets` 保底。Ldflda consumer / 跨块 fold 归 Step 17,当前实现已在 `NeoStep12StructWithRef` 等用例上验证 inline direct 命中,Neo 41/41 + Legacy 493/493 无回归。
