# Neo 模式 Step 1-2 实施 Spec

## Why
ILRuntime 需要引入 Neo 高性能执行模式。Step 1 建立编译宏控制体系，Step 2 清理 ILTypeInstance 的死代码并确保对象模型在两种模式下正确编译。

## What Changes
- 在 ILRuntime.csproj、ILRuntimeTestBase.csproj、ILRuntimeTestCLI.csproj 中添加 `Debug_Neo` 和 `Release_Neo` 构建配置，定义 `ENABLE_NEO_MODE` 编译符号
- 用 `#if ENABLE_NEO_MODE` 包裹 `ILIntepreter.Neo.cs` 全文件
- 移除 `ILTypeInstance.cs` 中所有 `USE_OLD_OBJ_MODEL` 死代码（`StackObject[]` 旧模型），保留 `byte[] fields` 作为唯一路径
- 确保 `Primitives` 属性在所有配置下无条件可用（Register.cs 依赖）

## Impact
- Affected code: ILRuntime.csproj, ILRuntimeTestBase.csproj, ILRuntimeTestCLI.csproj, ILTypeInstance.cs, ILIntepreter.Neo.cs

## 设计决策
- `USE_OLD_OBJ_MODEL` 从未在任何 .csproj 中定义，`StackObject[]` 是纯死代码，直接移除
- `byte[] fields` + `AutoList managedObjs` 是当前运行中的对象模型，无条件保留
- `ENABLE_NEO_MODE` 仅保护 Neo 解释器入口（ILIntepreter.Neo.cs），不影响对象模型代码
