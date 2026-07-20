# Step 12b 验收 Checklist

## 前置文档回填
- [x] D1:设计文档 §4.3 新增 "两种宽度规则并存" 段落(独立 slot evaluation-stack 宽度 vs struct 字段自然对齐 + 扩展/截断规则)
- [x] D2:设计文档 §2.5 新增 "Ref Slot 基础抽象" 段落,§15 精简为指令语义章节

## A1 — Struct-this ABI(managed pointer,II.13.3)
- [x] struct this 参数 slot 分配改为 8 字节 Ref Slot(Size=8, RefCount=0)
- [x] Caller emit `Ldloca_S t` → `(-1, absoluteFrameOffset)` 写入 callee 参数区
- [x] CopyNeoCallArguments / NeoCallParamMap 对 struct-this 改为拷贝 8 字节 Ref Slot
- [x] 无 struct-this by-value copyback 残留

## A2 — Ldfld_*/Stfld_* Ref Slot receiver 路径(II.4.10)
- [x] Ldfld_*/Stfld_* 新增 receiver 为 Ref Slot 的第三条分派路径
- [x] 按 objectIndex 三分派(-1 帧内 / ILTypeInstance / CLR 对象)
- [x] receiver 为 Ref Slot 时不产生 struct 拷贝、不 copyback
- [x] JITCompiler.cs L1211/L1244 的 Operand4 判定改为基于 receiver 是否持 Ref Slot

## A3 — Ref Slot 基础机制(III.3.43/III.3.29/III.4.12/III.4.28/III.4.26/III.4.14)
- [x] StackSlotInfo/AllocateSlotForType 支持 byref slot(Size=8, RefCount=0)
- [x] ExecuteNeo 可获取 runtime stack 绝对基址 stackBase,`objectIndex==-1` 时 `stackBase+offset` 还原正确
- [x] Ldloca / Ldloca_S / Ldarga / Ldarga_S handler
- [x] Ldflda handler(堆 IL 对象 / 帧内值类型 / CLR 对象 三分派)
- [x] Ldsflda handler(触发 cctor + 定位 StaticInstance)
- [x] Stind_I/I1/I2/I4/I8/R4/R8/Ref handler
- [x] Ldind_I/I1/U1/I2/U2/I4/U4/I8/R4/R8/Ref handler
- [x] Stind/Ldind 三分派(帧内 / ILTypeInstance 重取 Primitives / CLR + 值类型回写)
- [x] ILTypeInstance 场景每次重取 Primitives managed ref(GC safe)
- [x] CLR 对象场景值类型回写 `mStack[objectIndex]=obj`
- [x] Array 分支显式 NotImpl("Step 16")

## A4 — Ldsfld/Stsfld handler + cctor 闭环(I.8.9.5)
- [x] Ldsfld_* handler 覆盖 primitive + reference + Value 变体
- [x] Stsfld_* handler 同上
- [x] IL 类型:type.StaticInstance 触发 cctor,读写 Primitives + ManagedObjects
- [x] CLR 类型:CLRType.GetStaticFieldValue/SetStaticFieldValue
- [x] `class C{static int X=42}` 用例读到 42(cctor 已触发)

## A5 — sub-int 符号/零扩展(III.1.1.1)
- [x] Ldfld_U1/Boolean/U2 高位补 0
- [x] Ldfld_I1/I2 符号扩展
- [x] Ldsfld_* 对应 sub-int 变体扩展正确
- [x] Ldind_U1/I1/U2/I2 扩展正确
- [x] Stfld_*/Stsfld_*/Stind_* 按字段宽度截断(无需额外处理)
- [x] sub-int 字段读到 int slot 后与常量比较结果正确(高位无残值)

## 默认分支拆分
- [x] Ldloca_S/Ldarga/Ldflda/Ldsflda/Ldsfld/Stsfld/Stind_*/Ldind_* 全部拆为显式 case
- [x] grep 验证上述指令不再落 `not yet implemented (Step 6)` 兜底

## 工程约束
- [x] ExecuteNeo 新增 case 块内无新的同类型局部变量声明(iOS 栈安全)
- [x] 新增 handler 运行时不访问 localInfos/fieldOffsets(零查表)
- [x] 所有 Neo 改动在 `#if ENABLE_NEO_MODE` 内,Legacy Register VM 不动
- [x] 未实现能力显式 NotImplementedException 标注 step,无 Legacy/VTOR fallback

## 验收基准
- [x] NeoStep12Test 10 个用例全部通过(用例未修改)
- [x] 至少一个 struct-this 修改字段用例通过(A1/A2 闭环)
- [x] 至少一个 Ldsfld 触发 cctor 用例通过(A4 闭环)
- [x] 至少一个 sub-int 字段读到 int slot 后比较用例通过(A5 闭环)
- [x] NeoStep6/7/8/10/11 共 31 个用例保持全绿(无回归)
- [x] `-c Debug` 与 `-c Debug_Neo` 双配置 0 错误 0 警告
- [x] Legacy 模式(useRegister=false)无回归

## 收尾
- [x] D3:neo-implementation-steps.md 依赖关系图回填(Step 12b 为 Step 13/16/17/18 前置);同时新增 Step 13b 编排(CLR ↔ IL 外部入口迁移)
- [x] handoff.md 撰写完成,含 "ECMA-335 合规状态" 一节(A1-A5 闭环 + 转移项)
- [x] audit §1 全部违规项复审无回退
- [x] project_memory 更新
