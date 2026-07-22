# Step 13 Tasks — Box / Unbox 完整实现

## 工程约束(继承 Step 12b/12c,每个 Task 必须遵守)
- **`StructStorage` 是编译期一次性决策**:在 `CLRType.InitializeFields` 里计算并缓存,handler 内**禁止**通过 `ValueTypeBinder != null` / `slot.Size == 4` 之类做运行时布局判断
- **Box/Unbox/Initobj/Ldfld/Stfld 走 opcode 变体**:JIT 阶段基于 `StructStorage` emit 不同 opcode(或 Operand flag),handler 二态实现,拒绝三态运行时分支
- **Inline 布局允许含引用字段**:与 IL 值类型 flat layout 完全对称,primitive 段 + mStack ref 段,不做 blittable 限制
- **命名去 Neo 前缀**:`CLRType.TotalPrimitiveSize` / `TotalReferenceCount` / `GetFieldPrimitiveOffset` / `GetFieldReferenceOffset` / `StructStorage`,与 ILType 现有命名对齐
- **`[ILRuntimeBlittable]` 语义**:开发者显式声明 Inline 存储,不做 field-safety 校验(即使含引用字段也允许)
- **零装箱路径不走反射**:Inline 字段访问必须走 `GetFieldPrimitiveOffset` 直接读写 flat bytes,不允许 `GetFieldValue` + `Convert.To*`
- **Boxed 路径方法调用 in-place**:本 step 只让运行时反射 fallback 工作;binding 生成器改造归 Step 15
- **测试用例先立**:每个 ADDED Requirement 至少 1 个用例,覆盖 TestFramework 现有类型的 4 种 storage 路径

## Task Dependencies
```
Task 1 (ILRuntimeBlittableAttribute + ValueTypeBinder Neo API + StructStorage 判定 + CLRType 布局属性)
  └─→ Task 2 (AllocateSlotForType storage-aware 分支)
       ├─→ Task 3 (Initobj Inline/Boxed handler + opcode 变体)
       ├─→ Task 4 (Box Inline/Boxed handler + opcode 变体)
       └─→ Task 5 (Unbox Inline/Boxed handler + null/InvalidCast 边界)
            └─→ Task 6 (Ldfld_*/Stfld_* Inline/Boxed 双路径 + CLRType 单一入口收尾)
                 └─→ Task 7 (constrained.callvirt 编译期特化 + newobj 半构造修复 + InvocationFrame stub 落地)
                      └─→ Task 8 (NeoStep13Test.cs 全用例 + 回归 + handoff)
```

---

## [x] Task 1: 存储分派基础设施 + CLRType 布局查询 API
- **Depends On**: None
- **Description**:
  - **新增 `ILRuntime.Other.ILRuntimeBlittableAttribute`**:空 `AttributeUsage(AttributeTargets.Struct)` 类
  - **`ValueTypeBinder<T>` 不做任何 Neo-only 扩展**:legacy API 保持;Neo 模式下 Binder 通过现有 `RegisterCLRRedirection` 注册方法重定向,Redirection body 内部用 `CLRType.GetFieldPrimitiveOffset` 一次性拿到编译期常量 offset,hot path 直接指针算术
  - **`CLRType` 新增结构布局属性(总是计算,与 Binder 无关)**:
    - `public StructStorage StructStorage { get; }` — 在 `InitializeFields` 里按判定规则计算
    - `public int TotalPrimitiveSize { get; }` — 从 `Type.GetFields(Instance)` + 自然对齐算法计算(参考 [ILType.InitializeFieldsForFlatLayout L1794](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1794));Boxed 时 = 4
    - `public int TotalReferenceCount { get; }` — 统计引用字段数(嵌套 Inline struct 累加子级);Boxed 时 = 1
    - `public int GetFieldPrimitiveOffset(int fieldHash)` / `GetFieldReferenceOffset(int fieldHash)` — 仅 Inline 有效
    - 布局字段展开:primitive/enum/嵌套 Inline struct → primitive 段递归展开;引用类型字段或 Boxed 嵌套 struct → 4 字节 primitive slot + 1 个 ref 槽
    - `_fieldLayoutCache: Dictionary<int, (int primOff, int refOff, Type fieldType)>` 缓存
  - **不提供 per-field 读写包装函数**:调用方(codegen / Redirection / handler / CopyValueToNeoFrame)自行用 `GetFieldPrimitiveOffset` 拿 offset,再 `*(T*)(framePtr + offset)` 直接读写。理由:
    - CodeGenerator(Step 15)在生成阶段查 offset 并硬编码为编译期常量
    - Redirection 在初始化时缓存 offset 到 `static readonly int`,hot path 直接指针算术
    - Interpreter handler 通过 JIT lowering 把 offset 编到指令 Operand
    - CopyValueToNeoFrame 走整体 `Unsafe.CopyBlock`,不做 per-field lookup
  - **判定规则**:
    1. 非 valuetype → `NotValueType`
    2. `ValueTypeBinder != null` → `Inline`
    3. 有 `[ILRuntimeBlittable]` → `Inline`
    4. 无 instance methods(不含 static / operator / constructor,含 property getter/setter 与 override) → `Inline`
    5. 其他 → `Boxed`
  - **helper**:`private bool HasInstanceMethods()`,`type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)` 过滤;property getter/setter 名字 `IsSpecialName == true` 但仍算 instance methods,需显式识别
