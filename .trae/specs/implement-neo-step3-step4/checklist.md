# Checklist

## Step 3 — 静态字段
- [x] `ILType` 在 `#if ENABLE_NEO_MODE` 下暴露 `StaticTotalPrimitiveSize` / `StaticTotalReferenceCount` / `GetStaticFieldOffset(int)`
- [x] `InitializeFields` 中 `IsStatic` 分支正确累加 `staticPrimitiveOffset` 和 `staticReferenceOffset`
- [x] `ILTypeStaticInstance` Neo 路径按上述 size 分配 `byte[] fields` 和 `AutoList managedObjs`
- [x] `InitialValue` 字段在 Neo 模式下被写入 `managedObjs`（与 Legacy 行为一致）
- [x] `ILEnumTypeInstance` Neo 路径 `Clone()` 和 `ToString()` 基于 `byte[]` 实现且无 `StackObject` 引用

## Step 4 — 帧布局
- [x] `CompiledFrame` 包含 `ParamPrimitiveSize` / `LocalsPrimitiveSize` / `ReturnPrimitiveSize` / `ReturnRefCount` 字段（Neo 模式）
- [x] `JITCompiler.AllocateLocalStackSpaces` 正确计算并填充上述字段
- [x] `ILMethod` 暴露 `FrameSize` / `ParamPrimitiveSize` / `LocalsPrimitiveSize` / `TotalRefSize` / `ReturnPrimitiveSize` / `ReturnRefCount`
- [x] `ExecuteNeo` 签名变为 `byte* ExecuteNeo(ILMethod, byte* esp, byte* retDst, int retRefBase, out bool unhandledException)`
- [x] 帧入口：`esp` 推进 `FrameSize`、locals primitive 区被 `InitBlock` 清零、引用 local slot 被写入 -1
- [x] mStack 批量预留 `TotalRefSize` 槽
- [x] 退帧：`mStack` 回滚到 `frameRefBase`，`return frameBase`
- [x] `Ret` 指令在 retDst != null 时正确拷贝 `ReturnPrimitiveSize` 字节并拷贝 `ReturnRefCount` 个 mStack 引用
- [x] void 方法 retDst == null 时 Ret 不写任何数据

## 调用点 & 编译
- [x] `ExecuteNeo` 现有调用点适配新签名（参数写入 + retDst 分配 + 调用后读返回值）
- [x] `dotnet build -c Debug ILRuntime/ILRuntime.csproj` 0 错误
- [x] `dotnet build -c Debug_Neo ILRuntime/ILRuntime.csproj` 0 错误
- [x] Neo 模式下空方法 `void Foo() { return; }` 进入/退出后 `mStack.Count` 与 `esp` 平衡（手工或单元测试验证） — 推迟到 Step 6
