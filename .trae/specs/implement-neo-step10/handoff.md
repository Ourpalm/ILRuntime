# Neo Step 10 Handoff — VTable 构建与虚方法分派

本文件用于将 Step 10 的当前实现状态、关键设计决策、隐蔽坑点、遗留 TODO 交接给下一位实现者，方便无缝进入 Step 11（接口方法分派）或其他后续步骤。

## 1. Step 10 完成情况总览

- 所有 Task 1–8 已勾选，`.trae/specs/implement-neo-step10/checklist.md` 全部通过。
- 构建：`dotnet build "ILRuntime\ILRuntime.csproj" -c Debug_Neo` 通过，0 error。
- 测试（`Debug_Neo` + `useRegister=true`）：
  - `NeoStep10Test`：5 passed / 0 failed
  - `NeoStep7Step8Test`：7 passed / 0 failed（Step8/newobj 回归）
  - `NeoStep6Test`：14 passed / 0 failed（Step6 基础回归）
- 参考文档：
  - `.trae/documents/object-model-neo-design.md` 第 13 节（VTable 设计）已更新
  - `.trae/documents/neo-implementation-steps.md` Step 10 段已补 TODO
  - `.trae/specs/implement-neo-step10/spec.md` / `tasks.md` / `checklist.md`

## 2. 已落地的关键改动

### 2.1 类型系统
- [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs)
  - 新增 `NeoVTable`（`IMethod[]`）、`neoVTableSlotKeys`、`neoVTableSlots`（string→slot 的 dictionary），全部 `#if ENABLE_NEO_MODE`。
  - `BuildNeoVTable()` 递归继承基类 slot，`FindNeoOverrideSlot` 处理 override（同签名优先，然后 `Definition.HasOverrides` / 非 `IsNewSlot` fallback）。
  - CLR 基类虚方法通过 `AddNeoBaseVirtualSlots` 加入 slot。
  - `TryGetNeoVTableSlot(IMethod, out int)` 供 JIT 查询 slot。
  - 值类型 / 接口不构建 VTable，`EnsureNeoVTable` 幂等构建 + 循环保护。

### 2.2 方法签名
- [IMethod.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/IMethod.cs) 新增 `string SignatureString { get; }`。
- [ILMethod.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/ILMethod.cs) / [CLRMethod.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/CLRMethod.cs) 各自实现懒加载缓存：格式为 `Name|GenericParameterCount(paramFullName,...)->returnFullName`。
- **注意**：这是 Step 10 的过渡实现，仅用于 VTable 构建期匹配，不上运行时热路径。后续（尤其是 Step 11 / 值类型 / AOT VTableTemplate）需要改成结构化签名比较器，参考 Legacy 的 `ILType.GetVirtualMethod()` / `GetMethod(... exactMatch: true)`。

### 2.3 Opcode 编码
- [OpCodeREnum.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCodeREnum.cs)
  - **`Callvirt_IL` / `Callvirt_CLR` 必须放在 enum 尾部 Neo-only 区域**。之前误插到 `Callvirt` 与 `Cpobj` 之间，导致所有后续 opcode 数值整体偏移，出现假的 `Cpobj`/`Arglist`。
  - JIT 里存在 `op.Code = (OpCodeREnum)code.Code` 的强转，Cecil `Code` 前半段必须严格保持数值对齐。
- Callvirt/Callvirt_IL/Callvirt_CLR/Call/Newobj 复用同一套 ABI：
  - `Operand` = `NeoCallParamMap` index
  - `Operand2` = 方法 hash（`Callvirt_CLR` 存已解析的 CLRMethod hash；`Callvirt_IL` 存声明方法的 hash 作为 fallback）
  - `Operand3` = 返回值目标 register 的 `RefOffset`（由 `Optimizer.Neo` 在 lowering 时写入）
  - `Operand4` low16 = vtable slot（`0xFFFF` 表示"运行时按 declared method 反查"）
  - `Operand4` high16 = `this` 在 callee frame 中的 byte offset

