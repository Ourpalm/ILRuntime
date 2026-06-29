# Checklist

- [x] ILRuntime.csproj 包含 Debug_Neo 配置且定义 ENABLE_NEO_MODE
- [x] ILRuntime.csproj 包含 Release_Neo 配置且定义 ENABLE_NEO_MODE
- [x] ILRuntimeTestBase.csproj 包含 Debug_Neo / Release_Neo 配置
- [x] ILRuntimeTestCLI.csproj 包含 Debug_Neo / Release_Neo 配置
- [x] ILIntepreter.Neo.cs 被 `#if ENABLE_NEO_MODE` / `#endif` 包裹
- [x] ILTypeInstance.cs 中无任何 USE_OLD_OBJ_MODEL 引用
- [x] `byte[] fields` 和 `Primitives` 属性无条件存在（不在任何 #if 块内）
- [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 编译 0 错误
- [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 编译 0 错误
