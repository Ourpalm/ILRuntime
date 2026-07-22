# Step 12c Tasks

## 工程约束(继承 Step 12b,每个 Task 必须遵守)
- **零运行时查表**:fold pass 只在 JIT 编译期执行,不引入 handler 侧新的运行时判断
- **无 fallback**:未 fold 成功的 pattern 直接保留,交给 Step 12b Ref-Slot receiver 路径兜底(handler 已实现)
- **不引入正确性责任**:Step 12b `PropagateByRefReferentOffsets` 已保证未 fold 正确;禁止在 fold pass 内产出必须依赖 fold 才正确的编码
- **单一 pre-emit pass**:利用现有 `CanRemove` 机制,不新增 post-pass、不引入 sentinel、不动 emit / CleanupRegister / TypeSpecialize / LowerNeoOffsets

## Task Dependencies
```
Task 1 (FoldLdlocaFieldAccess pass 骨架 + Ldloca+Ldfld 一对一)
  └─→ Task 2 (扩展到 Stfld / Initobj / Ldflda 消费者 + Ldarga producer)
       └─→ Task 3 (回归 + JIT dump 断言 + handoff)
```

---

## [x] Task 1: FoldLdlocaFieldAccess pass 骨架 + Ldloca+Ldfld 一对一
- **Depends On**: None
- **Description**:
  - 新增文件 [Optimizer.FoldLdlocaFieldAccess.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.FoldLdlocaFieldAccess.cs)(或加到 `Optimizer` partial class),整个 pass 用 `#if ENABLE_NEO_MODE` 保护
  - `Optimizer.FoldLdlocaFieldAccess(List<CodeBasicBlock> blocks, bool hasReturn)`:
    - 按 basic block 扫描 `block.FinalInstructions`
    - 识别 pattern:
      - producer:`Ldloca` 或 `Ldloca_S`,`producer.Register1 = R`(byref dst),`producer.Register2 = V`(源 local reg)
      - consumer:紧邻的下一条(跳过 `CanRemove`)`Ldfld_I1/U1/I2/U2/I4/U4/I8/R4/R8/Ref R, ...`(即 `consumer.Register2 = R`)
      - 单一使用检查:`R` 在整个 block 内除 consumer 之外无其它读/写(利用 `GetOpcodeSourceRegister` / `GetOpcodeDestRegister` 扫描)
    - 命中改写:
      - `consumer.Register2 = V`(receiver 换成源 struct)
      - `consumer.Operand4 = 1`(inline direct 候选标志,与 Translate L1341 一致)
      - `lst[j] = consumer`
      - `block.CanRemove.Add(producerIdx)`
  - JITCompiler.Compile 内在 `Optimizer.EliminateConstantLoad(blocks, hasReturn)`(L293)之后、emit 循环(L309)之前插入 `#if ENABLE_NEO_MODE Optimizer.FoldLdlocaFieldAccess(blocks, hasReturn); #endif`
- **验收**:
  - Debug_Neo 编译 0 错误
  - `NeoStep12StructWithRef`/其它含 `Ldloca+Ldfld` 的用例通过 JIT dump 观察到 `Ldloca` 已从最终指令消失、`Ldfld_*` 的 `Operand4` 变成 `RefOffset+1`(正数)
  - Neo 全量 41/41 通过

---

## [x] Task 2: 扩展到 Stfld / Initobj 消费者 + Ldarga producer
- **Depends On**: Task 1
- **Description**:
  - Task 1 pass 内扩展:
    - **producer 扩展**:`Ldarga` / `Ldarga_S`(参数 slot 作为 struct 载体,`producer.Register2 = param slot idx`)
    - **consumer 扩展**:
      - `Stfld_I1/U1/I2/U2/I4/U4/I8/R4/R8/Ref/Value`(**receiver 在 Register1**,`consumer.Register2 = src`;参见 [JITCompiler.cs L1367-L1369](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1367-L1369)、lowering [Optimizer.Neo.cs L488-L497](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L488-L497));fold 需要 `Operand4 = 1`
      - `Initobj`(**receiver 在 Register1**,唯一操作数;参见 [JITCompiler.cs L1067-L1069](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1067-L1069)、lowering [Optimizer.Neo.cs L314-L317](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L314-L317));fold 无需修改 Operand4,只改 Register1
    - **不含 Ldflda**:Ldflda Operand4 是二态编码(0/1),fold 语义与 inline direct 不兼容,归 Step 17
    - pass 内针对不同 opcode 用辅助函数返回 receiver register 的位置(Register1 vs Register2)
- **验收**:
  - `NeoStep12StructAssignment` / `NeoStep12InitobjDefault` 用例通过 JIT dump 观察到全部相关 `Ldloca/Ldarga` 已消失,Ldfld/Stfld/Initobj `Operand4 > 0`
  - Neo 41/41 通过

---

## [x] Task 3: 回归 + JIT dump 断言 + handoff
- **Depends On**: Task 2
- **Description**:
  - Neo 全量测试 41/41 通过
  - Legacy 全量测试 493/493 无回归(fold pass 在 `#if ENABLE_NEO_MODE` 内)
  - `-c Debug` / `-c Debug_Neo` 双配置 0 错误
  - 撰写 handoff.md:落地情况 + fold 未覆盖场景(共享 byref、跨块)+ 与后续 Step 17 交互点

---

# Task Dependencies(汇总)
- Task 2 depends on Task 1
- Task 3 depends on Task 2
