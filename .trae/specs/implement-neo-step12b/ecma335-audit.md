# Neo 模式 ECMA-335 合规审计 — Step 12b 及此前步骤

> **审计时间**:2026-07-17
>
> **审计范围**:Step 1 至 Step 12 已落地实现 + Step 12b 待做工作
>
> **大原则**:Neo 模式所有已实现路径必须严格符合 ECMA-335 规范。唯一豁免场景是 C# 语言层无法表达的能力(如 P/Invoke、不受托管的 unmanaged pointer 存储、cpblk/localloc 等极少数指令与 CLR 交互层面的限制)。任何偏离规范的实现都必须显式记录并计划返工,不得默认作为技术债保留。
>
> **相关文档**:
> - 设计基线:[object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md)
> - 步骤拆解:[neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md)
> - Step 12 handoff:[implement-neo-step12/handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12/handoff.md)
>
> **本文档职责**:仅收敛 Step 12b 及之前步骤(Step 1–12)引入的 ECMA-335 语义偏差。后续 step(Step 13–26)相关的合规检查项已作为**每 step 内 "ECMA-335 检查项" 子节**回填至 `neo-implementation-steps.md`,不重复列在此。
>
> **不属于合规问题的项目**:纯性能优化、抽象层清理、命名规范、代码组织等非规范性问题不在此文档 scope 内,详见 Step 12 handoff §5。

---

## 1. 违规清单(必修)

按严重度与影响面排序。每条含:规范引用、当前实现偏差、代码位置、修复策略、归属 Task。

---

### A1. Struct-this ABI — `this` 必须是 managed pointer,非 by-value 拷贝

**规范条款**:ECMA-335 II.13.3 "Instance methods of value types"
> On a call to an instance method of a value type, the this pointer shall be a managed pointer to an instance of that value type.

**当前实现偏差**:
- [JITCompiler.cs L460-L469](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L460-L469) 对 struct-this 参数分配整个 struct primitive size + ref count 的 slot
- `CopyNeoCallArguments` 通过 `NeoCallParamMap` 按 primitive size CopyBlock 整个 struct 到 callee this 槽 → **by-value 语义**
- 无 copyback 逻辑 → callee 内 `this.field = x` 对 caller 帧上的原始 struct 无效
- 该偏差与 ECMA-335 II.13.3 明确规定的 managed pointer 语义直接冲突,任何依赖 struct 实例方法修改自身字段的场景在 Neo 模式下都会失败

**修复策略**(Step 12b):
- 将 struct-this 参数 slot 改为 8 字节 Ref Slot(`(objectIndex=-1, offset=absoluteFrameOffset)`)
- Caller emit `Ldloca_S t` 等价物,把 Ref Slot 编码到 callee 参数区
- Callee 内 `ldarg.0` 加载 Ref Slot,后续 `ldfld/stfld` 按 Ref Slot receiver 路径分派
- **不产生 struct 拷贝,不需要 copyback**
- 移除 `CopyNeoCallArguments` 对 struct-this 参数的整块 CopyBlock,改为 8 字节 Ref Slot 拷贝

**归属**:Step 12b Task A1

---

### A2. `Ldfld_*` / `Stfld_*` 必须支持 receiver 为 managed pointer

**规范条款**:ECMA-335 II.4.10 "ldfld/stfld"
> The stack transitional behavior … obj shall be an object, a value type, or a pointer to a value type.

以及 III.4.10 表 4-1 中 `ldfld` 的合法 receiver 类型包含 `& (managed pointer)` 和 `* (unmanaged pointer)`。

**当前实现偏差**:
- 现有 [Ldfld_*/Stfld_* handler](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) 只有两条路径:
  - `Operand4 == 1` inline 分支(receiver 为帧内 struct 起点直接偏移)
  - `Operand4 == 0` 堆对象分支(receiver 为 mStack index,取 `ILTypeInstance.Primitives`)
