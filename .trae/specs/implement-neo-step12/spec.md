# ILRuntime Neo Step 12 - 帧内值类型完整 Lowering（Move 统一、零查表、局部变量复用）

## Overview
- **Summary**: 完整实现 Neo 模式下值类型的 flat bytes 内存模型：拆分 `InitializeFields` 为 Legacy/Flat 两条独立路径（行为命名）；通过 OpCodeR.Operand4 标志位复用现有 Ldfld/Stfld 指令实现帧内值类型字段访问；扩展现有 `Move` 指令统一处理标量与值类型拷贝（不新增 Move_Vt opcode），所有编译期已知信息（sz/refCount/srcRefOffset/dstRefOffset）直接编码到指令字段，零运行时查表；彻底消除 Neo 模式下 `ValueTypeObjectReference` 及其所有 fallback；修复 struct-this ABI；恢复 cctor。
- **Purpose**: 完成 Neo 模式值类型从"描述符间接"到"flat bytes 内联"的彻底切换，为 Step 13/17/18 奠定完整基础。

## 核心工程原则

### 1. 局部变量复用（iOS 非 O3 栈安全）
`ExecuteNeo` 方法头部声明的共享局部变量区域（[ILIntepreter.Neo.cs L375-L381](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L375-L381)）是所有 case 块共用的。**严禁**在 case 块内声明同类型新变量。需要新增的指针变量（如 `byte* fieldBase`）加入共享声明区。

### 2. 不新增 Move_Vt opcode，统一为 Move
现有 Move 已具备 CopyBlock primitive + 引用拷贝的雏形（Operand=0/1 标志、Operand3=dstRefOffset）。扩展为：
- Operand = refCount（从 bool 扩展为 int，0=纯 primitive，1=单引用，>1=值类型多引用）
- Operand2 = primitiveSize（已使用，扩展到任意大小）
- Operand3 = dstRefOffset（已存在）
- Operand4 = srcRefOffset（新增，源 slot 的 RefOffset）

单 opcode 覆盖：标量 (sz≤8, refCnt=0)、引用 (sz=4, refCnt=1)、值类型 (sz 任意, refCnt≥0)。

### 3. 编译期直接编码，零运行时查表
- sz、refCount、dstRefOffset、srcRefOffset 均由 LowerMove pass 在编译期填入 OpCodeR 字段
- 字段 PrimitiveOffset/ReferenceOffset 在 JIT 字段 lowering 时直接填入 Operand2/Operand3
- 运行时不允许查 `localInfos[reg]` 表
- 唯一允许的运行时读取是从 frameBase 偏移读数据本身、从 mStack 读对象引用

## 设计决策汇总

| 决策 | 结论 | 理由 |
|------|------|------|
| _Inline opcode 系列 | ❌ 不新增 | Operand4 (offset 20-23) 在 Ldfld/Stfld 中完全空闲，0/1 标志 + 强预测分支，零开销 |
| Move_Vt opcode | ❌ 不新增 | Move 扩展 Operand 为 refCount 即可覆盖值类型拷贝 |
| Ldfld_Value/Stfld_Value | ✓ 实现 handler | 枚举已存在，整个嵌套 struct 加载无法用标量 Ldfld 表达 |
| Operand4 用途（Ldfld/Stfld） | inline 标志 (0=堆, 1=帧内) | 独立 4 字节字段，无重叠 |
| Operand4 用途（Move） | srcRefOffset | Move 目前不用 Operand4 |
| 运行时查表 localInfos | ❌ 禁止 | 所有偏移在 LowerMove 阶段编码进指令 |
| case 内局部变量 | ❌ 禁止 | 复用方法头部共享变量区 |

## Goals
- 重构 [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) 字段初始化逻辑为两条独立路径，行为命名
- 实现 struct flat layout：CLR 默认自然对齐、primitive 与引用字段交错排列
- 帧上值类型 local/temp 在 byte 区域连续内联存储，彻底消除 `ValueTypeObjectReference`
- 复用现有 Ldfld_*/Stfld_* opcode，Operand4=1 标志帧内路径
- 实现 `Ldfld_Value` / `Stfld_Value` handler（枚举已预留）
- 扩展现有 `Move` 指令统一处理标量/引用/值类型拷贝
- LowerMove pass 在所有优化之后、AllocateLocalStackSpaces 之后运行，把 sz/refCount/srcRefOff/dstRefOff 直接编码进指令
- JIT 根据静态类型设置 Operand4 标志和字段偏移
- struct-this ABI 一致性修复
- Initobj 帧内路径 memset + 引用置 null
- 恢复 cctor
- 无 VTOR fallback

