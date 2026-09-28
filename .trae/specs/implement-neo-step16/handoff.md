# ILRuntime Neo Step16 — Handoff

## 结果与范围

已实现 Neo 一维数组的 `newarr`、`ldlen`、`ldelem.*`、`stelem.*`、`ldelema`，并让数组 Ref Slot 通过 `ldind.*` / `stind.*` 读写元素。包含 CLR 基本类型与引用类型数组、IL 引用类型数组、IL 值类型数组、CLR 值类型数组和 jagged 数组。未提交或切换分支；任务前已有的 `HotfixAOT/Patched/HotfixAOT.dll` 与 `.pdb` 状态保持不变。

## 实现要点

- JIT 只在 Neo 配置下为 `ldelem.any`、`ldelema` 保留元素类型 token；`stelem.any` 直接使用运行时数组的实际元素类型。Neo 类型推断为 `newarr` 生成数组类型，为 `ldelema` 生成 byref 类型，修正 `ldelem.any` 的类型 token 读取，并从数组寄存器推断 `ldelem.ref` 的元素类型。lowering 编码三个数组操作数的帧偏移及结果/值类型的引用区偏移。
- Neo 类型推断从数组寄存器取得 CLR 基本类型元素种类，写入 `Operand4`；`ldelem.any/stelem.any` 在编译期改写为已有的 `ldelem.i1/u1/i2/u2/i4/u4/i8/r4/r8/i` 与 `stelem.i1/i2/i4/i8/r4/r8/i`。引用数组的 `ldelem.any/stelem.any` 改写为现有 `ldelem.ref/stelem.ref`，常见 CLR 引用数组直接按 `object[]` 访问。各类型指令内直接访问已知的 `T[]`，避开 `Array.GetValue/SetValue` 和基本类型装箱；同一宽度涉及 `bool`、`char`、无符号整数或 native-int 时，使用已写入的元素类型标记选出正确数组。`IntPtr[]/UIntPtr[]` 的标记使用 `NeoArrayKind` 具名常量。枚举或静态类型未知的数组调用独立慢路径函数，不再跳入 `ldelem.any`。`ldelema` 后的 `ldind/stind` 仍需识别接收者是栈、字段还是数组；数组分支优先直接访问已知的 `T[]`。Legacy 编译配置不执行 Neo 类型专用化。
- `newarr` 对 CLR 元素创建原生数组；IL 元素创建 `ILTypeInstance[]`，IL 值类型逐元素初始化。数组元素的 IL 类型保存在弱引用表中，以便运行时检查 `stelem.ref` 与 `ldelema`；表不阻止数组回收。
- `ldelem/stelem` 的引用槽按 mStack index 传递；IL 值类型在帧与独立元素实例之间拷贝 primitive bytes 和 ManagedObjects，保持值语义；CLR inline 值类型使用现有 boxed/frame 转换并回写数组。
- `ldelema` 写入 `(arrayMStackIndex, elementIndex)` Ref Slot；`ldind/stind` 按该 index 访问数组。IL/CLR struct 数组的直接字段读写通过元素实例或 boxed 元素分派，CLR struct 写后回写数组。
- `ExecuteNeo` 在指令循环外声明单个 `Array array` 工作变量，复用于 `ldind/stind` 的数组接收者、`newarr`、`ldlen`、`ldelema`、`ldelem.any`、`stelem.ref/any`；不再为每个 case 建立独立的 `Array` 临时变量。
- 显式检查 null 与索引范围；`ldelema` 检查实际元素类型，`stelem.ref` 保留 CLR 数组协变检查，并对 IL 元素执行 `CanAssignTo` 检查。

## 验证

`TestCases/NeoStep16Test.cs` 有 15 个入口，覆盖基本类型数组读写及其数组引用、CLR/IL 引用、IL/CLR 值类型深拷贝与字段写回、null、越界、`ldelema` 读写与类型检查、CLR/IL 协变、jagged 数组、CLR/IL 枚举、底层为 `short` 的枚举数组慢路径，以及 `ulong.MaxValue` 的数组引用往返。

| 配置/测试 | 结果 |
|---|---|
| Debug_Neo / NeoStep16 | 15/15 通过 |
| Release_Neo / NeoStep16 | 15/15 通过 |
| Debug_Neo / NeoStep | 129 项，128 通过，1 个既有失败 |
| Legacy Debug / TestCases.Test05 / register=true | 23/23 通过 |
| Legacy Debug / TestCases.Test05 / register=false | 23/23 通过 |
| TestCases、Debug_Neo/Release_Neo CLI、Legacy Debug CLI 构建 | 0 错误，有既有警告 |

既有失败仍是 `NeoStep13Test.NeoStep13ValueTypeInstanceMethodThis`，属于 Step 17 的 CLR 值类型实例方法 byref receiver 写回。

## ECMA-335 合规状态

- 已闭环：`ldelem/stelem/ldelema` 对 null 抛 `NullReferenceException`，越界抛 `IndexOutOfRangeException`；`ldelema` 实际元素类型不匹配抛 `ArrayTypeMismatchException`；`stelem.ref` 保留 CLR/IL 数组协变存储检查；数组 Ref Slot 使用 `(arrayMStackIndex, elementIndex)`；IL 值类型数组元素独立存储并深拷贝 primitive 与 managed references。
- `newarr` 仅创建一维 SZArray；jagged 数组由嵌套 SZArray 组成。多维数组继续由 CLR 构造函数和 `Get/Set/Address` 方法处理，不映射为 `newarr`。
- Step 17 仍需处理数组 Ref Slot 穿过方法边界的 ref/out 封送，以及多层 CLR 值类型字段引用的完整回写链。当前直接的数组元素和直接字段读写已覆盖。
- IL 数组的精确元素类型依赖 `newarr` 建立的弱引用元数据；若 CLR 代码直接构造或克隆裸 `ILTypeInstance[]` 并返回 Neo，该数组不带 IL 元素类型标签。此类跨边界数组的精确协变写入检查需在后续 CLR 互操作适配中补齐。