- **完全没有** receiver 为 8 字节 Ref Slot 的分派路径
- C# 编译器对 struct 实例方法生成的 `this.field` 访问就是 `ldarg.0` + `ldfld/stfld`(见 [TestCases.il](file:///f:/SVN/ILRuntime/TestCases/bin/Debug/netstandard2.1/TestCases.il) `NeoStep12ThisTarget::Bump`),而不是 `ldind_*/stind_*` —— A1 一旦改为 Ref Slot,若不同步给 `ldfld/stfld` 加 Ref Slot 路径,struct 实例方法会立即崩溃

**修复策略**(Step 12b):
- 新增 `Ldfld_*` / `Stfld_*` receiver 类型的第三条分支:receiver 是 Ref Slot(8 字节 `(objectIndex, offset)`)
- 按 `objectIndex` 三分派:
  - `-1` → 帧内 struct,读写 `*(T*)(nativePointer + offset + fieldPrimitiveOffset)`
  - `>=0 && mStack[objectIndex] is ILTypeInstance` → 通过 `Primitives.AsSpan()` 加字段偏移读写
  - `>=0 && mStack[objectIndex] is CLR object` → `CLRType.GetFieldValue/SetFieldValue` + writeback
- JIT 侧:识别 base 寄存器持 Ref Slot 时,emit 该分支的指令变体或用统一 handler 内运行时检查

**归属**:Step 12b Task A2

---

### A3. Ref Slot 基础机制未实现 — Ldloca/Ldarga/Ldflda/Ldsflda/Stind/Ldind 全缺失

**规范条款**:
- ECMA-335 III.3.43 `ldloca` / III.3.29 `ldarga` — 产生 managed pointer 指向本地变量/参数
- III.4.12 `ldflda` / III.4.28 `ldsflda` — 产生 managed pointer 指向字段
- III.4.26 `stind.*` / III.4.14 `ldind.*` — 通过 managed pointer 读写
- I.12.1.1.2 定义 managed pointer(`&T`)语义

**当前实现偏差**:
- 上述所有指令在 Neo 模式下都落到默认分支抛 `Neo: opcode {0} not yet implemented (Step 6)`
- 设计文档 §15 明确标注"已确定"了 Ref Slot 8 字节 `(objectIndex: int, offset: int)` 布局,但没有任何 step 曾经真正实现
- Step 6-16 多处 spec 引用"ref slot",但实际指向的是 mStack 引用槽(第 6 节 ManagedStack 的 slot),不是 §15 定义的 Ref Slot,存在**设计术语与实现术语混淆**

**修复策略**(Step 12b):
1. **Slot 分配**:在 `StackSlotInfo` / `AllocateSlotForType` 中新增 byref slot 类型(`Size=8, RefCount=0`)
2. **Handler 实现**:
   - `Ldloca / Ldloca_S / Ldarga / Ldarga_S` → 写 `(-1, absoluteFrameOffset)` 到目标 slot
   - `Ldflda`:
     - 堆 IL 对象 → `(objMStackIndex, fieldPrimitiveOffset)`
     - 帧内值类型 → `(-1, absoluteFrameOffset + fieldOffset)` 累加
     - CLR 对象 → `(objMStackIndex, fieldHash)` (与 Legacy 一致)
   - `Ldsflda` → 静态字段专用,`(staticInstanceMStackIndex, fieldOffset)`
   - `Stind_* / Ldind_*` → 按 `objectIndex` 分派:
     - `-1` → 帧内 `*(T*)(nativePointer + offset)`
     - IL 对象 → 通过 `Primitives.AsSpan()` 定位 + `ReadUnaligned/WriteUnaligned`(重取 managed ref 避免 GC 移动导致悬空)
     - CLR 对象 → `CLRType.GetFieldValue/SetFieldValue` + writeback
     - Array → `arr.GetValue/SetValue` (设计文档 §15.2 已定义)
3. **移除临时占位**:清理 [ExecuteNeo 默认分支的 "opcode not yet implemented (Step 6)"](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) 对上述指令的兜底,拆为显式 case

**归属**:Step 12b Task A3(核心基础机制)

---

### A4. Ldsfld / Stsfld handler 缺失导致静态构造器实际不会被触发

