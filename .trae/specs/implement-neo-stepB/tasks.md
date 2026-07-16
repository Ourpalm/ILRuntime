# Step B Tasks — Stack Register Slot Layout 修复

## Task 0: Stack register SSA rename pass（新增）✅

**目标**：合并 `TypeSpecializeNeoOpcodes` 与线性 SSA rename 为一个 pass `TypeSpecializeAndRenameNeoRegisters`。当同一 stack register 被写入两种 slot-layout 不兼容的类型时，为后续用途分配一个新的虚拟寄存器索引。

- [x] 新增 `SlotLayoutCompatible(IType a, IType b)`：其中一方为 `null` 即兼容；否则比较 `(Size, RefCount)` 桶。ILType 值类型比较包含具体 struct 身份。
- [x] 实现合并 pass `TypeSpecializeAndRenameNeoRegisters(List<OpCodeR> body, short paramRegEnd, int totalRegCnt, IType[] initialTypes, AppDomain appdomain, out int newTotalRegCnt)`：
  - 位于 `Optimizer.NeoTypeSpecialize.cs`（`partial class Optimizer`）
  - 参数 `paramRegEnd = locVarRegStart`：locals 也参与 rename，仅保护 this + params
  - Loop prologue 用 `GetOpcodeSourceRegister/ReplaceOpcodeSource` 精确 remap 源操作数
  - 处理 Branch (Beq/Blt/…) 的 Register1 作为 src 由 prologue 统一 remap
  - 处理 Ret/Push/Throw/Stfld 等指令 Register1 作为 src 的场景
- [x] `Compile` 主控流：调 pass 得到 `registerTypes` 与 `newTotalRegCnt`，更新 `frame.StackRegisterCount = Math.Max(newTotalRegCnt - baseRegStart, 0)`
- [x] Move 指令 dst 也参与 rename
- [x] Call/Callvirt/Newobj 参数寄存器作为 src 由 prologue 统一 remap

**关键 bug 发现（合入前）**：`OpCodeR` 是 union struct，`Register3` 与 `Operand` 都在 offset 8 alias。SSA rename 若盲目 remap `op.Register3`，会破坏 `Brtrue/Brfalse` 等 branch 指令的 `op.Operand`（跳转 target）。修复：`Optimizer.Utils.cs` 的 `GetOpcodeSourceRegister/ReplaceOpcodeSource` 按 opcode 精确路由。

## Task 1: `TypeSpecializeAndRenameNeoRegisters` 补齐 SetRegisterType ✅

覆盖所有产生 dst 的 opcode（`Call/Callvirt/Callvirt_IL/Callvirt_CLR/Call_Redirect/Call_Redirect_IL/Newobj/Ldelem_*/Ldelema/Box/Unbox/Unbox_Any/Ldsfld_*/Ldtoken/Ldftn/Ldvirtftn/Isinst/Castclass/Ldloca/Ldarga/Ldflda/Ldstr/Ldnull/Ldc_*/Neg/Not/算术/比较`）。

## Task 2: `AllocateLocalStackSpaces` stack register 段重写 ✅

- [x] Local 段 + Stack register 段合并为按 `registerTypes[reg]` 逐 slot 独立分配（等价 `AllocateSlotForType`）
- [x] `registerTypes[r] == null` fallback 到 `Size=4, RefCount=0`
- [x] 删除 `int maxSize = 8, maxRefCount = 1;` 与 `GatherValueTypes` upper-bound 路径
- [x] `LocalIsReference[reg]` 保持 `RefCount > 0 && Size == 4` 语义
- [x] **额外发现**：`AllocateSlotForType` 中 sub-int primitive（bool/byte/sbyte/short/ushort/char）必须上取 4 字节，符合 CIL evaluation stack 语义。struct field layout（`ILType.InitializeFields`）走独立路径不受影响。

## Task 3: Move 指令 refMove 判定下沉 ✅

- [x] JIT 侧删除 `op.Operand = IsNeoReferenceSlot(srcType) ? 1 : 0;`
- [x] `Optimizer.Neo.cs` Move case 基于 `LocalInfos[srcReg].RefCount > 0` 判定 isRefMove
- [x] `op.Operand2 = LocalInfos[srcReg].Size`，`op.Operand3 = LocalInfos[dstReg].RefOffset`
- [x] 删除 `min(srcSz, dstSz)` workaround
- [x] DEBUG 断言 src/dst layout 一致

