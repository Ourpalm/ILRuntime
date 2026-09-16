# ILRuntime Neo Step14 — Handoff

## 结果与范围

已完成 Neo 核心 try/catch/finally、throw/rethrow、leave/endfinally。filter（C# when）和 fault 按确认范围在 JIT 入口明确拒绝；不缓存失败的 CompiledFrame。未新增公共调用 API。

内部 catch 入口伪指令最终命名为 `EnterCatch`；它只显式定义异常分派器提供的隐式异常输入，不承担 handler 匹配或跳转。

基于当前 `features/object-model-overhaul` 开发，保留已有 HotfixAOT/Patched/HotfixAOT.dll、.pdb 改动。未提交或切换分支。

## 实现要点

- **编译**：`EnterCatch` 定义 catch 的隐式引用输入，贯通 optimizer 与 SSA 类型重命名；异常槽 byte/ref offset 写入内部 ExceptionHandler。EH 边界显式分块；ret/rethrow/endfinally/throw 不产生正常 fallthrough。lowering 删除 Push 时同步修正 EH 和 leave 目标。null HandlerEnd 使用最终指令末尾。
- **展开**：`ILIntepreter.Neo.Exceptions.cs` 中 `NeoExceptionState` 维护可嵌套 continuation 与 catch 上下文。保护区域按内到外匹配，同一区域保留子句顺序。finally 内嵌 catch 保留待执行操作，逃出 finally 的新异常替换旧操作。rethrow 使用 ExceptionDispatchInfo。
- **清理**：未处理异常保留 Frames；进入上层 catch/finally 前截断 callee 帧和引用区。InvocationFrame.Dispose 恢复入口 Frames 深度、ManagedStack 数量与 ValueTypeStackPointer；编译发生在任何运行时状态变更之前。`ExecuteNeo` 保持单层热路径，profiler 的异常安全 `try/finally` 仅在 `DEBUG && !NO_PROFILER` 下编译。
- **构造**：恢复原有 caller 引用槽暂存和失败回滚结构。try/finally 现在从暂存对象起覆盖参数准备、callee 引用槽分配与构造调用；失败恢复原引用和 primitive 索引，成功提交目标索引。
- **CLR 调用**：仅去除反射调用产生的一层 TargetInvocationException，EDI 保留原异常。参数准备或调用失败均清空缓存参数。CLR null 引用参数正确读取为 null。`NeoStep14CLRHost` 为普通 IL 用例创建异常并做 CLR 原生身份断言；只有外部传播、同一解释器复用及 Cecil 特殊 IL 用例从 CLRHost 调用 `AppDomain.Invoke`。
- **诊断**：保留原 CLR Exception，通过与 Legacy 一致的 `Data["StackTrace"]` 追加解释执行的方法及指令地址；外部调用者无需区分执行模式，也不构造会触发 Legacy 栈解码的 ILRuntimeException。
- **局部变量**：EH 没有正常前驱导致初始化分析覆盖更多局部变量；Neo 引用 local 用 Ldnull，且用当前 local 的类型 token，不误创建 string/引用类实例。

## 验证结果

| 配置/测试 | 结果 |
|---|---|
| 开发前 Debug_Neo：NeoStep | 67 项，65 通过，2 既有失败 |
| 最终 Debug_Neo：NeoStep | 94 项，92 通过，仍为原有 2 失败 |
| 最终 Release_Neo：NeoStep | 94 项，92 通过，仍为原有 2 失败 |
| Step14 新增测试（包含在上面两行） | 两配置均 27/27 通过 |
| Legacy Debug：TestCases.Test05，register=true | 23/23 通过 |
| Legacy Debug：TestCases.Test05，register=false | 23/23 通过 |
| TestCases + CLI 构建 | Debug_Neo、Release_Neo、Debug 均 0 错误；有既有警告 |

上述结果已在 `EnterCatch` 改名和 Step14 测试代码格式整理后重新执行确认。

保留的失败，名称与开发前一致：

1. `NeoStep13Test.NeoStep13ValueTypeInstanceMethodThis`：Neo CLR newobj，Step18。
2. `NeoStep7Step8Test.NeoTestCLRBindingSmallPrimitiveArgs`：既有 DivideByZeroException。

测试结构：27 项中 19 项直接在 `NeoStep14Test` 的 IL 方法执行，8 项由 CLRHost 驱动外部调用边界或 Cecil 特殊 IL。异常由 CLR 工具方法创建；身份断言用原生 `ReferenceEquals`，因为 Neo 当前把 CIL `ReferenceEquals` 折成按 mStack 索引的 ceq，同对象不同引用槽会误判。`AppDomain.Invoke` 不再参与普通异常控制流测试。

新增用例：异常身份/Message、throw null、空 catch、嵌套类型顺序、无匹配 catch、catch 局部变量、跨帧传播、正常/异常/return 的 finally、多层 leave、跨帧 finally、finally 内嵌 catch/finally/rethrow、正常 leave 被新异常替换、原始 CLR 栈保留、构造失败保持旧值、同一 interpreter 连续 20 次异常/失败/成功调用、Cecil filter/fault 重复拒绝、Cecil 方法末尾 HandlerEnd=null。

### 复现命令（仓库根目录，PowerShell）

```powershell
dotnet build TestCases/TestCases.csproj -c Debug_Neo -t:Rebuild --nologo -v:q
dotnet build ILRuntimeTestCLI/ILRuntimeTestCLI.csproj -c Debug_Neo -f net8.0 -t:Rebuild --nologo -v:q
dotnet ILRuntimeTestCLI/bin/Debug_Neo/net8.0/ILRuntimeTestCLI.dll TestCases/bin/Debug/netstandard2.1/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch true NeoStep
```

Release_Neo：构建配置替换为 Release_Neo；CLI 路径使用 `bin/Release_Neo/net8.0`；TestCases 路径使用 `bin/Release/netstandard2.1`。

Legacy：两项目以 Debug 强制重建，CLI 路径 `bin/Debug/net8.0`，过滤名 `TestCases.Test05`，分别传 true 和 false。务必按配置重建：TestCases 与 TestBase 的 Debug/Debug_Neo、Release/Release_Neo 有共享产物目录。

本次原始日志位于 `%TEMP%/neo14-baseline-tests.log`、`neo14-final-debug-tests.log`、`neo14-final-release-tests.log`、`neo14-legacy-register-tests.log`、`neo14-legacy-stack-tests.log`；此处保留可复现命令和结果，未把大量 JIT dump 加入仓库。

## 后续挂账及限制

- filter/fault/endfilter：明确拒绝，未实现。
- 完整调试器变量展示、栈溢出保护保持后续范围；异常诊断目前为原 Exception + `Data["StackTrace"]`。
- 测试过程中发现既有引用比较问题：`ex != null` 可被编成 `cgt.un`，现有执行路径比较 mStack index 与 -1，不是正确的引用 null 比较。本步没有扩展引用比较指令；构造失败测试以 int 参数条件触发 throw，独立验证 ctor 事务。应在引用比较完善时修复并回补用例。
- 原 step13c 手册与代码有差异：仓库已存在 CLR 测试宿主和 newobj 回滚。本步以实际代码为基准，不沿用历史 61/59 测试数量。
- 没有进行性能基准、AOT 序列化或 Unity/IL2CPP 设备验证；本步验证主机为 .NET 8，测试程序集 netstandard2.1。
