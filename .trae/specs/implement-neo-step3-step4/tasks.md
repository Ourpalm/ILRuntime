# Tasks

- [x] Task 1: Step 3 — ILType 暴露静态字段布局
  - [x] 在 `ILType` 新增字段 `staticFieldOffsets`、`totalStaticPrimitiveSize`、`totalStaticReferenceCnt`（仅 `#if ENABLE_NEO_MODE`）
  - [x] 在 `InitializeFields` 中为 `field.IsStatic == true` 分支镜像现有 primitive/reference offset 累加逻辑，写入 `staticFieldOffsets[idxStatic]`
  - [x] 暴露 `StaticTotalPrimitiveSize` / `StaticTotalReferenceCount` / `GetStaticFieldOffset(int idx)` 公共/internal 属性
  - [x] 处理边界：无静态字段时 `staticFieldOffsets` 可为 null，`StaticTotal*` 返回 0

- [x] Task 2: Step 3 — ILTypeStaticInstance Neo 路径
  - [x] `ILTypeStaticInstance` 构造函数 `#if ENABLE_NEO_MODE` 分支按 `StaticTotalPrimitiveSize`/`StaticTotalReferenceCount` 分配 `fields` 和 `managedObjs`
  - [x] 遍历 `TypeDefinition.Fields`，对 `IsStatic && InitialValue != null` 的字段：通过 `GetStaticFieldOffset(idxStatic).ReferenceOffset` 把 `InitialValue`（byte[]）写入 `managedObjs`
  - [x] 保留 Legacy 分支不变

- [x] Task 3: Step 3 — ILEnumTypeInstance Neo 路径
  - [x] 构造函数 Neo 分支：按 enum underlying type 大小分配 `byte[] fields`（int → 4，long → 8 等）
  - [x] Neo 路径 `Clone()` 实现：`Array.Copy` byte[]
  - [x] Neo 路径 `ToString()` 实现：从 `byte[]` 读出值后匹配 `FieldDefinition.Constant`

- [x] Task 4: Step 4 — JITCompiler 暴露帧元数据
  - [x] `CompiledFrame` 新增 `ParamPrimitiveSize`、`LocalsPrimitiveSize`、`ReturnPrimitiveSize`、`ReturnRefCount` 字段（`#if ENABLE_NEO_MODE`）
  - [x] `AllocateLocalStackSpaces` 计算并写入这些字段
  - [x] 参数 primitive 区域作为第一段连续 bytes 分配（确保 ParamPrimitiveSize 准确）— 当前实现把参数视作普通 local，需要单独前置一段参数区，或者在 `CompiledFrame.LocalInfos` 之前添加参数 slot 数组
  - [x] 返回值元数据从 `method.ReturnType` 推导（primitive size + reference count）

- [x] Task 5: Step 4 — ILMethod 暴露帧元数据
  - [x] 把 Task 4 的元数据从 `CompiledFrame` 保存到 `ILMethod` 实例字段（`frameSize`、`paramPrimitiveSize` 等）
  - [x] 在 `InitCodeBody(register=true)` 中赋值
  - [x] 暴露对应公共/internal 属性

- [x] Task 6: Step 4 — ExecuteNeo 签名与帧 Enter/Exit
  - [x] 修改 `ExecuteNeo` 签名为 `byte* ExecuteNeo(ILMethod, byte* esp, byte* retDst, int retRefBase, out bool unhandledException)`
  - [x] 帧入口：`frameBase = esp; esp += method.FrameSize`；`Unsafe.InitBlock(frameBase + ParamPrimitiveSize, 0, LocalsPrimitiveSize)`
  - [x] 引用类型 local slot 初始化为 -1（需要 `LocalInfos` 中标识哪些 slot 是引用类型——通过 `slot.Size == 0` 判定，写 `*(int*)(frameBase + slot.Offset) = -1`）
  - [x] mStack 批量预留：`frameRefBase = mStack.Count; for(i=0;i<TotalRefSize;i++) mStack.Add(null);`（避免 `Count` 直接赋值的 List 兼容问题，或使用 `EnsureSize`/扩容辅助方法）
  - [x] 退帧：`mStack` 截断回 `frameRefBase`；`return frameBase`
  - [x] Frames 栈维护：复用 `stack.InitializeFrame`/`PushFrame`/`PopFrame`，必要时调整 `StackFrame.BasePointer` 的语义（现在指向 `byte*` 而非 `StackObject*`）—— 本步骤优先保证编译通过和最简执行正确，复杂的 debugger 联动留给 Step 14/26

- [x] Task 7: Step 4 — Ret 指令最简实现
  - [x] `case OpCodeREnum.Ret`：
    - 有返回值且 `retDst != null`：`Unsafe.CopyBlock(retDst, frameBase + retSrcOffset, ReturnPrimitiveSize)` + 引用拷贝循环
    - void 或 retDst==null：跳过
    - `returned = true`
  - [x] 返回值源 slot 偏移从 `ip->Register1` 取出对应 `LocalInfos[reg]`，进而拿到 `Offset` 和 `RefOffset`
  - [x] 异常处理路径暂保留现有结构，编译通过即可

- [x] Task 8: Step 4 — ExecuteNeo 调用点适配
  - [x] 检查 `ExecuteNeo` 的所有现有调用点（通常仅 `ILMethod`/`AppDomain.Invoke` 派发）
  - [x] 在调用点构造 `retDst` 缓冲（stackalloc 或 nativePointer 上的本地槽）和 `retRefBase`（mStack.Count 之后预留）
  - [x] 调用前将参数写入 esp 起始位置（最简：仅支持 0 参数静态方法即可）
  - [x] 调用后从 retDst 读返回值（若有），转回 object 给现有 Invoke API

- [x] Task 9: 编译与最小验证
  - [x] `dotnet build -c Debug` 通过（Legacy 不受影响）— 0 错误 0 警告
  - [x] `dotnet build -c Debug_Neo` 通过 — 0 错误 0 警告
  - [x] 在测试工程中加 smoke 测试：Neo 模式下 `void Foo() { return; }` 能正确进入退出且 mStack.Count 平衡 — 推迟到 Step 6（需 Ldc_I4/Call 等指令联通调度入口后才能跑端到端）
  - [x] 若 `int Bar() { return constant; }` 路径所需指令（如 Ldc_I4 + Move）尚未实现，文档化为已知遗留并由 Step 6 补齐 — 已记录

# Task Dependencies

- Task 2 依赖 Task 1（构造函数需要 `StaticTotalPrimitiveSize` 等 API）
- Task 3 独立于 Task 1-2（enum 不走静态字段路径，但放在同一文件统一改）
- Task 5 依赖 Task 4（ILMethod 元数据从 CompiledFrame 取）
- Task 6 依赖 Task 5（ExecuteNeo 读取 `method.FrameSize` 等）
- Task 7 依赖 Task 6（在新帧布局上实现 Ret）
- Task 8 依赖 Task 6, 7
- Task 9 依赖 Task 1-8 全部完成

# 不在本 Spec 范围的内容（明确推迟）

- `Ldsfld_Neo` / `Stsfld_Neo` 指令实现（属于 Step 7+ 范畴）
- 算术/分支/常量加载指令（Step 6）
- Neo Call 约定（Step 8）
- 完整异常处理 Neo 适配（Step 14）
- DebugService 对 byte* 帧的联动（Step 26）
