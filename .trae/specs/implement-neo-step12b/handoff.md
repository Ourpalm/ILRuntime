# ILRuntime Neo Step 12b — Handoff

## 交付概述
Step 12b 目标是补齐 Ref Slot 8 字节 `(objectIndex, offset)` 基础机制、修复 Step 12 遗留的 ECMA-335 违规(A1-A5),使 [NeoStep12Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep12Test.cs) 10 个用例全绿且 NeoStep6/7/8/10/11 共 31 个用例零回归。本 step 已达成:

- **Neo 测试:41/41 通过**(NeoStep6 + NeoStep7Step8 + NeoStep10 + NeoStep11 + NeoStep12 全部)
- **Legacy 测试:493/493 通过**,useRegister=false 无回归
- **`-c Debug` / `-c Debug_Neo` 双配置 0 错误**
- **ECMA-335 A1-A5 全部闭环**,仅将两处非 Step 12b 范围的语义偏差挂账下游 step(见 §7)

---

## 1. Ref Slot 基础机制(A3 闭环)

**编码定义**(design §15.2,不变):8 字节 `(int objectIndex, int offset)`,`objectIndex == -1` 表示 FRAME_REF(帧内绝对偏移),`>= 0` 表示 mStack index,`< -1` 目前用于 `-1 - structRefOffset` 的紧凑编码(见 §3)。

**Slot 分配**:
- [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) `AllocateSlotForType` byref 分支 `Size=8, RefCount=0`,`RefOffset` 语义**重定义**为 referent struct RefOffset(见 §4)
- ExecuteNeo 入口缓存 `byte* stackBase = stack.StackBase`(runtime stack 绝对基址)
- 共享变量区新增 `int objIndex; byte* refTarget;` 等,严守 iOS 栈安全约束

**生产指令**([ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs)):
- `Ldloca / Ldloca_S / Ldarga / Ldarga_S` → `(-1, (frameBase - stackBase) + slotOffset)`
- `Ldflda`(堆 IL 对象) → `(objIndex, fieldPrimitiveOffset)`
- `Ldflda`(帧内 struct / Ref Slot receiver) → `(-1, baseAbs + fieldPrimitiveOffset)`
- `Ldsflda` → 触发 `type.StaticInstance`(cctor),产出 `(staticMStackIndex, fieldPrimitiveOffset)`

**消费指令**:`Stind_I/I1/I2/I4/I8/R4/R8/Ref` + `Ldind_I/I1/U1/I2/U2/I4/U4/I8/R4/R8/Ref` 全部按 objectIndex 三分派实现,`Ldind_U1/I1/U2/I2` 做零/符号扩展到 4 字节 dst。Array 分支显式 `NotImplementedException("Step 16")` 占位。

**GC 安全**:堆内存(`ILTypeInstance.Primitives`)一律用 `ref byte`(`ResolveNeoIndirectILTarget` / `ResolveNeoILTarget`),不再使用 `Unsafe.AsPointer`;非托管帧内存(`stackBase + offset`)才用 `byte*`。

**Helper 拆分**:`ResolveNeoIndirectPrimitiveTarget` 拆为 `ResolveNeoIndirectFrameTarget` / `ResolveNeoIndirectILTarget` / `ResolveNeoIndirectClrOwner` 三个 AggressiveInlining 极简 helper,按 receiver 类型分派;边界 throw 全部包在 `#if DEBUG` 内(Release 无分支开销)。

---

## 2. Struct-this ABI 从 by-value 改为 by-ref(A1 闭环)

- [JITCompiler.cs L460-L469](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L460-L469):struct this 参数 slot 从整块 struct 改为 8 字节 Ref Slot
- Caller emit `Ldloca_S t` 得到 `(-1, absoluteFrameOffset)`,写入 callee 参数区
- `CopyNeoCallArguments` / `NeoCallParamMap` 对 struct-this 只拷贝 8 字节 Ref Slot,不再整块复制
- Callee `ldarg.0` 加载 Ref Slot,`this.field` 走 §3 的 receiver 三态分派
- 移除 by-value copyback

**验证**:`NeoStep12ThisTarget::Bump` 等结构体实例方法修改字段可正确写回 caller 原始 struct。

---

## 3. Ldfld_* / Stfld_* Receiver 三态编码(A2 闭环)

**编码方案 A**(design §2.5 + §15,拒绝改 8 字节 Ref Slot 布局):

- `Operand4 == 0` → **heap**,mStack index 读自 receiver primitive slot
- `Operand4 > 0` → **inline direct**(同帧 struct),值 = struct.RefOffset + 1
- `Operand4 < 0` → **Ref-Slot receiver**,值 = `-1 - structRefOffset`(structRefOffset 来自 receiver byref slot 自身的 RefOffset 字段,见 §4)

