# ILRuntime Neo Step 12b - Ref Slot 基础机制 + Struct-this ABI + 静态字段访问（ECMA-335 合规返工）

## Why
Step 12 把值类型 flat 布局落地了,但设计文档 §15 定义的 **Ref Slot(8 字节 managed pointer 表示)基础机制从未在任何 step 实现**。这直接导致三处 ECMA-335 语义违规(见 [ecma335-audit.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step12b/ecma335-audit.md) §1):

- **A1** struct 实例方法的 `this` 被按 by-value 整块拷贝,违反 II.13.3(`this` 必须是 managed pointer)
- **A2** `Ldfld_*`/`Stfld_*` 缺少 receiver 为 managed pointer 的分派路径,违反 II.4.10
- **A3** `Ldloca/Ldarga/Ldflda/Ldsflda/Stind_*/Ldind_*` 全部落到 `not yet implemented (Step 6)` 兜底,Ref Slot 基础机制缺失
- **A4** `Ldsfld/Stsfld` handler 缺失 → 静态构造器(cctor)实际从不触发,违反 I.8.9.5
- **A5** sub-int 字段从紧凑 struct 布局搬到 4 字节帧 slot 时缺少符号/零扩展,残值污染高位

当前 [NeoStep12Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep12Test.cs) 10 个用例全部因 `Ldloca_S not yet implemented` / `Ldsfld not yet implemented` 失败。Step 12b 的目标是补齐 Ref Slot 基础机制并闭环这 5 处违规,使 10 个用例全绿,且 NeoStep6/7/8/10/11 共 31 个用例零回归。

## What Changes
- **新增 Ref Slot 8 字节 `(objectIndex:int, offset:int)` 帧 slot 类型**(设计文档 §15.2 已确定的基线),在 `StackSlotInfo`/`AllocateSlotForType` 支持 byref slot(Size=8, RefCount=0)
- **实现 Ref Slot 生产指令**:`Ldloca/Ldloca_S/Ldarga/Ldarga_S/Ldflda/Ldsflda`
- **实现 Ref Slot 消费指令**:`Stind_*/Ldind_*`,按 `objectIndex` 三分派(帧内 / ILTypeInstance / CLR 对象)
- **`Ldfld_*`/`Stfld_*` 新增 receiver 为 Ref Slot 的分派路径**(A2),使 struct 实例方法的 `this.field` 正确工作
- **BREAKING(内部 ABI)**:struct-this 参数 slot 从整块 by-value 改为 8 字节 Ref Slot 传 by-ref(A1);`CopyNeoCallArguments`/`NeoCallParamMap` 对 struct-this 改为拷贝 8 字节 Ref Slot
- **实现 `Ldsfld_*`/`Stsfld_*` handler**(A4),命中时触发 `ILType.StaticInstance` → cctor 闭环
- **审计并修正 `Ldfld_U1/I1/U2/I2/Boolean`、新增 `Ldsfld_*`、`Ldind_U1/I1/U2/I2` 的 sub-int 符号/零扩展**(A5)
- **拆分 ExecuteNeo 默认分支**:把上述指令从 `not yet implemented (Step 6)` 兜底拆为显式 case,归属 Step 12b
- **设计文档回填**:§4.3 两种宽度规则并存(D1)、§2.5 Ref Slot 基础抽象前置(D2)
- **可选优化**:Ldfld/Stfld inline fold pass(帧内地址链折叠),时间充裕时实现

## Non-Goals(本 step 不做,归后续)
- `ldelema` / 数组元素 Ref Slot(Step 16)
- 跨帧 ref/out 参数封送的完整场景(Step 17,仅 struct-this 这个首个使用场景在本 step 落地)
- 值类型 newobj 用 Ref Slot 传 this(Step 18)
- Box/Unbox、boxed initobj、CLR 值类型 initobj(Step 13)
- `constrained.` 前缀、异常处理、`cpobj/cpblk/localloc`、overflow 指令(见 audit §4,归 Step 13/14/26)
- 引用类型字段的 ldflda(design §15.6 objectIndex=-2 marker,极罕见,遇到再设计)

