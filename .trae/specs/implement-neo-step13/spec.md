# ILRuntime Neo Step 13 — Box / Unbox 完整实现(含 CLR 值类型双存储路径 + 编译期分派)

> **前置状态**: Step 12 + Step 12b + Step 12c 已完成,Neo 41/41、Legacy 493/493 全绿。IL 值类型 Box/Unbox handler 已存在,CLR 值类型 Box/Unbox/Initobj/字段访问 4 处 `NotImplementedException("... Step 13")` 挂账。
>
> **设计基线**: [object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) §18(Boxing/Unboxing)、§18.2(CLR 值类型双路径)、§18.3(Unsafe.Unbox<T> in-place)、§18.4(memcpy 策略)、§18.5(constrained callvirt 编译期特化)、§19(newobj 帧上 zero-init)。
>
> **步骤总纲**: [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) Step 13。
>
> **测试类型基线**: 全部使用 [ILRuntimeTestBase/TestFramework/TestVector3.cs](file:///f:/SVN/ILRuntime/ILRuntimeTestBase/TestFramework/TestVector3.cs) 里已有的 struct(`TestVector3` 有 Binder,`TestVectorStruct` 纯字段,`TestVector3NoBinding` 有 ToString/Normalize,`JInt` 含 property getter/setter,`TestStructA` 纯字段)。

---

## Why

Step 12b 打通了 IL 值类型 Box/Unbox 与 struct-this ABI,但 CLR 值类型的 flat-bytes ↔ boxed 转换全部 fail-fast。用户明确指出**"存储方式的选择直接决定性能"**:

- 纯字段无方法的 struct(如 `TestVectorStruct`),强制 boxed 意味着字段访问要走 mStack + 反射,性能远差于帧上 flat bytes
- 有 mutating 实例方法但**未注册 Binder**的 struct(如 `JInt` 大量 property getter/setter,构造方法读写字段),若强制 flat bytes 存储,每次方法调用都要 unbox → copy → box back 三步走反射,反而是负优化
- 有 Binder 的 struct(如 `TestVector3` / `Fixed64`)通过 CLR Redirection 直接操作栈上 flat bytes,零装箱且方法调用重定向,是性能最优路径

因此 Step 13 的关键不是"实现两个 handler 分支",而是**建立一套编译期分派机制**让每个 CLR 值类型走到最优路径,再补齐 handler。

## What Changes

### 1. 存储分派枚举 `StructStorage`(核心概念)

**是什么**:一个 CLR value type 在 Neo 模式下的**存储路径分类**,per-CLRType 编译期一次性计算并缓存。作用:JIT emit 阶段直接根据枚举选择 opcode 变体(Box/Unbox/Initobj/Ldfld/Stfld),让 interpreter handler **完全避免运行时布局判定**(不再有 `binder != null ?` 或 `slot.Size == 4 ?` 之类分支)。

```csharp
public enum StructStorage
{
    NotValueType, // 引用类型或未初始化
    Inline,       // 帧上"primitive 段 + mStack ref 段"布局,与 IL 值类型完全对称
    Boxed,        // mStack 存 boxed CLR 对象;方法调用走 Unsafe.Unbox<T> in-place
}
```

**判定规则**(在 `CLRType.InitializeFields` 里一次性计算):

- 非 valuetype → `NotValueType`
- `ValueTypeBinder != null` → `Inline`(Binder 显式声明帧布局 + 提供 CLR Redirection 处理方法调用)
- `[ILRuntimeBlittable]` 属性 → `Inline`(开发者显式声明可 Inline;attribute 本身不做 field-safety 校验,与 CLR 的 blittable 严格定义脱钩,允许含引用字段)
- **无 instance methods**(不包括 static / operator / constructor,包括 property getter/setter 与 override)→ `Inline`(编译期证明不会有 mutating 方法调用,含引用字段也 OK)
- 其他 → `Boxed`(默认 fallback,方法调用走 `Unsafe.Unbox<T>` in-place 零装箱)

**关键澄清**(相对上一版 spec 的修正):
- **Inline 布局允许含引用字段**:与 IL 值类型 flat layout 完全对称。字段展开规则:primitive/enum/嵌套 Inline struct → 占 primitive 段(自然对齐);引用类型字段或 Boxed 嵌套 struct → 占 4 字节 primitive slot(mStack index)+ 1 个 ref slot。**不做 blittable 限制**
- `[ILRuntimeBlittable]` 名字沿用现有惯例,语义上是"强制 Inline 存储"而非 CLR 传统的"可 memcpy blittable";即使 struct 含引用字段也可标注

### 2. CLRType 新增结构布局属性(不带 Neo 前缀,与 ILType 命名对齐)

```csharp
public class CLRType : IType
{
    public StructStorage StructStorage { get; }        // 缓存的分派枚举
    public int TotalPrimitiveSize { get; }             // Inline 布局 primitive 段总字节数(Boxed 时为 0)
    public int TotalReferenceCount { get; }            // Inline 布局 ref 段引用槽数(Boxed 时为 0)
    public int GetFieldPrimitiveOffset(int fieldHash); // Inline 布局字段的 primitive 段字节偏移
    public int GetFieldReferenceOffset(int fieldHash); // Inline 布局字段在 ref 段的引用槽索引(引用字段用)
}
```

- 命名与 [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) 的 `TotalPrimitiveSize` / `TotalReferenceCount` / `FieldPrimitiveOffset` 完全对齐
- 布局计算算法复用 [InitializeFieldsForFlatLayout](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1794) 的思路,遍历 `Type.GetFields(Instance)` 并递归展开嵌套 Inline struct
- Boxed / NotValueType 的 `TotalPrimitiveSize = 4, TotalReferenceCount = 1`(mStack index + ref slot)

### 3. ValueTypeBinder Neo 角色收窄:只做方法重定向

**核心决策**:布局与 Box/Unbox 拷贝完全由框架自动生成,Binder **不参与** —— 现有 Binder 类的 Neo 使命只剩"通过 `RegisterCLRRedirection` 注册 CLR 方法重定向,让方法调用直接操作栈上 flat bytes 数据"。

- `ValueTypeBinder<T>` **不新增任何 Neo-only 属性/方法**
- 现有 binder 的 legacy `AssignFromStack` / `CopyValueTypeToStack`(StackObject-based)保持不变,只在 Legacy 模式下调用
- `RegisterCLRRedirection` 保持不变;Redirection 函数内部通过 §4 提供的字段布局查询 API + 编译期常量 offset 直接读写 struct 字段(不做运行时 fieldHash lookup)

### 4. 框架级字段布局查询 API(供 Redirection / codegen / handler 共用)

`CLRType` 只暴露**布局元数据查询**,不提供包装型读写函数(避免 hot path 上多余的间接调用与 fieldHash lookup):

```csharp
public class CLRType
{
    public int GetFieldPrimitiveOffset(int fieldHash); // primitive 段字节偏移(仅 Inline 有效)
    public int GetFieldReferenceOffset(int fieldHash); // ref 段引用槽索引(仅 Inline 引用字段有效)
    public int TotalPrimitiveSize { get; }
    public int TotalReferenceCount { get; }
    public StructStorage StructStorage { get; }
}
```

**使用模式**:

- **CodeGenerator 生成的 CLRBinding 代码**(Step 15):在生成阶段一次性查 offset,把编译期常量硬编码进 emit 出的 Redirection body 里,hot path 上直接 `*(float*)(framePtr + 0)` / `*(float*)(framePtr + 4)` / `*(float*)(framePtr + 8)`,零查表
- **手工写的 Redirection**(如 `TestVector3Binder.Vector3_Add` 在 Neo 模式下如需迁移):caller 在初始化时通过 `clrType.GetFieldPrimitiveOffset(X_hash)` 缓存到 static readonly 字段,方法内直接 `*(float*)(framePtr + s_XOffset)`
- **反射 fallback**(CopyValueToNeoFrame 无 codegen 走通用路径):按 `TotalPrimitiveSize` 一次性 `Unsafe.CopyBlock`,ref 段循环 mStack index;不做 per-field lookup
- **Interpreter handler**(Ldfld_*_Inline / Stfld_*_Inline):JIT lowering 阶段已把 fieldOffset 编码到指令 Operand,handler 直接 `*(int*)(framePtr + ip->Operand2)`,不查 fieldHash

### 5. 框架统一 Box/Unbox/Initobj 通用拷贝

Box/Unbox handler 内部直接 `Unsafe.CopyBlock` primitive 段 + 循环拷贝 mStack ref 段,**无 Binder helper 依赖**:

```csharp
// Box_Inline 伪代码
object boxed = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(clrType.TypeForCLR);
// 或对 blittable 无引用字段类型:T boxed = default;
ref byte boxedRef = ref Unsafe.As<T, byte>(ref Unsafe.Unbox<T>(boxed));
Unsafe.CopyBlockUnaligned(ref boxedRef, ref *(frameBase + SrcOffset), (uint)clrType.TotalPrimitiveSize);
// 拷贝 ref 段(如果 TotalReferenceCount > 0),通过反射按字段填充 boxed 的引用字段
for (int i = 0; i < clrType.TotalReferenceCount; i++)
    clrType.WriteBoxedReferenceField(boxed, i, mStack[frameRefBase + srcRefOffset + i]);
```

对含引用字段的 CLR struct,`CLRType` 缓存的字段布局表额外记录"第 i 个 ref 段槽对应哪个 FieldInfo",Box 时通过反射 `FieldInfo.SetValue`(或缓存的 setter delegate)把 mStack 中的引用回填到 boxed CLR 对象。Unbox 反向。

### 6. Box/Unbox/Initobj 保留单 opcode + `StructStorage` 运行时分派

**决策**:不新增 `Box_Inline` / `Box_Boxed` 等变体,handler 内一次 `switch (clrType.StructStorage)` 完成分派。理由:

- Box/Unbox/Initobj 不在 hot loop(典型一次数据流一次 Box),分支预测器对单一类型的稳定 `StructStorage` 值几乎 100% 命中,单次开销 ~1-2 ns
- 每个 opcode 新增 2 个变体会显著膨胀 dispatch switch 表,伤 icache 与解释器主循环
- Step 12b 已建立通过 Operand-flag 分派的先例,但那是为 hot-path Ldfld 三态设计;Box/Unbox 频次远低,不值得

| CIL | Handler dispatch (单 opcode) |
|---|---|
| `Box T` | `switch (clrType.StructStorage) { Inline: 帧读 → 构造 boxed → 写 mStack; Boxed: 透传 mStack index }` |
| `Unbox T`/`Unbox_Any T` | 对称 |
| `Initobj T` | `switch (StructStorage) { Inline: InitBlock+清 ref 段; Boxed: Activator.CreateInstance 写 mStack }` |

### 7. Ldfld_* / Stfld_* 复用现有 Operand4 三态编码,不新增变体

**决策**:CLR value type 的 Inline vs Boxed 区分**已隐含**在 Step 12b 建立的 `Operand4` 三态 + receiver 类型分派里,不需要新增 opcode 变体。

- `Operand4 > 0` (inline direct):handler 从帧字节按 byte offset 读 primitive;**不区分**受者是 IL 值类型 还是 CLR Inline value type —— 帧字节区语义完全一致
- `Operand4 == 0` (heap):handler 已按 `receiver is ILTypeInstance` vs `receiver is CLR object` 分派:
  - `ILTypeInstance` → 直读 `Primitives`
  - CLR object(含 Boxed CLR value type) → `clrType.CopyFieldToNeoFrame(...)` / `AssignFieldFromNeoFrame(...)` Step 12b 单入口
- `Operand4 < 0` (Ref-Slot receiver):同上

**唯一需要动的地方**:JIT emit `Ldfld_* / Stfld_*` 时,若 receiver 类型是 CLR Boxed value type,选 `Operand4 == 0`(heap 路径);若是 CLR Inline value type,选 `Operand4 > 0`(inline direct)。这个选择本来就基于 receiver 的 `slot.Size == 4 ? heap : inline` 判据,只要 Task 2 的 `AllocateSlotForType` 分派正确,Ldfld/Stfld emit 侧几乎不用改。

### 8. Initobj boxed IL value type(§7 挂账)

`ILIntepreter.Neo.cs L2263` 的 `NotImplementedException("Initobj boxed: Step 13")`:receiver 指向 mStack 里已存在的 `ILTypeInstance` → 清 `Primitives` + null 化 `ManagedObjects`,**不**重新 Instantiate。

### 9. constrained.callvirt 编译期特化

在 Translate 阶段识别 `Constrained.` prefix + 紧邻 `Callvirt`:

- concrete valuetype T 且实现虚方法 → 直接 `Call`,this 走 Step 12b Ref Slot ABI
- concrete valuetype T 未实现虚方法 → emit `Box T`(按 StructStorage 分派)+ `Callvirt`
- reference type T → 忽略 prefix,直接 `Callvirt`
- 泛型 T → 记录 `PatchKind.ConstrainedCall`,实例化时二选一(patch 表完整实现留 Step 15,本 step concrete T 走通即可)

### 10. Newobj 半构造暴露修复

[ILIntepreter.Neo.cs L1439-L1445](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L1439-L1445) `Newobj` 的 dst mStack ref slot + primitive slot 写入延后到 `InvokeNeoCallTarget` 成功返回后。ctor 抛异常时 dst slot 保持原值。

### 11. AllocateSlotForType 按 StructStorage 分支

- CLR primitive/enum → 现有 primitive widen 分支
- CLR value type,`StructStorage == Inline` → `Size = clrType.TotalPrimitiveSize`,`RefCount = clrType.TotalReferenceCount`
- CLR value type,`StructStorage == Boxed` → `Size = 4`,`RefCount = 1`

### 12. CLRType.CopyValueToNeoFrame / ReadValueFromNeoFrame 单入口收尾

`ft.IsValueType` NotImpl 分支落地,按 `StructStorage` 分派(Inline → §5 通用 Box/Unbox 拷贝;Boxed → mStack index 回写)。

### 13. InvocationFrame primitive 泛型 stub 落地

`WriteInt32<T>` / `WriteInt64<T>` / `WriteSingle<T>` / `WriteDouble<T>` + 4 个 `Read*<T>` 对 primitive/enum T 走 `WriteNeoPrimitive` / `ReadNeoPrimitive` 单入口;对 reference T 抛 `NotSupportedException`(Step 13b)。

## Non-Goals(本 step 不做,归后续)

- **CLRBinding 代码生成器改造**(自动 emit `Unsafe.Unbox<T>` in-place 模式)→ Step 15;本 step 只让运行时反射 fallback 正确
- **CLR ↔ IL 外部反射入口迁移**(InvocationContext.Invoke / DelegateAdapter.ILInvokeSub / CLRRedirections.MethodInfoInvoke)→ Step 13b
- **异常类型 checked box / Rethrow** → Step 14
- **isinst / castclass + box peephole 消解** → Step 15
- **数组元素 CLR 值类型 ldelem / stelem** → Step 16
- **Ref/Out 跨帧封送 struct** → Step 17
- **值类型 newobj + Ref Slot 传 this**(CLR 类型分支)→ Step 18
- **泛型 T 的 constrained.callvirt patch 表完整实现** → 与 Step 15 patch 机制协同,本 step 只对 concrete T 走通

## Impact

- **Affected specs**: Step 12 handoff §7、Step 12b handoff §7 中所有 "Step 13" tag NotImpl 全部闭环或转移到 Step 13b/17;Step 15 binding 生成器改造依赖本 step 落地的运行时 API;Step 18 值类型 newobj CLR 分支依赖本 step 的 storage 分派
- **Affected code**:
  - [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs):Initobj / Box / Unbox / Ldfld_* / Stfld_* 6 处 "Step 13" NotImpl + newobj 延后写入 + `constrained.callvirt` 分派
  - [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) `AllocateSlotForType`:CLR value type Inline / Boxed 二分支
  - [JITCompiler.NeoHelpers.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.NeoHelpers.cs):Translate 阶段 `Constrained.` prefix 处理(不新增 opcode 变体;Box/Unbox/Initobj/Ldfld/Stfld handler 内一次 `StructStorage` switch 完成分派)
  - [CLRType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs):新增 `StructStorage` / `TotalPrimitiveSize` / `TotalReferenceCount` / `GetFieldPrimitiveOffset` / `GetFieldReferenceOffset` 布局查询 API + `CopyValueToNeoFrame` / `ReadValueFromNeoFrame` 值类型分支落地(不新增 per-field 读写包装)
  - [ValueTypeBinder.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ValueTypeBinder.cs):**不新增任何 Neo-only 成员**;现有 API 保持;Neo 场景下 Binder 只通过 `RegisterCLRRedirection` 注册方法重定向,Redirection body 用编译期常量 offset(通过 `CLRType.GetFieldPrimitiveOffset` 一次性查得后缓存)直接指针算术
  - 现有 `TestVector3Binder` / `Fixed64Binder` 等在 `#if ENABLE_NEO_MODE` 下 emit 新版 Redirection(下一 step 或本 step 附带,视范围;若本 step 不改,legacy Redirection 只在 Legacy 模式下调用,Neo 模式下走通用反射 fallback 或 §5 通用拷贝)
  - [ILRuntime/Other/ILRuntimeBlittableAttribute.cs](file:///f:/SVN/ILRuntime/ILRuntime/Other/ILRuntimeBlittableAttribute.cs)(新增)
  - [ILIntepreter.InvocationFrame.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs):8 个 primitive 泛型 stub 落地
  - [TestCases/NeoStep13Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep13Test.cs)(新增)

---

## ADDED Requirements

### Requirement: CLRType.StructStorage 编译期分派
The system SHALL classify every CLR value type into `Inline` or `Boxed` storage at type-initialization time,cached on `CLRType`. Classification is decision-forced (no runtime fallback):

- Non-value type → `NotValueType`
- `ValueTypeBinder != null` → `Inline`
- Has `[ILRuntimeBlittable]` attribute → `Inline`(allows fields of any type,including references,laid out per §2 dual-segment)
- Has no instance methods (excluding static / operator / constructor; including property getter/setter and override) → `Inline`
- Otherwise → `Boxed`

#### Scenario: TestVector3 with binder → Inline
- **WHEN** `CLRType` for `TestVector3` is initialized and `TestVector3Binder` is registered
- **THEN** `StructStorage == Inline`, `TotalPrimitiveSize == 12`, `TotalReferenceCount == 0`

#### Scenario: TestVectorStruct pure-data → Inline
- **WHEN** `TestVectorStruct` (only fields `A`, `B`, `C`; no instance methods) is initialized
- **THEN** `StructStorage == Inline`; layout recursively expands `B: TestVectorStruct2` and `C: TestVector3`

#### Scenario: JInt with property getter/setter → Boxed
- **WHEN** `JInt` (has `Value` property getter/setter,operator overloads,ctor,ToString override) is initialized without binder
- **THEN** `StructStorage == Boxed` (property accessors count as instance methods)

#### Scenario: TestVector3NoBinding with ToString override → Boxed
- **WHEN** `TestVector3NoBinding` (has `ToString()` override) is initialized without binder
- **THEN** `StructStorage == Boxed`

#### Scenario: Struct with reference fields and no methods → Inline
- **WHEN** a struct with only reference-type fields (or mixed primitive + reference) has no instance methods
- **THEN** `StructStorage == Inline`; primitive segment holds mStack indices,ref segment holds the actual references (symmetric to IL value type flat layout)

#### Scenario: [ILRuntimeBlittable] forces Inline for method-carrying struct
- **WHEN** a struct with instance methods carries `[ILRuntimeBlittable]`
- **THEN** `StructStorage == Inline` (developer overrides the method-driven default)

### Requirement: CLRType structure-layout members
`CLRType` SHALL expose `TotalPrimitiveSize`, `TotalReferenceCount`, `GetFieldPrimitiveOffset(int fieldHash)`, `GetFieldReferenceOffset(int fieldHash)` computed at `InitializeFields` time,following the same natural-alignment algorithm as [ILType.InitializeFieldsForFlatLayout](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1794). Nested Inline structs recursively expand into the parent's primitive+ref segments. Layout is **always** computed from reflected field information (regardless of `ValueTypeBinder` presence).

#### Scenario: TestVectorStruct layout expansion
- **WHEN** `TestVectorStruct.TotalPrimitiveSize` is queried
- **THEN** it equals `sizeof(int) + TestVectorStruct2.TotalPrimitiveSize + TestVector3.TotalPrimitiveSize` (with natural alignment padding)

### Requirement: CLRType layout-query API only (no wrapped read/write helpers)
`CLRType` SHALL expose only layout-metadata queries — `GetFieldPrimitiveOffset(int fieldHash)` and `GetFieldReferenceOffset(int fieldHash)` — plus the `TotalPrimitiveSize` / `TotalReferenceCount` / `StructStorage` properties from §2. It SHALL NOT provide wrapped `ReadField*` / `WriteField*` helper methods that internally hashtable-lookup a field on every call,because:

- CodeGenerator (Step 15) resolves offsets at code-generation time and emits compile-time-constant pointer arithmetic (`*(float*)(framePtr + 8)`) into the Redirection body — zero runtime lookup on hot path
- Manually authored Redirection code caches the offset once (e.g. `static readonly int s_XOffset = clrType.GetFieldPrimitiveOffset(X_hash);`) and uses the constant on each call
- Interpreter `Ldfld_*_Inline` / `Stfld_*_Inline` handlers get the offset baked into the instruction `Operand2` at JIT lowering time
- Reflection fallback (`CopyValueToNeoFrame`) uses whole-struct `Unsafe.CopyBlock` + `TotalReferenceCount` iteration,never per-field lookup

#### Scenario: Neo-mode Redirection uses cached offset constant
- **WHEN** a manually written Neo Redirection for `TestVector3.op_Addition` needs to read operand X
- **THEN** the class caches `static readonly int s_XOffset = clrType.GetFieldPrimitiveOffset(TestVector3_X_hash);` at init and reads via `*(float*)(operandAPtr + s_XOffset)` on the hot path,not through any `clrType.ReadFieldSingle(operandAPtr, hash)` call

### Requirement: ValueTypeBinder role in Neo mode
`ValueTypeBinder<T>` SHALL NOT gain any Neo-only virtual members. Its Neo-mode role is limited to registering CLR method Redirections via the existing `RegisterCLRRedirection` API. All layout,Box/Unbox copying,Initobj,and field-offset lookup are handled by the framework using the `CLRType` layout-query API.

Existing binder classes SHALL continue to compile and function; their legacy `AssignFromStack` / `CopyValueTypeToStack` are called only in Legacy mode.

### Requirement: AllocateSlotForType storage-aware branching
The JIT SHALL allocate CLR value-type slots based on `CLRType.StructStorage`,not on `ValueTypeBinder != null`:

- `Inline` → `Size = TotalPrimitiveSize`, `RefCount = TotalReferenceCount`
- `Boxed` → `Size = 4`, `RefCount = 1`

**Invariant — Boxed types never allocate flat bytes**: A Boxed CLR value-type slot is byte-for-byte identical to a reference-type slot:
- 4-byte primitive slot holding an `int` mStack index
- 1 mStack ref slot holding the boxed CLR object
- No frame region is reserved for the struct's field data;the entire struct body lives only inside the boxed CLR object on the managed heap
- All Box/Unbox/Initobj/Ldfld/Stfld handler variants for Boxed storage MUST route through mStack (index → CLR object → reflection or `Unsafe.Unbox<T>`),never through frame bytes

#### Scenario: JInt slot layout is byte-identical to a reference-type slot
- **WHEN** JIT allocates a local of type `JInt` (Boxed) and a local of type `object` in the same frame
- **THEN** both slots have `Size == 4, RefCount == 1`; the frame region occupied by the `JInt` slot never holds any of the struct's fields (`_obscuredInt`, `_obscuredKey`, `_originalValue` all live inside the boxed `JInt` in mStack)

#### Scenario: Box on a Boxed-storage struct triggers zero flat-bytes activity
- **WHEN** `box [JInt]` is executed
- **THEN** the handler reads only `*(int*)(frameBase + SrcOffset)` (an mStack index),never any byte after the index; no `Unsafe.CopyBlock` on the frame is performed

### Requirement: Box handler dispatches on StructStorage (no opcode variant)
`Box` handler SHALL remain a single opcode. Handler body performs a single `switch (clrType.StructStorage)`:
- `Inline` → framework-uniform copy: `Activator.CreateInstance` (or cached uninitialized-object factory) creates the boxed object; `Unsafe.CopyBlockUnaligned` copies `TotalPrimitiveSize` bytes from `frameBase + SrcOffset` into the box body; if `TotalReferenceCount > 0`,iterate ref segment and assign each mStack reference via cached FieldInfo setter delegate; write the boxed reference to mStack
- `Boxed` → no-op: propagate the source mStack index to the destination primitive slot; no allocation

#### Scenario: Box TestVector3
- **WHEN** IL `box [TestVector3]`
- **THEN** handler creates a boxed `TestVector3`,copies 12 bytes from `frameBase + SrcOffset` into the box body via `Unsafe.CopyBlock`,stores mStack ref;the `TestVector3Binder`'s legacy `CopyValueTypeToStack` is **not** invoked

#### Scenario: Box JInt
- **WHEN** IL `box [JInt]` on a Boxed-storage receiver
- **THEN** handler propagates the mStack index without allocation

### Requirement: Unbox / Unbox_Any handler dispatches on StructStorage (no opcode variant)
Symmetric to Box:
- Inline → primitive segment `Unsafe.CopyBlock` from box body to frame; ref segment: read from boxed CLR object's fields via cached getter delegates,write to frame's ref segment
- Boxed → propagate mStack index

Boundary:
- Source mStack index `< 0` → `NullReferenceException` before writing dst
- Boxed object not assignable to requested type → `InvalidCastException` before writing dst

### Requirement: Initobj handler dispatches on StructStorage (no opcode variant)
- Inline → `Unsafe.InitBlock` primitive segment + clear ref segment
- Boxed → `Activator.CreateInstance(clrType.TypeForCLR)` write mStack
- Boxed IL value type (existing NotImpl case,receiver is a mStack ref slot pointing to an existing `ILTypeInstance`) → clear `Primitives` + null `ManagedObjects` in place,do not re-Instantiate

### Requirement: Ldfld_* / Stfld_* reuses Step 12b Operand4 tri-state; no new opcode variant
CLR value-type field access reuses the existing `Operand4` tri-state encoding established in Step 12b:
- `Operand4 > 0` (inline direct): handler reads/writes frame bytes at compile-time byte offset — semantics identical for IL value types and CLR Inline value types
- `Operand4 == 0` (heap): handler dispatches on receiver runtime type:
  - `ILTypeInstance` → direct `Primitives` access
  - CLR object (which includes Boxed CLR value type) → `clrType.CopyFieldToNeoFrame` / `AssignFieldFromNeoFrame` (Step 12b single-entry)
- `Operand4 < 0` (Ref-Slot receiver): same as above via receiver resolve

**JIT emit-time selection**: `Operand4 > 0` chosen when receiver's slot is Inline (frame bytes); `Operand4 == 0` when receiver's slot is Boxed (mStack index). Since Task 2's `AllocateSlotForType` already assigns the right slot form,emit code needs minimal changes.

The `NotImplementedException("Neo CLR value-type field access: Step 13/17")` fail-fast SHALL be removed.

### Requirement: constrained.callvirt compile-time specialization
JIT SHALL specialize `constrained. T` + `callvirt` at Translate time per §9.

### Requirement: Newobj destination write deferred until ctor returns
Newobj SHALL delay writing dst mStack ref slot and primitive slot until the constructor returns successfully.

### Requirement: InvocationFrame primitive generic stubs
`WriteInt32<T>` / `WriteInt64<T>` / `WriteSingle<T>` / `WriteDouble<T>` and their `Read*<T>` counterparts SHALL delegate to `WriteNeoPrimitive` / `ReadNeoPrimitive` for primitive/enum `T`,throwing `NotSupportedException("Reference-type generic stub reserved for Step 13b")` for reference `T`.

### Requirement: [ILRuntimeBlittable] attribute
The system SHALL provide `ILRuntime.Other.ILRuntimeBlittableAttribute` (`AttributeUsage(AttributeTargets.Struct)`) to force `Inline` classification. Semantics: developer opts the struct into Inline storage (frame-layout);the attribute does **not** require the struct's fields to be CLR-blittable in the strict sense — reference fields are allowed and go through the ref segment. Name kept consistent with prior usage.

---

## MODIFIED Requirements

### Requirement: CLRType.CopyValueToNeoFrame / ReadValueFromNeoFrame value-type branch
Replace `throw new NotImplementedException` on `ft.IsValueType`. Dispatch by `StructStorage`:
- Inline → framework-uniform copy (§5 in What Changes): `Unsafe.CopyBlock` primitive segment + reflected setter/getter for reference segment
- Boxed → mStack index round-trip (existing reference-type semantics)

### Requirement: ValueTypeBinder role unchanged; Neo-mode Redirection uses layout-query API
`ValueTypeBinder<T>` API stays the same (no Neo-only additions). In Neo mode,binder classes only register CLR method Redirections via `RegisterCLRRedirection`;the Redirection function bodies query field byte offsets once via `CLRType.GetFieldPrimitiveOffset(fieldHash)` (cached to `static readonly int`) and then use compile-time-constant pointer arithmetic (`*(float*)(framePtr + s_XOffset)`) on the hot path,rather than hand-written `StackObject*` pointer arithmetic or per-field wrapper helpers.

---

## REMOVED Requirements

### Requirement: All `Step 13` fail-fast NotImpl in Neo handler / CLRType fallback
**Reason**: Replaced by ADDED/MODIFIED requirements. Six sites in [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) and two sites in [CLRType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs) migrate to storage-aware implementation.
**Migration**: Call sites reach the double-path implementation without new gates.
