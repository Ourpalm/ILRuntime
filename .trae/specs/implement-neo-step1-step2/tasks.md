# Tasks

- [x] Task 1: 在三个 .csproj 中添加 Debug_Neo / Release_Neo 配置
  - [x] ILRuntime.csproj 添加 Debug_Neo 和 Release_Neo PropertyGroup（含 ENABLE_NEO_MODE）
  - [x] ILRuntimeTestBase.csproj 添加对应配置
  - [x] ILRuntimeTestCLI.csproj 添加对应配置

- [x] Task 2: 用 `#if ENABLE_NEO_MODE` 包裹 ILIntepreter.Neo.cs
  - [x] 文件首行添加 `#if ENABLE_NEO_MODE`
  - [x] 文件末行添加 `#endif`
  - [x] 修复 ExecuteNeo 缺少 `return esp;` 的编译错误

- [x] Task 3: 清理 ILTypeInstance.cs 中 USE_OLD_OBJ_MODEL 死代码
  - [x] 移除所有 `#if USE_OLD_OBJ_MODEL` / `#else` / `#endif` 条件编译块
  - [x] 保留 `byte[] fields` + `AutoList managedObjs` 无条件存在
  - [x] 确保 `Primitives` 属性无条件编译可用

- [x] Task 4: 编译验证
  - [x] `dotnet build -c Debug` 通过（Legacy 模式）
  - [x] `dotnet build -c Debug_Neo` 通过（Neo 模式）

# Task Dependencies
- Task 2 依赖 Task 1（需要 ENABLE_NEO_MODE 宏才能正确条件编译）
- Task 3 独立于 Task 1/2
- Task 4 依赖 Task 1-3 全部完成
