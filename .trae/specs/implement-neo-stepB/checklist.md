# Step B Checklist — Stack Register Slot Layout 修复

## Stack register SSA rename（Task 0）
- [x] 新增 `SlotLayoutKey` / `SlotLayoutCompatible` 辅助（内嵌于 `Optimizer.NeoTypeSpecialize.cs`）
- [x] 合并 pass `TypeSpecializeAndRenameNeoRegisters` 落地（迁至 `Optimizer` partial class）
- [x] 跨类型复用触发 rename；同 layout 复写不 rename
- [x] param 段（`< paramRegEnd`）不 rename；locals + tmp 均参与 rename
- [x] `Compile` 主控流按 `newTotalRegCnt` 更新 `frame.StackRegisterCount`
- [x] `frame.CodeBody = res.ToArray()` 在 pass 之后
- [x] Move DEBUG 断言不再触发 `Move layout mismatch`（NeoStep6/7/8/11 全绿；async 测试已由用户注释，归 Step 12）
- [x] Loop prologue 通过 `GetOpcodeSourceRegister/ReplaceOpcodeSource` 精确 remap，避免破坏 Brtrue/Brfalse 等 branch 指令的 `op.Operand`（Union alias bug 已修）
- [x] Branch 指令（Beq/Blt/Beqi/...）的 `Register1` 作为 src 通过 prologue 统一 remap

## 修改范围
- [x] 所有代码改动位于 `#if ENABLE_NEO_MODE` 块内
- [x] 无 Legacy 路径任何行为改动（`dotnet build -c Debug` 0 错误 0 警告）
- [x] 新增文件：`Optimizer.NeoTypeSpecialize.cs`、`JITCompiler.NeoHelpers.cs`（均为 partial class 拆分，无独立类型）

## `TypeSpecializeAndRenameNeoRegisters` 覆盖度
- [x] Call 变体所有分支覆盖返回值类型（有返回值时写 `Register1`）
- [x] Callvirt / Callvirt_IL / Callvirt_CLR 覆盖
- [x] Call_Redirect / Call_Redirect_IL 覆盖
- [x] Newobj 覆盖（Register1 = 目标类型）
- [x] Box / Unbox / Unbox_Any 覆盖
- [x] Ldelem_* / Ldelema 覆盖
- [x] Ldsfld_* 全套 primitive & ref 覆盖
- [x] Ldsfld / Ldsflda / Ldsfld_Ref / Ldsfld_Value 覆盖
- [x] Ldtoken / Ldftn / Ldvirtftn 覆盖
- [x] Isinst / Castclass 覆盖
- [x] Ldloca / Ldarga / Ldflda 覆盖（managed pointer 兜底为 IntType）
- [x] Ldc_I4_*/I8/R4/R8/Ldnull/Ldstr 覆盖
- [x] Neg/Not/Add/Sub/Mul/Div/…（含 `_I8`/`_R4`/`_R8` 特化）
- [x] Ceq/Cgt/Clt 及 Beq/Bge/Bgt/Ble/Blt 特化

## `AllocateLocalStackSpaces` 重构
- [x] Local + Stack register 循环合并为按 `registerTypes[reg]` 逐 slot 独立分配
- [x] `registerTypes[r] == null` fallback 到 `Size=4, RefCount=0`
- [x] `int maxSize = 8, maxRefCount = 1;` 已删除
- [x] `GatherValueTypes` upper-bound 路径已删除
- [x] `LocalIsReference[baseRegStart + i]` 逻辑正确（RefCount>0 && Size==4）
- [x] `AllocateSlotForType` 对 sub-int primitive（bool/byte/sbyte/short/ushort/char）上取 4 字节（CIL evaluation stack 语义要求）

## Move 指令 refMove 判定
- [x] `TypeSpecializeAndRenameNeoRegisters` Move case 不再写 `op.Operand`
- [x] `Optimizer.Neo.cs` Move case 基于 `LocalInfos[srcReg].RefCount > 0` 判定 refMove
- [x] `op.Operand2 = LocalInfos[srcReg].Size`
- [x] `op.Operand3 = LocalInfos[dstReg].RefOffset`
- [x] `min(srcSz, dstSz)` 补丁已删除
- [x] DEBUG 断言 src/dst layout 一致

## 主控流验证
- [x] `TypeSpecializeAndRenameNeoRegisters` 在 `AllocateLocalStackSpaces` 之前调用
- [x] `registerTypes` 数组容量 ≥ `paramCnt + varCnt + StackRegisterCount`（rename 后按 `newTotalRegCnt` 动态扩容）
- [x] `BuildInitialRegisterTypes` 初始化 this + params + locals，stack register 段初值为 null

## 帧元数据 & 调用侧
- [x] `TotalRefSize` 与 `TotalStructSize` 反映精确 slot sizing 后的实际值
- [x] Callvirt/Call 返回引用 slot 定位仍通过 `LocalInfos[dstReg].RefOffset` 正确
- [x] Ret handler 引用返回仍正确

## 编译
- [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误 0 警告
- [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误 0 警告
- [x] `dotnet build --framework net8.0 -c Release HotfixAOT/` 0 错误
- [x] `dotnet build --framework net8.0 -c Release_Patched HotfixAOT/` 0 错误
- [x] `dotnet build TestCases/` 0 错误

## 回归测试

### Neo 模式（`useRegister=true`, `-c Debug_Neo`）
- [x] `NeoStep6Test` 14/14 全绿
- [x] `NeoStep7Step8Test` 7/7 全绿
- [x] `NeoStep9Test` 全绿
- [x] `NeoStep10Test` 4/5（`NeoStep10TestClrVirtualToString` 为 pre-existing NRE，属 Step 10 handoff 遗留项，非 Step B 回归）
- [x] `NeoStep11Test` 5/5 全绿（含单跑 & 全组）
  - [x] `NeoStep11InterfaceBasic`
  - [x] `NeoStep11InheritedInterfaceImpl`
  - [x] `NeoStep11InterfaceInheritance`（单跑 & 全组）
  - [x] `NeoStep11ExplicitImplementation`
  - [x] `NeoStep11MultipleInterfaces`（单跑 & 全组）

### Legacy 模式（`useRegister=false`, `-c Debug`）
- [x] 全套测试用例通过（Legacy 未回归；改动全部在 `#if ENABLE_NEO_MODE`）

### 已知遗留（不属 Step B 范围）
- `AsyncAwaitTest` 相关：async state machine `struct-this` ABI 一致性问题，用户已注释相关测试，归 Step 12 值类型完整 lowering 处理
- `NeoStep10TestClrVirtualToString`：CLR `ToString` 分派 NRE，属 Step 10 handoff 已知遗留

## 文档收尾
- [x] [handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/handoff.md) 撰写
- [x] Step B 完成情况回写 project_memory（sub-int 4B 对齐仅影响 stack slot，不污染 struct field layout）
- [ ] [implement-neo-step3-step4](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step3-step4) 添加"stack register 段遗漏由 Step B 补账"备注（推迟到 Step 12 spec 起草时一并回填）
- [ ] [implement-neo-step6/checklist.md L79](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step6/checklist.md#L79) 添加"Step B 已根除"备注（同上）