### 2.4 JIT lowering
- [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) 新增 `InitializeCallvirtDispatch`：
  - 非 abstract/virtual/interface 的 ILMethod `callvirt` → direct `Call`（继承 Legacy devirtualize）
  - ILMethod 且需要虚分派 → `Callvirt_IL` + slot
  - CLRMethod 且声明类型不是 `object`/interface → `Callvirt_CLR`
  - 其它（object/interface variable 等）→ 通用 `Callvirt`
- `Call_Redirect` 路径显式跳过 `Callvirt*` 三个 opcode，避免与 CLR redirection 撞车。

### 2.5 Optimizer / 参数区
- [Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs)
  - `Call/Callvirt/Callvirt_IL/Callvirt_CLR/Newobj` 走同一段参数复制生成（`NeoCallParamMap`）。
  - lowering 时把 `op.Operand3 = localInfos[op.Register1].RefOffset`，即返回目标 register 的 ref slot。
  - `hasConstrained` 判断显式排除 `Callvirt_IL` / `Callvirt_CLR`，只对普通 `Callvirt` 保留 `Operand4 == 1` 的 constrained marker（**冲突已被规避，但需要长期方案，见 §4**）。

### 2.6 解释器
- [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs)
  - `Call` / `Newobj` / `Callvirt_IL` / `Callvirt_CLR` / `Callvirt` 是**独立 switch case**，`Call` 热路径不再进入 callvirt 分派判断。
  - Helper：`CopyNeoCallArguments`、`ReadNeoCallThis`、`ResolveNeoCallvirtILTarget`、`ResolveNeoCallvirtCLRTarget`、`ResolveNeoGenericCallvirtTarget`、`InvokeNeoCallTarget`、`InvokeNeoClrMethod`。
  - **返回槽约定**：`targetRetRefBase = frameRefBase + ip->Operand3`。返回值 ref slot 用 caller frame 编译期预留的槽（`CompiledFrame.TotalRefSize` 已包含），不再用 `mStack.Count` 尾插临时 slot。
  - **`Ret` 引用返回逻辑**：只支持单引用返回场景，即 `!returnType.IsPrimitive && !returnType.IsValueType && returnPrimitiveSize == 4 && returnRefCount == 1`。写法上：
    ```csharp
    if (returnRefCount == 0) {
        if (returnPrimitiveSize > 0) CopyBlock(retDst, frameBase + ip->DstOffset, returnPrimitiveSize);
    } else {
        // 单引用返回：从 callee frame 读旧 mStack index，把对象复制到 caller 的 retRefBase，
        // 然后 caller 的 primitive slot 写成 caller 自己的 mStack index。
        int retSrcIdx = *(int*)(frameBase + ip->DstOffset);
        mStack[retRefBase] = retSrcIdx >= 0 ? mStack[retSrcIdx] : null;
        *(int*)retDst = retRefBase;
    }
    ```
  - 值类型返回、带引用字段的值类型返回：直接抛 `NotImplementedException("... Step 12/13 return layout support.")`。

### 2.7 调试器 / 异常包装
- [DebugService.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Debugger/DebugService.cs) 的 `GetLocalVariableInfo` / `GetThisInfo` 在 Neo frame 下直接返回占位字符串，不再按 Legacy `StackObject*` 解 Neo `byte*` frame。Neo 异常包装（`ILRuntimeException`）现在能正常打印堆栈，不再触发 `AccessViolationException`。

### 2.8 测试
- [NeoStep10Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep10Test.cs) 覆盖：
  - 简单继承 virtual/override
  - 多层继承链（含中间层 override 与叶子未 override）
  - CLR 虚方法（`string.ToString`）
  - `object` 变量分别持有 IL / CLR 对象
  - 非 virtual IL instance method 的 `callvirt` → direct `Call`
- 断言 helper 用多次 `Console.WriteLine` + `throw new Exception(...)`，避免触发 `string.Concat(string[])` 生成 `newarr`（属于 Step 6 未支持的 opcode）。

