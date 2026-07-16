# Neo Step 11 Handoff

> **状态**：Step 11 功能实现已完成；测试因**前置遗留 bug** 3 个未通过，需通过 **Step B** 补账后回归。
>
> **Handoff 时间点**：2026-07-16
>
> **分支**：`features/object-model-overhaul`（未提交，含 Step 11 代码 + Step B spec）
>
> **相关 spec**：
> - [Step 11 spec](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/spec.md)
> - [Step 11 plan](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/implement-neo-step11-plan.md)
> - [Step B spec](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/spec.md) — 修复 stack register slot 分配遗留
> - [Step 10 handoff](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step10/handoff.md) — 前置基础
>
> **设计文档基线**：[object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) §4.3/§4.4（slot 布局）、§13.5.1/§13.6（VTable/接口分派）

---

## 1. Step 11 已落地内容

### 1.1 VTable slot 字典 key：`string` → `IMethod`
- [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) 中 `neoVTableSlots` 类型改为 `Dictionary<IMethod, int>`
- 删除 `neoVTableSlotKeys[]`、`GetNeoVTableSlotKey(MethodReference)`
- `SignatureString` 降级为诊断字符串，热路径与匹配路径完全不依赖

### 1.2 `BuildNeoVTable` 本类阶段改用 Legacy API
- 显式 override：走 `method.Definition.HasOverrides` + `appdomain.GetMethod(overrideRef, ...)`
- 隐式 override：走 `BaseType.GetVirtualMethod(method)`
- slot map 用 `IMethod` 引用相等作 key
- 已删除 `FindNeoOverrideSlot`（字符串版）

### 1.3 接口自身 NeoVTable
- 放开 `BuildNeoVTable` 中 `!IsInterface` 限制，接口从 slot 0 起构建
- 接口继承链：父接口 slot 前置到本接口 slot 表
- 产物：`iface.NeoVTable[i]` = 接口方法本身；`iface.neoVTableSlots[interfaceMethod] = i`
- 使用 `RegisterParentInterfaceOffsets` 处理父接口 offset 映射

### 1.4 实现类接口偏移表
新增字段（`#if ENABLE_NEO_MODE`）：
```csharp
Dictionary<IType, int> neoInterfaceOffsets;   // IL interface IType → 本类 NeoVTable 中起始 slot

internal bool TryGetNeoInterfaceOffset(IType interfaceType, out int baseSlot);
internal bool TryGetInterfaceMethodSlot(IMethod interfaceMethod, out int slot);
```

`BuildNeoVTable` 末尾"接口阶段"：
- 基类 `neoInterfaceOffsets` 复制作起点
- 本类直接声明的接口重新分配 slot 块，覆盖 `neoInterfaceOffsets[iface] = slots.Count`
- 每个接口方法通过 `this.GetVirtualMethod(ifaceMethod)`（Legacy API）解析实现方法
- 显式接口实现命名匹配依赖 `GetVirtualMethod` 内部的 `{FullNameForNested}.{Name}` 规则
- `ResolveNeoInterfaceImplementation` 存在但为过渡诊断用途，实际匹配走 Legacy

### 1.5 JIT lowering
- [JITCompiler.cs InitializeCallvirtDispatch L2107-L2145](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L2107-L2145) 增加接口分支：
  - `declaringILType.IsInterface` → `op.Code = OpCodeREnum.Callvirt; op.Operand4 = EncodeCallvirtDispatch(ifaceSlot, 0);`
  - 非接口路径不变（`Callvirt_IL` / 通用 `Callvirt`）
- **未新增 opcode**（收敛于现有 `Callvirt` case）

### 1.6 运行时 handler
- [ILIntepreter.Neo.cs ResolveNeoGenericCallvirtTarget](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L256-L299) 内部先判 `DeclearingType is ILType && IsInterface`：
  - `TryGetNeoInterfaceOffset(declaringIL, out int baseSlot)`
  - `methodSlot = ip->Operand4 & 0xffff`
  - `actualSlot = baseSlot + methodSlot`
  - `instance.Type.NeoVTable[actualSlot]` 得实际方法
- Callvirt_IL / Callvirt_CLR case 未改动

### 1.7 测试
[TestCases/NeoStep11Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep11Test.cs) 覆盖 5 组：
- `NeoStep11Basic` — IL 类实现 IL 接口，接口变量调用
- `NeoStep11MultipleInterfaces` — 同类实现两个接口，各自分派
- `NeoStep11InterfaceInheritance` — 接口继承链，父/子接口变量均可调
- `NeoStep11ExplicitImplementation` — 显式接口实现 `int IFoo.GetValue()`
- `NeoStep11InheritedInterfaceImpl` — 基类实现接口 + 派生类 override

