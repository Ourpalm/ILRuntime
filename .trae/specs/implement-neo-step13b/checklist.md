# Step 13B Checklist — ILType 嵌套 CLR Inline 布局递归

## 布局规则

- [x] `ILType` 的 CLR primitive/enum 字段继续使用共享 primitive 尺寸与对齐规则。
- [x] CLR `StructStorage.Inline` 字段使用 `CLRType.TotalPrimitiveSize`。
- [x] CLR `StructStorage.Inline` 字段使用 `CLRType.MaxAlignment`，并按自然对齐放置。
- [x] CLR `StructStorage.Boxed` 字段使用 4 字节 mStack index，不展开内部字段。
- [x] CLR reference type 字段使用 4 字节 mStack index，不写入托管指针。
- [x] CLR Inline 子结构的 `TotalReferenceCount` 被完整累加到外层布局。
- [x] CLR Boxed/reference 字段各只增加一个 reference slot。
- [x] 空结构、enum、IL 嵌套 value type 的既有布局行为保持不变。

## 实例与静态字段

- [x] `fieldOffsets` 的 primitive/reference offset 与嵌套 CLRType 的布局一致。
- [x] `staticFieldOffsets` 使用与实例字段相同的 CLR Inline/Boxed 分类规则。
- [x] 等价的实例字段和静态字段产生一致的尺寸、对齐和引用游标推进。
- [x] 重复访问布局属性不会重复构建或产生不稳定结果。
- [x] 布局初始化不存在 CLR/IL 递归重入导致的栈溢出。

## 布局消费者

- [x] `Ldfld_Value` / `Stfld_Value` 能访问嵌套 CLR Inline primitive 字段。
- [x] `Ldfld_Value` / `Stfld_Value` 能保持嵌套 CLR Inline 引用字段的 mStack 根。
- [x] `Move_Vt` 同时复制 primitive 段和全部 reference slots。
- [x] Box → Unbox 往返保留嵌套 CLR Inline 的 primitive 与引用字段。
- [x] CLR Boxed 嵌套字段走 mStack 对象路径，不读取 flat bytes。
- [x] 不新增 opcode，不改变 Step 13 已确定的 `StructStorage` 判定。
- [x] 不把 Step 17 Ref Slot、Step 18 CLR value-type newobj 或 Step 16 数组能力误纳入本步骤。

## 测试

- [x] 新增 `NeoStep13BTest` 覆盖 CLR Inline primitive struct 嵌套。
- [x] 新增测试覆盖自然对齐及嵌套字段后的尾部字段。
- [x] 新增测试覆盖 CLR Inline struct 的非空引用、null 引用和后续 ref-slot。
- [x] 新增测试覆盖 CLR Boxed struct 的单 index/ref-slot 语义。
- [x] 新增测试覆盖嵌套值 Box/Unbox 往返。
- [x] 新增测试覆盖 `Move_Vt` 双向值保持。
- [x] 新增测试覆盖实例字段与静态字段访问对称性。
- [x] 定向测试不依赖 CLR value-type `newobj`、异常捕获或数组 newarr。

## 编译与回归

- [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误、无新增警告。
- [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误、无 Legacy 回归。
- [x] TestCases 按工程 workflow 构建成功。
- [x] NeoStep13B 定向测试全部通过。
- [x] Step 12/13 相关 Neo 回归通过，已知后续 step 挂账保持原断言。
- [x] Legacy 全量测试已执行；4 个失败均为既有 Legacy 基线失败，未归因本变更。