## Non-Goals
- StructLayout(Pack, FieldOffset) 显式布局（后续 Step）
- Box/Unbox（Step 13）
- 值类型 newobj/构造器（Step 18）
- CLR 值类型 Binder 路径（Step 13）
- 数组元素访问（Step 16）
- ldloca/ldflda/stind/ldind（Step 17）

## Background & Context
Step B 完成后 slot 按类型独立分配，SSA rename 保证类型单一，Move DEBUG 断言作为安全网。缺口：
1. InitializeFields 单方法内 #if 交错，需拆分
2. Struct flat layout 无自然对齐，引用字段未占 primitive index 槽
3. 帧内值类型字段访问需要双路径（堆/帧内）
4. Move 仅支持 sz≤8 和单引用，不支持值类型
5. struct-this ABI 不一致
6. cctor 临时禁用

分步实施仅为开发计划拆分，运行时不允许 fallback，未实现功能直接 throw NotImplementedException 标注归属 Step。

## Functional Requirements

### FR-1: 字段布局初始化逻辑拆分
- `InitializeFieldsForStackObjectLayout()` — Legacy 路径，独立方法
- `InitializeFieldsForFlatLayout()` — Neo 路径，独立方法
- 入口 `InitializeFields()` 按 `#if ENABLE_NEO_MODE` 分发
- 方法体内无 `#if` 交错；方法名行为命名，无 Neo 字样
- 静态字段初始化同样拆分

### FR-2: Struct Flat Layout 自然对齐
`InitializeFieldsForFlatLayout()` 实现：
- AlignUp、GetFieldNaturalAlignment、GetFieldPrimitiveSize、GetStructMaxAlignment 辅助函数
- 字段按声明顺序，每字段按自然对齐（bool/byte=1, short/char=2, int/uint/float/enum/object=4, long/ulong/double=8, struct=递归取其最大对齐）向上对齐
- 引用字段 Primitives 占 4 字节（存 slot-relative ref index），ReferenceOffset++
- 嵌套 ILType 递归累加 TotalPrimitiveSize/TotalReferenceCount，按其对齐
- bool=1B, char=2B；末尾对齐到 struct 最大对齐（≤8）
- 静态字段同算法
- 修复现有 bug：引用字段未占用 primitiveOffset

### FR-3: Opcode 扩展
- **不新增 Move_Vt**（Move 直接扩展）
- **不新增 _Inline 系列**（Operand4 标志位复用现有 Ldfld/Stfld）
- OpCodeREnum 仅需确保 `Ldfld_Value` / `Stfld_Value` 存在（已预留），补全则追加到枚举末尾
- ToString() 更新支持 Move refCount>1 的显示、Ldfld/Stfld inline 标志显示、Ldfld/Stfld_Value 显示

### FR-4: ExecuteNeo 共享局部变量区扩展
在 [ILIntepreter.Neo.cs L377-L381](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L377-L381) 共享变量声明区追加：
- `byte* fieldBase;`
- 不允许在任何 case 块内声明新的同类型局部变量；需要新的 int/byte*/object 变量时，一律在共享区追加声明
- 此规则防止 iOS IL2CPP 非 O3 编译时每个 case 产生独立栈槽导致栈溢出

### FR-5: Ldfld/Stfld handler 双路径（Operand4 标志）
handler 复用共享 `ins`、`obj`、`fieldBase` 变量，不声明新局部变量：
```csharp
case OpCodeREnum.Ldfld_I4:
    if (ip->Operand4 != 0)
        fieldBase = frameBase + ip->SrcOffset;
    else
    {
        ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
        fieldBase = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(ins.Primitives.AsSpan()));
    }
    *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<int>(fieldBase + ip->Operand2);
    break;
```
- Operand4=0（默认零初始化）：堆对象路径（现有逻辑）
- Operand4=1：帧内路径，fieldBase = frameBase + SrcOffset (Ldfld) / DstOffset (Stfld)
- Stfld 对称：Operand4=1 时 fieldBase = frameBase + DstOffset
- Ldfld_Ref inline：src 是值类型 slot，引用字段不读 inline index；srcIdx = frameRefBase + /*srcSlotRefOffset*/ + ip->Operand3（srcSlotRefOffset 也编码到指令字段）
  - 实际：Ldfld_Ref 现有 Operand 字段为 dstRefSlot，可将 srcSlotRefOffset 编到...需确认字段分配（见 Open Questions）
