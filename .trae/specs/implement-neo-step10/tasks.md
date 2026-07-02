# 任务列表 (Tasks)
- [x] Task 1: 梳理并接入 Neo VTable 数据结构
  - [x] SubTask 1.1: 检查 Step 1-9 已有的 `ILType`、`ILMethod`、`OpCodeR` 与调用约定实现，确认可复用字段和 helper
  - [x] SubTask 1.2: 在 `ILType` 中增加 Neo VTable 存储与只读访问入口
  - [x] SubTask 1.3: 定义 VTable slot 构建所需的内部数据结构，不引入 Legacy 栈模型依赖

- [x] Task 2: 实现 IL 类型 VTable 构建
  - [x] SubTask 2.1: 为基类虚方法复制稳定 slot
  - [x] SubTask 2.2: 为 override 方法复用被 override 的 slot
  - [x] SubTask 2.3: 为本类新增 virtual 方法分配新 slot
  - [x] SubTask 2.4: 支持 VTable slot 保存 `ILMethod` 和 `CLRMethod` / `IMethod` 引用

- [x] Task 3: 增加 Neo callvirt 指令形态
  - [x] SubTask 3.1: 在 Register VM opcode 定义中增加或确认 `Callvirt_IL`、`Callvirt_CLR`、通用 `Callvirt` 的编码方式
  - [x] SubTask 3.2: 明确每种指令使用的 operand/register 含义，包括 this offset、vtable slot、目标方法和返回目标
  - [x] SubTask 3.3: 保持已有 `Call` / `Newobj` 编码和符号表逻辑不被破坏

- [x] Task 4: 实现 Neo JIT lowering for `callvirt`
  - [x] SubTask 4.1: 继承 Legacy devirtualize 规则，将非 abstract、非 virtual、非 interface 声明类型的 ILMethod `callvirt` lowering 为 direct `Call`
  - [x] SubTask 4.2: 当静态 this 类型为 IL 类型且目标确实需要虚分派时生成 `Callvirt_IL`
  - [x] SubTask 4.3: 当静态 this 类型为 CLR 类型时生成 `Callvirt_CLR`
  - [x] SubTask 4.4: 当静态类型无法确定 IL/CLR 分派时生成通用 `Callvirt`
  - [x] SubTask 4.5: 评估 sealed class / final override 是否能安全 devirtualize；若无法完整保留语义和测试覆盖，则只记录结论不纳入 Step 10 实现
    - 结论：不纳入 Step 10 实现。sealed class / final override 直接 lowering 为 `Call` 会移除 `callvirt` 的 null this 检查语义，除非新增独立 null-check lowering 并补 targeted regression tests；当前 Step 10 只实现正确的虚分派路径。
  - [x] SubTask 4.6: 复用现有调用约定与 Optimizer 中的分支/指令维护 helper，避免新增并行规则
  - [x] SubTask 4.7: 明确 `constrained.` 前缀的 `callvirt` 不纳入 Step 10 虚分派优化范围，继续保留旧的 `Callvirt + Operand4 == 1` marker，避免与 Step 10 dispatch metadata 混用

- [x] Task 5: 在 Neo 解释器中执行虚方法分派
  - [x] SubTask 5.1: `Callvirt_IL` 从 Neo ref slot 读取 this，并通过 `instance.Type.VTable[slot]` 执行实际 IL 方法
  - [x] SubTask 5.2: `Callvirt_CLR` 复用 Step 9 的 `CLRMethod` Neo 调用和 Redirection/反射 fallback
  - [x] SubTask 5.3: 通用 `Callvirt` 根据 `mStack` 中 this 的运行时对象类型分派到 IL 或 CLR 路径
  - [x] SubTask 5.4: 对 null this、缺失 slot、非 IL/CLR 目标提供明确异常
  - [x] SubTask 5.5: 拆分 `ILIntepreter.Neo.cs` 中 `Call` / `Newobj` / `Callvirt_IL` / `Callvirt_CLR` / 通用 `Callvirt` switch case，避免 `Call` 热路径进入 callvirt opcode 分派判断

- [x] Task 6: 增加 Step 10 测试用例
  - [x] SubTask 6.1: 新增或扩展 `TestCases` 中的 Neo Step 10 测试类
  - [x] SubTask 6.2: 覆盖简单继承 virtual/override 分派
  - [x] SubTask 6.3: 覆盖多层继承链分派
  - [x] SubTask 6.4: 覆盖 CLR 类型虚方法调用，例如 `ToString`
  - [x] SubTask 6.5: 覆盖 object 变量持有 IL 或 CLR 对象的通用分派场景
  - [x] SubTask 6.6: 覆盖非 virtual IL instance method 被 C# 编译为 `callvirt` 后在 Neo lowering 中走 direct `Call`

- [x] Task 7: 构建并运行 Neo Step 10 验证
  - [x] SubTask 7.0: 构建 `ILRuntime\ILRuntime.csproj` 的 `Debug_Neo` 配置
    - 结果：`dotnet build "ILRuntime\ILRuntime.csproj" -c Debug_Neo` 通过，0 error；仍有项目既有 warning。
  - [x] SubTask 7.1: 构建 `TestCases`
  - [x] SubTask 7.2: 使用 `Debug_Neo` 配置运行 `ILRuntimeTestCLI`，参数 `useRegister=true`
    - 纠正：此前出现的 `Cpobj` / `Arglist` 是 `Callvirt_IL` / `Callvirt_CLR` 插入到 `OpCodeREnum` Cecil opcode 区间导致的 enum 数值错位，已通过将 Neo-only opcode 移到 enum 尾部修正。
    - 修复：Neo 引用返回值必须复制到 caller 提供的 `retRefBase`，并把 caller 返回 primitive slot 改写为 caller 自己的 mStack index，避免返回 callee frame 内即将被清理的 mStack index。
  - [x] SubTask 7.3: 使用 Step 10 nameFilter 跑新增测试，并确认失败信息可定位到具体分派场景
    - 结果：`NeoStep10Test` 5 个用例通过，0 failed。
  - [x] SubTask 7.4: 视影响范围运行相关 Step 8/9 回归测试，确认非虚调用、newobj、CLR 调用未退化
    - 结果：`NeoStep7Step8Test` 7 个用例通过，0 failed；`NeoStep6Test` 14 个用例通过，0 failed。

- [x] Task 8: 修复 Step 10 定向测试失败后重跑验证
  - [x] SubTask 8.1: 定位 `callvirt.il` 返回值不符合预期的原因
  - [x] SubTask 8.2: 修复 VTable slot 分派或返回值写回中的实际问题
  - [x] SubTask 8.3: 重新运行 `NeoStep10Test`
  - [x] SubTask 8.4: 重新运行相关 Step 8/9 回归测试

# 任务依赖 (Task Dependencies)
- Task 2 依赖于 Task 1
- Task 3 依赖于 Task 1
- Task 4 依赖于 Task 2 和 Task 3
- Task 5 依赖于 Task 3 和 Task 4
- Task 6 依赖于 Task 5
- Task 7 依赖于 Task 6
- Task 8 依赖于 Task 7 的阻塞定位结果