Handler 侧塌缩为一行 `ResolveNeoFieldReceiver(...)` + 三态分派,不做类型判断。CLR 字段访问一律走 §5 的 CLRType 单入口。

---

## 4. Byref Slot RefOffset 语义重定义 + PropagateByRefReferentOffsets(Task 9 替代 fold)

**问题**:同帧带引用字段的 struct 访问(如 `NeoStep12StructWithRef`)如果不 fold,`Stfld_Ref` 的 receiver 是 Ref Slot,handler 只拿到 8 字节 `(objIndex=-1, byteOffset)`,缺少定位帧内引用区(mStack ref 段)所需的 struct.RefOffset。

**决策**:拒绝让 fold 成为正确性依赖(用户明确要求"不 fold 也要正确")。方案 A(拓展 Ref Slot 编码字段)也被否决(会挤压 objectIndex/refOffset 空间上限)。最终采用:

- **byref slot `StackSlotInfo.RefOffset` 语义重定义为 "referent struct 的 RefOffset"**(byref slot 本身 RefCount=0,`RefOffset` 字段不参与引用区计数,重定义自洽)
- 在 [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) `AllocateLocalStackSpaces` 之后新增 `PropagateByRefReferentOffsets` pass:
  - `Ldloca / Ldloca_S / Ldarga / Ldarga_S` → 复制源 local/param slot 的 RefOffset 到 byref slot
  - `Ldsflda` → 置 0
  - `Ldflda` → 保留 0(Step 17 扩展 Ldflda 编码时再启用嵌套 RefOffset 传播)
- Ref-Slot 编码 `Operand4 = -1 - localInfos[reg].RefOffset` 在 Ldloca/Ldarga 产生的 byref slot 上直接携带 receiver struct 的 RefOffset,handler 一行给出正确的引用基址

**收益**:同帧 struct 引用字段的 `Stfld_Ref` / `Initobj` / `Ldfld_Ref` 在**未 fold** 场景下也正确,避免优化器承担正确性责任。

---

## 5. CLR 字段访问零装箱框架(方案 A 运行时基础设施)

**动机**:用户批评之前 CLR primitive/valueType 字段读写走 `GetFieldValue` + `Convert.To*` 有装箱 + 反射开销。要求参考 [`CLRType.CopyFieldToStack`](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs) 的 Legacy binding 模式,优先 binding 后 fallback。

**运行时基础设施**(本 step 落地,代码生成器留 Step 15):

- [AppDomain.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs) 新增 `CLRFieldNeoGetterDelegate` / `CLRFieldNeoSetterDelegate` + `fieldNeoBindingMap` + `RegisterCLRFieldNeoBinding` API
- [CLRType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs):
  - `fieldNeoBindingCache` per-type,`InitializeFields` 装填
  - `CopyFieldToNeoFrame` / `AssignFieldFromNeoFrame` / `CopyStaticFieldToNeoFrame` / `AssignStaticFieldFromNeoFrame` — CLR 字段读写单入口,先试 binding、缺失时走 fallback 内部分派 helper
  - `CopyValueToNeoFrame` / `ReadValueFromNeoFrame`:primitive/enum/IntPtr/UIntPtr **委派** `ILIntepreter.WriteNeoPrimitive` / `ReadNeoPrimitive`(单一 primitive 分派源头);value type 抛 Step 15 NotImpl;reference type 走 mStack + `-1` 哨兵
  - 全部方法用 `#if ENABLE_NEO_MODE` 保护(Legacy 编译不引用 Neo-only helper)
- [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) handler CLR 分支塌缩为一行 `clrType.CopyFieldToNeoFrame(...)` / `clrType.AssignFieldFromNeoFrame(...)`(此前的 `SetNeoIndirectClrField` wrapper 已删除,直接调用 CLRType API)

**Primitive marshal 单一入口**([ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs)):

- `WriteNeoPrimitive(byte* dst, IType type, object value)` — boxed CLR → Neo 4/8B slot,含 IL enum(`ILEnumTypeInstance` unpack)、CLR enum(`Enum.GetUnderlyingType` + `Convert.To*`)、sub-int widen(零 int slot 再写低位)、IntPtr/UIntPtr
- `ReadNeoPrimitive(byte* src, IType type)` — Neo slot → boxed CLR,含 **CLR enum → `Enum.ToObject(declared, ...)`**;IL enum 无独立分支,`ILType.TypeForCLR` 返回 underlying primitive Type,fall through 到 CLR primitive 分支返回 boxed underlying — 与 Legacy `StackObject.ToObject` 语义对齐(反射入口拿到的是 boxed underlying primitive,而非 `ILEnumTypeInstance`)

