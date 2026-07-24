# ILRuntime Neo Step 13B — ILType 嵌套 CLR Inline 布局递归 Spec

## Why

Step 13 已为 CLR 值类型建立 `StructStorage` 与 Inline/Boxed 布局，但 `ILType.InitializeFieldsForFlatLayout` 目前只对嵌套 `ILType` 展开布局。IL 类型包含 CLR Inline struct 时，外层 frame 的 primitive size、自然对齐和 mStack 引用段会被错误计算，导致字段访问、整体复制以及 Box/Unbox 的布局不一致。

本步骤补齐单向的 `ILType -> CLRType` 布局递归，使 IL frame 的布局元数据与已缓存的 CLR Inline 布局保持一致。

## What Changes

- `ILType.InitializeFieldsForFlatLayout` 的字段尺寸、自然对齐和引用槽计算支持 CLR `Inline` value type。
- CLR `Boxed` value type 和 CLR reference type 继续按单个 mStack 引用形态布局，不得被当作 Inline struct 展开。
- 实例字段和静态字段使用相同的 CLR Inline/Boxed 分类规则，保持 primitive/ref offset 计算对称。
- 嵌套 CLR Inline struct 的 `TotalPrimitiveSize`、`TotalReferenceCount`、字段基础 offset 递归累加。
- 校验 IL value type 的 Box/Unbox、`Ldfld_Value`/`Stfld_Value`、`Move_Vt` 和整体复制路径使用修正后的布局元数据。
- 新增 Neo Step 13B 测试，覆盖 CLR Inline primitive struct、带引用字段的 CLR Inline struct、自然对齐和嵌套 Box/Unbox 往返。

## Non-Goals

- 不建立 `CLRType -> ILType` 的反向字段递归模型；CLR 字段中的热更对象仍按引用形态处理。
- 不实现 CLR 值类型 `newobj`、实例方法分派、数组元素访问、异常处理或 Ref Slot 新语义。
- 不迁移 `InvocationContext.Invoke`、`DelegateAdapter.ILInvokeSub` 或 `CLRRedirections.MethodInfoInvoke` 等 Step 13b 外部入口。
- 不新增 opcode 或修改 `StructStorage` 的分类规则。

## Impact

- Affected specs: `implement-neo-step13` 中转移到 Step 13B 的布局递归验收项。
- Affected code:
  - `ILRuntime/CLR/TypeSystem/ILType.cs`
  - `ILRuntime/CLR/TypeSystem/MemoryLayoutHelpers.cs`（仅在共享尺寸/对齐规则确有缺口时调整）
  - `ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs`（仅修复嵌套布局复制所需的递归消费）
  - `TestCases/NeoStep13BTest.cs`
  - `ILRuntimeTestBase/TestFramework/` 中用于验证 CLR Inline 引用字段的最小 CLR struct fixture

## Layout Contract

对 ILType 的每个字段，布局规则必须按以下顺序判断：

| 字段类型 | primitive 段尺寸/对齐 | reference 段 |
|---|---:|---:|
| CLR primitive / enum | `MemoryLayoutHelpers` 的自然尺寸/对齐 | `0` |
| 嵌套 IL value type | 子类型 `TotalPrimitiveSize` / 最大自然对齐 | 子类型 `TotalReferenceCount` |
| CLR value type 且 `StructStorage == Inline` | CLRType `TotalPrimitiveSize` / `MaxAlignment` | CLRType `TotalReferenceCount` |
| CLR value type 且 `StructStorage == Boxed` | `4` 字节 mStack index，按 4 字节对齐 | `1` |
| CLR reference type | `4` 字节 mStack index，按 4 字节对齐 | `1` |

字段的 primitive offset 先按自然对齐向上取整，再记录该字段的 reference offset，最后递增对应的 primitive size 和 reference count。外层 ILType 的最终 primitive size 仍按自身字段最大对齐向上取整；空布局保持现有最小尺寸约束。

## ADDED Requirements

### Requirement: CLR Inline 字段尺寸与对齐递归

`ILType.InitializeFieldsForFlatLayout` SHALL resolve CLR value-type fields through `CLRType.StructStorage`. For `Inline`, it SHALL use `CLRType.TotalPrimitiveSize` and `CLRType.MaxAlignment`; it SHALL NOT use a fixed 4-byte fallback or CLR object pointer size.