## 3. 关键坑点（避免重复踩）

1. **enum 数值对齐**：`OpCodeREnum` 前半段是 Cecil `Code` 强转来源，Neo 新增 opcode 一律放到 enum 尾部 Neo-only 区。
2. **异常路径不能读 Neo frame**：Neo 的 `StackFrame.LocalVarPointer/BasePointer` 实际是 `byte*`，所有走 `StackObject.ToObject` 的调试 helper 在 Neo 下必须短路。
3. **`Cpobj` / `Arglist` 不是 Step 10 阻塞**：如果再次出现，第一反应是检查 enum 是否又被插入到中间。
4. **caller frame 已预留返回 ref slot**：`CompiledFrame.TotalRefSize` 覆盖当前方法所有 params/locals/stack registers。返回引用只能写进 caller 编译期分配好的 slot（用 `Operand3` 的 `RefOffset` 定位），不能尾插 `mStack`。
5. **`Ret` 引用返回不是 refCount 遍历**：当前 Neo slot layout 下 primitive 区里存的是 mStack index（size=4），所以引用返回要么走 `CopyBlock`（`returnRefCount == 0`），要么走单引用 mStack 复制分支，**不能同时**。
6. **`PrewarmBodyRegister`**：不要把 `Callvirt_IL/CLR` 加进 `case OpCodeREnum.Callvirt:` 的预热扫描——旧代码用 `ins.Operand` 查 method，与 Neo lowering 后的 `Operand2` 语义不符。当前实现已保持只扫描普通 `Callvirt` 分支。
7. **`Constrained` 与 `Operand4`**：普通 `Callvirt + Operand4 == 1` 仍然是 constrained marker；`Callvirt_IL/CLR` 不允许携带 constrained。这两条互斥必须持续保证，见 §4。
8. **JIT prewarm 意外触发的 opcode**：如果 `TestSession.Load` 阶段又冒出未支持的 opcode，八成不是 Step 10 引入的，而是原本就不完整（如异步状态机）。不要为了绕过预热去补 Step12/13 语义。

## 4. 已知遗留 TODO

以下项已经在 spec 和文档里写明，Step 11 或后续步骤实现时**必须**处理：

1. **VTable 签名匹配升级**（见 spec §Requirement `后续结构化签名比较` / design §13.5.1）
   - 当前 `IMethod.SignatureString` 是字符串拼接，泛型参数、`ref/out`、显式接口、泛型实例、开放/封闭泛型都会脆弱。
   - Step 11 接口分派、AOT `VTableTemplate`、Step 12/13 值类型 override，都需要改成结构化签名比较器。
   - 建议参考 Legacy `ILType.GetVirtualMethod()` / `GetMethod(name, param, genericArgs, returnType, exactMatch)` / `CanAssignTo` 的成熟逻辑，抽出共享比较器。

2. **`Callvirt_Interface` 接口分派**（Step 11）
   - 设计文档 §13.6 已经描述：为每个 `ILType` 维护 `interfaceMap` / `interfaceOffsets`，`Callvirt_Interface_Neo` 用 `baseSlot = GetInterfaceVTableOffset(interfaceTypeIndex)` + `interfaceMethodSlot`。
   - `ILType.BuildNeoVTable` 目前显式跳过 interface 类型，Step 11 需要在 VTable 构建里加入接口偏移表。
   - 显式接口实现（`InterfaceName.Method`）目前在 `InitializeMethods` 已有额外命名，接口 slot 匹配要沿用同一套规则。

3. **`constrained. callvirt` 独立编码**
   - Step 10 只是"排除"，长期不能让普通 `Callvirt` 的 `Operand4 == 1` 与 vtable dispatch metadata 混用。
   - 建议 Step 12/13 值类型完整化时引入独立 `Callvirt_Constrained` opcode：`Operand` = param map、`Operand2` = method hash、`Operand3` = ret refoffset、`Operand4` = constrained type token/hash。
   - 涉及场景：`T : struct` 无 boxing 调用、值类型 override `object.ToString/Equals/GetHashCode`、interface constrained call、有/无 ValueTypeBinder 的 CLR struct 路径、`this` 是 managed pointer 的情况。