- Stfld_Ref inline：对称
- sub-int (I1/U1/I2/U2) 读：符号/零扩展到 4 字节写 dst slot；写：只写低字节/半字
- 自然对齐字段用强类型指针，不滥用 ReadUnaligned

### FR-6: Ldfld_Value / Stfld_Value handler
- 编码：Operand=valueType.TotalPrimitiveSize, Operand2=field PrimitiveOffset, Operand3=field ReferenceOffset, Operand4=inline flag
- 用共享 fieldBase 变量：Operand4=0 堆路径取 ins.Primitives；Operand4=1 帧内路径取 frameBase+SrcOffset/DstOffset
- CopyBlock primitive
- 引用拷贝：堆路径从 ins.ManagedObjects[Operand3+i] 拷贝；帧内路径从 mStack[fieldBase 对应 slot 的 RefOffset + Operand3 + i] 拷贝到 dst
- 拷贝后对 dst 字节区域的 ref index 进行修复（dst slot 为 Ldfld 目标，是独立 slot，需写 dstIdx 到字节区域）
- 注意：值类型内的引用字段在本步约定为**不使用 inline index**，访问时通过 frameRefBase + slotRefOffset + field.ReferenceOffset 计算；但 Ldfld_Value 的目标 dst 是新 slot（可能是 ref type slot 或另一个 value type slot），需要根据 dst 类型正确处理
- 与 Move 共享公共拷贝辅助函数 `CopyValueTypedBytes`，参数通过寄存器/指令传入，不查 localInfos

### FR-7: Move 指令扩展（统一值类型拷贝）
Move 编码（最终）：
- DstOffset (Register1, offset 4-5)：dst slot 字节偏移
- SrcOffset (Register2, offset 6-7)：src slot 字节偏移
- Operand (offset 8-11)：refCount（原 bool 0/1 扩展为 int）
- Operand2 (offset 12-15)：primitiveSize
- Operand3 (offset 16-19)：dstRefOffset（dst 首个 ref 槽相对 frameRefBase）
- Operand4 (offset 20-23)：srcRefOffset（src 首个 ref 槽相对 frameRefBase）

Move handler 语义：
1. `Unsafe.CopyBlock(frameBase + DstOffset, frameBase + SrcOffset, (uint)Operand2);` — 总是执行
2. 若 Operand (refCount) > 0：
   - dstRefBase = frameRefBase + Operand3
   - srcRefBase = frameRefBase + Operand4
   - `for (int i = 0; i < Operand; i++) mStack[dstRefBase + i] = mStack[srcRefBase + i];`
   - 若 Operand == 1 且 Operand2 == 4 且 src 类型是引用类型（非值类型）：保留原单引用 null/index 修复逻辑（`*(int*)(frameBase+DstOffset) = src>=0 ? dstRefBase : -1`）
   - 若 Operand > 1 或 Operand2 > 4（值类型）：不做内联 index 修复（约定：值类型内 ref 字段不依赖 inline index）
3. refCount == 0：纯 CopyBlock（零 mStack 访问，快速路径）

DEBUG 断言：DstOffset、SrcOffset 对齐合理；Operand2、Operand 与 SSA 类型一致。

### FR-8: LowerMove Pass（编译期直接编码，零运行时查表）
运行时机：所有优化 pass (BCP/FCP/CopyPropagation) 之后，AllocateLocalStackSpaces 之后（LocalInfos 已就绪）。

对每条 Move：
```
srcInfo = frame.LocalInfos[srcReg]
dstInfo = frame.LocalInfos[dstReg]
DEBUG assert: srcInfo.Size == dstInfo.Size && srcInfo.RefCount == dstInfo.RefCount
op.Operand  = srcInfo.RefCount
op.Operand2 = srcInfo.Size
op.Operand3 = dstInfo.RefOffset
op.Operand4 = srcInfo.RefOffset
```
- 不新增 Move_Vt opcode，所有 Move 统一为上述编码
- 对 refCount==0 且 Size<=8 的 Move，CopyBlock 4 或 8 字节即可（自然对齐 CopyBlock 对小尺寸效率等同直接赋值）
- 编译期完成后，Move handler 运行时**不访问 localInfos**

