# ILRuntime Neo Step 12 - Verification Checklist

## 全局工程约束
- [ ] 所有 handler case 块内无新声明的同类型局部变量（byte*/int/object/ILTypeInstance）
- [ ] 新增临时变量全部在 ExecuteNeo 方法头部共享区声明
- [ ] Move/Ldfld/Stfld handler 运行时不访问 localInfos 数组，所有偏移来自 ip 字段
- [ ] 无临时 fallback；未实现功能抛 NotImplementedException 并标注 Step
- [ ] 所有 Neo 特有逻辑在 `#if ENABLE_NEO_MODE` 内

## Task 1: InitializeFields 拆分
- [ ] `InitializeFieldsForStackObjectLayout()` 承载完整 Legacy 逻辑
- [ ] `InitializeFieldsForFlatLayout()` 承载新布局逻辑
- [ ] 方法体内无 `#if` 交错
- [ ] 入口方法通过 `#if` 分发，逻辑极简
- [ ] 静态字段初始化同样拆分
- [ ] 方法名行为命名，无 Neo 字样
- [ ] Legacy 编译 0 错误，行为不变

## Task 2: Flat Layout 自然对齐
- [ ] AlignUp 正确
- [ ] GetFieldNaturalAlignment 正确（bool=1, char=2, int/ref=4, long=8, struct=递归取最大）
- [ ] GetFieldPrimitiveSize 正确（bool=1, char=2, ref=4, struct=TotalPrimitiveSize）
- [ ] `{byte; long; int}` → 0,8,16,Total=24
- [ ] `{int; object; float}` → 0,4,8,Total=12,RefCnt=1
- [ ] 引用字段 Primitives +4（修复现有 bug）
- [ ] 嵌套 struct 递归大小/引用数正确
- [ ] Enum 按 underlying type
- [ ] 末尾 totalPrimitiveSize 对齐到最大字段对齐（≤8）
- [ ] 静态字段同算法

## Task 3: Opcode 确认与 ToString
- [ ] 无 Move_Vt 新增；无 _Inline 系列新增
- [ ] Ldfld_Value/Stfld_Value 枚举存在（补齐则追加末尾）
- [ ] ToString() Move refCount>1 时显示 refCnt
- [ ] ToString() Ldfld/Stfld Operand4=1 时加 `.inline` 后缀
- [ ] ToString() Ldfld_Value/Stfld_Value 正确显示
- [ ] 编译通过
- [ ] 净增 opcode = 0

## Task 4: 共享变量区扩展 + Optimizer
- [ ] ExecuteNeo 共享区追加 `byte* fieldBase;`
- [ ] 所有需 byte* 的 case 使用共享 fieldBase
- [ ] Optimizer.Utils/TypeSpecialize 对 Ldfld_Value/Stfld_Value 正确识别 src/dst 寄存器
- [ ] Move TypeSpecialize dst type = src type（对值类型也成立）
- [ ] TypeSpecialize 不抛 unhandled opcode

## Task 5: Ldfld/Stfld handler Operand4 双路径
- [ ] 所有 Ldfld_I1/U1/I2/U2/I4/U4/I8/U8/R4/R8/Ref (11个) 加入 `if (ip->Operand4 != 0)` 分支
- [ ] 所有 Stfld_I1/U1/I2/U2/I4/U4/I8/U8/R4/R8/Ref (11个) 对称加入
- [ ] Operand4=0 路径行为与改写前完全一致
- [ ] Operand4=1 路径 fieldBase = frameBase+SrcOffset(Ldfld)/DstOffset(Stfld)
- [ ] Ldfld_Ref inline srcIdx = frameRefBase + srcSlotRefOffset + Operand3（srcSlotRefOffset 编码确定）
- [ ] Stfld_Ref inline 对称写回
- [ ] I1/U1 读 1 字节，符号/零扩展到 4 字节
- [ ] I2/U2 读 2 字节，符号/零扩展到 4 字节
- [ ] Stfld_I1/U1/I2/U2 写只写低字节/半字
- [ ] 自然对齐字段强类型指针，不滥用 ReadUnaligned
- [ ] 统一使用共享 ins/fieldBase 变量
- [ ] 无 case-scope 局部变量

## Task 6: Ldfld_Value/Stfld_Value handler
- [ ] CopyBlock primitive 正确（堆路径+inline 路径）
- [ ] 引用拷贝正确（与 Move 共享 CopyValueTypedBytes）
- [ ] dst 字节区域 ref index 修复（dst 为独立 slot）
- [ ] 所有参数来自指令字段/共享变量

