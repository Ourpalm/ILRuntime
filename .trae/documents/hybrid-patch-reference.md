# HybridPatch 机制参考文档

## 一、整体架构

HybridPatch 是 ILRuntime 的 AOP 注入热更新机制，通过三阶段流水线实现方法级热修复：

1. **注入阶段** (Inject): 使用 Mono.Cecil 修改原始程序集，在每个方法头部注入重定向桩代码
2. **生成阶段** (Generate Patch): 对比原始哈希与修改后程序集，生成差异补丁（.patch 文件）
3. **应用阶段** (Apply Patch): 运行时加载补丁，将方法体替换为 ILRuntime 解释器执行

## 二、构建和测试流程

### 目录结构
- `HotfixAOT/` - 测试用例项目（同时包含原始和 patched 代码，通过 `#if PATCHED` 区分）
- `ILRuntimeTestCLI/` - 测试运行器 CLI
- `ILRuntimeTestBase/` - 测试基础设施（TestSession, TestResultInfo）
- `PatchTool/` - 注入和生成补丁的命令行工具
- `ILRuntime/HybridPatch/` - 核心注入和补丁逻辑
- `TestCases/` - ILRuntime 本身的测试用例（非 Hotfix 专用）

### HotfixAOT.csproj 配置
- 目标框架: `netstandard2.0;net8.0`
- 配置: `Debug`, `Release`, `Release_Patched`
- `Release_Patched` 配置定义 `PATCHED` 常量

### 构建步骤（本地复现 CI）
```powershell
# 1. 构建原始版本
dotnet build --framework net8.0 -c Release HotfixAOT/

# 2. 构建 Patched 版本
dotnet build --framework net8.0 -c Release_Patched HotfixAOT/

# 3. 注入原始 DLL（生成 .hash 文件 + 修改 DLL）
dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -i -h HotfixAOT/Patched/HotfixAOT.hash -o HotfixAOT/Patched/HotfixAOT.dll HotfixAOT/bin/Release/net8.0/HotfixAOT.dll

# 4. 生成补丁
dotnet run --framework net8.0 -c Release --project PatchTool/PatchTool.csproj -- -p -o HotfixAOT/Patched/HotfixAOT.patch HotfixAOT/Patched/HotfixAOT.hash HotfixAOT/bin/Release_Patched/net8.0/HotfixAOT.dll

# 5. 构建 TestCases
dotnet build TestCases/

# 6. 运行测试
dotnet run --framework net8.0 --project ILRuntimeTestCLI/ILRuntimeTestCLI.csproj -- TestCases/bin/Debug/netstandard2.0/TestCases.dll HotfixAOT/Patched/HotfixAOT.patch false
```

### ILRuntimeTestCLI 参数
```
Usage: ILRuntimeTestCLI path patchPath useRegister[true|false] [filterName]
```
- `path`: TestCases DLL 路径
- `patchPath`: Hotfix Patch 文件路径
- `useRegister`: 是否使用寄存器模式（true/false）
- `filterName`: 可选，按名称过滤测试

### CI 测试矩阵
8 种组合：I(解释器 Debug/Release) × T(TestCases Debug/Release) × R(Register true/false)

## 三、核心文件

### 注入阶段
| 文件 | 用途 |
|------|------|
| `AssemblyInjector.cs` (1485行) | 主注入器：InjectType/InjectMethod 在方法头部插入重定向逻辑 |
| `InjectionContext.cs` (34行) | TypeInjectionContext/MethodRedirectionInjectionContext 上下文结构 |
| `CecilExtensions.cs` (889行) | IL 生成辅助：参数推入/读取、泛型处理、闭包名称一致性 |

### 生成阶段
| 文件 | 用途 |
|------|------|
| `PatchGenerator.cs` (567行) | 对比原始与修改版本，生成 AssemblyPatchInfo |
| `PatchInfo/AssemblyInfo.cs` | AssemblyHashInfo, TypeHashInfo, MethodHashInfo, TypePatchInfo, MethodPatchInfo 等数据结构 |
| `AssemblyHashInfo.cs` | 构建程序集的类型/方法哈希用于对比 |

### 应用阶段
| 文件 | 用途 |
|------|------|
| `AssemblyPatch.cs` (545行) | 运行时加载补丁：InitializeTypes/InitializeMethodBody 重建方法体 |

