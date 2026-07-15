# Neo Step 11 接口方法分派 — 实现计划

## Context（为什么做 / 目标）

Step 10 已完成 Neo 模式下类虚方法的 VTable 构建与 `Callvirt_IL / Callvirt_CLR / Callvirt` 三个变体的 O(1) 分派，但对**接口方法** `callvirt` 支持有洞：

- [ILType.BuildNeoVTable](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L410-L479) L425 显式跳过 `IsInterface`，接口没有 NeoVTable，也没有"接口方法 → 类 VTable slot"映射。
- [JITCompiler.InitializeCallvirtDispatch](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L2107-L2132) 对 IL 接口方法直接落到通用 `Callvirt`（slot = -1）。
- 运行时进入 [ResolveNeoGenericCallvirtTarget](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L256-L265)，转发给 `ResolveNeoCallvirtILTarget`，用**接口方法**去 `instance.Type.TryGetNeoVTableSlot(...)` 会 miss，抛 `MissingMethodException`。

Step 11 目标：为每个 `ILType` 建立"接口类型 → 类 NeoVTable 内起始 slot"映射，并为接口自身建立独立 slot 编号；**在现有 `Callvirt` case 内**做 O(1) 接口分派，不新增 opcode。

**范围（用户确认）**：
- 只覆盖 IL 类实现 **IL 接口**（含多接口、接口继承链、显式接口实现）。
- IL 类实现 **CLR 接口** 留到 Step 13。
- **避免与 Legacy 复刻两套基础设施**：VTable 构建期直接调 Legacy `GetMethod / GetVirtualMethod`。
- **如无必要勿增实体**：不新增 opcode / case / helper，把接口分派收敛进现有 `Callvirt` 通用路径。

---

## 两个核心决策

### 决策一：不新增 opcode，把接口分派合入现有 `Callvirt` case

**理由**：新 opcode 相比现有 `Callvirt` 通用 case 唯一节省的是 1 次 `is ILTypeInstance` 类型检查；同一 call site 每次分派目标恒定，CPU 分支预测器无差别。收益微弱、维护面显著（enum 数值对齐、Optimizer/Call_Redirect 白名单同步、多一处 case handler + helper）。

**实现**：在 `Callvirt` 运行时 case 的现有 sub-branch 上，先判断 `declaredMethod.DeclearingType is ILType iface && iface.IsInterface`，走 offset+slot 分派；否则回退现有类虚方法路径。JIT lowering 期把"接口内 slot"预编码进 `Operand4` 低 16 位（`Callvirt_IL` 目前也用同一编码位）。

### 决策二：签名匹配 & 接口方法解析全部复用 Legacy

Legacy 里两块已经调稳的 API 直接可用：

| Legacy API | 位置 | 能力 |
|---|---|---|
| `ILType.GetMethod(name, params, genericArgs, returnType, declaredOnly)` | [L1217-L1274](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1217-L1274) | 结构化签名匹配：Name + 参数 `IType` 引用相等 + 泛型参数/实参 + 返回类型 |
| `ILType.GetVirtualMethod(IMethod)` | [L1166-L1207](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1166-L1207) | 虚方法/接口方法解析：`GetMethod(exactMatch:true)` → `BaseType.GetVirtualMethod` 递归 → `{FullNameForNested}.{Name}` 显式实现 fallback |

Neo 侧**运行时热路径**保持零查询（slot 索引）；**VTable 构建期**（一次性、非热路径）直接调 Legacy API，不再自造 `MethodSignatureKey` 或 `FindInterfaceImplementation`。

`SignatureString` 保留为诊断字符串，热路径与匹配路径完全不依赖。

---

## 具体设计

### 1. VTable slot 字典 key 改为 `IMethod` 引用

**当前**（Step 10）：
```csharp
Dictionary<string, int> neoVTableSlots;   // SignatureString → slot
string[] neoVTableSlotKeys;
```

**改成**：
```csharp
Dictionary<IMethod, int> neoVTableSlots;  // IMethod 引用相等 → slot
// 不再需要 slotKeys 数组
```

同一 AppDomain 内 `IMethod` 实例稳定（`methods` 字典缓存），引用相等即身份相等。

### 2. VTable 构建重写 [BuildNeoVTable](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L410-L479)