---

## 2. 当前测试状态

**Neo 模式 `-c Debug_Neo`, `useRegister=true`**：

| 测试用例 | 单跑 | 全组 | 状态 |
|---|---|---|---|
| `NeoStep11Basic` | ✅ | ✅ | Pass |
| `NeoStep11InheritedInterfaceImpl` | ✅ | ✅ | Pass |
| `NeoStep11InterfaceInheritance` | ✅ | ❌ | **交叉污染失败** |
| `NeoStep11MultipleInterfaces` | ✅ | ❌ | **崩溃：ArgumentOutOfRangeException** |
| `NeoStep11ExplicitImplementation` | 未复核 | ❌ | Fail |

**交叉运行规律**：`NeoStep11MultipleInterfaces` 与 `NeoStep11InterfaceInheritance` **单跑均通过**，但连续运行时无论顺序，第二个必然失败。

---

## 3. 失败根因：Stack Register Slot 分配遗留 bug

### 3.1 症状定位（`NeoStep11MultipleInterfaces` 崩溃）

`ArgumentOutOfRangeException` 抛在 [ILIntepreter.Neo.cs L456 Move handler](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L456)：

```csharp
case OpCodeREnum.Move:
    Unsafe.CopyBlock(frameBase + ip->DstOffset, frameBase + ip->SrcOffset, (uint)ip->Operand2);
    if (ip->Operand == 1)  // 本应 = 0 却是 1
    {
        srcIdx = *(int*)(frameBase + ip->SrcOffset);
        // srcIdx = 100（int 值 100 被当成 mStack index）
        mStack[dstIdx] = mStack[srcIdx];  // ← mStack.Count = 11，OOR 崩溃
```

原始 JIT 是 `move r5, r1`，lowering 后 `move r4, r20`。观察到：
- `method.CompiledFrame.LocalInfos.Length == 11`，而指令引用 `r20`
- `r1` 在 `BuildInitialRegisterTypes` 被填为 `INeoStep11Basic`（`asBasic` local 类型）
- `callvirt r1, ...GetValue()` 复用 r1 存 int 返回值，**但没有 `SetRegisterType(r1, IntType)`**
- 后续 `move r5, r1` lowering 时 `registerTypes[1]` 仍是 `INeoStep11Basic` → `IsNeoReferenceSlot = true` → `op.Operand = 1`
- 运行时 primitive slot 里存的是 int 100（不是 mStack index），refMove 分支读 `srcIdx = 100` → OOR

### 3.2 根因（比表面 bug 更深）

调查 spec 记录发现，[JITCompiler.cs AllocateLocalStackSpaces L1181-L1205](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1181-L1205) 对 **stack register**（编译器生成的 tmp register）使用了全局 upper-bound 分配：

```csharp
int maxSize = 8, maxRefCount = 1;
foreach (var i in valueTypes) { ... }  // 全局值类型 upper-bound
for (int i = 0; i < frame.StackRegisterCount; i++)
{
    slot.Size = maxSize;
    slot.RefCount = maxRefCount;
    ...
}
```

