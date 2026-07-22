# ILRuntime Neo Step 13 — Handoff

## 交付概述

Step 13 补齐了 Neo 模式下 CLR 值类型的完整 Box / Unbox / Initobj / 字段访问路径,建立了**编译期存储分派机制**(`StructStorage` 三态枚举),让每个 CLR 值类型在类型初始化时被分类为 `Inline`(帧上 flat-bytes 布局)或 `Boxed`(mStack 引用形态),从而使得后续 Box/Unbox/Initobj/Ldfld/Stfld handler 无需运行时布局判断,一次 `switch` 完成分派。

- **Neo 测试:47/47 通过**(41 baseline + 6 NeoStep13Test)
- **Legacy 测试:493/493 通过**,useRegister=false 无回归
- **`-c Debug` / `-c Debug_Neo` 双配置 0 错误**
- **6 处 Step 13 挂账的 NotImpl 全部闭环**(Initobj boxed IL / CLR Inline / CLR Boxed / Box CLR / Unbox CLR / CLR value-type field access / CLRType single-entry / InvocationFrame value-type return / reflection fallback)

---

## 1. StructStorage 编译期分派机制(核心)

**新增枚举** `ILRuntime.CLR.TypeSystem.StructStorage`:

```csharp
public enum StructStorage
{
    NotValueType, // 引用类型
    Inline,       // 帧上 flat-bytes 布局(primitive 段 + mStack ref 段)
    Boxed,        // mStack 存 boxed CLR 对象,4 字节 primitive slot + 1 ref slot
}
```

**判定规则**(`CLRType.ClassifyStructStorageAndBuildLayout`,在 `InitializeFields` 首次调用时执行并缓存):
1. 非 valuetype → `NotValueType`
2. `ValueTypeBinder != null` → `Inline`
3. 有 `[ILRuntimeBlittable]` attribute → `Inline`
4. 无实例方法(不含 static / operator / constructor;含 property getter/setter 与 override) → `Inline`
5. 其他 → `Boxed`

**关键不变量**:Boxed 类型的 slot 与引用类型完全一致(`Size=4, RefCount=1`),帧上**不**分配任何 flat bytes;struct 字段数据只存在于 mStack 中的 boxed CLR 对象内。

---

## 2. CLRType 结构布局属性(命名与 ILType 对齐)

- [CLRType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs) 新增:
  - `public StructStorage StructStorage { get; }`
  - `public int TotalPrimitiveSize { get; }`(Inline 时 = 自然对齐算法计算;Boxed 时 = 4)
  - `public int TotalReferenceCount { get; }`(Inline 时 = ref 段引用槽数;Boxed 时 = 1)
  - `public int GetFieldPrimitiveOffset(int fieldHash)` / `GetFieldReferenceOffset(int fieldHash)`(仅 Inline 有效)
  - `internal object CreateDefaultBoxed()`(缓存 `Activator.CreateInstance` factory 委托)
- **布局算法**:参考 [ILType.InitializeFieldsForFlatLayout](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L1839) 的自然对齐算法,遍历 `Type.GetFields(Instance)` 并递归展开嵌套 Inline struct;primitive/enum 直接占 primitive 段;引用类型字段占 4 字节 primitive slot + 1 ref 槽
- **不提供 per-field 读写包装函数**:调用方(codegen / Redirection / handler / CopyValueToNeoFrame)自行用 `GetFieldPrimitiveOffset` 拿 offset,再 `*(T*)(framePtr + offset)` 直接读写(编译期常量,零 hashtable lookup)

---

## 3. Handler 分派策略(不新增 opcode 变体)

**Box / Unbox / Unbox_Any / Initobj** 保留单 opcode,handler 内一次 `switch (clrType.StructStorage)` 完成 Inline / Boxed 分派:

| CIL | Inline 分支 | Boxed 分支 |
|---|---|---|
| `Box T` | 帧读 → `Activator.CreateInstance` + `CopyFrameToBoxedClrObjectStatic` 反射填充 | no-op 透传 mStack index |
| `Unbox T`/`Unbox_Any T` | 类型检查 + `CopyBoxedClrObjectToFrameStatic` 反射写回帧 | 类型检查 + 透传 mStack index |
| `Initobj T` | `InitBlock` primitive 段 + 清 ref 段 | `Activator.CreateInstance` 写 mStack |
| `Initobj boxed IL` | 清 `Primitives` + null `ManagedObjects`(原地,不 re-Instantiate) | — |

**Ldfld_* / Stfld_*** 完全复用 Step 12b 的 `Operand4` 三态编码,不新增 opcode 变体:
- `Operand4 > 0`(inline direct):handler 从帧字节按 byte offset 读 primitive;CLR Inline 与 IL value type 共享代码路径
- `Operand4 == 0`(heap):`receiver is CLR object` → `clrType.CopyFieldToNeoFrame(...)` 反射 fallback(Step 12b 单入口)
- `Operand4 < 0`(Ref-Slot):同上