**规范条款**:ECMA-335 I.8.9.5 "Instance and type initialization"
> Prior to any access of a static field or invocation of a static method of a type, its type initializer (if any) shall be invoked.

**当前实现偏差**:
- Step 12 已把 [ILType.StaticInstance L156-L178](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L156-L178) 内 `#if ENABLE_NEO_MODE` 跳过 cctor 的分支移除,cctor 触发链已在 `StaticInstance` getter 上重建
- **但** `Ldsfld` / `Stsfld` 在 Neo 模式下落到默认 not-implemented 分支,IL 代码永远不会命中 `StaticInstance` getter → **cctor 实际上从未被触发**
- 一旦 Ldsfld/Stsfld handler 落地并调用 `ILType.StaticInstance`,cctor 触发链才闭环
- 当前状态下 Neo 模式对任何有静态字段的类型都无法正常执行,直接违反 I.8.9.5

**修复策略**(Step 12b):
- 实现 `Ldsfld` / `Stsfld` handler:
  - IL 类型 → `type.StaticInstance` 触发 cctor,通过 `field.PrimitiveOffset` 读写 `Primitives`
  - 引用字段 → `staticInstance.ManagedObjects[refOffset]`
  - CLR 类型 → 通过 `CLRType.GetStaticFieldValue/SetStaticFieldValue`
- Ldsfld 命中前 `type.StaticInstance` 会自动触发 `appdomain.Invoke(staticConstructor)`(Legacy 保持一致)

**归属**:Step 12b Task A4

---

### A5. Sub-int primitive 帧 slot widen 后 `Ldfld_U1/I1/U2/I2` 必须做符号/零扩展

**规范条款**:ECMA-335 III.1.1.1 "Numeric data types" + 表 III.6 `ldfld` 类型转换规则
> When loading integers of size less than 4 bytes onto the evaluation stack, they shall be sign-extended (for signed types) or zero-extended (for unsigned types) to fill the 32-bit stack slot.

**当前实现偏差**(**审计新发现**):
- [AllocateSlotForType L557-L598](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L557-L598) 对 sub-int primitive local 一律 widen 到 4 字节,这是正确对齐 III.1.1.1 evaluation-stack 语义的做法
- **但** struct 字段布局按自然对齐(`bool/byte=1, short=2`,见 [InitializeFieldsForFlatLayout L1794](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1794))
- `Ldfld_U1/I1/U2/I2/Boolean` 从 struct 的 1/2 字节字段搬运到 4 字节帧 slot 时,如果仅按字段原始宽度 CopyBlock,只写入 slot 的低 1/2 字节,**高位字节保留 slot 上一次残值**
- 后续 `Ceq / Brtrue / Add` 等按 4 字节读取,直接读到残值污染的高位 → 比较结果错误、算术溢出错误
- 这是 evaluation-stack 语义与紧凑字段布局之间的桥接层,当前 Ldfld_* handler 未见显式 sign/zero extension 代码,需审计每个具体 handler

**修复策略**(Step 12b):
- 审计每个 `Ldfld_U1 / Ldfld_I1 / Ldfld_U2 / Ldfld_I2 / Ldfld_Boolean` handler,确认写入 dst slot 时:
  - `Ldfld_U1 / Ldfld_Boolean / Ldfld_U2` → 高位补 0(等价 `*(int*)dst = *(byte*)src` 或 `*(uint*)dst = *(ushort*)src`)
  - `Ldfld_I1 / Ldfld_I2` → 符号扩展(`*(int*)dst = *(sbyte*)src` 或 `*(int*)dst = *(short*)src`)
- 相同规则应用到 `Ldsfld_*`(从 `StaticInstance.Primitives` 加载)以及未来的 `Ldind_U1/I1/U2/I2`
- 相反方向 `Stfld_U1/I1/U2/I2` 只需按字段宽度 CopyBlock 低位即可,高位丢弃是正常语义

**归属**:Step 12b Task A5

---

## 2. 设计文档需回填的规则

以下为**当前实现正确但设计文档未明确阐述**的规则,需在 Step 12b 落地前同步更新设计文档,避免后续开发者按旧文档做出错误改动。

