# ILRuntime Neo Step 12 - Implementation Plan

## 工程约束（每个 Task 必须遵守）
- **局部变量复用**：handler case 块内严禁声明新的同类型局部变量；所有临时变量（byte*, int, object, ILTypeInstance）统一在 ExecuteNeo 方法头部共享区声明
- **零运行时查表**：所有偏移（sz/refCount/dstRefOffset/srcRefOffset/PrimitiveOffset/ReferenceOffset）必须在编译期填入 OpCodeR 字段；handler 运行时禁止访问 localInfos、fieldOffsets 等表
- **无 fallback**：未实现功能直接 throw NotImplementedException 标注归属 Step，不回退到 ValueTypeObjectReference

## Task Dependencies
```
Task 1 (拆分 InitializeFields)
  └─→ Task 2 (Flat Layout 自然对齐)
       └─→ Task 3 (Opcode 确认：Ldfld/Stfld_Value 存在，无 Move_Vt/Inline)
            └─→ Task 4 (ExecuteNeo 共享变量区扩展 + Optimizer 更新)
                 └─→ Task 5 (Ldfld/Stfld handler 双路径 Operand4)
                      └─→ Task 6 (Ldfld/Stfld_Value handler)
                           └─→ Task 7 (Move handler 扩展：refCount/srcRefOffset)
                                └─→ Task 8 (JIT lowering Operand4 + 字段偏移)
                                     └─→ Task 9 (LowerMove pass 直接编码)
                                          ├─→ Task 10 (Call/Ret 值类型)
                                          ├─→ Task 11 (Initobj 帧内)
                                          ├─→ Task 12 (struct-this ABI)
                                          ├─→ Task 13 (cctor 恢复)
                                          └─→ Task 14 (VTOR 清除)
                                               └─→ Task 15 (NeoStep12Test)
                                                    └─→ Task 16 (全量回归 + handoff)
```

---

## [ ] Task 1: 重构 InitializeFields 为两条独立路径
- **Priority**: high
- **Depends On**: None
- **Description**:
  - `InitializeFieldsForStackObjectLayout()`：完整 Legacy StackObject[] 逻辑
  - `InitializeFieldsForFlatLayout()`：Neo flat bytes 逻辑（Task 2 实现算法）
  - 入口 `InitializeFields()` 按 `#if ENABLE_NEO_MODE` 分发，方法体内无 `#if` 交错
  - 同步拆分静态字段初始化；方法名行为命名，无 Neo 字样
- **AC**: FR-1, AC-1
- **Test**:
  - `-c Debug` 编译 0 错误
  - Legacy 字段读写行为与重构前一致
  - 代码审查：方法拆分清晰，职责单一

---

## [ ] Task 2: Flat Layout 自然对齐算法
- **Priority**: high
- **Depends On**: Task 1
- **Description**:
  - AlignUp / GetFieldNaturalAlignment / GetFieldPrimitiveSize / GetStructMaxAlignment 辅助
  - 按自然对齐（1/2/4/8）向上对齐 primitiveOffset
  - 引用字段 Primitives +4 字节存 mStack index 位，ReferenceOffset++
  - 嵌套 ILType 递归 TotalPrimitiveSize/TotalReferenceCount
  - bool=1, char=2, enum=underlying type；末尾对齐到 struct 最大对齐（≤8）
  - 静态字段同算法
  - 修复现有 bug：引用字段未占 primitiveOffset
- **AC**: FR-2, AC-2
- **Test**:
  - `{byte a; long b; int c}` → a=0, b=8, c=16, Total=24
  - `{int id; object name; float val}` → id=0, name=4, val=8, Total=12, RefCnt=1
  - 嵌套 struct 偏移和引用计数递归正确

---

