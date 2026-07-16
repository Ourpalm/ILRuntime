# Neo Step B Handoff — Stack Register Slot Layout 修复

> **状态**：Step B 全部落地完成，回归测试通过（`NeoStep6/7/8/9/11` 全绿；`NeoStep10` 4/5 pass，剩 1 例为 Step 10 遗留 NRE，非 Step B 引入）
>
> **Handoff 时间点**：2026-07-16
>
> **分支**：`features/object-model-overhaul`
>
> **相关 spec**：
> - [Step B spec](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/spec.md)
> - [Step B tasks](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/tasks.md)
> - [Step B checklist](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/checklist.md)
> - [Step 11 handoff](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/handoff.md) — 触发本 Step 的问题源
>
> **设计文档基线**：[object-model-neo-design.md §4.3/§4.4](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md#L237-L293)

---

## 1. 目标与最终范围

Step B 承担的核心目标：**修正 `AllocateLocalStackSpaces` 中 stack register 段的 upper-bound 分配偏离**，让 Neo 模式的所有 slot（params / locals / tmp）都按 `object-model-neo-design.md` §4.3/§4.4 的规则 —— 按实际承载类型独立分配 `(Size, RefCount)`。

在实施过程中，因遇到 `Move layout mismatch` DEBUG 断言，范围顺势扩展并**吸纳了 SSA rename pass**（B-0），从架构上根除了 stack register 跨类型复用引发的 slot 冲突。

**最终落地的子任务**：
| 子步 | 内容 |
|---|---|
| B-0 | 线性 SSA rename pass：locals + tmp 段（`>= paramRegEnd`）跨类型不兼容写入触发 rename |
| B-1 | `AllocateLocalStackSpaces` 按 `registerTypes[reg]` 逐 slot 分配 |
| B-2 | `TypeSpecializeAndRenameNeoRegisters` 覆盖所有产生 dst 的 opcode |
| B-3 | `Optimizer.Neo.cs` Move case 基于 slot layout 判定 refMove |
| B-4 | 帧元数据（`TotalRefSize` / `TotalStructSize`）语义收紧 |
| B-5 | Step 6 `min(src, dst)` workaround 移除 |
| B-6（新增） | Sub-int primitive 上取 4 字节（CIL evaluation stack 语义） |
| B-7（新增） | 修复 SSA rename 破坏 branch target 的 union-alias bug |

---

## 2. 实施过程中发现并修复的两处根本 bug

### 2.1 `OpCodeR` union alias 破坏 branch target

**现象**：`NeoStep6Test.NeoSumI8` 陷入死循环，`brtrue.s r2, 9` 目标错乱，`ldc.i4.1 r5` 从未被执行到（循环步进被跳过）。

**根因**：`OpCodeR` 是 union struct，`Register3` 与 `Operand` 均映射到 offset 8。SSA rename pass 若通用地把 `op.Register3` 用 `renameMap` remap，会误改 `Brtrue/Brfalse` 等 branch 指令的 `op.Operand`（分支目标 IL offset）。

**修复**：SSA rename pass 的 loop prologue 改用 [Optimizer.Utils.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Utils.cs) 的 `GetOpcodeSourceRegister/ReplaceOpcodeSource`，按 opcode 精确路由源操作数字段，避免读到 non-register 数据。

**Lesson**：**在读写 union struct 字段时，必须以 opcode 分类为准，而不是字段名。** 项目中已有的 `GetOpcodeSourceRegister/ReplaceOpcodeSource` 就是为此设计，Neo pass 复用即可。

### 2.2 Sub-int primitive slot 大小不符 CIL evaluation stack 语义

**现象**：`add.i8 r6, r1, r7` 时 `SrcOffset=8`、`OperandOffset=40` 读到的均为 0（应为 1）。断点确认 `ldc.i4.1 r5` 已执行，但写入的物理地址与 `add.i8` 读的地址不同。

**根因**：`AllocateSlotForType` 对 `bool/byte/sbyte/short/ushort/char` 只分配 1-2 字节 slot。当 IL 层写入是宽 `int*` 操作时，会跨 slot 越界覆盖相邻 slot 的低字节，破坏其它寄存器数据。

**修复**：`AllocateSlotForType` 中把 sub-int primitive size 上取到 4 字节：

```csharp
if (t.IsPrimitive)
{
    int size = appdomain.GetPrimitiveSize(t);
    if (size < 4) size = 4; // widen sub-int to CIL evaluation stack slot
    // ...
}
```

**范围澄清**：此改动**仅影响 `AllocateSlotForType`（stack slot 分配）**，与 `ILType.InitializeFields`（struct 内部字段布局）走**独立路径**。struct field layout 目前仍按声明顺序累加，尚未实现 `StructLayout (Pack/FieldOffset)` 完整语义，归 Step 12/13 处理。

**Lesson**：Stack slot 与 struct field 是两条独立的 layout 路径，不能一处改动污染另一处。CIL evaluation stack 语义要求 sub-int 在栈上按 4 字节 slot 存放，此规则应体现在 stack slot 分配，而非侵入 struct ABI。

---

## 3. 关键决策要点

### 3.1 CopyPropagation 不动，靠 SSA rename 消化冲突

Legacy `CopyPropagation` 优化会将不同类型合并到同一 local（例如 callvirt 的 int 返回值写入 IL 声明为 interface 的 local）。用户明确要求：**"CopyPropagation 不能动，Move 无法消除时必须能正确工作"**。

**方案确定**：把 SSA rename 阈值从 `baseRegStart`（this + params + locals）改为 `paramRegEnd`（this + params），让 locals 也参与 rename。这样 CopyPropagation 引入的类型冲突由 SSA rename 消化，物理 slot 单一类型属性保留。

**收益**：帧最小、mStack 引用槽最少、Move handler 分支预测最优、DEBUG 断言可永久保留作为未来 slot 分配 bug 的安全网。

### 3.2 SSA rename 归属 Optimizer，不侵入 JITCompiler

用户明确指出：**"SSA rename 是 optimizer 的 step，应放 Optimizer 而不是从 JITCompiler 访问"**。

**实现**：
- [Optimizer.NeoTypeSpecialize.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.NeoTypeSpecialize.cs)：`partial class Optimizer`，仅包含 `TypeSpecializeAndRenameNeoRegisters`、`GetRegisterTypeFromList`、`SlotLayoutCompatible` 等辅助方法。无状态、按 `AppDomain` 参数传入。
- [JITCompiler.NeoHelpers.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.NeoHelpers.cs)：`partial struct JITCompiler`，仅保留 `BuildInitialRegisterTypes`（元数据入口，需要访问 method / frame 结构）。
- [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs)：主控流调用 `Optimizer.TypeSpecializeAndRenameNeoRegisters`，接收 `newTotalRegCnt` 后更新 `frame.StackRegisterCount`。

### 3.3 Struct-this ABI 一致性归 Step 12，不在本 Step 收口

**现象**：async state machine `MoveNext` 中 `r0`（this）与 `r3`（local V_2）虽同为同一 struct 类型，但获取 IType 的路径不同（`declaringType.TotalReferenceCount=0` vs `AppDomain.GetType` 得到的 slot `(4, 1)`），导致 Move layout mismatch。

**决策**：属 Step 12 值类型完整 lowering 议题。用户已在测试用例中注释 async 相关代码，Step B 完成范围内所有非 struct-this 场景均已通过。

### 3.4 未来 struct layout 与 stack slot layout 的边界

- **Stack slot layout**（`AllocateSlotForType`）：CIL evaluation stack 语义，sub-int 上取 4 字节 → **本 Step 已完成**
- **Struct field layout**（`ILType.InitializeFields`）：按 CLR 规范支持 `StructLayout (Pack/FieldOffset)` → **Step 12/13 待做**
- 两者互不干扰。Step 12/13 实现 struct 内部布局时，无需回退本 Step 的 sub-int 上取 4 字节改动。

---

## 4. 代码文件索引

### 修改
- [ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs)
  - `Compile`：调用 `Optimizer.TypeSpecializeAndRenameNeoRegisters`，按 `newTotalRegCnt` 更新 `frame.StackRegisterCount`
  - `AllocateLocalStackSpaces`：locals + stack register 段统一按 `registerTypes[reg]` 分配（等价 `AllocateSlotForType`）
  - `AllocateSlotForType`：sub-int primitive 上取 4 字节
  - 删除旧 `TypeSpecializeNeoOpcodes` 冗余分支
- [ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs)
  - Move case：`isRefMove = LocalInfos[srcReg].RefCount > 0`；`sz = LocalInfos[srcReg].Size`；`min(src,dst)` workaround 已删；DEBUG 断言 src/dst layout 一致
- [ILRuntime/Runtime/Intepreter/OpCodes/OpCode.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCode.cs)
  - `ToString`：补齐 Neo 类型化变体（`_I8/_R4/_R8/Beq_/Beqi_/Addi_/…`）的 pretty print
- [ILRuntimeTestBase/Adapters/helper.cs](file:///f:/SVN/ILRuntime/ILRuntimeTestBase/Adapters/helper.cs)
- [ILRuntimeTestCLI/Properties/launchSettings.json](file:///f:/SVN/ILRuntime/ILRuntimeTestCLI/Properties/launchSettings.json)
  - 测试脚手架微调

### 新增
- [ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.NeoHelpers.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.NeoHelpers.cs)
  - `partial struct JITCompiler`：`BuildInitialRegisterTypes(this + params + locals)`
- [ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.NeoTypeSpecialize.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.NeoTypeSpecialize.cs)
  - `partial class Optimizer`：`TypeSpecializeAndRenameNeoRegisters` 合并 SSA rename + 类型推断 pass

---

## 5. 测试结果矩阵

**Neo 模式**（`-c Debug_Neo`, `useRegister=true`）：

| 测试组 | 用例数 | 通过 | 状态 |
|---|---|---|---|
| `NeoStep6Test` | 14 | 14 | ✅ |
| `NeoStep7Step8Test` | 7 | 7 | ✅ |
| `NeoStep9Test` | - | - | ✅ |
| `NeoStep10Test` | 5 | 4 | ⚠️ `NeoStep10TestClrVirtualToString` NRE — **pre-existing**，属 Step 10 handoff 遗留 |
| `NeoStep11Test` | 5 | 5 | ✅（含单跑 & 全组） |

**Legacy 模式**（`-c Debug`）：全套通过（改动全部在 `#if ENABLE_NEO_MODE` 内，未触及 Legacy 路径）

**编译验证**：
- `dotnet build -c Debug ILRuntime/ILRuntime.csproj` → 0 错误 0 警告
- `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` → 0 错误 0 警告
- `dotnet build --framework net8.0 -c Release HotfixAOT/` → 0 错误
- `dotnet build --framework net8.0 -c Release_Patched HotfixAOT/` → 0 错误
- `dotnet build TestCases/` → 0 错误

---

## 6. 遗留项与推迟事项

### 6.1 归 Step 12 处理
- **Async state machine struct-this ABI 一致性**：`declaringType` 与 `AppDomain.GetType` 两条路径解析同一 struct 类型时 `TotalReferenceCount` 不一致，导致 slot layout mismatch。用户已注释相关 async 测试。
- **Struct field 完整 layout**：`StructLayout (Pack/FieldOffset)` 语义、bool/byte 在 struct 内部按声明布局。`ILType.InitializeFields` 目前仅按声明顺序累加 primitive/reference 段。
- **Callvirt / constrained. callvirt opcode 变体独立化**：本 Step 未涉及。
- **`implement-neo-step3-step4/spec.md` 与 `implement-neo-step6/checklist.md L79` 的补账备注**：Step 12 spec 起草时一并回填。

### 6.2 归 Step 10 handoff 遗留
- **`NeoStep10TestClrVirtualToString` NRE**：CLR virtual `ToString` 分派 NRE，与 Step B 无关。

### 6.3 未来优化（无 spec）
- Slot 复用 / liveness 分析（graph coloring）
- 泛型 `T` 承载类型下 stack register 的静态类型未知场景（当前 null-fallback 为 `(4, 0)`）

---

## 7. 运行命令速查

**构建 HotfixAOT + patch**：
```
dotnet build --framework net8.0 -c Release HotfixAOT/
dotnet build --framework net8.0 -c Release_Patched HotfixAOT/
dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -i -h HotfixAOT/Patched/HotfixAOT.hash -o HotfixAOT/Patched/HotfixAOT.dll HotfixAOT/bin/Release/net8.0/HotfixAOT.dll
dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -p -o HotfixAOT/Patched/HotfixAOT.patch HotfixAOT/Patched/HotfixAOT.hash HotfixAOT/bin/Release_Patched/net8.0/HotfixAOT.dll
```

**构建 TestCases**：
```
dotnet build TestCases/
```

**Neo 模式跑测试**（`useRegister=true`，`-c Debug_Neo`）：
```
dotnet run -c Debug_Neo --verbosity normal --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj TestCases/bin/Debug/netstandard2.1/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch true [nameFilter]
```

**Legacy 模式跑测试**（`useRegister=false`，`-c Debug`）：
```
dotnet run --verbosity normal --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj TestCases/bin/Debug/netstandard2.0/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch false
```

---

## 8. 交给下一步（Step 12）的输入契约

1. **每个 slot 单一类型**：locals + tmp 经 SSA rename 后，每个物理 register 承载类型稳定；`LocalInfos[reg].Size == expectedType.SlotSize && LocalInfos[reg].RefCount == expectedType.SlotRefCount` 恒真。
2. **Stack slot layout 与 struct field layout 独立**：Step 12 实现 `StructLayout` 完整语义时，只需修改 `ILType.InitializeFields`；无需回退本 Step 的 sub-int 4 字节上取。
3. **Move handler 语义稳定**：`isRefMove = srcRef > 0`；`sz = srcSize`；DEBUG 下 src/dst layout 断言。任何未来引入的 slot 分配 bug 会在此处即时暴露。
4. **`registerTypes[]` 数组契约**：pass 结束后长度 = `newTotalRegCnt`；`registerTypes[r] == null` 表示该 register 从未被产生结果的 opcode 写入（安全 fallback 为 `(4, 0)`）。
5. **Struct-this ABI 归属 Step 12**：当前 async 测试被注释，Step 12 必须首先统一 `declaringType` 与 `AppDomain.GetType` 两条路径对同一 struct 的 `TotalReferenceCount` 解析结果。