**基类阶段**：不变（拷贝基类 slot 数组 + slot map；CLR 基类走 `AddNeoBaseVirtualSlots`）。

**本类阶段**：改为 Legacy 驱动
```csharp
foreach (ILMethod method in 本类 methods 去重遍历)
{
    if (!IsNeoVTableCandidate(method)) continue;

    int slot = -1;
    // ① 显式基类 override（HasOverrides）
    if (method.Definition.HasOverrides)
    {
        foreach (var overrideRef in method.Definition.Overrides)
        {
            IMethod overrideMethod = appdomain.GetMethod(overrideRef, this, method, out _);
            if (overrideMethod != null && slotMap.TryGetValue(overrideMethod, out slot))
                break;
        }
    }
    // ② 隐式 override（!IsNewSlot）：借用 Legacy BaseType.GetVirtualMethod
    if (slot < 0 && !method.Definition.IsNewSlot && BaseType != null)
    {
        IMethod baseMethod = BaseType.GetVirtualMethod(method);
        if (baseMethod != null && baseMethod != method && slotMap.TryGetValue(baseMethod, out slot))
            { /* ok */ }
        else slot = -1;
    }

    if (slot >= 0)
    {
        // 用本类方法覆盖 slot；从 map 里移除基类 key，写入本类 key
        IMethod prev = slots[slot];
        slots[slot] = method;
        if (prev != null && slotMap.TryGetValue(prev, out int prevSlot) && prevSlot == slot)
            slotMap.Remove(prev);
        slotMap[method] = slot;
    }
    else
    {
        AddNeoVTableSlot(method, slots, slotMap);   // 新 slot
    }
}
```

**移除**：`FindNeoOverrideSlot`（字符串版）、`GetNeoVTableSlotKey(MethodReference)`。

### 3. 接口自身 NeoVTable

放开 L425 的 `!IsInterface`：
- 接口无基类概念，slots 从 0 开始。
- 若接口继承其他 IL 接口：递归取父接口 `neoVTable`，把父接口的 IMethod 逐个前置到本接口 slot 表（key = 父接口 IMethod）。
- 本接口自身方法：`foreach method in methods.Values`，`IsNeoVTableCandidate` 通过就追加 slot。
- 值类型仍 skip。

产物：`iface.NeoVTable[i]` = 接口方法本身；`iface.neoVTableSlots[interfaceMethod] = i`。

### 4. 实现类的接口偏移表

新增字段（`#if ENABLE_NEO_MODE`）：

```csharp
Dictionary<IType, int> neoInterfaceOffsets;   // IL interface IType → 本类 NeoVTable 中起始 slot

internal bool TryGetNeoInterfaceOffset(IType interfaceType, out int baseSlot);
internal bool TryGetInterfaceMethodSlot(IMethod interfaceMethod, out int slot);  // 供 JIT 查
```

`BuildNeoVTable` 主流程末尾追加"接口阶段"：
1. `BaseType is ILType`: 复制 `baseILType.neoInterfaceOffsets` 作为起点。
2. 遍历本类 `Implements`（不含基类继承），对每个 IL 接口 `iface`：
   - **无论继承表里有没有**都为本类直接声明的接口重新分配 slot 块，覆盖 `neoInterfaceOffsets[iface] = slots.Count`（保证接口块指向本类最新 impl）。
3. 对每个待填充的 `iface`：
   - `foreach ifaceMethod in iface.NeoVTable`:
     - `IMethod impl = this.GetVirtualMethod(ifaceMethod);` （**Legacy API**）
     - 若 `impl == null` 或 `impl == ifaceMethod` 或 `impl.DeclearingType.IsInterface`：抛 `TypeLoadException`。
     - `slots.Add(impl);`（不入 slotMap；接口块只服务运行时 offset+slot 分派）。

Legacy `GetVirtualMethod` 已经涵盖隐式实现、显式实现（`{FullNameForNested}.{Name}` 命名）、基类继承的实现、CLR 接口的 `realName + "." + Name` 命名，无需在 Neo 侧重造。

### 5. JIT lowering

修改 [InitializeCallvirtDispatch](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L2107-L2132)：