**4 处使用点全部收拢到单一源头**:
1. `InvocationFrame.WriteObjectInternal` 参数封送
2. `ILIntepreter.InvokeNeoClrMethod` 返回值写回(此前 `NeoBoxReturnValue` 局部函数已删除)
3. `InvocationFrame.ReadObject` 返回值 boxing
4. `CLRType.CopyValueToNeoFrame` / `ReadValueFromNeoFrame` 字段 fallback

---

## 6. FCP 精确档 byref alias 追踪(Struct 复制传播正确性修复)

**问题**:BCP/FCP 在 SSA rename 之前就直接删除 `Move_Vt` 并重定向读取到源寄存器,导致 `NeoStep12StructAssignment` 里 `dst = src; ldloca dst; stfld dst.field` 变成了对 `src` 的间接修改(struct 值语义被破坏)。

**决策**:用户拒绝"遇到 ldloca 就保守终止"的粗档,要求精确追踪派生链。

**实现**([Optimizer.FCP.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.FCP.cs) L47-L158):
- 维护 `HashSet<short> xSrcAliases`(x 源变量的 byref alias 集合)
- 传播规则:`Ldloca/Ldloca_S/Ldarga/Ldarga_S`(Register2 == xSrc 或 ∈ aliases)、`Ldflda`(Register2 ∈ aliases)、`Move`(Register2 ∈ aliases)→ Register1 加入 aliases
- 终止条件:`Stfld*/Stind*/Initobj/Stobj/Call*/Newobj` 的 receiver ∈ aliases → 触发 `postPropagation = true`(视为对 xSrc 的间接写,停止 forward copy propagation)

**收益**:保留 struct copy propagation(用户明确要求"减少指令数对性能至关重要"),同时通过精确追踪派生指针把间接写识别为传播终止,保证正确性。

---

## 7. ECMA-335 合规状态

### 本 step 内闭环(A1-A5 全部完成)

- **A1 struct-this ABI**(II.13.3):by-ref 8 字节 Ref Slot,无 by-value copyback ✓
- **A2 Ldfld/Stfld Ref Slot receiver**(II.4.10):三态编码 `Operand4` 覆盖 heap / inline direct / Ref-Slot,handler 塌缩 ✓
- **A3 Ref Slot 基础机制**(III.3.43/29 + III.4.12/28/26/14):slot 分配 + Ldloca/Ldarga/Ldflda/Ldsflda 生产 + Stind/Ldind 消费 + Array NotImpl 占位 ✓
- **A4 Ldsfld/Stsfld + cctor 闭环**(I.8.9.5):handler 覆盖 primitive/reference/Value,StaticInstance 触发 cctor ✓
- **A5 sub-int 符号/零扩展**(III.1.1.1):`Ldfld_U1/Boolean/U2` 零扩展、`Ldfld_I1/I2` 符号扩展、对应 `Ldsfld_*` / `Ldind_*` 变体齐全 ✓

### 转移到下游 step

- **Inline fold 优化**(**Step 12c — 新增编排**):Step 12b Task 9 原始定义是"(可选)Ldfld/Stfld inline fold pass",但实际落地走了 `PropagateByRefReferentOffsets` 替代方案(见 §4)以避免让 fold 承担正确性责任。**inline fold 本身尚未实现**,作为纯优化(缩短 `Ldloca V; Ldfld/Stfld` 路径为 `Operand4 > 0` inline direct)编排到新增的 Step 12c(struct 访问优化)。依赖 Step 12b + FCP byref alias 追踪协同。
- **CLR value-type 字段 fallback**(Step 13/15):`CLRType.CopyValueToNeoFrame` / `ReadValueFromNeoFrame` 遇到 CLR value type 字段暂时抛 NotImpl,等 Step 13 Box/Unbox + Step 15 CLRBinding 代码生成器落地
- **CLR Ref Slot receiver 的 Ldfld_Value / Stfld_Value**(Step 13/17):当前 `NotImplementedException("Neo Ldfld_Value through a CLR Ref Slot: Step 13/17")`
- **Ldflda 携带 structRefOffset**(Step 17):嵌套 struct 引用字段的 ldflda 目前 RefOffset 保留 0,Step 17 完整实现跨帧 ref/out 时同步扩展
- **Array 分支 Ldind/Stind**(Step 16):handler 已经预留 `is Array` 检查位置,直接抛 `NotImplementedException("Step 16")`
- **CLR ↔ IL 外部入口 marshalling**(**Step 13b — 新增编排**):`InvocationContext.Invoke` / `DelegateAdapter.ILInvokeSub` / `CLRRedirections.MethodInfoInvoke` 目前 `#if ENABLE_NEO_MODE` 分支下 fail-fast NotImpl。原设计文档未编排这一批(Step 13 仅覆盖 Box/Unbox);Step 12b 已在 [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) 里回填 Step 13b 编排,依赖 Step 12b 落地的 `InvocationFrame` + `WriteNeoPrimitive/ReadNeoPrimitive` 单入口 + Step 13 落地的 CLR value-type marshalling。**代码里挂 "Step 13" tag 的 NotImpl(InvocationContext L458-L460、InvocationFrame.cs GenericStubMsg L47、Step 12 handoff L78/L239)Step 13b 落地时应统一改为 "Step 13b" 或直接实现**。

