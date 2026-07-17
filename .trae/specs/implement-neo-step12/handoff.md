# Neo Step 12 Handoff

> **状态**:Step 12 主体功能已达成(值类型 flat 布局、Move/Ret/Initobj 值类型、cctor 恢复触发链、VTOR 清除)。**Ref Slot 机制在设计文档中被明确定义但从未在任何 step 明确实现**,这是需要在 Step 13 前通过一个新 step **Step 12b** 补齐的关键遗漏。
>
> **Handoff 时间点**:2026-07-17
>
> **相关 spec**:
> - [Step 12 spec](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12/spec.md)
> - [Step 12 tasks](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12/tasks.md)
> - [Step 12 checklist](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12/checklist.md)
> - [Step 11 handoff](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/handoff.md) — 前置基础
>
> **设计文档基线**:[object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) §15(Ref/Out 参数 + Ldloca/Ldflda,标注"已确定")、§25(ldelema 与嵌套字段引用)
>
> **步骤总纲**:[neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) Step 12(帧内值类型 + Inline 字段访问)

---

## 1. Step 12 已落地内容

### 1.1 值类型 Flat 布局(Task 1-2)
- [ILType.cs InitializeFields L1697-L1704](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1697-L1704) 拆分两条独立路径:`InitializeFieldsForStackObjectLayout`(Legacy)/ `InitializeFieldsForFlatLayout`(Neo)
- [InitializeFieldsForFlatLayout L1794](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1794) 实现自然对齐算法:
  - `GetPrimitiveSizeFromClrType` — bool/byte/sbyte=1,short/ushort/char=2,int/uint/float=4,long/ulong/double=8
  - `AlignUp(offset, alignment)` — 上取整
  - `GetFieldNaturalAlignment` — 递归嵌套 struct 用 `GetStructMaxAlignment(≤8)`
  - 引用字段仍占 4 字节 Primitives(存 mStack index)+ 1 个 ReferenceCount
- `refFieldPrimitiveOffsets[]` 数组缓存(Initobj 用)

### 1.2 AllocateSlotForType 值类型分支(Task 4)
- [JITCompiler.cs L557-L598](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L557-L598) 三分支:
  - primitive → widen 到 4 字节(SlotLayoutCompatible 约束)
  - IL Struct → `slot.Size = TotalPrimitiveSize`,`slot.RefCount = TotalReferenceCount`
  - 引用/CLR 类型 → 4 字节 mStack index + 1 ref slot