```csharp
if (targetMethod is ILMethod ilMethod)
{
    ILType declaringILType = ilMethod.DeclearingType as ILType;
    if (declaringILType != null && declaringILType.IsInterface)
    {
        // 接口方法：走通用 Callvirt case，但把接口内 slot 预编码进 Operand4 低 16
        int ifaceSlot;
        if (!declaringILType.TryGetInterfaceMethodSlot(ilMethod, out ifaceSlot))
            ifaceSlot = -1;
        op.Code = OpCodeREnum.Callvirt;
        op.Operand4 = EncodeCallvirtDispatch(ifaceSlot, 0);
        return;
    }
    if (declaringILType != null && !declaringILType.IsInterface && declaringILType.TryGetNeoVTableSlot(ilMethod, out int slot))
    {
        op.Code = OpCodeREnum.Callvirt_IL;
        op.Operand4 = EncodeCallvirtDispatch(slot, 0);
    }
    else
    {
        op.Code = OpCodeREnum.Callvirt;
        op.Operand4 = EncodeCallvirtDispatch(-1, 0);
    }
}
else if (targetMethod is CLRMethod clrMethod)
{
    // 原 Step 10 逻辑
    ...
}
```

**注意**：接口方法 → `Callvirt`（复用现有 opcode），仅 `Operand4` 低 16 位携带的语义变化（-1 → 接口内 slot），运行时 case 内区分。

### 6. 运行时 handler：扩展现有 `Callvirt` case

修改 [ResolveNeoGenericCallvirtTarget](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L256-L265)：

```csharp
static IMethod ResolveNeoGenericCallvirtTarget(OpCodeR* ip, IMethod declaredMethod, byte* targetBase, AutoList mStack)
{
    object thisObj = ReadNeoCallThis(ip, targetBase, mStack);

    if (thisObj is ILTypeInstance instance)
    {
        // 接口方法快路径：Operand4 低 16 = 接口内 slot；DeclearingType 提供接口 identity
        if (declaredMethod.DeclearingType is ILType declaringIL && declaringIL.IsInterface)
        {
            if (!instance.Type.TryGetNeoInterfaceOffset(declaringIL, out int baseSlot))
                throw new MissingMethodException(
                    string.Format("Type {0} does not implement interface {1}.",
                        instance.Type.FullName, declaringIL.FullName));

            int methodSlot = ip->Operand4 & 0xffff;
            if (methodSlot == 0xffff)
            {
                // 兜底：lowering 期没查到 slot（不应发生）
                if (!declaringIL.TryGetInterfaceMethodSlot(declaredMethod, out methodSlot))
                    throw new MissingMethodException(...);
            }
            int actualSlot = baseSlot + methodSlot;
            var vtable = instance.Type.NeoVTable;
            if (actualSlot < 0 || actualSlot >= vtable.Length)
                throw new MissingMethodException(...);
            IMethod actual = vtable[actualSlot];
            if (actual == null)
                throw new MissingMethodException(...);
            return actual;
        }

        // 非接口的 IL 虚方法：原路径不变
        return ResolveNeoCallvirtILTarget(ip, declaredMethod, targetBase, mStack);
    }

    if (declaredMethod is CLRMethod)
        return declaredMethod;

    throw new InvalidOperationException(
        string.Format("Neo generic callvirt cannot dispatch non-IL object {0} to {1}.",
            thisObj.GetType().FullName, declaredMethod));
}
```

**其他 case（Callvirt_IL / Callvirt_CLR）不动**；enum 不加新项；Optimizer 与 Call_Redirect 白名单不动。

### 7. 测试