- **验收**:
  - Debug_Neo 编译 0 错误
  - `NeoStep13ClassifyStorage` 用例通过五路判定
  - `NeoStep13FieldOffsetQuery` 用例:`clrType.GetFieldPrimitiveOffset(TestVector3.X_hash) == 0`,`Y_hash == 4`,`Z_hash == 8`
  - Neo 41/41 全绿

---

## [x] Task 2: AllocateSlotForType storage-aware 分支
- **Depends On**: Task 1
- **Description**:
  - [JITCompiler.cs `AllocateSlotForType`](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L557-L598) 细化 CLR 分支:
    - `clrType.StructStorage == Inline` → `Size = clrType.TotalPrimitiveSize`, `RefCount = clrType.TotalReferenceCount`
    - `clrType.StructStorage == Boxed` → `Size = 4`, `RefCount = 1`
    - primitive/enum 继续走 primitive widen 分支(不变)
  - JIT dump 添加 CLR value type slot 的 storage 标注
- **验收**:
  - JIT dump 观察 `TestVector3` local slot Size=12/RefCount=0,`JInt` local slot Size=4/RefCount=1
  - Neo 41/41 全绿

---

## [x] Task 3: Initobj handler storage-aware 分派
- **Depends On**: Task 2
- **Description**:
  - **保留单 opcode `Initobj`**,handler 内一次 `switch (clrType.StructStorage)` 完成分派;不新增变体
  - **Handler 侧**([ILIntepreter.Neo.cs L2210-L2270](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2210)):
    - `IL value type`(现有):保持不变
    - `CLR value type & StructStorage == Inline`:`Unsafe.InitBlock(frameBase + DstOffset, 0, TotalPrimitiveSize)` + 循环清 ref 段
    - `CLR value type & StructStorage == Boxed`:`obj = Activator.CreateInstance(clrType.TypeForCLR)`,`mStack[frameRefBase + dstRefOffset] = obj`,`*(int*)(frameBase + DstOffset) = frameRefBase + dstRefOffset`
    - `Initobj boxed IL value type`(旧 NotImpl):receiver 指向 `ILTypeInstance` → 清 `Primitives` + null `ManagedObjects`,不 re-Instantiate
- **验收**:
  - `NeoStep13InitobjTestVector3` / `NeoStep13InitobjJInt` / `NeoStep13InitobjBoxedIL` 用例通过
  - Neo 44/44 全绿

---

## [x] Task 4: Box handler storage-aware 分派
- **Depends On**: Task 2
- **Description**:
  - **保留单 opcode `Box`**,handler 内 `switch (clrType.StructStorage)`;不新增变体
  - **Handler 侧**([ILIntepreter.Neo.cs L2272-L2327](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2272)),**框架统一拷贝,无 Binder 依赖**:
    - `IL value type`(现有):保持不变
    - `CLR value type & Inline`:
      1. `object boxed = clrType.CreateDefaultBoxed()`(封装 `Activator.CreateInstance` 或缓存 uninitialized-object factory)
      2. 通过 `CLRType.CopyPrimitiveIntoBox(byte* src, object boxed, int size)` helper 把帧字节复制进 box 内部(内部用反射拿 typed unbox / `Expression.Compile` 缓存一次)
      3. `Unsafe.CopyBlockUnaligned` primitive 段
      4. 若 `TotalReferenceCount > 0`,循环通过缓存的 FieldInfo setter delegate 把 mStack ref 段回填到 boxed 对象字段
      5. 写 mStack ref slot,更新 primitive slot
    - `CLR value type & Boxed`:读 `*(int*)(frameBase + SrcOffset)` mStack index → 若 dst 与 src slot 不同,复制 mStack 引用;更新 primitive slot