这**违背** [object-model-neo-design.md §4.3/§4.4](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md#L237-L293) 的明确规定：
> **在 Neo 中不存在"栈寄存器"与"局部变量"的区分。** 所有变量按实际类型大小占用空间。

**遗漏路径**（前置调查结论）：
| Commit | 阶段 | 责任 |
|---|---|---|
| `91cc26ab` (2025-12-16) | 老 Neo 尝试 | 首次引入 upper-bound 分配 |
| `9adfab15` (Step 3-4) | **主责** | params/locals 改为按类型分配，**漏改 stack register**；spec/tasks 未强制到 stack register 粒度 |
| `2247eff7 / ae47fbc4` (Step 6) | 次责 | 用 Move lowering `min(src, dst)` 打补丁绕过 CopyBlock 越界，未追根 |
| `f6b4ef14` (Step 7-8) | 顺延固化 | 新增 `RefCount` 字段，stack register 仍写 `maxRefCount` |
| Step 9 / 10 / 11 | 未涉及 | 但 Step 10 handoff 已默认此偏离 |

结论：**用户从未做过"临时用 upper-bound"的决策**，属于 Step 3-4 spec 未 mapping 到设计文档 §4.3/§4.4 的执行遗漏。

### 3.3 二次症状：全组交叉污染
`NeoStep11InterfaceInheritance` 单跑通过、全组失败的根源同样在 stack register slot 复用：
- 前一个测试跑完后，某些方法的编译产物残留了错误的 refMove 判定
- 下一个测试用例进入相同的 tmp slot 时 layout 与实际承载类型不匹配

具体是否是 Ldfld_Ref 里 `this` 指针的 mStack index 偏移问题，或是 vtable 加载顺序污染，**Step B 修复后需要复测确认**。当前假设：Step B 消除 slot 复用类型污染后，交叉污染大概率自动消除；若仍有残留，则另立诊断。

---

## 4. Step B 已就位（未实施）

已创建 [.trae/specs/implement-neo-stepB/](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB)：

- [spec.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/spec.md) — 完整设计
- [tasks.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/tasks.md) — 9 项任务分解
- [checklist.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/checklist.md) — 验收清单

**核心修复**：

1. **B-1** [AllocateLocalStackSpaces L1181-L1205](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1181-L1205) 的 stack register 循环改为按 `registerTypes[baseRegStart + i]` 逐 slot 独立分配（走与 locals 完全同构的 `AllocateSlotForType`）
2. **B-2** [TypeSpecializeNeoOpcodes L480-L648](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L480-L648) 补齐 Call/Callvirt* / Newobj / Ldelem* / Box / Unbox* / Ldsfld* / Ldtoken / Ldftn / Isinst / Castclass 的 SetRegisterType
3. **B-3** Move refMove 判定下沉：
   - JIT 侧不再写 `op.Operand`（删除 `IsNeoReferenceSlot(srcType)` 判定）
   - [Optimizer.Neo.cs Move case L94-L118](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L94-L118) 改为 `isRefMove = LocalInfos[srcReg].RefCount > 0`
   - `op.Operand2 = LocalInfos[srcReg].Size`
4. **B-5** 顺带移除 Step 6 Move lowering 的 `min(srcSz, dstSz)` workaround；DEBUG 断言 src/dst layout 一致

**依赖前提**（已满足）：`TypeSpecializeNeoOpcodes` 在 `AllocateLocalStackSpaces` 之前调用，见 [JITCompiler.cs L459-466](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L459-L466)。

**实施顺序**：Task 1 → Task 2 → Task 3 → 编译 → 测试。

---

## 5. Step B 完成后的验证矩阵

### 5.1 Neo 模式（`-c Debug_Neo`, `useRegister=true`）
- [ ] `NeoStep6Test` 全绿（Move workaround 移除后原地验证）
- [ ] `NeoStep7Test / NeoStep8Test / NeoStep9Test` 全绿
- [ ] `NeoStep10Test` 全绿
- [ ] `NeoStep11Test` 全绿（含**单跑 + 全组** — 全组交叉污染必须消除）

### 5.2 Legacy 模式（`-c Debug`, `useRegister=false`）
- [ ] 全套测试用例通过

### 5.3 Legacy Register VM（`-c Debug`, `useRegister=true`）
- [ ] 全套测试用例通过

### 5.4 帧元数据抽查
Step B 后 `TotalRefSize` / `TotalStructSize` 会缩小，抽样验证：
- 手工挑 3 个方法（int-only、含 object、含 Vector3），比对 Step B 前后：Step B 后应严格 ≤ 之前

---

## 6. 已知残留（Step B 之后仍待处理）

### 6.1 `Operand4` 语义细化风险（Step 11 plan §关键坑点 #2）
- 普通 `Callvirt` 的 `Operand4 == 1` 目前是 constrained flag
- 接口分派时低 16 = 接口内 slot 若恰好 = 1，会撞车
- 当前实施：`Optimizer.Neo.cs hasConstrained` 判断的执行时机在 `InitializeCallvirtDispatch` 之后 —— 尚未在实施中撞到此坑，但 Step 12/13 引入更多值类型场景时需重新核查
- **备选修法**：接口 slot 编码移到 `Operand4` 高 16 位（当前 high16 未使用）

### 6.2 `constrained. callvirt` 与普通 `callvirt` 的 `Operand4` 语义冲突
- Step 11 已通过运行时判断 `DeclearingType.IsInterface` 分流
- 但 Step 12/13 引入值类型完整支持时，`constrained.` 调用需要分配独立 opcode（`Callvirt_Constrained`）以正确处理值类型无装箱调用、managed pointer 与 `this` 引用槽
- 详见 [context 记忆 2026-07-15 15:50 topic](context://memory/projects/-f-SVN-ILRuntime/20260715/topics.md)

### 6.3 CLR 接口 / 值类型接口 / 泛型接口方法
- 全部留到 Step 12/13
- Step 11 spec 明确不覆盖

### 6.4 `Ldvirtftn` / 委托接口方法
- 留到 Step 19

---

## 7. Step B 之后 Step 11 收尾清单

- [ ] Step 11 全套测试单跑 + 全组全绿
- [ ] 更新 [object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) §13.5.1 / §13.6：
  - "重写签名匹配" TODO 替换为 "运行时构建期复用 Legacy `GetMethod` / `GetVirtualMethod`"
  - 补 `neoInterfaceOffsets` 字段名与"接口分派收敛到 `Callvirt` 现有 case"说明
- [ ] 更新 [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) Step 11 段：
  - CLR 接口留 Step 13
  - 接口分派合入 `Callvirt` 现有 case
- [ ] 更新 [implement-neo-step6/checklist.md L79](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step6/checklist.md#L79)：追加"Move lowering `min(src, dst)` workaround 已被 Step B 根除"标注
- [ ] 更新 [implement-neo-step3-step4](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step3-step4)：追加"stack register 段按类型分配的遗漏由 Step B 补账"备注
- [ ] 撰写 [Step B handoff](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/handoff.md)
- [ ] 补 `project_memory.md` lesson：
  - **Neo 模式的运行时元数据构建期应最大化复用 Legacy 已调稳的 `GetMethod / GetVirtualMethod / CanAssignTo`，只在热路径引入 Neo 特化**
  - **接口分派收敛到现有 `Callvirt` case 而非新增 opcode，性能等价但可维护性显著提升**
  - **Neo slot 分配严格按 §4.3/§4.4，禁止对 stack register 使用 upper-bound 共享槽**

---

## 8. 关键文件与代码位置索引

| 文件 | 关注区段 | 用途 |
|---|---|---|
| [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) | L1070-L1225 `AllocateLocalStackSpaces` | Step B 核心修改点 |
| [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) | L480-L648 `TypeSpecializeNeoOpcodes` | Step B 补齐 SetRegisterType |
| [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) | L1454-L1552, L2107-L2145 | Callvirt lowering / InitializeCallvirtDispatch |
| [Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs) | L94-L118 Move case | Step B 移除 workaround |
| [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) | L256-L299 `ResolveNeoGenericCallvirtTarget` | 接口分派运行时入口 |
| [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) | L447-L465 Move runtime | Step 11 崩溃点 |
| [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) | `BuildNeoVTable / neoInterfaceOffsets / TryGet*` | Step 11 类型元数据 |
| [TestCases/NeoStep11Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep11Test.cs) | 全文件 | 测试用例 |

---

## 9. 运行命令速查

### 构建
```powershell
dotnet build --framework net8.0 -c Release HotfixAOT/
dotnet build --framework net8.0 -c Release_Patched HotfixAOT/
dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -i -h HotfixAOT/Patched/HotfixAOT.hash -o HotfixAOT/Patched/HotfixAOT.dll HotfixAOT/bin/Release/net8.0/HotfixAOT.dll
dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -p -o HotfixAOT/Patched/HotfixAOT.patch HotfixAOT/Patched/HotfixAOT.hash HotfixAOT/bin/Release_Patched/net8.0/HotfixAOT.dll
dotnet build TestCases/
dotnet build "ILRuntime\ILRuntime.csproj" -c Debug_Neo
```

### 测试（Neo 模式，`useRegister=true`）
```powershell
# 单跑（含 nameFilter）
dotnet run -c Debug_Neo --verbosity normal --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj TestCases/bin/Debug/netstandard2.1/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch true NeoStep11MultipleInterfaces

# 全组
dotnet run -c Debug_Neo --verbosity normal --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj TestCases/bin/Debug/netstandard2.1/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch true NeoStep11Test
```

### 死循环处理
Neo 单个测试应在 5-20ms 完成。超过 10 秒几乎肯定卡在解释器死循环：
```powershell
Stop-Process -Name ILRuntimeTestCLI -Force
```

### 日志查看
- 严禁 `>` 重定向（Windows bug 会覆盖）
- 严禁 pipeline `| Select-String`
- Agent harness 自动把 stdout/stderr 保存到 `X:\Users\XXXXX\AppData\Local\Temp\trae-agent-toolhost\jobs\job-xxx\output.log`
- 用 `Read` 或 `Grep` 工具分析该 log

---

## 10. 交接总结

Step 11 **代码实现完整**，架构方向正确（VTable slot key 用 `IMethod` 引用相等 + Legacy `GetVirtualMethod` 复用 + 收敛到现有 `Callvirt` case）。

阻塞测试通过的**唯一未修复项**：Step 3-4 遗留的 stack register slot 分配错误（upper-bound 共享槽），后续演化到 Step 11 引入接口方法返回 int 复用 tmp register 时首次爆发。

**下一步行动**：按 [Step B tasks.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-stepB/tasks.md) 顺序（Task 1 → Task 2 → Task 3 → 编译 → 测试）实施 Step B，完成后回归 Step 6 / 7 / 8 / 9 / 10 / 11 全套测试，然后按本 handoff §7 收尾 Step 11。