4. **sealed / final override 精确 devirtualize**
   - 结论：Step 10 不做。直接 lowering 为 `Call` 会失去 `callvirt` 的 null-this 检查语义。
   - 后续如果要做，必须先补独立的 Neo null-check lowering，并加定向 regression tests。

5. **值类型 / 带引用字段的值类型返回**
   - `Ret` 目前抛 `NotImplementedException`，属 Step 12/13 范围。
   - 需要按 `CompiledFrame` 的 `RefOffset` / field layout 精确复制，而不是 `i * 4` 遍历。

6. **Neo 调试信息**
   - `DebugService.GetLocalVariableInfo/GetThisInfo` 目前只是短路提示 `Neo local variable inspection is not supported yet.`。
   - Step 14 / Step 26 计划做完整的 Neo 调试符号，届时替换。

7. **CLRMethod redirection 与 constrained/interface 的交叉**
   - `Call_Redirect` 已排除 Callvirt* 三种 opcode，但 CLR redirection 对 interface / constrained 的 fallback 语义要在 Step 11 & Step 13 一并复核。

## 5. 快速上手命令

```powershell
# 构建 ILRuntime（Debug_Neo）
dotnet build "ILRuntime\ILRuntime.csproj" -c Debug_Neo

# 构建 TestCases
dotnet build TestCases/

# 运行 Step10 定向测试
dotnet run -c Debug_Neo --verbosity normal --framework net8.0 `
  --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj `
  TestCases/bin/Debug/netstandard2.1/TestCases.dll `
  HotfixAOT/Patched/HotfixAOT.patch true NeoStep10Test

# Step8 / Step6 回归
dotnet run ... true NeoStep7Step8Test
dotnet run ... true NeoStep6Test
```

**注意事项**（参见 `.trae/rules/unittest_guide.md`）：
- 必须用 `Debug_Neo` 配置跑 `ILRuntimeTestCLI`；`TestCases.dll` 输出仍在 `bin/Debug/netstandard2.1/`。
- CLI 参数 `useRegister` 必须传 `true`，否则跑的是 Legacy 栈机。
- 命令行不要用 pipeline (`| Select-String`) 或重定向 (`>`)。
- 单个 Neo 用例正常应在 5–20ms 内完成，超时（>10s）几乎是解释器死循环，`Stop-Process -Name ILRuntimeTestCLI -Force` 后看 JIT dump。

## 6. 关键文件索引

- 类型系统 / VTable：[ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs)
- 方法签名：[IMethod.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/IMethod.cs) / [ILMethod.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/ILMethod.cs) / [CLRMethod.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/Method/CLRMethod.cs)
- Opcode：[OpCodeREnum.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCodeREnum.cs) / [OpCode.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCode.cs)
- JIT lowering：[JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs)
- Neo optimizer / 参数区：[Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs)
- Neo optimizer helper：[Optimizer.Utils.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Utils.cs)
- Neo 解释器：[ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs)
- 调试服务：[DebugService.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Debugger/DebugService.cs)
- 测试：[NeoStep10Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep10Test.cs)
- 设计文档：[object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md)（§13, §16）
- 步骤文档：[neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md)（Step 10 / Step 11）
- 本 Spec：[spec.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step10/spec.md) / [tasks.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step10/tasks.md) / [checklist.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step10/checklist.md)

## 7. 建议的下一步

- 直接进入 Step 11（接口方法分派）。已有 `.trae/specs/implement-neo-step11/` 骨架文档（spec/tasks/checklist），可以复用 Step 10 的 lowering / dispatch / testing 基础设施。
- Step 11 实现时**优先**升级签名匹配为结构化比较器（见 §4-1），因为接口分派对显式实现、泛型接口方法非常敏感。
- Step 12/13 开始处理值类型 override / constrained callvirt / 值类型返回时，回来实现 §4-3 / §4-5。
