# 实现 Neo Step 9: CLRRedirectionDelegateNeo 和 CLR 方法调用 Spec

## 为什么 (Why)
在 Neo 模式下，调用 CLR 方法需要一种新的调用约定，以避免装箱/拆箱的开销，并能正确地与 `byte*` 帧和 `AutoList` 托管栈进行交互。我们需要引入 `CLRRedirectionDelegateNeo`，适配 CLR 方法的分派，并生成 Neo 重定向代码。

## 做了什么更改 (What Changes)
- 在 `AppDomain.cs` 中定义 `CLRRedirectionDelegateNeo`。
- 在 `AppDomain` 和 `CLRMethod` 中添加 `RedirectMapNeo` 以及相关的 Neo 重定向注册入口。
- 添加互斥保护：在 Neo 模式下注册旧版委托（Legacy Delegate）应该抛出异常。
- `ILRuntimeJITFlags` 枚举新增 `JITNeo = 16`。
- `AppDomain` 新增 `bool IsNeoMode { get; }` 属性用于全局判断。
- `AppDomain` 构造函数增加宏校验：`ENABLE_NEO_MODE` 下必须传入 `JITNeo`，Legacy 模式下不能传入 `JITNeo`（注意这是Flag，所以判断时应该用 `&` 运算符）。
- 更新 `ILIntepreter.Neo.cs` 中的 `ExecuteNeo`，处理 CLR 方法分派（如果存在 Neo 重定向则直接调用，否则回退到反射 `Invoke`）。
- 更新 CLR 绑定代码生成器（`MethodBindingGenerator.cs`, `BindingCodeGenerator.cs` 等），生成 Neo 重定向代码。目前仅实现 Step 9 已支持的特性（例如生成重定向方法体和读取参数的存根）。
- 升级 Step 6 的冒烟测试（`NeoStep6Test.cs`），使用 `Console.WriteLine` 和 `Exception` 来进行更细粒度的断言。

## 影响范围 (Impact)
- 受影响的功能: Neo 模式执行，CLR 绑定生成，单元测试断言，AppDomain 配置。
- 受影响的代码:
  - `ILRuntime/Runtime/Enviorment/AppDomain.cs`
  - `ILRuntime/Runtime/ILRuntimeJITAttribute.cs`
  - `ILRuntime/CLR/Method/CLRMethod.cs`
  - `ILRuntime/Runtime/Intepreter/ILIntepreter.Neo.cs`
  - `ILRuntime/Runtime/CLRBinding/BindingCodeGenerator.cs`
  - `ILRuntime/Runtime/CLRBinding/MethodBindingGenerator.cs`
  - `ILRuntime/Runtime/CLRBinding/ConstructorBindingGenerator.cs`
  - `TestCases/NeoStep6Test.cs`
  - `f:\SVN\ILRuntime\.trae\specs\implement-neo-step6\checklist.md`

## 新增需求 (ADDED Requirements)
### 需求: AppDomain 的 Neo 模式配置与验证
系统应确保在正确的模式下使用正确的 JIT Flag：
- **WHEN** 实例化 `AppDomain`
- **THEN** 如果定义了 `ENABLE_NEO_MODE` 宏，`defaultJITFlags` 必须等于 `ILRuntimeJITFlags.JITNeo`，否则抛出异常。
- **THEN** 如果没有定义 `ENABLE_NEO_MODE` 宏，`defaultJITFlags` 必须不能等于 `ILRuntimeJITFlags.JITNeo`，否则抛出异常。
系统提供 `IsNeoMode` 属性以供其他模块查询当前运行模式。

### 需求: Neo 重定向委托
系统应当提供 `CLRRedirectionDelegateNeo`，签名如下:
`unsafe delegate void CLRRedirectionDelegateNeo(ILIntepreter intp, byte* frameBase, AutoList mStack, CLRMethod method, bool isNewObj, byte* retDst, int retRefBase);`

### 需求: Neo 模式下的 CLR 方法分派
系统应当能在 Neo 模式下分派 CLR 方法:
- **WHEN** 在 `ExecuteNeo` 中通过 `Call` 或 `Callvirt`（如果适用）调用 CLR 方法时
- **THEN** 检查是否注册了 Neo 重定向。如果有，则调用它。如果没有，则回退使用反射 `Invoke`。

### 需求: Neo 模式的 CLR 绑定生成器
系统应当生成 Neo 重定向绑定:
- **WHEN** 生成 CLR 绑定时
- **THEN** 生成符合 `CLRRedirectionDelegateNeo` 签名的方法，包含从 `byte*` 帧提取参数的逻辑，然后调用底层的 CLR 方法并将结果写回。

## 修改需求 (MODIFIED Requirements)
### 需求: Step 6 冒烟测试断言
Step 6 的测试应当使用 `Console.WriteLine` 和 `Exception` 来处理失败断言，而不是使用除以零的 hack 方式。
