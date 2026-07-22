# Step 12c 验收 Checklist

## Fold pass 落地
- [x] `Optimizer.FoldLdlocaFieldAccess` pass 加在 [JITCompiler.cs L293](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L293) `EliminateConstantLoad` 之后、emit 循环之前
- [x] `Ldloca V, R` + `Ldfld_* R` 一对一 pattern 命中时 fold 成功(receiver 在 Register2)
- [x] `Ldloca V, R` + `Stfld_* R` 一对一 pattern 命中时 fold 成功(receiver 在 Register1)
- [x] `Ldloca V, R` + `Initobj R` 一对一 pattern 命中时 fold 成功(receiver 在 Register1)
- [x] `Ldarga P, R` + `Ldfld_*/Stfld_*/Initobj R` 一对一 pattern 命中时 fold 成功
- [x] `Ldflda` 不作为 consumer(Non-Goal,归 Step 17)
- [x] `R` 在块内除 consumer 外有其它读/写时,pass 主动放弃 fold,不改任何指令(通过 forward-scan 到 "first-ref" 实现)
- [x] fold 命中后 Ldfld/Stfld 的 consumer.Operand4 = 1(inline direct 候选);Initobj 无需改 Operand4(lowering 只看 IsRef)
- [x] `Ldloca V, R` / `Ldarga P, R` 指令下标加入 `CodeBasicBlock.CanRemove` 集合

## 下游 pass 兼容性
- [x] fold 完成后 emit 循环([L339-L344](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L339-L344))自动跳过被 fold 的 Ldloca/Ldarga
- [x] CleanupRegister 消除 byref R(无引用),V/P 保留
- [x] TypeSpecializeAndRenameNeoRegisters 对 consumer.Register2 = V 走同一 rename map,无类型冲突
- [x] AllocateLocalStackSpaces 不为已消除的 byref R 分配 slot
- [x] LowerNeoOffsets 根据 `localInfos[V].IsRef == false` + `Operand4 == 1` 编码为 `Operand4 = RefOffset + 1`(inline direct)

## Debug 符号一致性(emit 循环自动处理)
- [x] Ldloca/Ldarga 被 CanRemove 过滤时,emit 循环 [L322-L338](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L322-L338) 自动把该 CIL Instruction 的 addr 绑定到下一条存活指令
- [x] `addr[eh.HandlerStart/TryStart/...]` 在 [ILMethod.cs L741-L744](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/ILMethod.cs#L741-L744) 建立异常处理表时读取的地址正确
- [x] JIT dump(`OUTPUT_JIT_RESULT`)显示 fold 前后 block 结构一致

## 兼容性 & 正确性
- [x] `Operand4 < 0` Ref-Slot receiver 分支保留不变,fold 未命中 pattern 走 Step 12b 兜底
- [x] 未 fold 场景(比如 byref R 有共享消费)Neo 41/41 仍通过(Step 12b 兜底)

## 验收基准
- [x] NeoStep12Test 10 个用例全部通过(未修改用例)
- [x] Neo 全量 41/41 通过
- [x] Legacy 全量 493/493 无回归(fold pass 在 `#if ENABLE_NEO_MODE` 内)
- [x] `-c Debug` 与 `-c Debug_Neo` 双配置 0 错误
- [x] `NeoStep12StructWithRef` 通过 JIT dump 观察到目标 `Operand4 > 0` 命中 + `Ldloca` 已 `(x)` 标记并从 Final Results 消失

## 收尾
- [x] handoff.md 撰写完成(fold 覆盖场景 + 未覆盖场景 + Step 17 交互)
- [x] project_memory 更新(fold pass 已落地为 struct 访问 hot path 的标准优化)
