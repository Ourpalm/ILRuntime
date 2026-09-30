# ILRuntime Neo Step17 — Handoff

## 结果与范围

已实现 Step17 的 Ref Slot 跨帧读写与回写，包括 IL ref/out、CLR ref/out 反射封送、CLR struct this 绑定、字段/静态字段/数组元素引用、泛型 ldobj/stobj、跨帧 initobj 和引用生存期保护。Ref Slot 仍是 8 字节 `(objectIndex, offset)`，未改变 IL 参数 ABI。未提交或切换分支；任务前已有的 `HotfixAOT/Patched/HotfixAOT.dll`、`.pdb` 修改未主动改动。

## 实现要点

- `ILIntepreter.Neo.References.cs` 提供按地址读写的公共内部实现，以及供生成绑定使用的 `ReadRef<T>` / `WriteRef<T>`。FRAME_REF 使用 runtime stack 绝对偏移；需要引用区时查找实际拥有该地址的活动帧及其字段布局，不再误用 callee 的 ref base。
- 引用类型局部变量的 ldloca/ldarga 指向变量本身。stind.ref 把对象写入目标变量所属帧的引用槽，不能保存 callee 的临时 mStack index。IL 对象引用字段通过布局计算 ManagedObjects 偏移，不需要另加 `objectIndex = -2`。
- ldflda、ldelema 与 byref Move 使用编译期预留的隐藏 GC 锚点。对象变量重新赋值、循环重用 producer 临时槽后，已保存的 ref 仍指向原对象。锚点属于帧的 TotalRefSize，异常处理保留活动帧的锚点，退帧统一释放。
- CLR 嵌套值类型字段由 `CLRFieldAddressFallback` 保存父位置及强引用的父 owner。每次操作重新读取父值，写后逐层回写到原字段/数组；不保存脱离原位置的中间 box，也不保存托管对象的裸指针。地址对象存于 mStack，Ref Slot 的 8 字节结构不变。
- 该 fallback 不是通用引用机制：纯 IL 字段/数组元素内部字段仍使用对象根与布局偏移，不分配描述对象。CLR 嵌套字段取址及 CLR 静态字段取址会分配描述对象；反射产生的装箱是另一项开销。
- 引用分派按 frame → IL instance → array → CLR 处理，IL 快速路径在 fallback 检查前返回。`CommitFieldReceiver` 缓存 owner，数组优先返回，普通 CLR receiver 提交后返回，特殊回写集中到 `CommitCLRFieldAddressFallback`。普通 CLR 分支仍需一次描述对象识别，不能直接按 offset 分流。
- Step17 引用辅助方法按职责命名，去掉 Neo 前缀，包括 `CreateFieldAddress`、`ResolveFieldReceiver`、`ResolveIndirectReceiver`、`ReadAddress` / `WriteAddress`、`ReadRef` / `WriteRef` 等；调用方及 binding 生成器同步。模式开关、构建配置和其他模块的命名保持原范围。
- 补齐 ldobj/stobj 的类型推断、lowering 和执行，覆盖泛型引用类型与值类型 ref 参数。补齐 initobj 的统一地址路径，以及 CLR 整体值类型字段与嵌套值类型字段读写。
- IL ref return 可返回 caller 的帧地址或堆字段/数组地址；堆 owner 转存 caller 的返回锚点。返回当前即将退出的帧地址，抛 InvalidProgramException。新增 Cecil 构造的非法 CIL 测试验证此行为。
- CLR 反射调用按与 lowering 相同的参数对齐规则读取参数，支持 ref/out 和值类型 this，正常调用后提交修改；使用独立参数数组避免重入破坏共享参数缓冲区。反射无法保存 CLR ref return 的位置身份，该路径明确抛 NotSupportedException，需专用 redirection。
- 绑定生成器为 byref 参数生成 8 字节对齐的 Ref Slot 读取，为 out 参数使用默认初值，为值类型 this 读取实际实例，在 finally 中提交可写参数和 receiver。普通 CLR 构造绑定也生成 ref/out 回写。现有 TestVector3.Normalize 绑定已同步修复，原 Step13 失败用例转绿。
- CLR 含引用字段的 struct 不能按 Neo 布局偏移直接覆盖 CLR 实际对象 payload；CLR 可能重排托管字段。这类 box/frame 封送改为按字段转换，primitive 使用非对齐复制，引用走 GC 正常赋值；不含引用的既有路径保留。
- 修正地址折叠 pass 删除仍被后续 store 使用的 ldloca 的问题。Neo 尊重 MethodImplOptions.NoInlining，新增跨帧测试明确禁用 helper 内联，另外保留独立的内联地址复用回归。
- 修正无局部变量 void 方法 Ret lowering 访问不存在 r0 的问题。

## 验证

`TestCases/NeoStep17Test.cs` 新增 18 个测试入口。涵盖多层 IL 调用、out、异常后的 IL 写入、堆/静态引用字段、引用生命周期、IL/CLR struct、数组、泛型交换、ref alias、合法/非法 ref return、initobj、窄整数写回、整字段复制、内联地址复用、owner 重新赋值与循环内保存 ref。

