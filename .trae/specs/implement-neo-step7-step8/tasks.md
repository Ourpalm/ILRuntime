# Tasks

- [x] Task 1: Step7 — 补齐 Neo 引用 slot 写入与基础引用指令
  - [x] 确认帧入口批量预留 `TotalRefSize` 并把引用 local/temp 帧内值初始化为 `-1`
  - [x] 实现 `Ldnull`：目标帧 slot 写入 `-1`
  - [x] 实现 `Ldstr`：字符串写入目标 ref slot 对应 mStack 槽，目标帧 slot 写入 mStack 绝对 index
  - [x] 修正引用类型 `Move`：primitive slot 复制 mStack index，引用目标 ref slot 同步持有对象引用

- [x] Task 2: Step7 — 实现堆 IL 对象字段访问
  - [x] 在 Neo lowering 中覆盖 `Ldfld_*` / `Stfld_*` 非 Inline 变体需要的寄存器 offset 与 ref offset 编码
  - [x] 在 `ILIntepreter.Neo.cs` 实现 primitive 字段 `Ldfld_*` / `Stfld_*`：从帧内对象 mStack index 取 `ILTypeInstance`，读写 `Primitives`
  - [x] 实现引用字段 `Ldfld_*` / `Stfld_*`：通过对象 `ManagedObjects` 与帧/mStack ref slot 同步引用
  - [x] 保持 Inline 字段访问不纳入本任务，避免提前进入 Step12

- [x] Task 3: Step7 — 恢复 Neo 静态构造函数调用
  - [x] 删除 `ILType.cs` 中两处 Neo 模式跳过 `appdomain.Invoke(staticConstructor)` 的临时条件编译分支
  - [x] 确认恢复后静态字段初始化路径仍通过 Step7 字段访问 handler 执行
  - [x] 暂不运行端到端测试，留到 Task 7 统一验证

- [ ] Task 4: Step8 — 实现 IL 方法间 Call 约定
  - [ ] 在 JIT Neo 路径生成或调整 IL-to-IL `Call` 所需 operand 编码，包含目标方法、参数来源、返回目标 offset/ref offset
  - [ ] 在 `ILIntepreter.Neo.cs` 实现 `Call_Neo` 或 Neo 模式下的 `Call` handler
  - [ ] Caller 按 callee 参数 offset 把 primitive 参数写入 `esp`
  - [ ] Caller 按 callee 参数 offset 把 reference 参数的 mStack index 写入 `esp`
  - [ ] 调用 `ExecuteNeo(target, esp, retDst, retRefBase, out unhandledException)`，返回后 caller 的 `esp` 不被 callee 破坏
  - [ ] void 方法传 `retDst = null`、`retRefBase = 0`

- [ ] Task 5: Step8 — 校正 Ret 与跨帧返回引用拷贝
  - [ ] primitive 返回值从 callee 返回 slot 直接 CopyBlock 到 caller `retDst`
  - [ ] 引用返回值从 callee `frameRefBase + srcRefOffset` 拷贝到 caller `retRefBase`
  - [ ] 确保 callee 截断自己的 mStack 时不会清掉 caller 的返回引用
  - [ ] 确保 void 返回不写 `retDst`

- [ ] Task 6: Step8b — 实现 IL 引用类型 Newobj
  - [ ] 在 Neo `Newobj` handler 中识别 IL 引用类型构造函数路径
  - [ ] 分配 `new ILTypeInstance(type)` 并写入 caller 目标 ref slot
  - [ ] 将 `this` 的 mStack index 写入 callee 帧 param0
  - [ ] 将构造函数实参按 Step8 call 约定写入 callee 帧后调用构造函数
  - [ ] 构造函数返回后保持 caller 目标 slot 指向新对象
  - [ ] CLR 引用类型 `newobj` 保持明确 NotImplemented 或现有行为，不在本任务中提前实现 Step9

- [ ] Task 7: Step8 完成后统一添加并运行验证
  - [ ] 添加 `NeoStep7Step8Test` 或等价 TestCases 覆盖引用 local、`Ldnull`、`Ldstr`、引用返回、mStack 平衡
  - [ ] 覆盖 IL 堆对象 primitive 字段读写
  - [ ] 覆盖 IL 堆对象引用字段读写
  - [ ] 覆盖静态字段初始化：`class C { static int X = 42; }` 读取为 42
  - [ ] 覆盖 IL-to-IL 调用：primitive 多参数、混合 reference 参数、reference 返回、递归调用
  - [ ] 覆盖 IL 引用类型 `newobj`：默认构造、带参数构造、构造函数字段赋值、对象作为参数传递
  - [ ] 运行 `dotnet build -c Debug`
  - [ ] 运行 `dotnet build -c Debug_Neo`
  - [ ] 运行 Neo 模式 `NeoStep6Test` 回归
  - [ ] 运行 Neo 模式 Step7-Step8 新增测试

# Task Dependencies
- Task 2 依赖 Task 1（字段访问需要稳定的引用 slot 读写）
- Task 3 依赖 Task 2（恢复 cctor 需要字段访问 handler）
- Task 4 依赖 Task 1（Call 参数和返回需要引用 slot 约定）
- Task 5 依赖 Task 4（Ret 需要 Call 的 retDst/retRefBase 约定）
- Task 6 依赖 Task 4 和 Task 5（Newobj 构造函数调用依赖 IL Call）
- Task 7 依赖 Task 1-6 全部完成

# 不在本 Spec 范围的内容
- CLR 方法调用与 `Console.WriteLine` 回填（Step9）
- CLR 引用类型 `newobj` 完整实现（Step9/Step18）
- VTable / `Callvirt` 分派（Step10）
- 帧内值类型 Inline 字段访问（Step12）
- Ref/Out、`Ldloca`、`Ldflda`、`Ldind`、`Stind`（Step17）
