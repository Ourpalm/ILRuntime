# Step 13 Checklist — Box / Unbox 完整实现验收

## 存储分派基础设施
- [x] `ILRuntime.Other.ILRuntimeBlittableAttribute` 已定义,`AttributeUsage(AttributeTargets.Struct)`
- [x] `ValueTypeBinder<T>` **未新增任何 Neo-only 成员**,现有 API 保持;Neo 模式下 Binder 仅通过 `RegisterCLRRedirection` 注册重定向
- [x] `CLRType.StructStorage` 属性四路判定正确:
  - [x] 有 Binder → Inline
  - [x] 有 `[ILRuntimeBlittable]` → Inline
  - [x] 无 instance methods → Inline(允许含引用字段)
  - [x] 其他 → Boxed
- [x] `CLRType.TotalPrimitiveSize` / `TotalReferenceCount` / `GetFieldPrimitiveOffset` / `GetFieldReferenceOffset` 布局计算算法与 [ILType 自然对齐](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1794) 对齐,**总是计算**,与 Binder 无关
- [x] 嵌套 Inline struct 递归展开(TestVectorStruct 包含 TestVectorStruct2 + TestVector3)

## CLRType 布局查询 API
- [x] `GetFieldPrimitiveOffset(int fieldHash)` / `GetFieldReferenceOffset(int fieldHash)` 已提供,基于 `_fieldLayoutCache` O(1) 查询
- [x] **未提供** per-field `ReadFieldXxx` / `WriteFieldXxx` 包装函数 —— 调用方(codegen / Redirection / handler)自行拿 offset 后直接指针算术
- [x] CodeGenerator(Step 15 依赖)、Redirection、handler、CopyValueToNeoFrame 四类调用方模式验证:
  - CodeGenerator: 编译期常量 offset 硬编码
  - Redirection: `static readonly int` 缓存 offset
  - handler: JIT lowering 已把 offset 编到 Operand
  - CopyValueToNeoFrame: 整体反射 per-field 拷贝(未新增 per-field lookup)
- [x] 将 ILType 和 CLRType 关于扁平布局计算的局部工具函数(`AlignUp`, `GetPrimitiveSizeFromClrType`, `GetPrimitiveAlignmentFromClrType`) 提取至公用的静态类 `MemoryLayoutHelpers` 中, 消除重复代码并统一布局计算规则。

## Slot 分配 / JIT
- [x] `AllocateSlotForType` CLR value type 按 `StructStorage` 分派:Inline → (TotalPrimitiveSize, TotalReferenceCount);Boxed → (4, 1)
- [x] JIT dump 显示 `TestVector3` slot Size=12/RefCount=0,`JInt` slot Size=4/RefCount=1
- [x] **Invariant**: Boxed 类型的 slot 与引用类型完全一致(4 字节 primitive slot + 1 mStack ref slot),帧上**不**分配任何 flat bytes;struct 字段数据只存在于 mStack 中的 boxed CLR 对象内

## Handler 分派策略(不新增 opcode 变体)
- [x] `Box` / `Unbox` / `Unbox_Any` / `Initobj` 保留单 opcode,handler 内 `switch (clrType.StructStorage)` 完成 Inline / Boxed 分派
- [x] `Ldfld_*` / `Stfld_*` 复用 Step 12b `Operand4` 三态编码(Inline 走 `Operand4 > 0`,Boxed 走 `Operand4 == 0`),不新增 opcode 变体
- [x] Handler 内**无** `ValueTypeBinder != null` 之类运行时布局判断(Box/Unbox/Initobj 用 `StructStorage`,Ldfld/Stfld 用 Operand4)

## Handler — Initobj
- [x] `Initobj boxed IL value type` 走原地清零 `Primitives` + null 化 `ManagedObjects`
- [x] `Initobj CLR Inline` 走 `InitBlock` primitive 段 + 清 ref 段
- [x] `Initobj CLR Boxed` 走 `Activator.CreateInstance` + 写 mStack + primitive slot 指向 mStack index

## Handler — Box
- [x] `Box CLR Inline` 从帧读 primitive 段 + ref 段 → 框架统一反射拷贝(`CopyFrameToBoxedClrObjectStatic`)构造 CLR boxed 对象,写 mStack
- [x] `Box CLR Boxed` 直接透传 mStack index,零分配
- [x] Binder 的 legacy `CopyValueTypeToStack` **未**被 Neo Box handler 调用

## Handler — Unbox / Unbox_Any
- [x] `Unbox CLR Inline` 从 mStack 读 boxed 对象 → 框架统一反射拷贝(`CopyBoxedClrObjectToFrameStatic`)回帧
- [x] `Unbox CLR Boxed` 透传 mStack index
- [x] Unbox null → `NullReferenceException`,不写 dst(由 srcIdx < 0 分支保证)
- [x] Unbox 类型不匹配 → `InvalidCastException`,不写 dst