- **验收**:
  - `NeoStep13BoxTestVector3` — Inline → boxed 存 mStack,断言 boxed 对象的 X/Y/Z 字段等于帧内值
  - `NeoStep13BoxJInt` — no-op 透传
  - `NeoStep13BoxTestVectorStruct` — 纯字段无 Binder,走 Inline 通用拷贝
  - `NeoStep13BoxStructWithRefField` — 含引用字段 Inline struct,验证 ref 段回填
  - Neo 47/47 全绿

---

## [x] Task 5: Unbox / Unbox_Any handler storage-aware 分派
- **Depends On**: Task 2
- **Description**:
  - **保留单 opcode**(`Unbox` / `Unbox_Any` 共享 handler),`switch (clrType.StructStorage)`;不新增变体
  - **Handler 侧**([ILIntepreter.Neo.cs L2979-L3030](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2979)),**框架统一拷贝**:
    - `IL value type`(现有):保持不变
    - `CLR value type & Inline`:读 mStack CLR object → 类型检查 → 
      1. `CLRType.CopyPrimitiveOutOfBox(object boxed, byte* dst, int size)` 反向 helper
      2. `Unsafe.CopyBlockUnaligned` primitive 段
      3. 若 `TotalReferenceCount > 0`,循环通过 getter delegate 读 boxed 对象引用字段,写入 mStack ref 段
    - `CLR value type & Boxed`:类型检查后透传 mStack index
    - 边界:src < 0 → `NullReferenceException`;mStack[idx] 类型不匹配 → `InvalidCastException`;均在写 dst 之前抛
- **验收**:
  - `NeoStep13UnboxTestVector3` / `NeoStep13UnboxJInt` / `NeoStep13UnboxNullThrowsNRE` / `NeoStep13UnboxMismatchThrowsICE` 通过
  - Neo 51/51 全绿

---

