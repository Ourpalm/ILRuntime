# Step 13B Tasks — ILType 嵌套 CLR Inline 布局递归

## 工程约束

- 仅扩展 `ILType -> CLRType` 的单向布局解析；不得新增 `CLRType -> ILType` 字段递归。
- Neo frame 的 primitive 段只保存值或 mStack index；托管引用必须保存在 mStack reference 段。
- Inline/Boxed 判定必须读取已缓存的 `CLRType.StructStorage`，不得在 handler 中用 Binder、指针大小或字段数量猜测。
- 实例字段、静态字段、Box/Unbox、`Move_Vt` 的 primitive/ref 游标必须保持对称。
- 所有偏移在初始化/JIT 阶段确定；运行时不得新增按字段名或哈希查表的 hot-path 包装。
- 兼容 Unity Mono/IL2CPP 与 CoreCLR；不要引入仅 CoreCLR 可用的对象指针假设。

## Task Dependencies

```text
Task 1 (布局规则审计与测试 fixture)
  └─→ Task 2 (ILType CLR 字段尺寸/对齐/引用游标)
       └─→ Task 3 (实例/静态 offset 与布局消费者对称性)
            └─→ Task 4 (NeoStep13B 测试)
                 └─→ Task 5 (双配置编译与回归验证)
```

## Tasks

- [x] Task 1: 固化 CLR 字段布局映射并准备测试类型
  - [x] 在 `ILType.InitializeFieldsForFlatLayout` 入口确认 CLR primitive、CLR Inline、CLR Boxed、CLR reference 四类映射及现有 IL value type 行为。
  - [x] 确认 `CLRType.MaxAlignment`、`TotalPrimitiveSize`、`TotalReferenceCount` 的可用性和初始化时序；若公共/内部访问级别不足，只做最小 API 调整。
  - [x] 准备一个包含 CLR Inline primitive struct 的 IL fixture，以及一个含引用字段但无实例方法的 CLR Inline struct fixture。
  - [x] 明确测试 fixture 不依赖 Step 18 的 CLR value-type `newobj` 或 Step 14 异常处理。

- [x] Task 2: 扩展 ILType 实例/静态字段平面布局
  - [x] 修改 `GetFieldNaturalSize`：CLR Inline 使用子 `CLRType.TotalPrimitiveSize`，CLR Boxed/reference 使用 4 字节 mStack index。
  - [x] 修改 `GetFieldNaturalAlignment`：CLR Inline 使用子类型最大自然对齐，CLR Boxed/reference 使用 4 字节对齐。
  - [x] 在实例字段与静态字段分支中按 CLR `StructStorage` 累加嵌套 reference count；确保 Boxed/reference 只累加 1。
  - [x] 保持字段顺序、自然对齐、空结构最小尺寸和现有 ILType/enum 语义不变。
  - [x] 增加必要的初始化递归保护，避免重复查询导致类型初始化重入或栈溢出。

- [x] Task 3: 校验 offset 编码与布局消费者
  - [x] 验证 `fieldOffsets` / `staticFieldOffsets` 的 primitive/reference 起始 offset 与嵌套 CLRType 元数据一致。
  - [x] 审计 `Ldfld_Value`、`Stfld_Value`、`Move_Vt`、Box、Unbox 和整体复制路径，确认均使用外层修正后的 primitive size/ref count。
  - [x] 对含嵌套 CLR Inline 引用字段的路径补齐递归引用搬运；不得把 CLR 托管指针写入 frame primitive 段。
  - [x] 保持 CLR Boxed 字段的 mStack index + 单 ref-slot 语义，不得误展开内部字段。
  - [x] 如发现属于 Step 17 Ref Slot 或 Step 18 newobj 的缺口，保留明确 NotImpl/挂账，不扩大本步骤范围。

- [x] Task 4: 添加 NeoStep13B 定向测试
  - [x] 覆盖 CLR Inline primitive struct 嵌套、自然对齐和尾部字段访问。
  - [x] 覆盖含引用字段的 CLR Inline struct 嵌套，验证非空引用、null 和后续 ref-slot 不重叠。
  - [x] 覆盖 CLR Boxed struct 嵌套，验证只占一个 mStack index/ref slot，不读取其内部 flat bytes。
  - [x] 覆盖嵌套值的 Box/Unbox 往返及 `Move_Vt` 双向值保持。
  - [x] 覆盖实例字段与静态字段布局/访问对称性。

- [x] Task 5: 编译、测试与收尾
  - [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 通过且无新增错误/警告。
  - [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 通过且 Legacy 无行为改动。
  - [x] 按工程 workflow 构建 TestCases，并运行 NeoStep13B 定向测试。
  - [x] 运行 Step 12/13 相关 Neo 回归与 Legacy 全量回归；记录已知 Step 18/14/16 挂账，不弱化断言。
  - [x] 更新本变更的 checklist；仅在实现完成后回写 Step 13 handoff 的 Step 13B 状态。

- [x] Task 4.1: 获取 Boxed/静态字段运行时证据并完成最小修复
  - [x] 在 `ILIntepreter.Neo.cs` 的 `Ldflda`/Boxed receiver 分支观察 `objIndex`、receiver offset、外层 Boxed 字段 primitive index/ref slot 及前一条 `Stfld_Ref` 的 source index/object。
  - [x] 在 `Ldsfld`/`Stsfld` 分支观察 `Operand2`、`Operand3`、`Operand4`、static primitive offset 和 8 字节写入前后值。
  - [x] 基于断点证据修复 Boxed 嵌套字段与 IL static CLR Inline 字段；不通过放宽断言或扩大到 Step 17 规避失败。
  - [x] 重新运行 `NeoStep13BBoxedNestedIndexRefSlot` 与 `NeoStep13BStaticInstanceSymmetry`。

## Task Dependencies

- Task 2 depends on Task 1.
- Task 3 depends on Task 2.
- Task 4 depends on Task 3.
- Task 4.1 depends on Task 4 and runtime breakpoint evidence.
- Task 5 depends on Tasks 1–4.