---

### D1. 设计文档 §4.3 需回填:帧内 primitive slot 使用 CIL evaluation-stack 宽度

**当前文档现状**:[object-model-neo-design.md §4.3](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) 只写了"帧上值类型字段布局必须与 CLR StructLayout 一致"、"字段按自然对齐"。

**实际实现规则**:
- **帧内 primitive local / temp slot** → CIL evaluation-stack 宽度(sub-int 一律 widen 到 4 字节)
- **struct 字段布局(含帧内 struct 内部)** → CLR StructLayout,按自然对齐,`bool`/`byte` 占 1 字节

这两条规则并存且不冲突 —— 前者约束**独立** slot 大小,后者约束 struct 内部字段布局。跨越两个域的读写(如 `Ldfld_U1` 从 struct 字段搬到独立 slot)必须在指令 handler 内做 sign/zero extension(见 A5)。

**回填内容**:在设计文档 §4.3 新增一段:

> **两种宽度规则并存**:
> - 独立帧内 primitive slot(用户 local + 编译器 temp)按 CIL evaluation-stack 宽度分配(bool/byte/sbyte/short/ushort/char 一律 4 字节),保证 SlotLayoutCompatible + 整数运算指令按 4 字节读写不会污染邻近 slot
> - struct 内部字段(无论 struct 在堆或在帧)按 CLR StructLayout 自然对齐,sub-int 字段占实际字节数
> - `Ldfld_*` / `Ldsfld_*` / `Ldind_*` 加载 sub-int 字段到独立 slot 时必须做符号扩展(有符号)或零扩展(无符号)。反向 `Stfld_*` / `Stsfld_*` / `Stind_*` 按字段宽度截断写入
> - `Move` / `Move_Vt` 在独立 slot 之间搬运数据时按 slot 宽度整体拷贝,不涉及扩展/截断

**归属**:Step 12b 起草前作为文档 patch 完成

---

### D2. 设计文档 §15 Ref Slot 定义应上升为跨 step 共享的基础机制

**当前文档现状**:Ref Slot 8 字节 `(objectIndex, offset)` 的完整定义在 §15(排在 Step 17 的功能范围章节内),但 §12.5 Ldfld_*_Inline、§13.6 接口分派、§16 数组 ldelema、§18.5 constrained callvirt、§19 值类型 newobj、§22.3 stelem 都在概念层面依赖 Ref Slot,导致读者需要跳读第 15 节才能理解前面章节。

**回填内容**:
- 在设计文档 §2(Neo 解释器设计目标)之后新增一节 "§2.5 基础抽象:Ref Slot(managed pointer 表示)",把 §15.2 的 Ref Slot 定义前置提炼为一段独立说明,后续所有章节引用时只需引用 §2.5 而非 §15
- §15 保留 Ldloca/Ldflda/Ldsflda/Stind/Ldind 的具体指令语义,不重复 Ref Slot 编码定义

**归属**:Step 12b 起草前作为文档 patch 完成

---

### D3. `neo-implementation-steps.md` step 拆分依赖关系需重排

**当前拆分现状**:Step 6-16 多处 spec 描述隐式使用 Ref Slot 概念,但 Ref Slot 正式实现被排到 Step 17。Step 12 通过引入 Step 12b 修正此错位。

**回填内容**:
- 在 Step 12b 落地后,同步更新 [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) 的依赖关系图:
  - Step 12b 依赖 Step 12,是 Step 13/16/17/18 的前置
  - Step 16 数组 `ldelema` 依赖 Step 12b(Ref Slot 已建立)
  - Step 17 精简为**跨帧 ref/out 参数封送**,不再重新定义 Ref Slot
  - Step 18 值类型 newobj 依赖 Step 12b 而非 Step 17

**归属**:Step 12b 落地后作为文档 patch 完成

---

## 3. Step 12b 执行 Checklist

以下为可直接对应 Task 的可执行清单。**每一项对应 §1 的一条违规**,不遗漏。