### FR-9: JIT 字段 lowering
- Ldfld/Stfld：查询对象寄存器类型
  - ILType && IsValueType && !IsEnum：op.Operand4=1；Register2(Ldfld)/Register1(Stfld) 指向值类型 slot；Operand2=field.PrimitiveOffset; Operand3=field.ReferenceOffset；额外：值类型 slot 的 RefOffset 编到...（见 Open Questions，推荐用 Operand 字段编码 srcSlotRefOffset）
  - 否则：op.Operand4=0（默认）；Operand2=field.PrimitiveOffset; Operand3=field.ReferenceOffset; 保持现有逻辑
- 嵌套值类型：每层 Ldfld_Value 的目标寄存器类型是字段值类型，后续字段访问继续 Operand4=1
- ldsfld/stsfld：Operand4=0；静态字段偏移使用 fieldOffsets 正确值

### FR-10: Call/Ret 值类型
- CopyNeoCallArguments 值类型参数：CopyBlock primitive + 引用拷贝（复用 CopyValueTypedBytes 逻辑）；参数映射包含 PrimitiveSize/RefCount/dstRefOffset
- Ret 值类型返回：CopyBlock 到 retDstPtr + 拷贝引用到 targetRetRefBase
- 不依赖 VTOR
- 所有偏移在编译期填入指令/参数映射

### FR-11: Initobj 帧内路径
- Initobj 帧内值类型：InitBlock primitive 为 0；引用字段位置写 -1（需递归遍历类型的 ref field offsets，这是类型的静态元数据，可在类型加载时缓存为 ref field offset 列表，运行时遍历该列表）
- boxed initobj 抛 NotImplementedException("Step 13")，无 VTOR 回退

### FR-12: struct-this ABI 修复
- this 类型走 appdomain.GetType(method.Definition.DeclaringType) 统一路径
- 确保 appdomain.GetType 缓存/去重返回同一 ILType 实例
- Move this→local 不触发 DEBUG layout mismatch

### FR-13: 恢复 cctor
- 移除 StaticInstance/InitializeMethods 中 `#if ENABLE_NEO_MODE` 跳过 cctor 的分支

### FR-14: 消除 VTOR
- `#if ENABLE_NEO_MODE` 下 ExecuteNeo 无 VTOR 分支
- JIT lowering 不生成 VTOR 依赖
- Grep 验证无 VTOR 运行时引用
- Legacy 路径 VTOR 保留

## Non-Functional Requirements
- **NFR-1**: 字段访问 handler 分支预测率 100%，等价分离 opcode 性能
- **NFR-2**: Move 零运行时查表，所有偏移来自指令字段
- **NFR-3**: ExecuteNeo 所有 case 块无 case-scope 局部变量（iOS 非 O3 栈安全）
- **NFR-4**: Layout 计算 O(n) 一次性
- **NFR-5**: Legacy 全量回归通过
- **NFR-6**: Move/Move 扩展 DEBUG 断言持续有效
- **NFR-7**: 无 VTOR 残留；无临时 fallback
- **NFR-8**: 净增 opcode 数量：0（Ldfld_Value/Stfld_Value 已存在）

## Constraints
- 所有 Neo 改动在 `#if ENABLE_NEO_MODE` 内
- Legacy InitializeFieldsForStackObjectLayout 行为不变
- OpCodeR union 字段使用严格按 opcode 分类，避免别名冲突
- Operand4 在 Ldfld/Stfld 用作 inline 标志；在 Move 用作 srcRefOffset；其他 opcode (Beqi/Callvirt) 对 Operand4 的使用不受影响
- 自然对齐指针访问字段，不滥用 ReadUnaligned
- 所有编译期已知偏移直接编码，禁止运行时查 localInfos/fieldOffsets 表

## Assumptions
- CLR 默认 struct layout 自然对齐规则（LayoutKind.Sequential 默认）
- bool struct field=1B, char=2B
- StructLayout(Pack/FieldOffset) 后续 Step
- C# struct 默认零初始化保证新 OpCodeR 字段为 0（Operand4=0 即堆路径默认）
- 值类型内的 ref 字段不使用 inline mStack index，统一通过 frameRefBase+slotRefOffset+field.ReferenceOffset 计算访问
- 独立 ref 类型 slot（非值类型内 ref 字段）保留 inline index 约定（字节区域存 mStack index 或 -1）