### 测试基础设施
| 文件 | 用途 |
|------|------|
| `HotfixAOT/ITestCase.cs` | ITestCase 接口 + DelegateTestCase + AllTestCases 聚合器 |
| `ILRuntimeTestBase/` | TestSession 加载 DLL 和 patch，运行所有测试 |

## 四、注入机制详解

### 方法注入逻辑 (InjectMethod)
每个方法头部注入：
```csharp
// 伪代码
if (___@ILMethod_{methodIdx} != null) {
    var ctx = new InvocationContext(___@ILMethod_{methodIdx});
    ctx.PushObject(this);  // 实例方法
    ctx.PushXxx(arg0);     // 按类型推入参数
    ctx.PushXxx(arg1);
    ctx.Invoke();
    // 读回 ref/out 参数
    // 读取返回值
    return result;
}
// 原始方法体继续执行
```

### 泛型处理
- **泛型类型**: 每个泛型类型有一个 `GenericDefinition` 辅助类，通过静态字段按类型参数隔离
- **泛型方法**: 使用嵌套泛型类缓存模式（`___GenericMethodCache_<Name><T>`），静态字段天然按类型参数隔离，零开销缓存

### 嵌套类型/编译器生成类型处理
- 父类型标记 `[ILRuntimePatch]` 后，所有嵌套类型**自动被强制纳入**（`forceInclude` 递归）
- 编译器生成的类型（迭代器状态机 `<Method>d__X`、闭包 `<>c__DisplayClass`）作为独立类型注入
- `FixClosureNameConsistency` 保证编译器生成类型的名称跨编译一致（用方法哈希替代不稳定的序号）
- 嵌套 private 类型被提升为 NestedAssembly 可见性

### 值类型 this 回写
对于值类型（struct）的实例方法，patched 版本需要将修改后的 this 写回调用者的栈位置。通过 ref 参数前置 slot 实现。

## 五、补丁序列化格式

### MethodPatchInfo
- `CodeBody`: OpCode[] 指令数组
- `LocalVariables`: 局部变量类型引用
- `ExceptionHandlers`: ExceptionHandlerPatchInfo[] 异常处理表
- 类型/方法/字段/字符串引用均索引化存储

### ExceptionHandlerPatchInfo
- HandlerType (byte): Catch/Finally/Filter/Fault
- TryStart/TryEnd (int): try 块范围
- HandlerStart/HandlerEnd (int): handler 块范围
- CatchTypeIndex (int): catch 类型索引（-1 表示非 catch）

## 六、已知限制和注意事项

1. **`$"..."` 字符串插值**: .NET 8 编译为 `DefaultInterpolatedStringHandler`（ref struct），ILRuntime 无法动态创建。**必须用 `string.Format()` 或 `string.Concat()` 替代**
2. **委托/接口/枚举不纳入补丁**: `AddTypeInfo` 中显式排除
3. **foreign 字段不可补丁**: 跨程序集引用的字段不处理
4. **异常处理表**: 之前缺失序列化支持，已修复。迭代器的 MoveNext() 天生含 try/finally
5. **闭包名称一致性**: 依赖 `FixClosureNameConsistency`，如果名称匹配失败会导致补丁无法正确应用

## 七、测试用例编写规范

```csharp
[ILRuntimePatch]
public class HotfixXxx
{
    public int SomeMethod(int input)
    {
#if PATCHED
        return input * 2;  // 修改后逻辑
#else
        return input;      // 原始逻辑
#endif
    }
}

public class HotfixXxxTestCases
{
    static bool TestSomeMethod(bool patched)
    {
        var obj = new HotfixXxx();
        int result = obj.SomeMethod(5);
        return patched ? result == 10 : result == 5;
    }

    public static IEnumerable<ITestCase> GetTestCases()
    {
        yield return new DelegateTestCase("HotfixXxxTestCases.TestSomeMethod", TestSomeMethod);
    }
}
```

注意：
- 测试方法接收 `bool patched` 参数，分别验证两种预期
- 避免使用 `$"..."` 字符串插值
- 在 `ITestCase.cs` 的 `AllTestCases.GetAllTestCases()` 中注册新测试组