- struct-this ABI 在 [JITCompiler.NeoHelpers.cs L14](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.NeoHelpers.cs#L14) 中把 struct 类型的 this 寄存器标为 `declaringType`(struct 类型),自然走 struct slot 分支

### 1.3 Move handler 支持值类型(Task 7,合并 Step 12b/Move_Vt)
- [ILIntepreter.Neo.cs L484-L513](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L484-L513) 一条 Move handler 通吃:
  - `Unsafe.CopyBlock` primitive 部分(Operand2 = size)
  - `Operand`(refCnt)>0 时循环拷贝 mStack 引用槽
  - `Operand4 < 0` — 独立引用槽(mStack index 修复)
  - `Operand4 >= 0` — 值类型 ref 区(源 ref offset)
- 无 Move_Vt 新增,LowerMove 已合并进 LowerNeoOffsets

### 1.4 Initobj 帧内值类型(Task 11)
- [ILIntepreter.Neo.cs L1593-L1628](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L1593-L1628):
  - `Operand4 != 0` 走帧内路径:`Unsafe.InitBlock` primitives 清 0 + 清 mStack refs 循环
  - `Operand4 == 0` 走 boxed initobj 抛 `NotImplementedException("Initobj boxed: Step 13")`
- 引用类型 initobj:直接写 `*(int*)(frameBase + DstOffset) = -1`
- CLR 值类型 initobj:抛 NotImpl Step 13

### 1.5 Call/Ret 值类型传递(Task 10)
- `CopyNeoCallArguments` 通过 `NeoCallParamMap` 支持任意 primitive size 参数 CopyBlock
- `CopyNeoCallRefs` 处理引用参数
- Ret 值类型返回 [L1554-L1591](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L1554-L1591):`Unsafe.CopyBlock(retDst, ...)` + 引用循环拷贝
- 值类型 Newobj [L1439-L1445](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L1439-L1445):零初始化 primitives + 清 refs + 走 by-value 构造

### 1.6 struct-this ABI **参数槽分配部分,ABI 语义错误待返工**(Task 12 未闭环)
- 当前 [JITCompiler.cs L460-L469](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L460-L469) 对 `declaringType.IsValueType` 的 this 参数分配 **整个 struct 大小的 slot**(`TotalPrimitiveSize + TotalReferenceCount`);对应地 CopyNeoCallArguments 在调用时会 **CopyBlock 整个 struct** 到 callee 的 this 槽 —— 这是 **by-value + 需要 copyback** 的语义
- **该语义与 CLR 不符**:C# 里 struct 实例方法的 `this` 是 **byref** (managed pointer),callee 里 `this.field = x` 应该直接写 caller 帧上的 struct,**不需要 copyback**
- Task 12 tasks.md 描述过于简略(只写"BuildInitialRegisterTypes this 类型走 GetType"),未明确 ABI 语义应为 by-ref
- 结论:**struct-this ABI 应改为 Ref Slot 传 byref this**(caller emit `Ldloca t` 得到 Ref Slot,callee 内 `this.field` 走 Stind/Ldind 或 Ref-Slot-aware Ldfld/Stfld)。这本质上是 Ref Slot 的**第一个使用场景**,完全依赖 Step 12b 的基础机制
- **Step 12 当前的 by-value slot 分配代码需要在 Step 12b 中改写为 byref(8 字节 Ref Slot)分配**


### 1.7 cctor 恢复**触发链部分**(Task 13 半部分)
- [ILType.StaticInstance L156-L178](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L156-L178) 中 `#if ENABLE_NEO_MODE` 跳过 cctor 分支已经移除
- 静态构造器在 StaticInstance 首次访问时自动触发
- **Ldsfld / Stsfld handler 未实现** — cctor 恢复的能力闭环卡在此处;归 Step 12b

### 1.8 VTOR 清除(Task 14)
- Neo 分支(`ILIntepreter.Neo.cs`, `ILIntepreter.InvocationFrame.cs`)中**无** `ObjectTypes.ValueTypeObjectReference` 残留
- `ILIntepreter.Register.cs`(Legacy Register VM)保留 VTOR — 符合设计,Legacy 路径不动

### 1.9 CLR → IL 入口 InvocationFrame(Step 11 后新增基础设施)
- 新建 [ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs) 承载 CLR → IL 调用的参数封送 / 返回值拆包
- `ILIntepreter.Run` 通过 `InvocationFrame.Begin` 使用
- 其他 CLR → IL 入口(`InvocationContext.Invoke` / `DelegateAdapter.ILInvokeSub` / `CLRRedirections.MethodInfoInvoke`)在 `#if ENABLE_NEO_MODE` 分支下抛 NotImpl Step 13,防止走 Legacy 路径产生静默错误

### 1.10 hot path 优化(附带)
- `UncheckedList<T>.ExpandBySize(int count)` [Other/UncheckedList.cs L222-L236](file:///f:/SVN/ILRuntime/ILRuntime/Other/UncheckedList.cs#L222-L236) — 一次 O(1) `_size += count`,消除 IL 方法调用时 `Add(null)` 循环
- `PrepareNeoCallFrame` [ILIntepreter.Neo.cs L153-L175](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L153-L175) — 抽取 IL Call helper,消除 5-6 处样板重复
- `WriteNeoPrimitive` [ILIntepreter.InvocationFrame.cs L329](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs#L329) — 装箱 CLR primitive → Neo 帧字节的统一入口

---

## 2. 关键遗漏 — Ref Slot 机制

### 2.1 问题定位

设计文档 [§15 "Ref/Out 参数与 ldloca/ldflda"](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md#L998) 定义了完整的 **Ref Slot** 机制:

> Ref Slot 8 bytes = `(objectIndex: int, offset: int)`
> - objectIndex == -1 → 帧内非托管内存
> - objectIndex >= 0 → mStack 中的对象(ILTypeInstance / CLR 对象 / Array)

设计文档明确标注"**已确定**",且 §25(ldelema 嵌套字段)、§16.2(Ldflda CLR 对象)都是 Ref Slot 的直接应用。

**但在 [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) 的 step 划分中,Ref Slot 的实现被排到 Step 17(Ref/Out 参数),而在此之前的 Step 6-16 多处已经"使用"Ref Slot 概念但未实现**:

| Step | 引用 Ref Slot 的位置 | 是否实现 Ref Slot? |
|---|---|---|
| Step 8b L350 | "存入 mStack 预分配的 ref slot" | 这里的 "ref slot" 其实是 mStack 引用槽,不是 §15 定义的 8 字节 `(objectIndex, offset)` |
| Step 12 L554 | 异常对象存储 ref slot | 同上,mStack 引用槽 |
| Step 12 目标 3-4-6 | 帧内 struct 字段访问 Inline 指令 | **依赖 fold Ldloca_S+Stfld,但 fold pass 从未实现** |
| Step 13 L530 | CLRMethod 参数 ref slot | mStack 引用槽 |
| **Step 16 L602** | **ldelema 产生 Ref Slot `(arrIdx, elemIdx)`** | **§15 的 Ref Slot,但 Step 16 依赖只写 Step 7,未依赖 Step 17** — 依赖倒置 |
| Step 17 L620 | 首次明确定义 Ref Slot 8 字节 | **本 step 才做 Ref Slot 基础实现** |
| Step 18 | 值类型 newobj 用 Ref Slot 传 this | 依赖 Step 17 |

### 2.2 已经踩到的坑

- **Step 12 目标 3-4-6(Inline Ldfld/Stfld)从未真正工作过**:
  - [JITCompiler.cs L1211](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1211) 根据"字段类型是否 struct"判定 `Operand4 = 1`(inline path)
  - 但**caller 端 IL 里的 Ldloca_S 从未被 fold**,导致 Register2 语义未定义(既不是 mStack index 也不是 struct 起点)
  - 之所以 NeoStep6-11 的 31 个测试全过 —— 是因为它们**全都不含 struct**,inline 路径从未被真实执行过
  - NeoStep12Test 的 10 个用例暴露了这个 gap:全部因 `Ldloca_S not yet implemented (Step 6)` 或 `Ldsfld not yet implemented (Step 6)` 失败

### 2.3 归属决策

**Step 12 的 6 项设计目标里,第 3/4/6 项(Inline 版指令 + 帧内纯指针算术)从设计上就依赖 Ref Slot 基础机制**。这些目标本轮**不能达成**,必须先补齐 Ref Slot。

**决策**:
- **Step 12 保留现状,不再补齐"实为 Ref Slot 依赖"的目标**(inline 优化路径、Ldsfld、struct-this copyback)
- **新增 Step 12b**(见 §5),在 Step 13 之前专门实现 Ref Slot 基础机制 + 相关 handler + inline fold pass
- **Step 16/17/18** 的 Ref Slot 相关部分改为**依赖 Step 12b**,而不是重新定义 Ref Slot

---

## 3. NeoStep12Test.cs 现状

- 10 个用例文件:[TestCases/NeoStep12Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep12Test.cs)
- 当前状态:**全部 10 个失败**,错误类型分布:
  - 9 × `Neo: opcode Ldloca_S not yet implemented (Step 6)` — struct 字段访问、struct 实例方法调用、default initobj 等
  - 1 × `Neo: opcode Ldsfld not yet implemented (Step 6)` — StaticCctor 用例
- 测试文件**保留不动** — 作为 Step 12b 的验收目标

---

## 4. Step 12 tasks.md 完成状态复盘

| Task | 描述 | 状态 |
|---|---|---|
| Task 1 | InitializeFields 拆分两条独立路径 | ✅ 完成 |
| Task 2 | Flat Layout 自然对齐算法 | ✅ 完成 |
| Task 3 | Ldfld_Value/Stfld_Value opcode 确认 + ToString 更新 | ✅ 完成(存在 opcode 定义) |
| Task 4 | ExecuteNeo 共享变量区扩展 + Optimizer 更新 | ✅ 完成 |
| Task 5 | Ldfld/Stfld handler Operand4 双路径 | ⚠️ **inline 分支存在但从未被真实调用**(fold pass 缺失) |
| Task 6 | Ldfld_Value/Stfld_Value handler | ⚠️ 同 Task 5 |
| Task 7 | Move handler 扩展 refCount/srcRefOffset | ✅ 完成 |
| Task 8 | JIT lowering Operand4 标志 + 字段偏移 | ⚠️ 生成 Operand4=1 但下游无对应 fold,inline path 语义不闭环 |
| Task 9 | LowerMove pass 直接编码零查表 | ✅ 完成(已合并进 LowerNeoOffsets) |
| Task 10 | Call/Ret 值类型 | ✅ 完成 |
| Task 11 | Initobj 帧内路径 | ✅ 完成 |
| Task 12 | struct-this ABI 修复 | ❌ **ABI 语义错误**:当前分配整 struct slot 走 by-value + copyback,应该改为 8 字节 Ref Slot 走 by-ref。归 Step 12b 返工 |
| Task 13 | cctor 恢复 | ⚠️ **触发链恢复,但 Ldsfld/Stsfld handler 缺失** |
| Task 14 | VTOR 清除 | ✅ 完成 |
| Task 15 | NeoStep12Test 单元测试 | ⚠️ 用例文件已创建,10/10 失败 |
| Task 16 | 全量回归 + handoff | ⚠️ NeoStep6-11 保持绿(31/31),NeoStep12 归 Step 12b |

**回归验证**(Debug_Neo + Debug 双配置):
- ✅ Debug 构建 0 错误
- ✅ Debug_Neo 构建 0 错误
- ✅ NeoStep6/7/8/10/11 共 **31 个测试全通过**,0 回归

---

## 5. Step 12b — 后续必做工作

### 5.1 目标

补齐设计文档 §15 定义的 Ref Slot 基础机制 + Ldfld/Stfld inline 优化 fold pass,使 NeoStep12Test 10 个用例全部通过。

### 5.2 建议 scope

1. **Ref Slot 8 字节编码**
   - 在 `StackSlotInfo` / `AllocateSlotForType` 中新增 byref slot 类型(size=8, refCount=0)
   - JIT 为 Ldloca / Ldarga / Ldflda 的目标寄存器分配 8 字节 slot
2. **handler 实现**
   - `Ldloca / Ldloca_S / Ldarga / Ldarga_S`:emit `(-1, absoluteFrameOffset)`
   - `Ldflda`:
     - 堆 IL 对象 → `(objMStackIndex, fieldPrimitiveOffset)`
     - 帧内值类型 → `(-1, absoluteFrameOffset + fieldOffset)` 累积
     - CLR 对象 → `(objMStackIndex, fieldHash)`
   - `Ldsflda`:类比,静态字段专用
   - `Stind_* / Ldind_*`:按 objectIndex 分派(帧内 / ILTypeInstance / CLR 对象 / Array)
3. **Ldsfld / Stsfld handler**(cctor 配套)
   - `ILType.StaticInstance.Primitives` + `field.PrimitiveOffset` 走 flat 布局读写
   - 引用字段走 `staticInstance.ManagedObjects[refOffset]`
4. **struct-this ABI 用 Ref Slot 传 by-ref this**(**改写现有代码**)
   - **返工**:[JITCompiler.cs L460-L469](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L460-L469) 当前对 struct this 分配整 struct slot,改为分配 8 字节 Ref Slot
   - Caller emit `Ldloca_S t` 得到 `(-1, tOffset)` Ref Slot
   - 通过参数区传给 callee this
   - **Callee 内 `this.field` 依然是 `ldarg.0 + ldfld/stfld`**(C# 编译器实测输出,见 [TestCases.il](file:///f:/SVN/ILRuntime/TestCases/bin/Debug/netstandard2.1/TestCases.il) 中 `NeoStep12ThisTarget::Bump`:`IL_0001 ldarg.0 / IL_0002 ldarg.0 / IL_0003 ldfld A / IL_0008 ldarg.1 / IL_0009 add / IL_000a stfld A`)。ECMA-335 II.4.10 允许 `ldfld`/`stfld` 的 receiver 为 byref/managed pointer,编译器不会为 struct 实例方法生成 `stind_*/ldind_*`
   - 因此 **`Ldfld_*` / `Stfld_*` handler 必须新增 Ref Slot receiver 路径**:当 receiver 寄存器持有 8 字节 `(objIndex, offset)` Ref Slot 时,按 objIndex 三分派(`-1` 帧内 / `>=0` ILTypeInstance 或 CLR 对象)读写字段。不产生 struct 拷贝,不需要 copyback
   - CopyNeoCallArguments 对 struct-this 参数改为拷贝 8 字节 Ref Slot(而不是整个 struct 内容)
   - `Stind_*/Ldind_*` handler 仍需实现,但服务于 `ref`/`out` 参数写回、显式 byref 解引用等场景,不是 struct-this 主路径
5. **Fold pass — Ldfld/Stfld inline 优化**
   - 在 LowerNeoOffsets 之前跑
   - 识别 `Ldloca_S/Ldarga_S/Ldflda 帧内链 + Stfld/Ldfld/Initobj` 组合
   - Fold 成 `Stfld_*.inline` / `Ldfld_*.inline` 一步指令(编码累积 offset 到 Operand2)
   - 消除 Ldloca_S/Ldflda 中间产物
   - 逃逸场景(Ldloca 结果被 Call/Stind/其他 opcode 消费)保留原指令走 Ref Slot 路径

### 5.3 现有代码需要清理

- [JITCompiler.cs L1211](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1211) 和 L1244 的 `op.Operand4 = (type.IsValueType && !type.IsEnum) ? 1 : 0` 这行的语义要重新审视:
  - 现在的判定基于"字段属于什么类型",这不对 —— 应该基于"base 寄存器是不是帧内地址链"
  - Step 12b 用 fold pass 决定 inline 后,这行判定要么改为 fold pass 的产物,要么移除
- [Optimizer.Neo.cs L361-L362](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L361-L362) 的 `Operand4 = localInfos[r2].RefOffset + 1` 转换是配合 inline path 的 ref slot 索引编码,要在 fold pass 落地后重新对齐

### 5.4 与后续 step 的关系

- **Step 12b 落地后**:Step 16(数组访问 ldelema)、Step 17(ref/out 参数)、Step 18(值类型 newobj)都改为**依赖 Step 12b** 而不是自己定义 Ref Slot
- Step 17 简化为:只做 ref/out 参数在**方法边界跨帧传递**的 marshalling(caller/callee 侧的 Ref Slot 编码 + 参数封送),不再实现基础 Ref Slot 概念
- Step 16 简化为:ldelema handler emit Ref Slot,复用 Step 12b 的 Stind/Ldind 分派

---

## 6. 移交给下一 session 的要点

1. **不要动 Step 12 相关代码** — 现有 Step 12 主体工作已完成,回归稳定,Step 12b 应作为**独立 step** 增量补齐
2. **Step 12b 起草前** 必须先重读设计文档 §15 / §16 / §25,把 Ref Slot 相关的所有条款作为**已确定基线**,不再讨论 Ref Slot 的编码格式
3. **Step 12b spec 起草时应显式检查**:
   - Step 16 / Step 17 / Step 18 的 Ref Slot 引用点是否都被 Step 12b 覆盖
   - 是否有其他 Step 也隐式引用 Ref Slot(比如异常处理里的 ref slot 到底是 mStack 引用槽还是 §15 Ref Slot,需澄清)
4. **NeoStep12Test.cs 10 个用例是 Step 12b 的验收基准**,不要修改用例,要修改的是运行时/JIT
5. **教训**(已同步到 project_memory):
   - 每个 Step 的 spec 阶段**必须**逐条把设计文档相关章节映射为 Task,不能凭理解自由发挥
   - NeoStep*Test 用例设计前**必须**先查看对应 Step 的 scope 边界,避免覆盖后续 Step 的内容

---

## 7. 无遗留 TODO 代码

Step 12 相关代码路径中已经无隐藏 TODO / NotImpl 混淆项。所有 NotImpl 都明确指向后续 step:
- `Initobj boxed: Step 13`
- `CLR value type Initobj: Step 13`
- `Neo InvocationContext.Invoke has not been migrated to InvocationFrame yet (Step 13).`
- `Neo mode: DelegateAdapter.ILInvokeSub has not been migrated to InvocationFrame yet (Step 13).`
- `Neo mode: MethodInfoInvoke has not been migrated to InvocationFrame yet (Step 13).`
- 默认分支 `Neo: opcode {0} not yet implemented (Step 6)` 会捕获 Ldloca_S / Ldflda / Ldsfld / Stsfld / Stind / Ldind 等 —— Step 12b 中先拆成显式 case 归属 Step 12b,再逐个实现

---

## 8. 建议的下一步

1. 起草 [implement-neo-step12b](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12b) 目录下的 spec.md + tasks.md + checklist.md
2. 起草前先**扫描全文档**确认 Ref Slot 引用点已收敛
3. Step 12b 完成后回填 Step 16 / Step 17 / Step 18 的依赖列表