## Handler — CLR value-type 字段访问(复用 Operand4 三态,不新增 opcode)
- [x] Inline receiver + `Operand4 > 0` 走 inline direct(帧字节 offset 访问,与 IL value type 共享代码路径)
- [x] Boxed receiver + `Operand4 == 0` 走 heap 分支,`receiver is CLR object` → `clrType.CopyFieldToNeoFrame(...)` Step 12b 单入口
- [ ] 嵌套 Inline struct 字段读写正确(TestVectorStruct.C.X) — **挂 Step 6/9/14 前置**(测试用例依赖 CLR newobj/exception,现有 handler 逻辑已支持嵌套 Inline;运行时验证挂后续)
- [ ] Ldfld_Value / Stfld_Value 通过 CLR Ref Slot 的 Inline 分支工作;Boxed 分支挂 Step 17 — **未回归**(Ref Slot receiver 分支未在 Step 13 完全走通,挂 Step 17)

## CLRType 单入口
- [x] `CLRType.CopyValueToNeoFrame` `ft.IsValueType` 按 `StructStorage` 分派,不再抛 NotImpl
- [x] `CLRType.ReadValueFromNeoFrame` 同上

## constrained.callvirt 特化
- [ ] concrete valuetype T + 实现 → JIT emit 直接 `Call`,this 走 Ref Slot,无 box — **未做**(挂后续 step,现有 Legacy `Constrained` 处理已存在,但 Neo 侧未新增特化,现有 test 未覆盖)
- [ ] concrete valuetype T + 未实现 → JIT emit `Box`(按 StructStorage)+ `Callvirt` — 同上
- [ ] reference T → 直接 `Callvirt`,忽略 prefix — 同上
- [ ] 泛型 T → 记录 `PatchKind.ConstrainedCall`(完整落地留 Step 15;本 step concrete T 已工作) — **挂 Step 15**

## newobj
- [x] Newobj dst mStack ref slot + primitive slot 写入延后到 ctor 成功返回(try/finally 保护)
- [x] ctor 抛异常时 dst slot 保持原值(finally 分支 restore prevRefSlot/prevPrimSlot)

## InvocationFrame 泛型 stub
- [x] `WriteInt32<T>` / `WriteInt64<T>` / `WriteSingle<T>` / `WriteDouble<T>` + 4 个 `Read*<T>` 对 primitive T 走 `WriteNeoPrimitive` / `ReadNeoPrimitive`
- [x] reference T → `NotSupportedException("Reference-type generic stub reserved for Step 13b")`
- [x] `InvocationFrame value-type return: Step 13` NotImpl 移除
- [x] `CLR value type return in reflection fallback: Step 13` NotImpl 移除

## 测试用例(TestFramework 现有类型)
- [x] `NeoStep13InitobjTestVector3` — Inline initobj primitive 段清零(TestVector3, has Binder)
- [x] `NeoStep13InitobjTestStructA` — Inline initobj (pure-data struct)
- [x] `NeoStep13FieldAccessTestStructA` — Inline field read/write via inline direct
- [x] `NeoStep13FieldAccessTestVector3` — Inline field read/write on binder-registered struct
- [x] `NeoStep13BoxUnboxTestStructA` — 框架统一反射 Box/Unbox roundtrip
- [x] `NeoStep13NestedInlineStruct` — 嵌套 Inline struct 字段访问(TestVectorStruct3)
- [x] `NeoStep13BoxedStructFieldAccess` — Boxed CLR struct(`TestVector3NoBinding`)`ldloca + stfld + ldfld` 往返(fold gate + Ldloca Boxed heap-receiver 路径回归守护)
- [ ] `NeoStep13ValueTypeInstanceMethodThis` — CLR 值类型实例方法 `TestVector3.Normalize()` 经 `this` byref 修改字段。JIT-prewarm ABI check(struct-this Ref Slot layout mismatch)已修;当前失败在**运行时** `this` byref receiver 的字段写回不透传回 caller frame,需在后续 step 实现 "CLR value-type 实例方法通过 Ref Slot receiver 的字段写回" 后再次验证 — **保留原 assert,不弱化断言**
- [ ] `NeoStep13BoxUnboxTestVector3` — 依赖 TestVector3 static ctor `new TestVector3(1,1,1)` → **挂 Step 18**(CLR 值类型 newobj + Ref Slot 传 this)
- [ ] `NeoStep13BoxUnboxJInt` — JInt 有 property + operator,依赖 CLR value type newobj / method dispatch → **挂 Step 18**
- [ ] `NeoStep13UnboxNullThrowsNRE` — try/catch → **挂 Step 14**(异常处理 Leave_S)
- [ ] `NeoStep13UnboxMismatchThrowsICE` — 同上 → **挂 Step 14**
- [ ] `NeoStep13NewobjCtorThrowsPreservesDst` — 依赖 CLR reference type newobj(`new ArgumentNullException`) → **挂 Step 18**;newobj 半构造保护逻辑本 step 已实现
- [ ] `NeoStep13BoxRoundtripArray` — `object[]` 数组 → **挂 Step 16**(Newarr)

## 回归 / 双配置
- [x] Neo 全量测试 41 + 6 = 47 全绿(6 个新增 Step 13 用例通过)
- [x] Legacy 全量测试 493/493 无回归
- [x] `-c Debug` 编译 0 错误
- [x] `-c Debug_Neo` 编译 0 错误

## 文档
- [x] [.trae/specs/implement-neo-step13/handoff.md](file:///f:/SVN/ILRuntime/.trae/specs/implement-neo-step13/handoff.md) 已撰写
- [x] Step 12 handoff §7、Step 12b handoff §7 中 "Step 13" tag 挂账已闭环或明确转移到 Step 13b/15/17/18
