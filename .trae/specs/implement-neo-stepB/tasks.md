# Step B Tasks — Stack Register Slot Layout 修复

## Task 1: `TypeSpecializeNeoOpcodes` 补齐 SetRegisterType

- [ ] `Call / Callvirt / Callvirt_IL / Callvirt_CLR / Call_Redirect / Call_Redirect_IL`：`SetRegisterType(op.Register1, m.ReturnType)`（有返回值时）
  - 需要读取 op.Operand / TokenLong 得到 IMethod（借用现有 InitializeFunctionParam 逻辑或从 JIT 阶段传递上下文）
- [ ] `Newobj`：`SetRegisterType(op.Register1, targetType)`，targetType 从 Operand token 解析
- [ ] `Ldelem_I1 / U1 / I2 / U2 / I4 / U4 / I8 / U8 / R4 / R8 / Ref`：类型固定，直接 SetRegisterType
- [ ] `Ldelema`：写入 managed pointer；当前 Neo 未细分类型，暂 SetRegisterType 为 target 引用（Size=4, RefCount=0 即可）
- [ ] `Box`：`SetRegisterType(op.Register1, appdomain.ObjectType)`
- [ ] `Unbox / Unbox_Any`：`SetRegisterType(op.Register1, targetType)`（Operand token 解析）
- [ ] `Ldsfld_I1 / U1 / I2 / U2 / I4 / U4 / I8 / U8 / R4 / R8`：primitive 类型固定
- [ ] `Ldsfld_Ref / Ldsfld_Value`：从 Operand 高 32 位 typeToken 解析
- [ ] `Ldsfld / Ldsflda`：类似 Ldfld 处理
- [ ] `Ldtoken`：结果为 `RuntimeTypeHandle` / `RuntimeMethodHandle` / `RuntimeFieldHandle`，视 token 类型，暂用 ObjectType
- [ ] `Ldftn / Ldvirtftn`：结果为 IntPtr → `SetRegisterType(op.Register1, appdomain.IntType)`（当前 Neo 无 IntPtr 独立 slot 语义）
- [ ] `Isinst / Castclass`：`SetRegisterType(op.Register1, targetType)`
- [ ] `Ldloca / Ldarga / Ldflda`：暂 SetRegisterType 为 IntType（managed pointer 未细分）
- [ ] `Dup`：`SetRegisterType(op.Register1, GetRegisterType(op.Register2))`（若 Dup 存在于 Neo lowering 后 IR）
- [ ] `Ldstr`：已有；确认 Register1 覆盖到位

## Task 2: `AllocateLocalStackSpaces` stack register 段重写

- [ ] 修改签名：把 `IType[] registerTypes` 传入（`ref frame` 之外多一个 in 参数），或在 `TypeSpecializeNeoOpcodes` 结束后把它挂到 `CompiledFrame` 上做过渡容器
- [ ] 循环 `for (int i = 0; i < frame.StackRegisterCount; i++)`：读 `registerTypes[baseRegStart + i]`
  - null → 分配 `Size=4, RefCount=0`
  - non-null → 走与 locals 完全同构的 `AllocateSlotForType(t, ref offset, ref refOffset)` 路径
- [ ] 删除 `int maxSize = 8, maxRefCount = 1;` 及 `GatherValueTypes` 的调用（若 `GatherValueTypes` 无其它调用点，整个方法删掉）
- [ ] 保持 `LocalIsReference[baseRegStart + i]` 的填充逻辑：`RefCount > 0 && Size == 4` 时为 true，值类型局部（RefCount > 0 但 Size > 4）不算 IsReference

## Task 3: Move 指令 refMove 判定下沉

- [ ] `JITCompiler.TypeSpecializeNeoOpcodes` Move case：删除 `op.Operand = IsNeoReferenceSlot(srcType) ? 1 : 0;`，保留 `SetRegisterType(op.Register1, srcType)`
- [ ] `Optimizer.Neo.cs` LowerNeoOffsets Move case：
  - `bool isRefMove = LocalInfos[srcReg].RefCount > 0;`
  - `int sz = LocalInfos[srcReg].Size;`
  - 删除 `min(srcSz, dstSz)` 分支与旧注释
  - `op.Operand = isRefMove ? 1 : 0; op.Operand2 = sz; op.Operand3 = LocalInfos[dstReg].RefOffset;`
- [ ] DEBUG 断言：`Debug.Assert(LocalInfos[srcReg].Size == LocalInfos[dstReg].Size && LocalInfos[srcReg].RefCount == LocalInfos[dstReg].RefCount)`（用条件编译或纯 if throw）

## Task 4: 主控流验证