---

## 8. 关键代码变更清单

| 文件 | 关键变更 |
|---|---|
| [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) | byref slot Size=8/RefOffset 语义重定义;`PropagateByRefReferentOffsets` pass(§4);struct-this ABI(§2);Ldflda/Ldsflda Translate |
| [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) | 全部 Ref Slot handler、Ldfld/Stfld 三态分派、CLR field 单入口调用、`ReadNeoReference` `-1` 短路、`Ldfld_Value/Stfld_Value` refs 段改走 `mStack.CopyTo` |
| [ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs) | `WriteNeoPrimitive` / `ReadNeoPrimitive` 单一 primitive marshal 入口(含 IL enum vs CLR enum 分治);删除 `NeoBoxReturnValue` |
| [CLRType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs) | `CopyFieldToNeoFrame` / `AssignFieldFromNeoFrame`(实例 + 静态)+ `CopyValueToNeoFrame` / `ReadValueFromNeoFrame`;`fieldNeoBindingCache`;`#if ENABLE_NEO_MODE` 保护 |
| [AppDomain.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs) | `CLRFieldNeoGetterDelegate` / `CLRFieldNeoSetterDelegate`、`fieldNeoBindingMap`、`RegisterCLRFieldNeoBinding` API |
| [Optimizer.FCP.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.FCP.cs) | byref alias 精确追踪(§6),识别派生指针间接写作为传播终止 |
| [Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs) | 三态 lowering:Initobj / Ldfld_* / Stfld_* 的 `Operand4` 编码 |
| [Optimizer.NeoTypeSpecialize.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.NeoTypeSpecialize.cs) | 配合 byref slot RefOffset 语义调整 |
| [Other/UncheckedList.cs](file:///f:/SVN/ILRuntime/ILRuntime/Other/UncheckedList.cs) | `CopyTo(int, UncheckedList<T>, int, int)`(Array.Copy)+ DEBUG `List<T>` 同名 extension |
| [object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) | §2.5(Ref Slot 抽象前置)+ §4.3(两种宽度规则并存)回填 |
| [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) | Step 12b 作为 Step 13/13b/16/17/18 前置说明回填;Step 13b 新增编排 |

---

## 9. 遗留 open items(不阻塞 Step 12b 交付)

1. **NotImpl fail-fast 状态清点**:`ILIntepreter.Neo.cs` / `CLRType.cs` / `InvocationContext.cs` / `InvocationFrame.cs` 里的 `NotImplementedException` 全部归属到下游 step(见 §7)。Step 13b 落地时统一改 tag 或实现覆盖。
2. **DEBUG `List<T>.CopyTo` extension aliasing 语义**:DEBUG 分支不保证 memmove 语义,注释已声明"调用方需保证 src/dst 不重叠"。Neo 内部 struct copy 的 ref 段跨字段永远不重叠,当前 8 处调用点安全。CLRBinding 生成器(Step 15)emit `mStack.CopyTo(...)` 时同样需要遵守。
3. **byref slot `RefOffset` 语义重定义的文档化**:design §4.3 / §15 需要补一段说明 byref slot RefOffset 语义与 non-byref slot 的差异,避免后续 step 误用。

---

## 10. 测试与验证

- **命令**(Neo):
  ```
  dotnet run -c Debug_Neo --verbosity quiet --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj TestCases/bin/Debug/netstandard2.1/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch true Neo
  ```
- **命令**(Legacy):
  ```
  dotnet run --verbosity quiet --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj TestCases/bin/Debug/netstandard2.0/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch false
  ```
- **最终结果**:
  - Neo:41/41 通过,0 failed / 0 ignored / 0 todos
  - Legacy:493/493 通过,0 failed / 20 ignored / 7 todos(与基线一致)

---

## 11. 交给下一位的三行结论

Step 12b 把 Ref Slot 8 字节 `(objectIndex, offset)` 基础机制、struct-this by-ref ABI、Ldfld/Stfld 三态 receiver、Ldsfld/Stsfld + cctor 闭环、sub-int 扩展 5 处 ECMA-335 违规全部闭环。CLR 字段访问零装箱运行时框架就位,`CLRBinding` 代码生成器改造留 Step 15。CLR ↔ IL 外部反射入口(InvocationContext 等)迁移原设计文档漏排,已新增 **Step 13b** 编排,依赖 Step 12b + Step 13。