## [ ] Task 3: Opcode 确认与 ToString 更新
- **Priority**: high
- **Depends On**: Task 2
- **Description**:
  - **不新增 Move_Vt**；**不新增 _Inline 系列**
  - 确认 OpCodeREnum 中 `Ldfld_Value` / `Stfld_Value` 存在，缺失则追加到枚举末尾
  - 更新 ToString()：
    - Move 显示 refCount（Operand>1 时）
    - Ldfld/Stfld 显示 inline 标志（Operand4=1 时加 `.inline` 后缀）
    - Ldfld_Value/Stfld_Value 显示字段 size/refCnt
- **AC**: FR-3, AC-14
- **Test**:
  - `-c Debug_Neo` 编译 0 错误
  - ToString 输出可读
  - 代码审查：净增 opcode 为 0（仅确认/补齐已有）

---

## [ ] Task 4: ExecuteNeo 共享变量区扩展 + Optimizer 更新
- **Priority**: high
- **Depends On**: Task 3
- **Description**:
  - 在 [ILIntepreter.Neo.cs L377-L381](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L377-L381) 共享变量区追加 `byte* fieldBase;`
  - 不允许任何 case 块内新增同类型变量声明
  - 更新 Optimizer.Utils.cs / Optimizer.NeoTypeSpecialize.cs 支持 Ldfld_Value/Stfld_Value（不涉及任何 _Inline 或 Move_Vt）
  - Move 作为已有 opcode，无需新增 case；但 TypeSpecialize 需确保 Move dst type = src type（对值类型 Move 也成立）
- **AC**: FR-4
- **Test**:
  - TypeSpecialize 不抛 unhandled opcode
  - 代码审查：所有需要 byte* 的地方复用 fieldBase

---

## [ ] Task 5: Ldfld/Stfld handler Operand4 双路径
- **Priority**: high
- **Depends On**: Task 4
- **Description**:
  - 改写 11 个 Ldfld (I1/U1/I2/U2/I4/U4/I8/U8/R4/R8/Ref) 和 11 个 Stfld handler
  - 统一使用共享变量 `ins`、`fieldBase`；不在 case 内声明新变量
  - 每个 handler 结构：
    ```
    if (ip->Operand4 != 0)
        fieldBase = frameBase + ip->SrcOffset;  // Ldfld
    else
    {
        ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
        fieldBase = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ins.Primitives.AsSpan()));
    }
    // 之后统一 fieldBase + Operand2 读
    ```
  - Stfld：Operand4=1 时 fieldBase = frameBase + DstOffset
  - Ldfld_Ref inline：srcIdx = frameRefBase + srcSlotRefOffset + Operand3（srcSlotRefOffset 编码方案见 Open Questions；候选：Register3/Register4 可用字段）
  - sub-int 读：符号/零扩展到 4 字节写 dst；写：只写低字节/半字
  - Operand4=0 路径行为完全等价于改写前（回归保证）
- **AC**: FR-5, AC-3, AC-5, AC-8
- **Test**:
  - NeoStep7-11 回归（Operand4=0 路径验证）
  - 单测：Operand4=1 帧内读写正确
  - 代码审查：无 case-scope 局部变量

---

## [ ] Task 6: Ldfld_Value/Stfld_Value handler
- **Priority**: high
- **Depends On**: Task 5
- **Description**:
  - 编码：Operand=vt TotalPrimitiveSize, Operand2=field PrimitiveOffset, Operand3=field ReferenceOffset, Operand4=inline flag
  - fieldBase 复用共享变量，路径选择同 Task 5
  - CopyBlock primitive 部分
  - 引用拷贝逻辑（与 Move 共享辅助函数 `CopyValueTypedBytes`）
  - dst 字节区域的 ref index 修复（dst 是独立 slot）
- **AC**: FR-6, AC-4
- **Test**:
  - 嵌套 struct 字段拷贝 primitive 完整
  - 含引用嵌套 struct 浅拷贝正确
- **Notes**: 提取 CopyValueTypedBytes 辅助函数供 Move/Call/Ret/Ldfld_Value 复用；所有参数来自指令字段或共享变量，不查表