- [ ] 确认 `InitCodeBody` / `Compile` 主控流：`TypeSpecializeNeoOpcodes` 在 `AllocateLocalStackSpaces` 之前调用（读一遍现有代码）
- [ ] 如果 `TypeSpecializeNeoOpcodes` 当前对 stack register 段不做写入（因它循环 CodeBody 而 stack register 索引超出 `locVarRegStart + varCnt`），必须确认 `registerTypes` 数组容量到 `totalRegCnt`（=`locVarRegStart + varCnt + StackRegisterCount`），并且 `BuildInitialRegisterTypes` 初始化到 `paramCnt + varCnt`，stack register 段初值为 null（由 opcode 写入填充）
- [ ] 如果 `BuildInitialRegisterTypes` 的容量参数是 `totalRegCnt`（应已包含 stack registers），无需改动；否则需扩容

## Task 5: 帧元数据 & 调用侧回归验证

- [ ] Callvirt / Call 返回引用 slot 定位链（`Operand3 = LocalInfos[dstReg].RefOffset`）自动生效，读一遍 [InitializeCallvirtDispatch](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) 相关代码，确认返回 refOffset 的写入依赖 `LocalInfos[dstReg]` 而非老 upper-bound 假设
- [ ] Ret handler 引用返回：确认 [ILIntepreter.Neo.cs Ret case](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) 从 `ReturnRefCount` 与 `LocalInfos[srcReg]` 定位，B-1 后仍正确
- [ ] mStack 帧入口预留：`mStack.Count += TotalRefSize` 收缩到实际引用槽总数，无需其它改动

## Task 6: 移除 Step 6 workaround 痕迹

- [ ] `Optimizer.Neo.cs` L94-98 的注释（"Use min(src,dst) so a wide stack register (8 bytes) copied into a narrow local ..."）删除
- [ ] Step 6 checklist L79 的 workaround 描述保留（历史记录），在本 Spec 完成后写 handoff 时说明"Step B 已根除"

## Task 7: 编译验证

- [ ] `dotnet build --framework net8.0 -c Release HotfixAOT/`
- [ ] `dotnet build --framework net8.0 -c Release_Patched HotfixAOT/`
- [ ] 生成 patch：`dotnet run --project PatchTool/PatchTool.csproj -- -i ... && ... -p ...`
- [ ] `dotnet build TestCases/`
- [ ] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误 0 警告（Legacy 不受影响）
- [ ] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误 0 警告

## Task 8: 回归测试

- [ ] Neo 模式 `NeoStep6Test` 全绿（Move workaround 移除后原地验证）
- [ ] Neo 模式 `NeoStep7Test / NeoStep8Test / NeoStep9Test` 全绿
- [ ] Neo 模式 `NeoStep10Test` 全绿
- [ ] Neo 模式 `NeoStep11Test` 全绿（含单跑 & 全组）：
  - `NeoStep11Basic` ✅（先前通过）
  - `NeoStep11InheritedInterfaceImpl` ✅（先前通过）
  - `NeoStep11InterfaceInheritance` ✅（含单跑 & 全组）
  - `NeoStep11ExplicitImplementation` ✅
  - `NeoStep11MultipleInterfaces` ✅（含单跑 & 全组）
- [ ] Legacy 模式（`useRegister=false`）全套测试全绿（回归检测）
- [ ] Legacy Register VM 模式（`useRegister=true`, `-c Debug`）全套测试全绿

## Task 9: Handoff & Spec 收尾

- [ ] 撰写 [handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/handoff.md)：记录 slot 分配统一化后对 Step 12/13（值类型完整 lowering、CLR struct ABI）的输入契约变化
- [ ] 撰写 [checklist.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/checklist.md)：验收清单落地
- [ ] 更新 [implement-neo-step3-step4/spec.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step3-step4/spec.md) 或独立备注：说明当年 stack register 段的遗漏及 Step B 补账
- [ ] 更新 [implement-neo-step6/checklist.md L79](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step6/checklist.md#L79)：补一行"已被 Step B 根除"标注

# Task Dependencies

- Task 2 依赖 Task 1（slot 分配读 registerTypes，需 SetRegisterType 覆盖 stack register）
- Task 3 依赖 Task 2（Move refMove 判定读 LocalInfos.RefCount）
- Task 5 依赖 Task 2（调用侧依赖 LocalInfos 正确性）
- Task 7 依赖 Task 1-6
- Task 8 依赖 Task 7
- Task 9 依赖 Task 8

# 不在本 Spec 范围（明确推迟）

- Slot 复用 / liveness 分析
- 值类型完整 lowering（Step 12/13）
- Callvirt / constrained. callvirt opcode 变体独立化
- 泛型 `T` 承载类型下 stack register 的静态类型未知场景（当前假设编译期能确定 IType；若命中 unknown，退化路径已有兜底）