## Impact
- **Affected specs**:Step 12 handoff 遗留的 Task 12(struct-this ABI)/ Task 13(cctor 的 Ldsfld/Stsfld 半部分);Step 16/17/18 后续改为依赖本 step 的 Ref Slot(D3,落地后回填)
- **Affected code**:
  - [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs)(struct-this slot 分配 L460-L469、AllocateSlotForType L557-L598、Ldfld/Stfld lowering L1190-L1252、Ldflda/Ldsflda/Ldsfld/Stsfld/Stind/Ldind 的 Translate)
  - [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs)(新增 handler、Ldfld/Stfld Ref Slot 分支、共享变量区、默认分支拆分、CopyNeoCallArguments)
  - [Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs)(LowerNeoOffsets 对新指令的偏移下降)
  - [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs)(StaticInstance/cctor 触发链已在 Step 12 恢复,本 step 仅验证 Ldsfld 命中后闭环)
  - [object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md)(D1/D2 文档回填)

---

## 核心工程原则(继承 Step 12,每条 Task 必须遵守)

1. **局部变量复用(iOS 非 O3 栈安全)**:ExecuteNeo 所有 case 块严禁声明新的同类型局部变量;需要的新变量(如 `int objIndex`、`byte* refTarget`)一律追加到方法头部共享声明区([ILIntepreter.Neo.cs L412-L418](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L412-L418))
2. **零运行时查表**:所有编译期已知偏移(slotOffset / fieldPrimitiveOffset / refOffset / staticFieldOffset)必须在 JIT / LowerNeoOffsets 阶段编码进 OpCodeR 字段;handler 运行时禁止访问 `localInfos` / `fieldOffsets` 表
3. **无 fallback**:未实现能力直接 `throw NotImplementedException` 并标注归属 step,严禁隐式回退 Legacy 或 VTOR
4. **Neo 改动全部在 `#if ENABLE_NEO_MODE` 内**,Legacy Register VM(ILIntepreter.Register.cs)不动

---

## Ref Slot 基线定义(设计文档 §15.2,已确定,不再讨论编码格式)

Ref Slot 占 **8 字节** = `(objectIndex:int, offset:int)`:

```
objectIndex == -1 (FRAME_REF):
  → offset = 相对 runtime stack 起始基址(nativePointer)的【绝对】字节偏移
  → 目标在非托管帧内存,读写 *(T*)(stackBase + offset)
  → 之所以用绝对偏移而非相对 frameBase:被调方 frameBase 与调用方不同,
    Ref Slot 跨帧传递时必须用与帧无关的绝对基址

objectIndex >= 0:
  → objectIndex = mStack index
  → 运行时按 mStack[objectIndex] 的类型分派:
    - ILTypeInstance → offset = 字段在 Primitives 的 primitiveOffset
    - CLR 对象       → offset = 字段 hash(FieldInfo.GetHashCode()),走 CLRType.Get/SetFieldValue
    - Array          → offset = elementIndex(归 Step 16,本 step 不做)
```

**实现前置**:ExecuteNeo 需要能拿到 runtime stack 的绝对基址 `stackBase`(`nativePointer`),用于 `objectIndex==-1` 时 `stackBase + offset` 的地址还原。`Ldloca` 产出时 `offset = (frameBase - stackBase) + slotOffset`。

---

## Functional Requirements

### FR-1(A3):Ref Slot slot 类型分配
- `AllocateSlotForType` / `StackSlotInfo` 支持 byref slot:`Size=8, RefCount=0`
- JIT 为 `Ldloca/Ldarga/Ldflda/Ldsflda` 的目标寄存器分配 8 字节 byref slot
- `AllocateLocalStackSpaces` 中 byref 类型 slot 取 8 字节(design §15.5:同 temp slot 类型冲突取 max size)