## Task 7: Move handler 扩展（无 Move_Vt）
- [ ] CopyBlock 总是执行
- [ ] Operand(refCount) > 0 时：dstRefBase = frameRefBase+Operand3, srcRefBase = frameRefBase+Operand4，循环拷贝 mStack
- [ ] Operand==1 && Operand2==4（独立 ref slot）保留 null/index 修复
- [ ] Operand>1 或 Operand2>4（值类型）不做 inline index 修复
- [ ] refCount==0 纯 CopyBlock 快速路径
- [ ] 原 Operand==1 bool 判断改为 refCount int 循环
- [ ] Vector3(refCnt=0) 纯 CopyBlock 正确
- [ ] object(refCnt=1,sz=4) 引用/null 正确（回归）
- [ ] 含引用 struct 浅拷贝正确
- [ ] DEBUG 断言 Size/RefCount 一致

## Task 8: JIT lowering Operand4
- [ ] ILType 值类型对象寄存器：op.Operand4=1
- [ ] class/Enum/CLR 值类型：op.Operand4=0
- [ ] Register2(Ldfld)/Register1(Stfld) 正确指向值类型 slot
- [ ] Operand2=field.PrimitiveOffset, Operand3=field.ReferenceOffset
- [ ] srcSlotRefOffset 编码到空闲字段（Register3/4 等）
- [ ] 嵌套值类型 Ldfld_Value 后续字段 Operand4=1
- [ ] ldsfld/stsfld Operand4=0
- [ ] 堆路径字段偏移用 fieldOffsets 正确值

## Task 9: LowerMove pass
- [ ] 在所有优化 pass 之后、AllocateLocalStackSpaces 之后运行
- [ ] 每条 Move 填 Operand=refCount, Operand2=sz, Operand3=dstRefOffset, Operand4=srcRefOffset
- [ ] 无 Move_Vt opcode，所有 Move 统一编码
- [ ] DEBUG 断言 src/dst Size/RefCount 一致
- [ ] 运行时 Move handler 不访问 localInfos
- [ ] Vector3 赋值 Move 编码正确（Operand=0,Operand2=12）
- [ ] object 赋值 Move 编码正确（Operand=1,Operand2=4）

## Task 10: Call/Ret 值类型
- [ ] CopyNeoCallArguments 值类型使用 CopyBlock + 引用拷贝
- [ ] NeoCallParamMap 包含 PrimitiveSize/RefCount/dstRefOffset
- [ ] Ret 值类型 CopyBlock + 引用拷贝正确
- [ ] 无 VTOR 依赖
- [ ] 所有偏移编译期填入

## Task 11: Initobj
- [ ] 帧内值类型 InitBlock primitive 为 0
- [ ] 遍历 RefFieldPrimitiveOffsets 写 -1
- [ ] 类型加载时缓存 RefFieldPrimitiveOffsets
- [ ] boxed initobj 抛 NotImplementedException("Step 13")

## Task 12: struct-this ABI
- [ ] this 类型通过 appdomain.GetType 统一路径
- [ ] Move this→local 无 DEBUG layout mismatch

## Task 13: cctor 恢复
- [ ] StaticInstance/InitializeMethods #if 跳过分支移除
- [ ] 静态字段读写正确

## Task 14: VTOR 清除
- [ ] ExecuteNeo ENABLE_NEO_MODE 下无 VTOR case
- [ ] JIT lowering 无 VTOR 依赖
- [ ] Grep ObjectTypes.ValueTypeObjectReference Neo 路径无引用
- [ ] Legacy Register VM VTOR 保留

## Task 15: NeoStep12Test
- [ ] NeoStep12BasicFieldAccess（Vector3-style）通过
- [ ] NeoStep12Alignment（byte+long+int 对齐）通过
- [ ] NeoStep12NestedStruct 通过
- [ ] NeoStep12StructWithRef 通过
- [ ] NeoStep12StructAssignment（Move 扩展）通过
- [ ] NeoStep12StructMethodCall（struct-this）通过
- [ ] NeoStep12StructParam 通过
- [ ] NeoStep12StaticCctor 通过
- [ ] NeoStep12InitobjDefault 通过
- [ ] NeoStep12BoolByteField 通过
- [ ] 失败有 Console.WriteLine + throw
- [ ] 单用例 5-20ms，无死循环

## Task 16: 全量回归
- [ ] Neo: NeoStep6Test 14/14
- [ ] Neo: NeoStep7Step8Test 通过
- [ ] Neo: NeoStep9Test 通过
- [ ] Neo: NeoStep10Test 4/5（pre-existing NRE）
- [ ] Neo: NeoStep11Test 5/5 单跑+全组
- [ ] Neo: NeoStep12Test 10/10
- [ ] Legacy（useRegister=false）全套通过
- [ ] Legacy Register VM（-c Debug, useRegister=true）全套通过
- [ ] -c Debug 编译 0 错 0 警
- [ ] -c Debug_Neo 编译 0 错 0 警
- [ ] handoff.md 清晰
- [ ] project_memory.md 更新 lessons