### 前置文档更新
- [ ] D1:设计文档 §4.3 新增 "两种宽度规则并存" 段落
- [ ] D2:设计文档 §2.5 新增 "Ref Slot 基础抽象" 段落,§15 精简为指令语义章节
- [ ] D3:实施步骤文档更新依赖关系图(Step 12b 落地后再回填)

### 实现任务(与 §1 一一对应)
- [ ] **A1 Struct-this ABI**:
  - [ ] [JITCompiler.cs L460-L469](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L460-L469) struct-this slot 分配改为 8 字节 Ref Slot
  - [ ] Caller 端 emit Ref Slot `(-1, thisSlotOffset)` 写入 callee 参数区
  - [ ] `CopyNeoCallArguments` / `NeoCallParamMap` 对 struct-this 参数改为 8 字节拷贝
  - [ ] 移除任何隐含的 struct-this by-value copyback 逻辑(当前不存在,但需确认无回退隐患)
- [ ] **A2 Ldfld/Stfld Ref Slot receiver 分支**:
  - [ ] `Ldfld_*` / `Stfld_*` handler 新增 receiver 为 Ref Slot 的第三条路径
  - [ ] 按 `objectIndex` 三分派(帧内 / IL 对象 / CLR 对象)
  - [ ] JIT 侧根据 receiver 寄存器 slot 类型决定生成哪个变体或使用统一 handler
  - [ ] 重新审视 [JITCompiler.cs L1211](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1211) 和 L1244 的 `op.Operand4 = (type.IsValueType && !type.IsEnum) ? 1 : 0` 判定,统一 receiver 类型识别到 Ref Slot 路径
- [ ] **A3 Ref Slot 基础机制**:
  - [ ] `StackSlotInfo` / `AllocateSlotForType` 支持 byref slot 类型(Size=8, RefCount=0)
  - [ ] `Ldloca` / `Ldloca_S` / `Ldarga` / `Ldarga_S` handler
  - [ ] `Ldflda` handler(帧内/IL 对象/CLR 对象三分派)
  - [ ] `Ldsflda` handler(触发 cctor + 定位 StaticInstance)
  - [ ] `Stind_I / Stind_I1 / Stind_I2 / Stind_I4 / Stind_I8 / Stind_R4 / Stind_R8 / Stind_Ref` handler
  - [ ] `Ldind_I / Ldind_I1 / Ldind_U1 / Ldind_I2 / Ldind_U2 / Ldind_I4 / Ldind_U4 / Ldind_I8 / Ldind_R4 / Ldind_R8 / Ldind_Ref` handler(含 sub-int 扩展,见 A5)
  - [ ] 拆分 ExecuteNeo 默认分支的 "opcode not yet implemented (Step 6)" 为显式 case,归属 Step 12b
- [ ] **A4 Ldsfld / Stsfld handler**:
  - [ ] `Ldsfld_*` handler 覆盖所有 primitive + reference + Value(struct copy)变体
  - [ ] `Stsfld_*` handler 同上
  - [ ] IL 类型:通过 `type.StaticInstance` 触发 cctor,访问 `Primitives` + `ManagedObjects`
  - [ ] CLR 类型:通过 `CLRType.GetStaticFieldValue/SetStaticFieldValue`
  - [ ] 端到端回归:定义 `class C { static int X = 42; }` 用例读到 42
- [ ] **A5 Sub-int extension 审计**:
  - [ ] 审计现有 `Ldfld_U1 / Ldfld_I1 / Ldfld_U2 / Ldfld_I2 / Ldfld_Boolean` handler,确认 sign/zero extension 正确
  - [ ] 新增/修正 `Ldsfld_*` 对应变体
  - [ ] 新增 `Ldind_*` 对应变体(见 A3)
  - [ ] `Stfld_*` / `Stsfld_*` / `Stind_*` 按字段宽度截断即可,无需额外处理
- [ ] **Fold pass — Ldfld/Stfld inline 优化**(可选,若时间充裕):
  - [ ] 在 LowerNeoOffsets 之前跑
  - [ ] 识别 `Ldloca_S/Ldarga_S/Ldflda 帧内链 + Stfld/Ldfld/Initobj` 组合
  - [ ] Fold 成 `Stfld_*.inline` / `Ldfld_*.inline` 一步指令
  - [ ] 逃逸场景保留原 Ref Slot 路径

