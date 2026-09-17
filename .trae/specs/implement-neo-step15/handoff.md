# ILRuntime Neo Step15 — Handoff

## 结果与范围

已完成 Neo `isinst` / `castclass`、相邻 `box` 类型检查静态消解、IL 泛型接口/委托 variance 判断，以及这些路径所需的引用身份比较和 CLR primitive box/unbox 补全。未切换分支或提交。进入任务前已有的 `HotfixAOT/Patched/HotfixAOT.dll` 与 `.pdb` 改动保持不变。

## 实现要点

- `ILIntepreter.Neo.cs` 从源引用槽取实际对象：`ILTypeInstance` 使用 `CanAssignTo`，CLR 对象使用 `Type.IsAssignableFrom`。`isinst` 失败写 null；`castclass` 失败抛 `InvalidCastException`；两者对 null 都返回 null。
- `isinst/castclass` 的目标 token 即使是值类型，结果槽仍固定分类为 O（mStack index + ref slot）。泛型引用类型 `unbox.any !!T` 按 ECMA 的 castclass 等价语义处理，包含 null。
- 新增 `Ceq_Ref` / `Cgt_Un_Ref`，按 mStack 中对象身份而非索引数值比较，覆盖 C# `is` 生成的 `isinst; ldnull; cgt.un`，也修复同一对象位于不同引用槽时的比较。
- CLR primitive box/unbox 直接复用统一的 `ReadNeoPrimitive` / `WriteNeoPrimitive`，在 flat primitive slot 与 CLR boxed object 间转换，覆盖 bool/char/整数/浮点/native int；已移除 Step15 曾引入的重复读写 helper。
- `IntPtr` / `UIntPtr` 的 primitive size、自然对齐、frame/field copy、调用封送和字段 opcode 统一使用 `IntPtr.Size`；补充 `AppDomain.UIntPtrType`，32 位使用 I4/U4，64 位使用 I8/U8。Ref Slot 仍固定为设计规定的 8 字节 `(objectIndex, offset)`，不受 native-int 宽度影响。
- `FoldBoxTypeTests` 在 BCP/FCP 前运行：兼容 pair 保留 box、删除重复检查；不兼容 `isinst` 改写为 null；兼容的 `box + check + unbox.any T` 完整链消除；nullable 和不兼容 castclass 保留运行时路径。
- 当前 JIT 的每个泛型实例拥有独立 `ILMethod`，类型 token 已解析为具体参数，因此本步直接静态消解。`.neo` 共享模板需要的 `PatchKind.IsinstResult` 仍随 Step 22 的通用 patch 表实现。
- `ILType.CanAssignTo` 按 Cecil generic parameter variance attribute 支持 IL 定义接口/委托的协变、逆变和 invariant；值类型泛型实参不参与 variance 转换。

## 测试

`TestCases/NeoStep15Test.cs` 共 11 个入口：

1. IL class 精确类型、继承链、接口、无关类型与 `as`。
2. CLR class/interface 精确和失败检查。
3. null 的 `isinst/castclass` 语义。
4. `castclass` 失败的 `InvalidCastException`。
5. boxed int 对 object/int/long/IComparable 的严格检查。
6. `IntPtr` / `UIntPtr` 非零值的 box/unbox 类型与数值往返。
7. 泛型 `box !!T; isinst int` 的 true peephole。
8. 泛型 `box !!T; isinst int` 的 false peephole。
9. 泛型 `as T` / `(T)obj` 的具体 token 路径。
10. IL 泛型接口 covariance、contravariance 与 invariant。
11. CLR `IEnumerable<out T>` covariance。

| 配置/测试 | 结果 |
|---|---|
| Debug_Neo / NeoStep15 | 11/11 通过 |
| Debug_Neo / NeoStep | 114 项，113 通过，1 个既有失败 |
| Release_Neo / NeoStep15 | 11/11 通过 |
| Legacy Debug / TestCases.Test05 / register=true | 23/23 通过 |
| Legacy Debug / TestCases.Test05 / register=false | 23/23 通过 |
| TestCases 与 CLI 强制重建 | Debug_Neo、Release_Neo、Debug 均 0 错误，有既有警告 |

唯一失败仍为 `NeoStep13Test.NeoStep13ValueTypeInstanceMethodThis`：CLR 值类型实例方法通过 byref receiver 修改后的 caller 帧写回，归 Step 17。

## 后续边界

- Variant IL interface 的类型检查已正确；通过变体视图进行接口 callvirt 仍需要分派层把 variant interface slot 映射到实际实现接口，留给 VTable/接口分派后续完善。本步测试只验证 Step 15 的赋值兼容性。
- Step 22 仍需定义通用 `PatchEntry/PatchKind`，并为预编译泛型模板记录 `IsinstResult`；运行时 JIT 已按具体泛型实例工作。
- nullable boxing、CLR enum 的通用 box/unbox、复杂值类型与 Step 17/18/19 边界保持原计划。