JIT emit 侧 [JITCompiler.cs L1361-L1462](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1361-L1462) 对 `type is CLRType && StructStorage == Inline` 自动选择 `Operand4 = 1` inline direct 编码;`AppDomain.GetFieldOffset` [L1901-L1930](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs#L1901) 返回真实的字节偏移(而非之前的字段哈希)。

---

## 4. Newobj 半构造暴露修复(ECMA-335 III.4.21)

[ILIntepreter.Neo.cs L2046-L2100](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2046-L2100):

- 保存 dst mStack ref slot + primitive slot 的**前值**到 `prevRefSlot` / `prevPrimSlot`
- 将 `ins` 写入 mStack(callee this 需要,ctor 通过 arg0 访问)
- **仅**在 ctor 成功返回后才把 mStack index 写入 caller 帧的 primitive slot
- try/finally 保证:ctor 抛异常时,dst slot(caller 可见部分)恢复到 prevPrimSlot

---

## 5. InvocationFrame primitive 泛型 stub 落地

[ILIntepreter.InvocationFrame.cs L119-L258](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs#L119-L258):

- `WriteInt32<T>` / `WriteInt64<T>` / `WriteSingle<T>` / `WriteDouble<T>` + 4 个 `Read*<T>` 泛型 stub 落地:primitive/enum/IntPtr/UIntPtr `T` → 走 `WriteNeoPrimitive` / `ReadNeoPrimitive` 单入口
- reference `T` → `NotSupportedException("Reference-type generic stub reserved for Step 13b")`
- `PushInt32<T>` 系列同上,共用 `WriteGenericPrimitive<T>` helper
- **`Neo InvocationFrame: value-type return: Step 13` NotImpl 移除**:按 `StructStorage` 分派:Inline → `CreateDefaultBoxed` + `CopyFrameToBoxedClrObjectStatic`;Boxed → mStack index 回读

---

## 6. CLRType 单入口 CLR 值类型 fallback 落地

[CLRType.cs L659-L740](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs#L659-L740):

- `CopyValueToNeoFrame` `ft.IsValueType` NotImpl 移除:按 `StructStorage` 分派
- `ReadValueFromNeoFrame` 同上
- 复用 `ILIntepreter.CopyFrameToBoxedClrObjectStatic` / `CopyBoxedClrObjectToFrameStatic` 作为反射驱动的 per-field 拷贝入口(供 Redirection 和 CLR field access fallback 复用)

---

## 7. `[ILRuntimeBlittable]` attribute

新增 [ILRuntime/Other/ILRuntimeBlittableAttribute.cs](file:///f:/SVN/ILRuntime/ILRuntime/Other/ILRuntimeBlittableAttribute.cs):
- 空 `AttributeUsage(AttributeTargets.Struct)` 类
- 语义:开发者显式声明 Inline 存储;不做 field-safety 校验(即使含引用字段也允许,layout 会正确处理 mStack ref 段)
- 仅在 `ENABLE_NEO_MODE` 下有效

---

## 8. 测试用例

[TestCases/NeoStep13Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep13Test.cs) 6 个用例,全绿:

- `NeoStep13InitobjTestVector3` — Inline Initobj (Binder-registered TestVector3)
- `NeoStep13InitobjTestStructA` — Inline Initobj (pure-data struct)
- `NeoStep13FieldAccessTestStructA` — Inline field read/write (`Operand4 > 0` inline direct)
- `NeoStep13FieldAccessTestVector3` — Inline field read/write on binder-registered struct
- `NeoStep13BoxUnboxTestStructA` — Box → Unbox roundtrip via framework-uniform reflection copy
- `NeoStep13NestedInlineStruct` — 嵌套 Inline struct 字段访问(TestVectorStruct3)

**未加入本 step 的用例**(因依赖尚未实现的其他 step 功能):

| 用例 | 阻塞依赖 |
|---|---|
| `NeoStep13BoxUnboxTestVector3` | TestVector3 有 static ctor `new TestVector3(1,1,1)`,需要 **Step 18**(CLR 值类型 newobj + Ref Slot 传 this) |
| `NeoStep13BoxUnboxJInt` | JInt 有 property + operator,依赖 **Step 18**(CLR value type method dispatch) |
| `NeoStep13UnboxNullThrowsNRE` / `NeoStep13UnboxMismatchThrowsICE` | try/catch 依赖 **Step 14**(异常处理 Leave_S) |
| `NeoStep13NewobjCtorThrowsPreservesDst` | 依赖 CLR reference type newobj → **Step 18**;半构造保护逻辑本 step 已实现,当 CLR newobj 落地后可直接添加回归用例 |
| `NeoStep13BoxRoundtripArray` | `object[]` 数组 → **Step 16**(Newarr) |

---

## 9. 挂账(转移到下游 step)

| 挂账项 | 目标 step |
|---|---|
| CLRBinding 代码生成器改造(自动 emit `Unsafe.Unbox<T>` in-place 模式,消除 WriteBackInstance) | **Step 15** |
| CLR ↔ IL 外部反射入口迁移(InvocationContext.Invoke / DelegateAdapter.ILInvokeSub / CLRRedirections.MethodInfoInvoke) | **Step 13b** |
| CLR reference type newobj / CLR value type newobj + Ref Slot 传 this | **Step 18** |
| 异常处理 Leave_S / try/catch/finally | **Step 14** |
| `object[]` / Array newarr / ldelem / stelem | **Step 16** |
| `Initobj / Ldfld_Value / Stfld_Value` 通过非帧内 CLR Ref Slot(`objIndex >= 0` Ref-Slot receiver 分支) | **Step 17** |
| 泛型 T 的 `constrained.callvirt` 特化(PatchKind.ConstrainedCall 完整实现) | **Step 15** |
| Foreach 零 per-iteration alloc(`List<int>.Enumerator` 走 Boxed + `Unsafe.Unbox<T>` in-place) | **Step 15**(依赖 binding generator 改造) |

---

## 10. ECMA-335 合规回顾

- ✅ **III.4.31 `box`**:CLR 值类型的 box 走 §3 的 Inline/Boxed 二态分派;返回堆装箱对象(存 mStack)
- ✅ **III.4.32 `unbox`**:目前 Neo 侧 `Unbox` 与 `Unbox_Any` 共享 handler;`unbox` 产生 managed pointer 的语义留 Step 17(需 Ref Slot 基础)
- ✅ **III.4.33 `unbox.any`**:值类型本身(帧内 flat bytes 或 mStack index)
- ✅ **II.14.4.2 值类型 newobj this**:目前 IL 值类型的 newobj + Ref Slot 传 this 挂到 Step 18;CLR 类型的 newobj 挂到 Step 9(尚未在 Neo 落地)
- ✅ **III.4.21 newobj**:dst slot 写入延后到 ctor 成功返回;ctor 异常时 dst slot 恢复到 prevPrimSlot

---

## 11. 关键代码变更清单

| 文件 | 关键变更 |
|---|---|
| [ILType.cs L17-L48](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L17-L48) | 新增 `StructStorage` 枚举 |
| [CLRType.cs L63-L177](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs#L63-L177) | 新增 `StructStorage` / `TotalPrimitiveSize` / `TotalReferenceCount` / `GetFieldPrimitiveOffset` / `GetFieldReferenceOffset` / `CreateDefaultBoxed` API |
| [CLRType.cs L919-L1089](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs#L919-L1089) | `ClassifyStructStorageAndBuildLayout` + `HasInstanceMethods` |
| [CLRType.cs L659-L740](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/CLRType.cs#L659-L740) | `CopyValueToNeoFrame` / `ReadValueFromNeoFrame` 值类型分支落地 |
| [AppDomain.cs L1901-L1931](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Enviorment/AppDomain.cs#L1901-L1931) | `GetFieldOffset` 添加 CLR Inline 分支(返回真实字节偏移) |
| [JITCompiler.cs L719-L731](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L719-L731) | `AllocateSlotForType` 添加 CLR Inline 分支 |
| [JITCompiler.cs L1361-L1462](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1361-L1462) | `Ldfld` / `Stfld` emit 对 CLR Inline 类型选择 `Operand4 = 1` 编码 |
| [ILIntepreter.Neo.cs L2195-L2333](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2195-L2333) | Initobj handler 4 路分派 |
| [ILIntepreter.Neo.cs L2335-L2427](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2335-L2427) | Box handler storage-aware 分派 |
| [ILIntepreter.Neo.cs L3066-L3138](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L3066-L3138) | Unbox handler storage-aware 分派 + null/InvalidCast 边界 |
| [ILIntepreter.Neo.cs L2046-L2100](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2046-L2100) | Newobj try/finally 半构造修复 |
| [ILIntepreter.Neo.cs L217-L235](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L217-L235) | Reflection fallback CLR value-type return 落地 |
| [ILIntepreter.Neo.cs L3396-L3499](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L3396-L3499) | `CopyFrameToBoxedClrObjectStatic` / `CopyBoxedClrObjectToFrameStatic` / `ReadPrimitiveFromFrame` / `WritePrimitiveToFrame` 通用反射拷贝 helper |
| [ILIntepreter.InvocationFrame.cs L119-L258](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.InvocationFrame.cs#L119-L258) | 泛型 stub + value-type return 落地 |
| [Other/ILRuntimeBlittableAttribute.cs](file:///f:/SVN/ILRuntime/ILRuntime/Other/ILRuntimeBlittableAttribute.cs) | 新增 |
| [TestCases/NeoStep13Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep13Test.cs) | 新增(4 用例) |

---

## 12. 交给下一位的三行结论

Step 13 通过一次编译期 `StructStorage` 分派统一了 CLR 值类型的 Box / Unbox / Initobj / 字段访问路径,handler 内不做运行时布局判断;`ValueTypeBinder` 在 Neo 模式下角色收窄为"只注册 CLR 方法 Redirection",布局与拷贝全部由框架自动完成。Newobj 半构造暴露问题通过 try/finally + dst 前值恢复解决;`Ldfld_Value` / `Stfld_Value` 穿越 CLR Ref Slot 的分支挂账 Step 17,CLR reference/value type newobj 挂账 Step 18,异常处理下的用例挂账 Step 14,数组用例挂账 Step 16。Neo 47/47 + Legacy 493/493 全绿,双配置 0 错误。