### 验收
- [ ] NeoStep12Test 10 个用例全部通过(handoff §3 定义的验收基准)
- [ ] NeoStep6/7/8/10/11 共 31 个用例保持全绿,无回归
- [ ] 至少一个 struct-this 修改字段的用例通过(验证 A1/A2 闭环)
- [ ] 至少一个 `Ldsfld` 触发 cctor 的用例通过(验证 A4 闭环)
- [ ] 至少一个 sub-int 字段读到 int slot 后与常量比较的用例通过(验证 A5 闭环)

---

## 4. 不属于本文档 scope 的合规问题

以下审计发现涉及 **Step 13 及之后的 step**,已作为检查项回填到 [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) 对应 step 内,不重复列于此文档:

| 检查项 | 归属 step |
|---|---|
| `constrained.` 前缀 callvirt 语义 | Step 13(Box/Unbox 完整实现) |
| Newobj 构造函数抛异常时不应把半构造对象暴露给 caller | Step 14(异常处理) |
| Neo 未实现指令抛出的 CLR 异常必须正确恢复 mStack / Frames 状态 | Step 14 |
| `throw` 指令要求异常对象派生自 `Exception` | Step 14 |
| MethodImpl 重定向 / 显式接口实现 / 协变返回类型 | Step 10(补充)/ Step 11 |
| CLR 对象 `ldflda` writeback 并发/竞争说明 | Step 17(文档层面) |
| Managed pointer 生存期(逃逸检测) | Step 17 |
| `initobj !!T`(泛型参数)在 patch 后的正确行为 | Step 22 |
| `volatile.` / `unaligned.` 前缀 | Step 26(边界完善) |
| `cpobj` / `cpblk` / `initblk` / `localloc` 等剩余指令 | Step 26 |
| Overflow 检查指令(`add.ovf`, `mul.ovf`, `conv.ovf.*`)行为 | Step 26 |

---

## 5. C# 语言限制的合规豁免清单

以下 ECMA-335 特性因 C# 编译器不会生成 / Managed 环境无法安全表达,不在 Neo 模式规范符合性要求范围内(与 Legacy 保持一致):

- **P/Invoke / DllImport** — 属于 CLR 平台层,ILRuntime 通过 CLR redirection 走 CLR 层实现,不解释执行
- **`fixed` 语句 + `pinned` local** — GC pinning 需要 Managed 环境原生支持,Neo 帧在非托管内存上,pinning 只对通过 mStack 引用的对象有意义
- **`cpblk` / `initblk` / `localloc`** — 极少数场景使用,C# 编译器仅在 `Span<T>.Fill` 等极少数库方法中直接 emit;Neo 可选择在 Step 26 支持或标记为 not-implemented
- **`arglist` / `__arglist` / `TypedReference` / `varargs`** — C# 不支持 varargs 定义,仅通过反射调用;`TypedReference` 有 unsafe 特性,Neo 不支持
- **Native pointer(`int*` 等 unmanaged pointer 存储)** — 非托管指针无 GC 追踪,Neo 帧内 `byte*` 是解释器自身实现细节,不对 IL 层暴露原生指针 slot
- **`.mresource` embedded resources 通过 Reflection.Assembly** — 属于 Assembly 元数据能力,与 IL 执行解耦

上述豁免场景**必须在遇到时以 `NotImplementedException` 或类似机制显式 fail-fast**,严禁隐式回退到 Legacy 或返回错误结果。

---

## 6. 复审计划

- Step 12b 完成后,重新对全部 §1 违规项进行一次复审,确认无回退
- Step 13-26 每个 step 完成 handoff 前必须回读本文档 §4 列出的对应检查项,验证已实现
- 每个 step 的 handoff 文档需在末尾附上"ECMA-335 合规状态"一节,列出本 step 已闭环的检查项和向下一 step 转移的未闭环项