## Task 4: 主控流验证 ✅

- [x] `TypeSpecializeAndRenameNeoRegisters` 在 `AllocateLocalStackSpaces` 之前调用
- [x] `registerTypes` 数组按 `newTotalRegCnt` 动态扩容
- [x] `BuildInitialRegisterTypes`（迁至 `JITCompiler.NeoHelpers.cs`）初始化 this + params + locals

## Task 5: 帧元数据 & 调用侧回归验证 ✅

- [x] Callvirt/Call 返回引用 slot 定位链依赖 `LocalInfos[dstReg].RefOffset`，自动生效
- [x] Ret handler 引用返回仍正确
- [x] `mStack.Count += TotalRefSize` 收缩到实际引用槽总数

## Task 6: Step 6 workaround 痕迹移除 ✅

- [x] `Optimizer.Neo.cs` L94-98 `min(src,dst)` 注释与逻辑已删除
- [x] Step 6 checklist L79 保留历史记录，将在 Step 12 spec 起草时补一行"已被 Step B 根除"

## Task 7: 编译验证 ✅

- [x] `dotnet build --framework net8.0 -c Release HotfixAOT/` 0 错误
- [x] `dotnet build --framework net8.0 -c Release_Patched HotfixAOT/` 0 错误
- [x] `PatchTool` 生成 patch 成功
- [x] `dotnet build TestCases/` 0 错误
- [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误 0 警告（Legacy 未受影响）
- [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误 0 警告

## Task 8: 回归测试 ✅

- [x] Neo `NeoStep6Test` 14/14 全绿
- [x] Neo `NeoStep7Step8Test` 7/7 全绿
- [x] Neo `NeoStep9Test` 全绿
- [x] Neo `NeoStep10Test` 4/5（`NeoStep10TestClrVirtualToString` pre-existing NRE，属 Step 10 handoff 遗留项）
- [x] Neo `NeoStep11Test` 5/5 全绿：
  - `NeoStep11InterfaceBasic` ✅
  - `NeoStep11InheritedInterfaceImpl` ✅
  - `NeoStep11InterfaceInheritance` ✅（单跑 & 全组）
  - `NeoStep11ExplicitImplementation` ✅
  - `NeoStep11MultipleInterfaces` ✅（单跑 & 全组）
- [x] Legacy 模式（`useRegister=false`, `-c Debug`）全套通过（改动全部在 `#if ENABLE_NEO_MODE` 内）

**推迟到 Step 12 的遗留**：
- `AsyncAwaitTest` 相关：async state machine `struct-this` ABI 一致性问题（用户已注释相关测试）

## Task 9: Handoff & Spec 收尾 ✅

- [x] 撰写 [handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/handoff.md)
- [x] 更新 [checklist.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/checklist.md)
- [x] 更新 [tasks.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/tasks.md)（本文档）
- [ ] `implement-neo-step3-step4/spec.md` 备注补账（推迟到 Step 12 起草时一并回填）
- [ ] `implement-neo-step6/checklist.md L79` 补一行"已被 Step B 根除"（推迟到 Step 12 起草时一并回填）

# Task Dependencies

- Task 2 依赖 Task 1（slot 分配读 registerTypes）
- Task 3 依赖 Task 2（Move refMove 判定读 LocalInfos.RefCount）
- Task 5 依赖 Task 2
- Task 7 依赖 Task 1-6
- Task 8 依赖 Task 7
- Task 9 依赖 Task 8

# 不在本 Spec 范围（明确推迟）

- Slot 复用 / liveness 分析（未来 Step）
- 值类型完整 lowering / `StructLayout` (Pack/FieldOffset) 语义（Step 12/13）
- Async state machine `struct-this` ABI 一致性（Step 12）
- CLR virtual `ToString` 分派 NRE（Step 10 handoff 遗留）
- Callvirt / constrained. callvirt opcode 变体独立化（Step 12 议题）
- 泛型 `T` 承载类型下 stack register 的静态类型未知场景（已有 null-fallback 兜底）
