# Step B Checklist — Stack Register Slot Layout 修复

## 修改范围
- [ ] 所有代码改动位于 `#if ENABLE_NEO_MODE` 块内
- [ ] 无 Legacy 路径任何行为改动
- [ ] 无新文件（除 `.trae/specs/implement-neo-stepB/`）

## `TypeSpecializeNeoOpcodes` 覆盖度
- [ ] Call 变体所有分支 `SetRegisterType(op.Register1, m.ReturnType)`（有返回值时）
- [ ] Callvirt / Callvirt_IL / Callvirt_CLR 覆盖
- [ ] Call_Redirect / Call_Redirect_IL 覆盖
- [ ] Newobj 覆盖
- [ ] Box / Unbox / Unbox_Any 覆盖
- [ ] Ldelem_* / Ldelema 覆盖
- [ ] Ldsfld_* 全套 primitive & ref 覆盖
- [ ] Ldsfld / Ldsflda / Ldsfld_Ref / Ldsfld_Value 覆盖
- [ ] Ldtoken / Ldftn / Ldvirtftn 覆盖
- [ ] Isinst / Castclass 覆盖
- [ ] Ldloca / Ldarga / Ldflda 覆盖（managed pointer 兜底）
- [ ] Dup 覆盖（若存在于 Neo lowering 后 IR）

## `AllocateLocalStackSpaces` 重构
- [ ] Stack register 循环改为按 `registerTypes[baseRegStart + i]` 逐 slot 独立分配
- [ ] `registerTypes[r] == null` fallback 到 `Size=4, RefCount=0`
- [ ] `int maxSize = 8, maxRefCount = 1;` 已删除
- [ ] `GatherValueTypes` 若无其它调用点，已删除
- [ ] `LocalIsReference[baseRegStart + i]` 逻辑正确（RefCount>0 && Size==4）

## Move 指令 refMove 判定
- [ ] `JITCompiler.TypeSpecializeNeoOpcodes` Move case 不再写 `op.Operand`
- [ ] `Optimizer.Neo.cs` Move case 基于 `LocalInfos[srcReg].RefCount > 0` 判定 refMove
- [ ] `op.Operand2 = LocalInfos[srcReg].Size`
- [ ] `op.Operand3 = LocalInfos[dstReg].RefOffset`
- [ ] `min(srcSz, dstSz)` 补丁已删除
- [ ] DEBUG 断言 src/dst layout 一致

## 主控流验证
- [ ] `TypeSpecializeNeoOpcodes` 在 `AllocateLocalStackSpaces` 之前调用
- [ ] `registerTypes` 数组容量 ≥ `paramCnt + varCnt + StackRegisterCount`
- [ ] BuildInitialRegisterTypes 初始化到 paramCnt + varCnt，stack register 段初值为 null

## 帧元数据 & 调用侧
- [ ] `TotalRefSize` 与 `TotalStructSize` 反映精确 slot sizing 后的实际值
- [ ] Callvirt/Call 返回引用 slot 定位仍通过 `LocalInfos[dstReg].RefOffset` 正确
- [ ] Ret handler 引用返回仍正确

## 编译
- [ ] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误 0 警告
- [ ] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误 0 警告
- [ ] `dotnet build --framework net8.0 -c Release HotfixAOT/` 0 错误
- [ ] `dotnet build --framework net8.0 -c Release_Patched HotfixAOT/` 0 错误
- [ ] `dotnet build TestCases/` 0 错误

## 回归测试

### Neo 模式（`useRegister=true`, `-c Debug_Neo`）
- [ ] `NeoStep6Test` 全绿
- [ ] `NeoStep7Test` 全绿
- [ ] `NeoStep8Test` 全绿
- [ ] `NeoStep9Test` 全绿
- [ ] `NeoStep10Test` 全绿
- [ ] `NeoStep11Test` 全绿（含单跑 & 全组）
  - [ ] `NeoStep11Basic`
  - [ ] `NeoStep11InheritedInterfaceImpl`
  - [ ] `NeoStep11InterfaceInheritance`（单跑 & 全组）
  - [ ] `NeoStep11ExplicitImplementation`
  - [ ] `NeoStep11MultipleInterfaces`（单跑 & 全组）

### Legacy 模式（`useRegister=false`, `-c Debug`）
- [ ] 全套测试用例通过（确保 Legacy 未回归）

### Legacy Register VM 模式（`useRegister=true`, `-c Debug`）
- [ ] 全套测试用例通过

## 文档收尾
- [ ] [handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/handoff.md) 撰写
- [ ] [implement-neo-step3-step4](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step3-step4) 添加"stack register 段遗漏由 Step B 补账"备注
- [ ] [implement-neo-step6/checklist.md L79](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step6/checklist.md#L79) 添加"Step B 已根除"备注
