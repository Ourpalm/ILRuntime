# 任务列表 (Tasks)
- [ ] Task 1: 增加 Neo 模式全局配置与 JITFlag
  - [ ] SubTask 1.1: 在 `ILRuntime/Runtime/ILRuntimeJITAttribute.cs` 中增加 `JITNeo = 16`
  - [ ] SubTask 1.2: 在 `ILRuntime/Runtime/Enviorment/AppDomain.cs` 中增加 `IsNeoMode` 属性
  - [ ] SubTask 1.3: 在 `AppDomain` 的构造函数中添加宏 `ENABLE_NEO_MODE` 和 `defaultJITFlags` 的交叉校验逻辑

- [ ] Task 2: 添加 `CLRRedirectionDelegateNeo` 和 `RedirectMapNeo`
  - [ ] SubTask 2.1: 在 `ILRuntime/Runtime/Enviorment/AppDomain.cs` 中定义 `CLRRedirectionDelegateNeo`
  - [ ] SubTask 2.2: 在 `AppDomain` 中添加 `RedirectMapNeo` 和互斥保护（例如在 Neo 模式下注册 Legacy 委托时抛出异常等）
  - [ ] SubTask 2.3: 更新 `CLRMethod` 以解析和缓存 `RedirectionNeo`

- [ ] Task 3: 在 Neo 解释器中实现 CLR 方法调用分派
  - [ ] SubTask 3.1: 在 `ILIntepreter.Neo.cs` 中，更新 `ExecuteNeo` 针对 `OpCodeEnum.Call` 当目标为 `CLRMethod` 时的处理逻辑
  - [ ] SubTask 3.2: 添加分派逻辑：如果 `RedirectionNeo` 存在，则调用它；否则回退使用反射 `Invoke`

- [ ] Task 4: 实现 Neo 模式的 CLR 绑定生成
  - [ ] SubTask 4.1: 更新 `BindingCodeGenerator.cs`, `MethodBindingGenerator.cs` 和 `ConstructorBindingGenerator.cs` 以生成 `CLRRedirectionDelegateNeo` 方法
  - [ ] SubTask 4.2: 在生成的代码中实现基础类型和引用类型的参数提取以及返回值写入的存根（stub）
  - [ ] SubTask 4.3: 在 `RegisterBinding` 中正确注册生成的 Neo 重定向

- [x] Task 5: 升级 Step 6 冒烟测试
  - [x] SubTask 5.1: 重构 `NeoStep6Test.cs`，使用 `Console.WriteLine` 和 `throw new Exception` 替换掉 `DivideByZeroException` 的 hack，以实现更细粒度的断言
  - [x] SubTask 5.2: 移除 `NeoStep6Test.cs` 中的文件头部约束注释

- [x] Task 6: 勾选 Step 6 Checklist 事项
  - [x] SubTask 6.1: 修改 `f:\SVN\ILRuntime\.trae\specs\implement-neo-step6\checklist.md`，勾选冒烟测试断言升级任务

# 任务依赖 (Task Dependencies)
- Task 3 依赖于 Task 2
- Task 4 依赖于 Task 2
- Task 5 依赖于 Task 3 和 4
- Task 6 依赖于 Task 5
