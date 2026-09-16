# ILRuntime Neo Step14.5 — Handoff

## 结果与范围

已完成普通 CLR 引用类型 newobj：Neo constructor redirection、反射 fallback、string 专用构造、构造事务回滚。未新增公共调用 API，未切换分支或提交。保留进入任务前已有的 HotfixAOT/Patched/HotfixAOT.dll 和 .pdb 改动。

## 实现要点

- `ILIntepreter.Neo.cs` 的 Newobj 先按声明类型分派：delegate 明确报 Step19，IL/CLR value type 明确报 Step18；普通 CLR class 进入同文件内的 `InvokeNeoClrConstructor`，IL 引用构造保留已有实现。构造辅助方法集中在现有 Neo 解释器文件中，不另拆文件。
- CLR 构造事务保存真实 caller 目标 primitive index 和引用槽；参数复制、redirection/反射调用在同一 try/finally 内。redirection 接收真实 retDst/retRefBase，反射对象通过 `CommitNeoReferenceResult` 提交到预分配引用槽。任何失败恢复两者。
- 反射沿用 Step14 的单层 TargetInvocationException 解包和 EDI，不修改原异常身份及 CLR 构造栈；已有 invocationParam 清理仍覆盖参数准备和调用失败。
- 按性能优先约定，已移除构造参数预检查及缓存标记，以及本步增加的反射参数预检查。不为未实现能力添加临时运行时检查，也不转移到 JIT 提前拒绝。
- `AppDomain` 在注册阶段匹配 string 构造签名，为 `(char,int)`、`(char[])`、`(char[],int,int)` 分别注册独立 Neo redirection；执行时按固定 ABI offset 读参数，不读取参数元数据或分派重载。包括 null/空数组语义与参数异常。pointer、ReadOnlySpan 等超出当前参数能力的重载明确拒绝。
- 已移除本步增加的 JIT delegate 提前拒绝；实际执行到未实现分支时沿用现有行为。Newobj 保留构造类型分派。
- JIT 原本已按自然对齐分配 CLR 参数，但 ReadNeo* helpers 只累加大小，遇到小整数或首个 long 参数会漏过 padding。本步读取 helper 按同一自然对齐规则前移游标，char 保持既有 4 字节 ABI；反射引用参数复用 ReadNeoReference，Int32 enum 显式转换为 enum 对象。这也修复了 Step14 基线中的小整数 CLR binding 失败。
- 指令编码没有改变；设计文档第 27.14 节补齐 CLR 构造参数首 4 字节、operand 域、返回及回滚契约。

## 测试

`TestCases/NeoStep145Test.cs` 共 9 个测试入口，均直接执行 IL，不依赖 CLRHost 或 TestSession：

1. Exception：IL 内 new Exception、Message、throw/catch 和身份。
2. GeneratedBinding：现有 List<int> Neo 构造 binding，两个 List 的对象和结果槽相互独立。
3. GeneratedLongParameter：现有 BindableProperty<long> 生成构造 binding，覆盖返回头之后 long 对齐。
4. ReflectionArguments：未注册 binding 的普通 CLR class，byte/long/short/double/string/null/bool/char 混合参数；IL 读取属性并断言。
5. Strings：重复字符、空字符串、char[]、数组切片、null 数组；char[] 由 IL 调用 string.ToCharArray 获得。
6. StringFailure：string 参数错误在 IL catch 中捕获，保留旧值。
7. ReflectionRollback：CLR 构造抛出传入异常，IL 内检查身份、原始 CLR ctor 栈和旧对象。
8. RepeatedConstruction：IL 内连续 20 轮构造失败回滚和成功构造。
9. RedirectionRollback：直接调用 string 专用重定向，连续 20 轮参数失败、旧值保留、成功重试。

`ILRuntimeTestBase/TestFramework/NeoStep145CLRObject.cs` 仅保留普通 CLR 测试类，包含构造函数和数据属性，没有测试调度或断言。异常身份通过继承的 CLR Equals 验证，避免当前引用 ceq 按 mStack 索引比较的既有问题。

已删除 NeoStep145CLRHost、辅助 Targets、测试专用 redirection 和捕获原始帧指针的故障注入。当前用例不再直接检查 Frames/ManagedStack 内部计数，也不模拟 redirection 写入结果后主动抛异常；覆盖实际构造路径的成功、失败及旧值保留。解释器边界清理仍由已有 Step14 测试覆盖。

Step14 的 `Unsupported(int)` 原先返回 `new Exception()` 以制造未实现错误，本步更新为 `new DateTime(2020,1,1)`，仍验证尚未实现的 Step18 路径。

| 配置/测试 | 结果 |
|---|---|
| 开发前 Debug_Neo / NeoStep | 94 项，92 通过，2 既有失败 |
| 最终 Debug_Neo / NeoStep | 103 项，102 通过，1 既有失败 |
| 最终 Release_Neo / NeoStep | 103 项，102 通过，1 既有失败 |
| 本步新增测试（包含于上两行） | 两配置均 9/9 通过 |
| Legacy Debug / TestCases.Test05 / register=true | 23/23 通过 |
| Legacy Debug / TestCases.Test05 / register=false | 23/23 通过 |
| TestCases 与 CLI 强制重建 | Debug_Neo、Release_Neo、Debug 均 0 错误，有既有警告 |

唯一剩余失败：`NeoStep13Test.NeoStep13ValueTypeInstanceMethodThis`。CLR 值类型实例方法通过 byref receiver 修改后的 caller 帧写回仍归 Step17。`NeoStep7Step8Test.NeoTestCLRBindingSmallPrimitiveArgs` 本步已通过。

## 复现

仓库根目录 PowerShell：

```powershell
dotnet build TestCases/TestCases.csproj -c Debug_Neo -t:Rebuild --nologo -v:q
dotnet build ILRuntimeTestCLI/ILRuntimeTestCLI.csproj -c Debug_Neo -f net8.0 -t:Rebuild --nologo -v:q
dotnet ILRuntimeTestCLI/bin/Debug_Neo/net8.0/ILRuntimeTestCLI.dll TestCases/bin/Debug/netstandard2.1/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch true NeoStep
```

Release_Neo：配置改为 Release_Neo，CLI 路径使用 bin/Release_Neo/net8.0，测试 DLL 使用 bin/Release/netstandard2.1。Legacy：配置改为 Debug，CLI 路径 bin/Debug/net8.0，过滤 TestCases.Test05，分别传 true/false。

务必按配置重建，TestCases/TestBase 仍有共享输出目录。测试结构简化后已重新构建 Debug_Neo 和 Release_Neo。

原始日志位于 `%TEMP%/neo145-baseline-tests.log`、`neo145-debug-tests.log`、`neo145-release-tests.log`、`neo145-legacy-register-tests.log`、`neo145-legacy-stack-tests.log`，对应 build/cli 日志同前缀。

## 后续边界

- Step15 可继续开始，普通 CLR 引用类型和异常已可从 IL 构造。
- Step17：byref/out、CLR 值类型 receiver 写回；Step18：IL/CLR value-type newobj；Step19：delegate 构造与函数指针。
- CLR 复杂值类型、pointer/IntPtr/UIntPtr、byref/out 和非 Int32 enum 参数及生成 binding 的通用复杂参数实现仍需后续完善；当前不承诺这些未实现路径的统一失败方式。
- Step14 交接中的引用 ceq/cgt.un、filter/fault、完整调试器展示、栈溢出及 AOT/Unity 设备验证等挂账保持不变。
- 验证主机为 .NET 8，测试 DLL 为 netstandard2.1；未做性能基准或 Unity/IL2CPP 设备验证。