---

## [ ] Task 7: Move handler 扩展（统一值类型拷贝，无 Move_Vt）
- **Priority**: high
- **Depends On**: Task 6
- **Description**:
  - 改写现有 Move handler：
    - 总是 `Unsafe.CopyBlock(frameBase+DstOffset, frameBase+SrcOffset, (uint)Operand2)`
    - 若 Operand (refCount) > 0：
      - dstRefBase = frameRefBase + Operand3
      - srcRefBase = frameRefBase + Operand4
      - for i in 0..Operand-1: mStack[dstRefBase+i] = mStack[srcRefBase+i]
      - 若 Operand == 1 且 Operand2 == 4（独立 ref slot）：保留原有 null/index 修复逻辑
      - 若 Operand > 1 或 Operand2 > 4（值类型）：不做 inline index 修复（约定：值类型内 ref 字段不依赖 inline index）
    - refCount == 0：纯 CopyBlock（快速路径）
  - 删除 Operand==1 的 bool 判断，改为 refCount（int）循环
  - DEBUG 断言：Operand2、Operand 与类型一致
- **AC**: FR-7, AC-6
- **Test**:
  - Vector3 (refCnt=0) Move 纯 CopyBlock 正确
  - object (refCnt=1, sz=4) Move 引用/ null 正确（原有语义不变）
  - 含引用 struct Move 引用浅拷贝正确

---

## [ ] Task 8: JIT lowering Operand4 标志 + 字段偏移
- **Priority**: high
- **Depends On**: Task 7
- **Description**:
  - 修改 JIT Ldfld/Stfld 生成：
    - 若对象寄存器类型是 ILType && IsValueType && !IsEnum：op.Operand4=1；Register2(Ldfld)/Register1(Stfld) 指向值类型 slot；Operand2=field.PrimitiveOffset; Operand3=field.ReferenceOffset；srcSlotRefOffset 编到 Register3/Register4 或其他空闲字段
    - 否则：op.Operand4=0；保持现有逻辑
  - 嵌套值类型：中间 Ldfld_Value 目标类型为字段值类型，后续字段访问继续 Operand4=1
  - ldsfld/stsfld：Operand4=0；字段偏移用 fieldOffsets 正确值（修正现有硬编码）
  - 所有字段偏移（PrimitiveOffset/ReferenceOffset）编译期填入 Operand2/Operand3
- **AC**: FR-9, AC-3, AC-4
- **Test**:
  - 断点验证：值类型 local 字段访问 Operand4=1
  - class 字段访问 Operand4=0

---

## [ ] Task 9: LowerMove pass（直接编码，零查表）
- **Priority**: high
- **Depends On**: Task 8
- **Description**:
  - 在所有优化 pass 之后、AllocateLocalStackSpaces 之后新增 LowerMove
  - 遍历指令，对每条 Move：
    ```
    srcInfo = frame.LocalInfos[srcReg]
    dstInfo = frame.LocalInfos[dstReg]
    DEBUG assert srcInfo.Size == dstInfo.Size && srcInfo.RefCount == dstInfo.RefCount
    op.Operand  = srcInfo.RefCount
    op.Operand2 = srcInfo.Size
    op.Operand3 = dstInfo.RefOffset
    op.Operand4 = srcInfo.RefOffset
    ```
  - 不新增 Move_Vt，所有 Move 统一上述编码
  - 运行时 Move handler 不访问 localInfos
- **AC**: FR-8, AC-6, AC-16
- **Test**:
  - Vector3 赋值：Operand=0, Operand2=12, Operand3/4 正确
  - object 赋值：Operand=1, Operand2=4
  - 含引用 struct 赋值：Operand=N, Operand2=sz, Operand3/4 为 ref offsets

---