### FR-2(A3):Ref Slot 生产指令 handler
- `Ldloca / Ldloca_S / Ldarga / Ldarga_S` → 写 `(-1, (frameBase - stackBase) + slotOffset)` 到目标 8 字节 slot
- `Ldflda`:
  - 堆 IL 对象 → `(objMStackIndex, fieldPrimitiveOffset)`
  - 帧内值类型(receiver 本身是 Ref Slot 或帧内 struct)→ `(-1, baseAbsOffset + fieldPrimitiveOffset)` 累加
  - CLR 对象 → `(objMStackIndex, fieldHash)`
- `Ldsflda` → 静态字段专用:触发 `type.StaticInstance`(cctor),产出 `(staticInstanceMStackIndex, fieldPrimitiveOffset)`
- 所有偏移编译期编码,运行时不查表

### FR-3(A3):Ref Slot 消费指令 handler(Stind/Ldind 三分派)
`Stind_I / Stind_I1 / Stind_I2 / Stind_I4 / Stind_I8 / Stind_R4 / Stind_R8 / Stind_Ref` 与
`Ldind_I / Ldind_I1 / Ldind_U1 / Ldind_I2 / Ldind_U2 / Ldind_I4 / Ldind_U4 / Ldind_I8 / Ldind_R4 / Ldind_R8 / Ldind_Ref`
按 Ref Slot 的 `objectIndex` 分派:
- `-1` → `*(T*)(stackBase + offset)` 直接读写
- `>=0 && mStack[objectIndex] is ILTypeInstance` → 重取 `Primitives` 的 managed ref 后按 offset 读写(GC safe,避免悬空)
- `>=0 && CLR 对象` → `CLRType.GetFieldValue/SetFieldValue(fieldHash, ...)` + 值类型回写 `mStack[objectIndex] = obj`
- Array 分支归 Step 16,本 step 命中 Array 抛 NotImpl("Step 16")

### FR-4(A2):Ldfld_*/Stfld_* 新增 Ref Slot receiver 分派路径
- 现状:两条路径 —— `Operand4==0` 堆对象(receiver 为 4 字节 mStack index)、`Operand4!=0` 帧内 struct 直接偏移(未被真实使用)
- 新增第三条:receiver 寄存器持 8 字节 Ref Slot(struct-this、ldloca/ldflda 结果)。读出 `(objIndex, off)`,按 objIndex 三分派(同 FR-3 的帧内/ILTypeInstance/CLR),再加字段 `Operand2`(fieldPrimitiveOffset)定位
- **不产生 struct 拷贝,不需要 copyback**
- receiver 类型识别:JIT 侧根据 receiver 寄存器 slot 是否为 byref/struct-this,选择生成 Ref-Slot receiver 变体;运行时用统一 handler 内的 flag 分派(复用 Operand4 语义,具体编码见 Open Questions)
- 重新审视 [JITCompiler.cs L1211 / L1244](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1211) 的 `Operand4 = (type.IsValueType && !type.IsEnum) ? 1 : 0` 判定:当前基于"字段声明类型",应改为基于"receiver 寄存器是否持 Ref Slot"

### FR-5(A1):struct-this ABI 改为 Ref Slot 传 by-ref
- [JITCompiler.cs L460-L469](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L460-L469):`declaringType.IsValueType` 的 this 参数 slot 从整块 struct(`TotalPrimitiveSize + TotalReferenceCount`)改为 8 字节 Ref Slot(Size=8, RefCount=0)
- Caller 端 emit `Ldloca_S t` 等价物,把 `(-1, absoluteFrameOffset)` Ref Slot 写入 callee 参数区
- `CopyNeoCallArguments` / `NeoCallParamMap` 对 struct-this 参数改为拷贝 8 字节 Ref Slot,而非整块 struct
- Callee 内 `ldarg.0` 加载 Ref Slot,后续 `ldfld/stfld` 走 FR-4 的 Ref Slot receiver 路径
- 移除任何隐含的 struct-this by-value copyback 逻辑(当前不存在,需确认无回退隐患)