## OpCodeR 字段布局（Ldfld/Stfld 与 Move 最终约定）

```
Offset  Size  Field            Ldfld(I4/R4/etc)  Ldfld_Ref         Ldfld_Value       Move
0-3     4     Code             opcode            opcode            opcode            opcode
4-5     2     Register1/DstOff dst reg/offset    dst reg/offset    dst reg/offset    dst slot offset
6-7     2     Register2/SrcOff obj reg/offset    obj reg/offset    obj reg/offset    src slot offset
8-11    4     Operand          -                 dstRefSlot        vt PrimitiveSize  refCount
12-15   4     Operand2         PrimitiveOffset   PrimitiveOffset   field PrimOff     primitiveSize
16-19   4     Operand3         -                 ReferenceOffset   field RefOff      dstRefOffset
20-23   4     Operand4         inline flag(0/1)  srcSlotRefOff?    inline flag       srcRefOffset
```

注：Ldfld_Ref 的 srcSlotRefOffset（帧内路径时）需要额外字段；根据最终编码确定是复用 Register3/4 还是 Operand 字段（Operand 在 Ldfld 中已用于 dstRefSlot）。

## Acceptance Criteria

### AC-1: InitializeFields 拆分清晰
- **Verification**: `human-judgment`

### AC-2: Struct 字段对齐正确
- `{byte; long; int}` → 0,8,16,Total=24；`{int; object; float}` → 0,4,8,Total=12,RefCnt=1
- **Verification**: `programmatic`

### AC-3: 帧内 primitive 字段读写（Operand4=1）
- Vector3 写读正确
- **Verification**: `programmatic`

### AC-4: 嵌套 struct
- Ldfld_Value 加载内层 struct，字段访问正确
- **Verification**: `programmatic`

### AC-5: 帧内 ref 字段
- struct object 字段赋值/读取正确
- **Verification**: `programmatic`

### AC-6: Move 统一值类型拷贝
- Vector3/含引用 struct 赋值走 Move（非 Move_Vt），数据正确，引用浅拷贝
- **Verification**: `programmatic`

### AC-7: 值类型参数/返回值
- Callee 字段值正确，返回正确
- **Verification**: `programmatic`

### AC-8: 堆路径回归
- NeoStep7-11 全绿，Operand4=0 路径与改写前一致
- **Verification**: `programmatic`

### AC-9: struct-this ABI
- Move this→local 无 DEBUG layout mismatch
- **Verification**: `programmatic`

### AC-10: cctor 恢复
- 静态构造函数触发，静态字段读到正确值
- **Verification**: `programmatic`

### AC-11: Initobj
- initobj 后 primitive=0, ref=null
- **Verification**: `programmatic`

### AC-12: Legacy 无回归
- Legacy 全套通过
- **Verification**: `programmatic`

### AC-13: VTOR 清除
- Grep 验证 Neo 路径无 VTOR 引用
- **Verification**: `programmatic` + `human-judgment`

### AC-14: Opcode 不膨胀
- 无 _Inline、无 Move_Vt；净增 opcode 0
- **Verification**: `human-judgment`

### AC-15: iOS 栈安全（代码审查）
- ExecuteNeo case 块内无新的同类型局部变量声明；所有临时变量在方法头部共享区声明
- **Verification**: `human-judgment`

### AC-16: 零运行时查表
- Move/Ldfld/Stfld handler 运行时不访问 localInfos 数组；所有偏移来自 ip 字段
- **Verification**: `human-judgment`

## Open Questions
- [ ] Ldfld_Ref/Stfld_Ref 帧内路径下，源/目标值类型 slot 的 RefOffset 编码在哪个字段？Operand 字段已被 dstRefSlot 使用（Ldfld 结果写入的 ref slot）。候选：复用 Register3/4（这两个字段在 Ldfld_Ref 中目前未使用）。实现时根据实际编码确定。
- [ ] 帧内值类型 ref 字段是否完全不写 inline index？还是写 slot-relative index（0-based within slot）？后者可能更统一但需要 Ldfld_Ref 读取时加 slotRefOffset。推荐前者（不写 inline index），保持 ref 字段 4 字节位置为填充/don't-care，访问时一律通过 frameRefBase+slotRefOffset+field.ReferenceOffset 计算。
- [ ] Initobj 遍历 ref field offset 列表：类型加载时缓存 `int[] RefFieldPrimitiveOffsets` 供 Initobj 快速遍历。