#### Scenario: IL struct contains CLR primitive Inline struct

- **WHEN** an IL value type contains a CLR Inline struct field followed by a primitive tail field
- **THEN** the CLR field occupies its complete primitive payload, the tail field is aligned after that payload, and no reference slot is added when the CLR struct has no references

#### Scenario: Nested CLR Inline struct with natural alignment

- **WHEN** an IL value type contains a CLR Inline struct whose maximum alignment is greater than the preceding field
- **THEN** the field primitive offset and outer total size include the required natural-alignment padding

### Requirement: CLR Inline 引用槽递归

For a CLR Inline field, `ILType` SHALL add exactly `CLRType.TotalReferenceCount` reference slots and preserve their order after the field's recorded reference offset. Nested CLR Inline reference fields SHALL remain rooted in the Neo `mStack` reference region; their raw managed pointers SHALL never be written into primitive bytes.

#### Scenario: Nested CLR Inline struct contains references

- **WHEN** an IL value type contains a CLR Inline struct with primitive and reference fields
- **THEN** the outer field's reference offset is the current ref-region cursor, the cursor advances by the nested CLR count, and subsequent reference fields do not overlap those slots

#### Scenario: CLR Boxed struct remains one reference

- **WHEN** an IL value type contains a CLR value type classified as `Boxed`
- **THEN** the field uses a 4-byte mStack index plus exactly one reference slot; the boxed struct's internal fields are not flattened into the outer IL frame

### Requirement: 实例字段与静态字段布局对称

The same CLR Inline/Boxed mapping SHALL be used for instance and static IL fields. `fieldOffsets` and `staticFieldOffsets` SHALL advance primitive and reference cursors identically for equivalent field types.

#### Scenario: Static CLR Inline field

- **WHEN** an IL type declares a static field whose type is a CLR Inline struct
- **THEN** static primitive size, alignment, and reference count match the corresponding instance-field calculation, and `Ldsfld`/`Stsfld` can address the complete nested payload

### Requirement: Offset metadata remains composable

The outer `ILTypeFieldOffset` SHALL identify the beginning of the nested field's primitive payload and reference range. Nested field access SHALL compose the outer base offset with the nested type's own offset metadata; no per-access field-name lookup or new wrapper API is permitted.

#### Scenario: Nested field access

- **WHEN** JIT lowering accesses a field inside a CLR Inline field nested in an IL value type
- **THEN** the generated primitive and reference offsets point into the outer frame's corresponding segments and do not address the nested field as a boxed object

### Requirement: Layout consumers remain symmetric

Box, Unbox, `Ldfld_Value`, `Stfld_Value`, `Move_Vt`, and whole-value copy paths SHALL consume the same `TotalPrimitiveSize` and `TotalReferenceCount` metadata. A round trip SHALL preserve primitive values, null references, and non-null references.

#### Scenario: Nested CLR Inline Box/Unbox round trip

- **WHEN** an IL value containing a CLR Inline struct is boxed and then unboxed
- **THEN** all nested primitive fields and mStack-rooted references equal the original values, including fields after the nested CLR payload

#### Scenario: Move_Vt preserves nested references

- **WHEN** a value containing a nested CLR Inline struct is copied with `Move_Vt`
- **THEN** primitive bytes and every reference slot are copied to the destination without aliasing or slot overlap

### Requirement: Layout initialization is deterministic and cycle-safe

Layout initialization SHALL use the existing type initialization guards and deterministic field ordering. It SHALL not introduce recursive CLR-to-IL type construction or unbounded re-entry when resolving nested metadata.

#### Scenario: Repeated metadata query

- **WHEN** `TotalPrimitiveSize`, `TotalReferenceCount`, or field offsets are queried repeatedly
- **THEN** the cached layout is reused and the result is stable across queries and JIT compilations

## MODIFIED Requirements

### Requirement: ILType flat-layout field classification

The existing `ILType.InitializeFieldsForFlatLayout` requirement is modified so that CLR fields are no longer handled solely by the default reference fallback. It SHALL distinguish CLR Inline, CLR Boxed, and CLR reference fields according to the Layout Contract while preserving all existing ILType, primitive, enum, and legacy behavior.

## REMOVED Requirements

None.

