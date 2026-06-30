# 测试用例工程
测试用例工程为TestCases，HybridPatch的测试用例为HotFixAOT，Neo模式只需要TestCases工程里的测试用例

# 测试用例运行方法

参考 [run_tests.bat](file:///f:/SVN/ILRuntime/run_tests.bat)。完整流程：

1. **构建 HotfixAOT**（原始 + Patched 两份）：
   ```
   dotnet build --framework net8.0 -c Release HotfixAOT/
   dotnet build --framework net8.0 -c Release_Patched HotfixAOT/
   ```
2. **生成 patch 文件**：
   ```
   dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -i -h HotfixAOT/Patched/HotfixAOT.hash -o HotfixAOT/Patched/HotfixAOT.dll HotfixAOT/bin/Release/net8.0/HotfixAOT.dll
   dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -p -o HotfixAOT/Patched/HotfixAOT.patch HotfixAOT/Patched/HotfixAOT.hash HotfixAOT/bin/Release_Patched/net8.0/HotfixAOT.dll
   ```
3. **构建 TestCases**：
   ```
   dotnet build TestCases/
   ```
4. **运行 CLI**（参数顺序：`<TestCases.dll> <HotfixAOT.patch> <useRegister true|false> [nameFilter]`）：
   ```
   dotnet run --verbosity normal --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj TestCases/bin/Debug/netstandard2.0/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch false
   ```

## Neo 模式（ENABLE_NEO_MODE）测试

- 使用 `Debug_Neo` 配置构建
- `nameFilter` 可选；CLI 会用 `Contains` 匹配 `TypeName.MethodName`，可用 `NeoStep6Test` 跑整组、`NeoAddI4` 跑单个。
- `useRegister=true` 时 `TestSession.Load` 会以 `JITImmediately` 模式启动，所有 IL 方法在加载阶段就被 JIT 编译。

## Shell 输出 / 测试运行约定

- **不要给可执行命令加 PowerShell pipeline (`| Select-String`, `| Select-Object` 等) 或重定向 (`>`, `2>`)**。Agent harness 会自动把 stdout/stderr 保存到 job 输出文件；额外管道会改变退出码并破坏诊断。直接调用可执行文件，让 harness 保存输出，再用 `Read` / `Grep` 工具看文件。
- 单个 Neo 测试用例正常应在 5–20ms 完成。如果命令运行超过 10 秒，几乎肯定卡在解释器死循环里（lowering bug 居多）。立刻通过 `Stop-Process -Name ILRuntimeTestCLI -Force` 杀掉，然后查看已经写出的 JIT dump 与异常堆栈定位。
- CLI 已经在 `Main` 外层加了 try/catch 输出未处理异常。否则 .NET 进程会直接以 `0xE0434352` 退出码崩溃，吞掉所有诊断。任何对 CLI 入口的改动都必须保留这层兜底。
- ENABLE_NEO_MODE 下 `ILType.cs` 暂时跳过了 `appdomain.Invoke(staticConstructor)`（参见 Step 6 checklist），Step 7 实现完整的 `Stfld_*` / `Ldfld_*` 后会一并恢复。