## [ ] Task 10: Call/Ret 值类型
- **Priority**: high
- **Depends On**: Task 9
- **Description**:
  - CopyNeoCallArguments 值类型参数：CopyBlock primitive + 引用拷贝（复用 CopyValueTypedBytes）
  - 参数映射 (NeoCallParamMap) 包含 PrimitiveSize/RefCount/dstRefOffset，编译期填入
  - Ret 值类型返回：CopyBlock 到 retDst + 引用拷贝到 targetRetRefBase
  - 所有偏移来自参数映射，不查 localInfos
  - 无 VTOR 依赖
- **AC**: FR-10, AC-7
- **Test**:
  - Vector3 参数传 callee 字段正确
  - 值类型返回 caller 接收正确

---

## [ ] Task 11: Initobj 帧内路径
- **Priority**: medium
- **Depends On**: Task 10
- **Description**:
  - 帧内值类型 InitBlock primitive 为 0
  - 遍历类型缓存的 `RefFieldPrimitiveOffsets` 在对应字节位置写 -1
  - 类型加载时缓存 ref field offsets 列表（InitializeFieldsForFlatLayout 中构建）
  - boxed initobj 抛 NotImplementedException("Step 13")，无 VTOR 回退
- **AC**: FR-11, AC-11
- **Test**:
  - initobj 后 primitive=0, ref 字段=null

---

## [ ] Task 12: struct-this ABI 修复
- **Priority**: high
- **Depends On**: Task 11
- **Description**:
  - BuildInitialRegisterTypes 中 this 类型改走 appdomain.GetType(method.Definition.DeclaringType) 统一路径
  - 验证 appdomain.GetType 缓存/去重
- **AC**: FR-12, AC-9
- **Test**:
  - struct 实例方法内 Move this→local 无 DEBUG layout mismatch

---

## [ ] Task 13: cctor 恢复
- **Priority**: medium
- **Depends On**: Task 12
- **Description**:
  - 移除 ILType.StaticInstance 和 InitializeMethods 中 `#if ENABLE_NEO_MODE` 跳过 cctor 分支
- **AC**: FR-13, AC-10
- **Test**:
  - `class C{static int X=42}` C.X = 42
  - NeoStep6-11 无回归

---

## [ ] Task 14: VTOR 清除
- **Priority**: high
- **Depends On**: Task 13
- **Description**:
  - `#if ENABLE_NEO_MODE` 下 ExecuteNeo 删除 VTOR 分支
  - JIT lowering 不生成 VTOR 依赖
  - Grep `ObjectTypes.ValueTypeObjectReference` 验证 Neo 路径无运行时引用
  - Legacy Register VM VTOR 保留
- **AC**: FR-14, AC-13
- **Test**:
  - Grep 验证

---

## [ ] Task 15: NeoStep12Test 单元测试
- **Priority**: high
- **Depends On**: Task 14
- **Description**:
  - 新建 TestCases/NeoStep12Test.cs
  - 10 个用例：BasicFieldAccess(Vector3), Alignment(byte+long+int), NestedStruct, StructWithRef, StructAssignment(Move 扩展), StructMethodCall(struct-this), StructParam, StaticCctor, InitobjDefault, BoolByteField
- **AC**: AC-2..AC-11
- **Test**:
  - 10 个用例单跑+全组通过
  - 失败输出 expected/actual
  - 单用例 5-20ms，无死循环

---

## [ ] Task 16: 全量回归 + handoff
- **Priority**: high
- **Depends On**: Task 15
- **Description**:
  - Neo 模式：NeoStep6/7/8/9/10(pre-existing)/11/12
  - Legacy 模式：useRegister=false 全套
  - Legacy Register VM：-c Debug useRegister=true
  - `-c Debug` 和 `-c Debug_Neo` 两配置编译 0 错误 0 警告
  - 回填文档备注，更新 project_memory.md，撰写 handoff.md
- **AC**: AC-8, AC-12, AC-15, AC-16
- **Test**:
  - 全部测试通过（除 pre-existing NRE）
  - handoff 清晰