| 配置 / 测试 | 结果 |
|---|---|
| Debug_Neo / NeoStep 全量 | 147/147 通过 |
| Release_Neo / NeoStep 全量 | 147/147 通过 |
| Legacy Debug / TestCases.Test05 / register=true | 23/23 通过 |
| Legacy Debug / TestCases.Test05 / register=false | 23/23 通过 |
| 新生成 CLR 绑定独立编译并运行 | 5/5 通过 |
| Neo netstandard2.1 / Debug_Neo / Release_Neo / Legacy 构建 | 0 错误，既有警告 |

生成绑定验证工程在 `binding-validation/`；它先生成 NeoStep17CLRHost 的绑定，再编译并运行 5 个 CLR ref/out 场景，未替换项目的全套历史生成绑定。构建与测试输出保存在 `logs/`。

### 引用分派调整复验（2026-09-30）

Debug_Neo 与 Release_Neo 全量各 147/147，重新生成绑定 5/5；netstandard2.1 与 Legacy 编译通过，Legacy Test05 寄存器/栈模式各 23/23。日志前缀为 `ref-dispatch-`。

Release 运行时端到端采样：每个场景预热 2,000 次，再取 9 轮、每轮 10,000 次调用的耗时中位数。含 Invoke、测试对象创建、反射与 GC 成本，并非纯分派指令基准。

| 场景 | 修改前 ms | 修改后 ms | 修改后独立复测 ms | 分配 B/调用（前后相同） |
|---|---:|---:|---:|---:|
| HeapAndStatic | 35.580 | 34.148 | 34.025 | 384.0 |
| StructFields | 32.842 | 31.143 | 35.111 | 384.0 |
| Arrays | 50.659 | 42.198 | 48.066 | 523.5 |
| ClrWholeFields | 47.608 | 58.437 | 45.399 | 672.0 |
| ClrNested | 99.562 | 98.762 | 99.053 | 1880.0 |

分配未增加；耗时存在进程间波动，不能据此宣称固定加速比例或已严格证明无性能回退。代码层面的确定变化是 IL 路径不再检查 fallback，数组回写不再先检查 fallback，普通间接回写不再重复检查描述类型。

主要命令（仓库根目录）：

```powershell
dotnet build TestCases/TestCases.csproj -c Debug --no-restore
dotnet build ILRuntimeTestCLI/ILRuntimeTestCLI.csproj -c Debug_Neo -f net8.0 --no-restore
dotnet ILRuntimeTestCLI/bin/Debug_Neo/net8.0/ILRuntimeTestCLI.dll TestCases/bin/Debug/netstandard2.1/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch true NeoStep
```

## 后续边界与 Step18 接口

- Step18 的 IL/CLR 值类型 newobj 可使用本步的 typed address 读写与 Ref Slot this；本步没有实现值类型构造指令本身。
- 历史生成绑定中其他 `TODO: ValueType instance in Neo` 或 byref 参数占位代码，需要用更新后的生成器重新生成；本步只同步了已纳入回归的 Normalize 绑定，没有批量重写全部自动生成文件。
- CLR 反射 fallback 继承 MethodInfo.Invoke 的限制：不能提供多个 CLR ref 参数指向同一位置时的即时别名语义，也不能依靠反射在目标抛异常时保留所有 ref/out 修改。新生成绑定的 finally 支持异常写回，但多个参数共享同一位置时仍采用独立参数临时值；需要精确 CLR alias 语义的接口应使用专用 redirection。IL→IL alias 与异常写入已覆盖。
- CLR 方法直接返回 ref 仍需专用 redirection；IL 方法返回 CLR 嵌套字段 ref 已验证。
- Step16 handoff 中外部裸 ILTypeInstance[] 缺少元素 IL 类型标签的边界继续保留。

## ECMA-335 合规状态

- 已闭环：8 字节 managed pointer 跨 IL 帧传递；ref/out 写入原始存储；泛型 T& 的 ldobj/stobj；引用字段的 GC root 生命周期；IL alias；原 owner 变量重赋值后的地址稳定；跨帧/数组 initobj；窄整数按目标宽度写回；CLR 嵌套字段逐层回写。
- I.12.4.1.5.3：Ret 在 Debug 与 Release 均拒绝指向 retiring frame 的 FRAME_REF；允许指向仍存活 caller 帧和堆存储的合法 ref return。
- III.1.8.1.3：IL 类型布局拒绝 managed-pointer 字段，抛 NotSupportedException；不提供把 managed pointer 保存为普通堆字段的能力。
- III.3.35 readonly.：与 Legacy 一致，不提供 IL verification 层的只读写入校验；C# 的只读 ref 约束由编译器负责。
- III.4.14 alignment：新增 primitive 搬运使用 Unsafe.CopyBlockUnaligned；堆对象不暴露跨 GC 的裸指针。
- 未闭环的 CLR reflection/generated-binding alias 与 reflection 异常写回语义见上节；不可将这些边界表述为已经完整满足 ECMA-335。完整 CLR binding/反射边界复审继续列入后续互操作验收。