新增 [TestCases/NeoStep11Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep11Test.cs)，仿 [NeoStep10Test](file:///f:/SVN/ILRuntime/TestCases/NeoStep10Test.cs)（`AssertEqual` + `Console.WriteLine` + `throw`，避免 `newarr`）。

| 用例 | 覆盖 |
|---|---|
| `NeoStep11InterfaceBasic` | IL 类实现 IL 接口，接口变量调用 |
| `NeoStep11MultipleInterfaces` | 同类实现两个接口，各自分派互不污染 |
| `NeoStep11InterfaceInheritance` | 接口继承链，父/子接口变量均可调 |
| `NeoStep11ExplicitImplementation` | 显式接口实现 `int IFoo.GetValue()` |
| `NeoStep11InheritedInterfaceImpl` | 基类实现接口 + 派生类 override，接口变量拿 override |

**不覆盖**（留后续）：CLR 接口（Step 13）、值类型接口（Step 12/13）、泛型接口方法。

---

## 修改文件一览

| 文件 | 改动 |
|---|---|
| [ILRuntime/CLR/TypeSystem/ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) | ①`neoVTableSlots` 改 `Dictionary<IMethod,int>`；删除 `neoVTableSlotKeys` / `GetNeoVTableSlotKey`；②`BuildNeoVTable` 本类阶段改用 `HasOverrides` + `BaseType.GetVirtualMethod` 定位基类 slot；③放开 `IsInterface` 分支让接口建 slot；④新增 `neoInterfaceOffsets` + `TryGetNeoInterfaceOffset` + `TryGetInterfaceMethodSlot`；⑤新增"接口阶段"用 `this.GetVirtualMethod(ifaceMethod)` 填 slot |
| [ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) | `InitializeCallvirtDispatch` 加接口分支（emit `Callvirt` + `Operand4` 编接口内 slot） |
| [ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) | `ResolveNeoGenericCallvirtTarget` 内部先判 `DeclearingType is ILType && IsInterface`，走 offset+slot 分派 |
| [TestCases/NeoStep11Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep11Test.cs) | 新增：5 组测试用例 |
| [.trae/documents/object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) §13.5.1/§13.6 | 更新 TODO 与接口分派实际字段名；明确"运行时构建期复用 Legacy `GetVirtualMethod`" |
| [.trae/documents/neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) Step 11 段 | 更新：CLR 接口留 Step 13；接口分派合入 `Callvirt` 现有 case |
| [.trae/specs/implement-neo-step11/tasks.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/tasks.md) & [checklist.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/checklist.md) | 新增：任务分解与验证清单 |
| [.trae/specs/implement-neo-step11/handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/handoff.md) | 完成后写 handoff（对齐 Step 10 结构） |

**不改动**：`OpCodeREnum.cs`、`Optimizer.Neo.cs`、`IMethod.cs`、`ILMethod.cs`、`CLRMethod.cs`。

---

## 与初版计划的差异（简洁性收敛）

| 事项 | 初版 | 现版 |
|---|---|---|
| 新增 opcode | 1 (`Callvirt_Interface_IL`) | 0 |
| 新增解释器 case | 1 | 0（扩展现有） |
| 新增 helper | 1 (`ResolveNeoCallvirtInterfaceILTarget`) | 0（扩展 `ResolveNeoGenericCallvirtTarget`） |
| Optimizer 排除列表 | 需同步 | 不动 |
| Call_Redirect 白名单 | 需同步 | 不动 |
| 签名匹配 | 自造 `MethodSignatureKey` | 复用 Legacy `GetMethod` |
| 接口→实现解析 | 自造 `FindInterfaceImplementation` | 复用 Legacy `GetVirtualMethod` |
| VTable slot key | `MethodSignatureKey` | `IMethod` 引用 |

---

## 关键坑点

1. **`Callvirt` 的 `Operand4` 语义细化**：低 16 现在承担双语义 —— 非接口方法时是 -1（保留原 fallback）、接口方法时是"接口内 slot"。区分依据是运行时 `declaredMethod.DeclearingType.IsInterface`（非 `Operand4` 本身）。
2. **`Operand4` 与 constrained marker**：普通 `Callvirt` 的 `Operand4 == 1` 目前是 constrained flag（handoff §2.5 / §3-7）。接口分派时低 16 = 接口内 slot 若恰好 = 1，会撞车。**风险评估**：一个接口至少要有其他 slot 才会让 slot=1 存在；已经存在的 `Optimizer.Neo.cs` `hasConstrained = op.Code == OpCodeREnum.Callvirt && op.Operand4 == 1` 逻辑（[L388-L390](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs#L388-L390)）在 lowering 阶段执行——lowering 之前 `Callvirt` 的 `Operand4` 语义还没被 `InitializeCallvirtDispatch` 改写（`InitializeCallvirtDispatch` 在 [JITCompiler.cs L1482-L1487](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1482-L1487) 里，紧邻 `Callvirt` 编译点；`Optimizer.Neo` 参数区处理在其后）。
   - **必须在实现时确认顺序**：如果 `Optimizer.Neo` 的 `hasConstrained` 判断发生在 `InitializeCallvirtDispatch` 之后，就要把接口 case 的 `Operand4` 编码位区分开（例如接口 slot 加一个高位标记 bit，或在实现类 offset 表里预先偏移使 slot 永不为 1）。
   - **实施动作**：写 code 前先追一遍 lowering pipeline 顺序，若确有冲突，最简修法是把接口 slot 存到 `Operand4` 高 16 位（当前 high16 是 thisArgOffset，恒为 0，未使用），低 16 保持 -1 供 `hasConstrained` 兼容。**这是唯一需要在实现时定夺的技术细节**。
3. **`GetVirtualMethod` 返回原方法**：Legacy [L1202-L1205](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1202-L1205) 在泛型 identity 不匹配时返回入参本身。接口阶段填 slot 时若 `impl == ifaceMethod` 视为未实现，抛 `TypeLoadException` 提前暴露。
4. **接口自身 `BuildNeoVTable`**：`neoVTableBuilding` guard 是实例级，交叉调用（类构建期访问 `iface.NeoVTable`）安全。
5. **CLR 接口不参与**：`InitializeCallvirtDispatch` 里 `declaringType is CLRType && IsInterface` 走原路径（`MayCallvirtTargetILObject == true` → `Callvirt`）。
6. **`Ldvirtftn`**：Step 19 委托再动。

---

## 验证方案

### 构建
```powershell
dotnet build "ILRuntime\ILRuntime.csproj" -c Debug_Neo
dotnet build TestCases/
```

### 测试
```powershell
# Step 11 新增
dotnet run -c Debug_Neo --verbosity normal --framework net8.0 `
  --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj `
  TestCases/bin/Debug/netstandard2.1/TestCases.dll `
  HotfixAOT/Patched/HotfixAOT.patch true NeoStep11Test

# 回归
dotnet run ... true NeoStep10Test    # 期望 5/5
dotnet run ... true NeoStep7Step8Test # 期望 7/7
dotnet run ... true NeoStep6Test      # 期望 14/14
```

### 成功标准

| 项 | 期望 |
|---|---|
| Debug_Neo 编译 | 0 error / 无新增 warning |
| NeoStep11Test | 5 passed / 0 failed |
| NeoStep10Test | 5 passed / 0 failed（回归） |
| NeoStep7Step8Test | 7 passed / 0 failed（回归） |
| NeoStep6Test | 14 passed / 0 failed（回归） |
| 单个 Neo 测试耗时 | < 20ms |

### 边界排查
- `ExplicitImplementation` 失败：确认接口阶段调 `this.GetVirtualMethod(ifaceMethod)` 命中显式实现命名（`{FullNameForNested}.{Name}` 分支）。
- `InheritedInterfaceImpl` 失败：接口块 slot 是否指向本类 override（应指向；因为接口阶段对本类直接声明的接口重新分配 slot 块并用 `this.GetVirtualMethod` 拉取最新 override）。
- Constrained call 用例（Step 10 回归）失败：立刻核对上文坑点 #2（`Operand4` 低 16 撞 marker），若发生就把接口 slot 移到高 16 位。

### 完成后动作
- 写 [.trae/specs/implement-neo-step11/handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step11/handoff.md)，对齐 Step 10 handoff 结构。
- 更新 [object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) §13.5.1（替换"重写签名匹配"TODO 为"运行时构建期复用 Legacy `GetMethod`/`GetVirtualMethod`"）；§13.6 补 `neoInterfaceOffsets` 字段名 + 接口分派现在走 `Callvirt` 现有 case 的说明。
- [project_memory.md](context://memory/projects/-f-SVN-ILRuntime/project_memory.md) 补 lesson：**Neo 模式的运行时元数据构建期应最大化复用 Legacy 已调稳的 `GetMethod / GetVirtualMethod / CanAssignTo`，只在热路径引入 Neo 特化**；**接口分派收敛到现有 `Callvirt` case 而非新增 opcode，性能等价但可维护性显著提升**。