### FR-6(A4):Ldsfld_*/Stsfld_* handler + cctor 闭环
- `Ldsfld_*` / `Stsfld_*` 覆盖所有 primitive + reference + Value(struct copy)变体
- IL 类型:通过 `type.StaticInstance` 触发 cctor(`appdomain.Invoke(staticConstructor)` 首次访问自动触发),再按 `field.PrimitiveOffset` 读写 `Primitives`,引用字段走 `staticInstance.ManagedObjects[refOffset]`
- CLR 类型:通过 `CLRType.GetStaticFieldValue/SetStaticFieldValue`
- 静态字段偏移在 JIT 阶段用正确的 fieldOffsets 编码进指令,运行时不查表

### FR-7(A5):sub-int 符号/零扩展
- 审计 `Ldfld_U1 / Ldfld_I1 / Ldfld_U2 / Ldfld_I2 / Ldfld_Boolean` handler,确认写入 4 字节 dst slot 时:
  - 无符号(U1/Boolean/U2)→ 高位补 0(`*(int*)dst = *(byte*)src` / `*(uint*)dst = *(ushort*)src`)
  - 有符号(I1/I2)→ 符号扩展(`*(int*)dst = *(sbyte*)src` / `*(int*)dst = *(short*)src`)
- 新增/修正 `Ldsfld_*`(从 `StaticInstance.Primitives` 加载)对应变体
- 新增 `Ldind_U1/I1/U2/I2` 对应扩展变体
- 反向 `Stfld_* / Stsfld_* / Stind_*` 按字段宽度截断写低位即可,高位丢弃是正常语义(无需额外处理)

### FR-8:默认分支拆分
- 把 `Ldloca_S / Ldarga / Ldflda / Ldsflda / Ldsfld / Stsfld / Stind_* / Ldind_*` 从 ExecuteNeo 默认 `not yet implemented (Step 6)` 兜底([L2009](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2009))拆为显式 case,归属 Step 12b

### FR-9(D1/D2):设计文档回填(实现前完成)
- **D1**:§4.3 新增 "两种宽度规则并存" 段落(独立帧内 primitive slot 用 CIL evaluation-stack 宽度 / struct 内部字段用 CLR StructLayout 自然对齐 / Ldfld_* 等加载 sub-int 到独立 slot 时做扩展)
- **D2**:§2 之后新增 "§2.5 基础抽象:Ref Slot(managed pointer 表示)",把 §15.2 定义前置;§15 保留具体指令语义

### FR-10(可选):Ldfld/Stfld inline fold 优化
- 在 LowerNeoOffsets 之前跑
- 识别 `Ldloca_S/Ldarga_S/Ldflda 帧内链 + Stfld/Ldfld/Initobj` 组合,fold 成直接偏移一步指令(复用现有 Operand4 帧内 direct 路径)
- 逃逸场景(Ldloca 结果被 Call/Stind/其他 opcode 消费)保留原 Ref Slot 路径
- **时间充裕时实现;不影响正确性,仅优化**

---

## Constraints
- Ref Slot 编码格式严格按 design §15.2,8 字节 `(objectIndex, offset)`,不重新设计
- `objectIndex==-1` 的 offset 必须是相对 stackBase 的绝对偏移(跨帧安全)
- ILTypeInstance 场景每次读写重取 `Primitives` managed ref(GC 移动安全)
- CLR 对象场景值类型必须回写 `mStack[objectIndex] = obj`
- OpCodeR union 字段使用按 opcode 分类,避免别名冲突;struct-this / Ref-Slot-receiver 的 flag 编码不得破坏现有 Ldfld_Ref 的 Operand/Operand3/Operand4 用法
- 自然对齐字段用强类型指针,不滥用 ReadUnaligned