## [x] Task 6: CLR 值类型字段访问复用 Step 12b 三态编码 + CLRType 单入口收尾
- **Depends On**: Task 3, 4, 5
- **Description**:
  - **Ldfld_* / Stfld_* 不新增 opcode 变体**:复用 Step 12b 的 `Operand4` 三态编码
  - [ILIntepreter.Neo.cs L2329-L2335](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2329-L2335) NotImpl 移除:
    - JIT emit `Ldfld_*` / `Stfld_*` 时,若 receiver `slot.Size > 4`(Inline)→ `Operand4 > 0`(inline direct 编码);若 `slot.Size == 4`(Boxed 或引用)→ `Operand4 == 0`(heap 路径)。Step 12b 逻辑已隐含此判据(基于 slot.Size / IsRef),Task 2 分派正确后 emit 侧几乎无需改动
    - Handler 侧 `Operand4 == 0` heap 路径已在 Step 12b 支持 `receiver is CLR object` → `clrType.CopyFieldToNeoFrame(...)`,Boxed CLR value type 自然走通
    - Handler 侧 `Operand4 > 0` inline direct 已支持帧字节 offset 访问,CLR Inline value type 与 IL value type 语义一致,无需区分
  - [ILIntepreter.Neo.cs L2898/L2964](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2898) `Ldfld_Value / Stfld_Value through a CLR Ref Slot`:
    - Ref Slot receiver + CLR value target:Inline → frame bytes ref;Boxed → `Unsafe.Unbox<T>` 引用后逐字段读写(此路径若非常规,可先落地 Inline 分支,Boxed 分支挂 "Step 17")
  - [CLRType.cs L552 / L575](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs#L552) `CopyValueToNeoFrame` / `ReadValueFromNeoFrame` 的 `ft.IsValueType` NotImpl:
    - `switch (StructStorage)`:Inline → 框架统一拷贝(Task 4/5 的 helper);Boxed → mStack index 回写
  - [ILIntepreter.Neo.cs L219](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L219) `CLR value type return in reflection fallback: Step 13` NotImpl 处理
  - [ILIntepreter.InvocationFrame.cs L181](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs#L181) `value-type return: Step 13` NotImpl 处理
- **验收**:
  - `NeoStep13ReadTestVector3X` / `NeoStep13WriteTestVector3Y` — Inline primitive 字段访问,`Operand4 > 0` inline direct
  - `NeoStep13ReadNestedStructField` — TestVectorStruct.C.X 嵌套 Inline 访问
  - `NeoStep13ReadJIntValue` — Boxed 字段访问(`Operand4 == 0` heap + `CopyFieldToNeoFrame` 反射 fallback)
  - `NeoStep13ReadStructWithRefField` — 含引用字段的 Inline struct 访问引用字段
  - Neo 55/55 全绿

---

## [x] Task 7: constrained.callvirt 特化 + newobj 半构造 + InvocationFrame stub
- **Depends On**: Task 6
- **Description**:
  - **constrained.callvirt**:Translate 阶段([JITCompiler.NeoHelpers.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.NeoHelpers.cs) 或 CIL→Register 转换处)识别 `Constrained.` prefix + 紧邻 `Callvirt`:
    - concrete valuetype T 且实现方法 → 直接 `Call`,this 走 Ref Slot
    - concrete valuetype T 未实现 → emit `Box T`(按 StructStorage)+ `Callvirt`
    - reference T → 忽略 prefix
    - 泛型 T → 记录 `PatchKind.ConstrainedCall`(具体落地留 Step 15;本 step concrete T 必须走通)
    - 参考 Legacy [ILIntepreter.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/ILIntepreter.cs) `Constrained` 处理
  - **newobj 半构造**:[ILIntepreter.Neo.cs L1439-L1445](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L1439-L1445) mStack 与 primitive slot 写入延后到 `InvokeNeoCallTarget` 成功返回;IL 引用 / IL 值 / CLR 三路径都修
  - **InvocationFrame primitive stub**:[ILIntepreter.InvocationFrame.cs L119-L156 / L216-L219](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs#L119) 的 `WriteInt32<T>` / `WriteInt64<T>` / `WriteSingle<T>` / `WriteDouble<T>` + 4 个 `Read*<T>`:
    - primitive/enum/IntPtr/UIntPtr T → 走 `WriteNeoPrimitive` / `ReadNeoPrimitive`
    - reference T → `throw new NotSupportedException("Reference-type generic stub reserved for Step 13b")`
  - `PushInt32<T>` 系列保留 NotSupported(Step 13b 范围)
- **验收**:
  - `NeoStep13ConstrainedTestVector3ToString` — override 直接 Call
  - `NeoStep13ConstrainedTestVectorStructToString` — 无 override 走 Box + Callvirt
  - `NeoStep13NewobjCtorThrowsPreservesDst`
  - Neo 58/58 全绿

---

## [x] Task 8: NeoStep13Test.cs 全用例 + 回归 + handoff
- **Depends On**: Task 7
- **Description**:
  - 补齐 [TestCases/NeoStep13Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep13Test.cs) 全部 Scenario 覆盖(见 checklist)
  - **Foreach 零 alloc 验证**:`foreach(int i in new List<int>{1,2,3,4,5})` per-iteration `GC.GetAllocatedBytesForCurrentThread()` 采样;List<int>.Enumerator 无 Binder 有方法 → Boxed,`Unsafe.Unbox<T>` in-place 期望 per-iter alloc = 0(但依赖 CLRBinding 代码生成器改造,Step 15 前可能仍有 alloc → `[Ignore("Step 15 binding generator required")]` 挂账,不阻塞 Step 13)
  - Neo 全量(预期 ≥ 58)全绿
  - Legacy 全量 493/493 无回归
  - `-c Debug` / `-c Debug_Neo` 双配置 0 错误
  - 撰写 [.trae/specs/implement-neo-step13/handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step13/handoff.md):
    - 落地:`StructStorage` 编译期分派、CLRType 结构布局属性、Inline 允许含引用字段与 IL 值类型对称、6 组 opcode 变体、newobj 半构造修复、constrained. 特化
    - 挂账 → Step 13b:InvocationContext / DelegateAdapter / CLRRedirections 反射入口迁移
    - 挂账 → Step 15:CLRBinding 代码生成器(`Unsafe.Unbox<T>` 模式 + 消除 WriteBackInstance),foreach 零 alloc 验收
    - 挂账 → Step 17:Ldfld_Value / Stfld_Value 通过 CLR Ref Slot 的 Boxed 分支若未闭环
    - 挂账 → Step 18:CLR 值类型 newobj 的 Ref Slot 传 this
    - ECMA-335 合规回顾:III.4.31 / III.4.32 / III.4.33 / II.14.4.2 / III.4.21

---

# Task Dependencies(汇总)
- Task 2 depends on Task 1
- Task 3, 4, 5 depend on Task 2
- Task 6 depends on Task 3, 4, 5
- Task 7 depends on Task 6
- Task 8 depends on Task 7