## Assumptions
- `ILType.StaticInstance` getter 已在 Step 12 恢复 cctor 触发链(handoff §1.7 确认,`#if ENABLE_NEO_MODE` 跳过分支已移除)
- C# 编译器对 struct 实例方法生成 `ldarg.0 + ldfld/stfld`(不是 stind/ldind),已由 [TestCases.il](file:///f:/SVN/ILRuntime/TestCases/bin/Debug/netstandard2.1/TestCases.il) `NeoStep12ThisTarget::Bump` 实测确认
- `Stind_*/Ldind_*` 服务于 ref/out 写回、显式 byref 解引用,不是 struct-this 主路径(struct-this 走 FR-4 的 Ldfld/Stfld)

## Open Questions
- [ ] Ldfld/Stfld receiver 三态(heap mStack index / 帧内 struct direct / Ref Slot)在 Operand4 上的具体编码:当前 0=heap、非0=帧内 direct(且 Ldfld_Ref 复用 Operand4 编 srcRefOffset+1)。新增 Ref-Slot receiver 需一个不冲突的编码(候选:引入独立标志位或复用 Register3/4)。实现时确定并在 handoff 记录
- [ ] `objectIndex==-1` 的绝对偏移基址:确认用 `RuntimeStack.nativePointer` 还是 `StackBase`,以及 ExecuteNeo 如何最低成本拿到该基址(候选:方法入口缓存 `byte* stackBase`)
- [ ] Ldsflda / Ldsfld 触发 cctor 时,StaticInstance 在 mStack 中的 index 如何稳定获取(静态实例是否常驻 mStack 或每次入栈)

---

## Acceptance Criteria

### AC-1(A3):Ref Slot 基础机制
- `Ldloca/Ldarga/Ldflda/Ldsflda` 产出正确的 `(objectIndex, offset)`;`Stind_*/Ldind_*` 三分派读写正确
- **Verification**: `programmatic`(NeoStep12Test 帧内 struct 字段读写、default initobj 用例)

### AC-2(A1+A2):struct-this 修改字段闭环
- 至少一个 struct 实例方法修改自身字段(`NeoStep12ThisTarget::Bump`)的用例:callee 内 `this.field = x` 正确写回 caller 帧上的原始 struct,无拷贝
- **Verification**: `programmatic`

### AC-3(A4):Ldsfld 触发 cctor 闭环
- 至少一个 `class C { static int X = 42; }` 用例:首次 `Ldsfld C::X` 触发 cctor,读到 42
- **Verification**: `programmatic`(NeoStep12Test StaticCctor 用例)

### AC-4(A5):sub-int 扩展正确
- 至少一个 sub-int 字段(sbyte/byte/short/ushort/bool)读到 int slot 后与常量比较/参与算术的用例:高位无残值污染,比较/算术结果正确
- **Verification**: `programmatic`

### AC-5:NeoStep12Test 全绿
- [NeoStep12Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep12Test.cs) 10 个用例全部通过(不修改用例,只改运行时/JIT)
- **Verification**: `programmatic`

### AC-6:无回归
- NeoStep6/7/8/10/11 共 31 个用例保持全绿
- **Verification**: `programmatic`

### AC-7:双配置零错误
- `-c Debug` 与 `-c Debug_Neo` 均 0 错误 0 警告编译
- **Verification**: `programmatic`

### AC-8:无 fallback / 无兜底残留
- 上述指令均为显式 case,不再落 `not yet implemented (Step 6)`;未实现能力(Array ldind/stind、boxed 等)显式 NotImpl 并标注 step
- **Verification**: `human-judgment` + `programmatic`(grep)

### AC-9:iOS 栈安全
- ExecuteNeo 新增 case 块内无新的同类型局部变量声明,临时变量全部在方法头部共享区
- **Verification**: `human-judgment`

### AC-10:零运行时查表
- 新增 handler 运行时不访问 localInfos/fieldOffsets;所有偏移来自 ip 字段
- **Verification**: `human-judgment`

### AC-11(D1/D2):设计文档已回填
- §4.3 两种宽度规则、§2.5 Ref Slot 基础抽象已写入
- **Verification**: `human-judgment`
