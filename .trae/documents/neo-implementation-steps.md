# Neo 模式实现步骤拆解

> 基于 `object-model-neo-design.md` 设计方案  
> 分支: `features/object-model-overhaul`  
> 当前已完成: 新 Object Model 字段存储 (byte[]+AutoList)、特化 Ldfld/Stfld、StackSlotInfo、ExecuteNeo 骨架(仅 Ret)

---

## 编译模式架构

Neo 模式同时支持 JIT 和 AOT 两种编译路径，共享同一个解释器：

```
┌─────────────────────────────────────────────────────────┐
│                     ENABLE_NEO_MODE                      │
│                                                         │
│  ┌─── Neo JIT ───┐     ┌─── Neo AOT ───────────┐      │
│  │ DLL (Cecil)   │     │ .neo 文件 (预编译产出) │      │
│  │     │         │     │     │                  │      │
│  │     ▼         │     │     ▼                  │      │
│  │ JITCompiler   │     │ Runtime Loader         │      │
│  │ (运行时编译)  │     │ (反序列化 + 绑定)      │      │
│  │     │         │     │     │                  │      │
│  └─────┼─────────┘     └─────┼──────────────────┘      │
│        │                      │                         │
│        ▼                      ▼                         │
│     OpCodeR[] + CompiledFrame + VTable                  │
│        │                                                │
│        ▼                                                │
│     ExecuteNeo (统一解释器)                              │
└─────────────────────────────────────────────────────────┘
```

| 模式 | 编译时机 | 适用场景 |
|------|---------|---------|
| Neo JIT | 运行时方法首次被调用时 | 开发调试、热更新、不需要提前处理 DLL 的场景 |
| Neo AOT | 构建期由 ilrt_neoc 预编译 | 追求启动速度、减少运行时内存（无需 Cecil 元数据）|

**设计原则**：
- Step 1-20 的所有功能在 Neo JIT 下完整可用——不依赖 .neo 文件
- Neo AOT 是纯优化层，把 JIT 的工作提前到构建期
- 两种路径产出的 OpCodeR[] 语义完全一致，ExecuteNeo 无需区分来源
- VTable、接口偏移表等元数据在 JIT 模式下运行时构建，在 AOT 模式下从 .neo 文件加载

---

## 依赖关系总览

```
Step 1 (宏体系)
  │
  ├─→ Step 2 (ILTypeInstance 条件编译重构+方法补全) ─→ Step 5 (Box/Unbox 基础)
  │                                                      │
  ├─→ Step 3 (静态字段)                                  │
  │                                                      ▼
  ├─→ Step 4 (Neo 帧布局 byte*)                     Step 13 (Box/Unbox 完整)
  │     │
  │     ├─→ Step 6 (基本算术/分支/常量加载指令)
  │     │     │
  │     │     ├─→ Step 7 (ManagedStack 批量分配 + 引用类型基础 + 堆对象字段访问)
  │     │     │     │
  │     │     │     ├─→ Step 8 (Neo Call 约定 - IL 方法间调用)
  │     │     │     │     │
  │     │     │     │     ├─→ Step 8b (引用类型 newobj) ← 不需要 VTable！
  │     │     │     │     │     │
  │     │     │     │     │     ├─→ Step 9 (CLRRedirectionDelegateNeo + CLR 调用)
  │     │     │     │     │     │     │
  │     │     │     │     │     │     ├─→ Step 10 (VTable + 虚方法分派)
  │     │     │     │     │     │     │     │
  │     │     │     │     │     │     │     ├─→ Step 11 (接口分派)
  │     │     │     │     │     │     │     │
  │     │     │     │     │     │     │     └─→ Step 19 (委托)
  │     │     │     │     │     │     │
  │     │     │     │     │     │     └─→ Step 15 (isinst/castclass)
  │     │     │     │     │     │
  │     │     │     │     │     └─→ [后续所有引用类型场景均可验证]
  │     │     │     │     │
  │     │     │     │     └─→ Step 14 (异常处理)
  │     │     │     │
  │     │     │     └─→ Step 16 (数组访问)
  │     │     │
  │     │     └─→ Step 12 (帧内值类型 + Inline 字段访问)
  │     │           │
  │     │           ├─→ Step 12b (Move_Vt + LowerMove)
  │     │           │
  │     │           └─→ Step 13 (Box/Unbox 完整)
  │     │
  │     └─→ Step 17 (Ref/Out + ldloca/ldflda)
  │           │
  │           └─→ Step 18 (值类型 newobj + CLR newobj) ← 需要 Ref Slot 传 this
  │
  └─→ Step 20 (Async/Await) ← 依赖 Step 13, 17, 19
```

```
Neo JIT 编译器改造（贯穿 Step 4-20）:
  Step 21 (JIT Neo 完整改造) ← 随各功能步骤同步进行

Neo AOT 工具链（纯优化层，不影响功能正确性）:
  Step 22 (泛型模板+Patch) ← 依赖 Step 21（AOT 特有：JIT 模式下泛型走逐实例化编译）
  Step 23 (NeoAssembly 格式定义) ← 依赖 Step 21, 22
  Step 24 (ilrt_neoc 预编译工具) ← 依赖 Step 23
  Step 25 (运行时加载器) ← 依赖 Step 23, 24
  Step 26 (性能验证) ← 依赖所有
```

---

## Step 1: 编译宏体系与代码组织

**目标**: 建立 `ENABLE_NEO_MODE` 宏控制体系，替代现有 `USE_OLD_OBJ_MODEL`。

**内容**:
1. 引入 `ENABLE_NEO_MODE` 编译宏
2. 将现有 `USE_OLD_OBJ_MODEL` 条件编译替换为 `ENABLE_NEO_MODE`（逻辑取反：`!USE_OLD_OBJ_MODEL` → `ENABLE_NEO_MODE`）
3. 确立文件组织：
   - `ILIntepreter.Register.cs` — Legacy 模式（默认）
   - `ILIntepreter.Neo.cs` — Neo 模式（`#if ENABLE_NEO_MODE`）
4. 添加运行时互斥保护（Neo 模式下调用 Legacy 入口抛异常，反之亦然）
5. 清理 `ILIntepreter.Register.cs` 中的 `#if !USE_OLD_OBJ_MODEL` 分支（移至 Neo.cs）

**依赖**: 无（基础设施）

**验证方式**:
- 不定义 `ENABLE_NEO_MODE` 时，编译通过，所有现有测试通过（Legacy 行为不变）
- 定义 `ENABLE_NEO_MODE` 时，编译通过（功能暂时为空壳）

---

## Step 2: ILTypeInstance 条件编译重构 + 核心方法补全

**目标**: 重构 `ILTypeInstance.cs` 的条件编译结构，恢复 Legacy 模式完整性，并实现 Neo 模式的新版本。

**背景问题**:
当前文件中 `#else`（新模型）分支存在大量被 `/* */` 注释掉的旧实现代码，这些代码原本是 Legacy 模式需要的完整逻辑。在切换到 `ENABLE_NEO_MODE` 宏体系后：
- 不定义 `ENABLE_NEO_MODE` = Legacy 模式，必须恢复全部旧行为
- 定义 `ENABLE_NEO_MODE` = Neo 模式，需要基于 `byte[] + AutoList` 重新实现

**内容**:

### 2a. Legacy 路径恢复（确保不定义 ENABLE_NEO_MODE 时行为不变）

将以下被 `/* */` 注释掉的方法恢复到 `#if !ENABLE_NEO_MODE` 保护区内（即恢复原有 `StackObject[]` 模型的完整实现）：

| 方法 | 当前状态 | 恢复动作 |
|------|---------|---------|
| 索引器 `this[int index]` getter/setter | 方法体被注释，返回 null/空体 | 取消注释，放入 Legacy 分支 |
| `InitializeFields(ILType)` | 空体 | 取消注释，放入 Legacy 分支 |
| `PushToStack(int fieldIdx, ...)` | 空体 | 取消注释，放入 Legacy 分支 |
| `CopyToRegister(int fieldIdx, ...)` | 空体 | 取消注释，放入 Legacy 分支 |
| `CopyValueTypeToStack(...)` | 空体 | 取消注释，放入 Legacy 分支 |
| `InitializeField(int fieldIdx)` | 空体 | 取消注释，放入 Legacy 分支 |
| `AssignFromStack(int fieldIdx, ...)` | 空体 | 取消注释，放入 Legacy 分支 |
| `Equals(object)` | 仅 `base.Equals()` | 恢复旧逻辑（调 ILType.EqualsMethod）|
| `GetHashCode()` | 仅 `base.GetHashCode()` | 恢复旧逻辑（调 ILType.GetHashCodeMethod）|
| `Clone()` | 有代码但基于旧 fields 遍历逻辑，不兼容 | 修正或恢复旧版本 |
| `GetSizeInMemory()` | 使用 `sizeof(StackObject) * fields.Length`，新模型下错误 | 修正为条件编译 |
| `PushToStackSub()` / `AssignFromStackSub()` | 未注释但引用 StackObject[] 逻辑 | 放入 Legacy 分支 |
| `ILEnumTypeInstance.Clone()` / `ToString()` | 仅存在于 `#if USE_OLD_OBJ_MODEL` | 迁移到 Legacy 分支 |
| `AssignFieldNoClone(...)` | 仅存在于旧宏分支 | 迁移到 Legacy 分支 |
| `Fields` 属性 | 返回 null | Legacy 分支恢复返回 `StackObject[]` |

### 2b. Neo 路径实现（`#if ENABLE_NEO_MODE` 内）

为上述方法编写基于 `byte[] Primitives + AutoList ManagedObjects` 的新实现：

1. **索引器 `this[int index]`** — 通过 `ILTypeFieldOffset` 定位 byte 区域偏移 / ManagedObjects 索引，按字段类型读写
2. **`Clone()`** — `Array.Copy(Primitives)` + 逐个拷贝 ManagedObjects 条目（引用浅拷贝，值类型字段递归 Clone）
3. **`Equals()` / `GetHashCode()`** — 调 ILType.EqualsMethod/GetHashCodeMethod（与旧逻辑相同，只是内部字段访问路径变了）
4. **`InitializeFields(ILType)`** — Neo 模式下构造函数已完成分配，此方法可能退化为空操作或仅处理特殊默认值
5. **`PushToStack` / `CopyToRegister` / `AssignFromStack`** — Neo 模式下这些接口**可能不再需要**（Neo 解释器直接操作 byte* 帧，不经过 StackObject 中转）。如果仍有外部调用（如 DebugService、反射），需提供兼容实现
6. **`CopyValueTypeToStack`** — Neo 模式下值类型直接 CopyBlock，不再需要 StackObject 描述符。但如果有 Legacy 调用入口尚未清理，需提供桥接或抛异常
7. **`ILEnumTypeInstance`** — Neo 模式下 enum 按 underlying type 在 byte[] 中存储，Clone = byte copy，ToString 需读 byte[] 中的值
8. **`GetSizeInMemory()`** — 返回 `fields.Length + managedObjs.Count * IntPtr.Size + 对象头`

### 2c. 接口一致性检查

- 确保 `ILTypeInstance` 的所有 public/internal 方法在两种模式下都有实现（无论是完整逻辑还是明确的 `throw new NotSupportedException` 标记）
- 确保 `CrossBindingAdaptor` 相关路径在 Legacy 模式下行为不变

**依赖**: Step 1（需要 `ENABLE_NEO_MODE` 宏体系先建立）

**验证方式**:
- **Legacy 模式验证**（不定义 `ENABLE_NEO_MODE`）：
  - 编译通过
  - 运行现有全部单元测试，全部通过（行为与重构前完全一致）
  - 特别关注：对象索引器读写、Clone、Equals 比较、值类型入栈出栈
- **Neo 模式验证**（定义 `ENABLE_NEO_MODE`）：
  - 编译通过
  - 单元测试：创建 ILTypeInstance，通过索引器设值/取值
  - Clone 后修改原对象不影响副本
  - 含引用字段的值类型 Clone 正确深拷贝
  - Enum 类型 ToString 输出正确

---

## Step 3: 静态字段适配

**目标**: `ILTypeStaticInstance` 使用新 Object Model。

**内容**:
1. `ILTypeStaticInstance` 构造函数使用 `byte[] + AutoList` 布局
2. `ldsfld` / `stsfld` 指令通过 `StaticInstance.Primitives` + 字段偏移访问
3. 静态构造器触发时机不变

**依赖**: Step 1, Step 2

**验证方式**:
- 单元测试：加载含静态字段的 IL 类型，读写静态字段，验证值正确
- 验证静态构造器在首次访问时触发

---

## Step 4: Neo 帧布局 — byte* 语义

**目标**: 将 `ExecuteNeo` 的帧从 `StackObject*` 改为 `byte*`，实现紧凑帧布局。

**内容**:
1. 修改 `ExecuteNeo` 签名：
   ```csharp
   unsafe void ExecuteNeo(ILMethod method, byte* esp, byte* retDst, int retRefBase)
   ```
2. 帧初始化：
   - `frameBase = esp`
   - `esp += method.FrameSize`（TotalStructSize）
   - `InitBlock(frameBase + ParamPrimitiveSize, 0, LocalsPrimitiveSize)` 清零 locals 区
   - 引用类型 local 的 byte 位置写入 -1（null 约定）
3. 栈溢出检查：`esp + frameSize <= stackEnd`
4. 退帧：`esp -= frameSize`，`mStack.Count = frameRefBase`
5. Frames 栈维护（DebugService 用）
6. 实现 `Ret` 指令的 Neo 版本（CopyBlock retDst + 引用字段拷贝）

**依赖**: Step 1

**验证方式**:
- 编写最简单的 IL 方法（`void Foo() { return; }`），ExecuteNeo 能正确进入和退出
- 验证 esp 在调用前后恢复一致
- 验证 mStack.Count 在调用前后恢复一致
- 验证带返回值的方法（`int Bar() { return 42; }`）retDst 写入正确

---

## Step 5: Box/Unbox 基础（IL 值类型）

**目标**: 实现 IL 值类型在新 Object Model 下的 Box/Unbox。

**内容**:
1. **Box**: `new ILTypeInstance(type)` → `CopyBlock(instance.Primitives, srcBytes, size)` + 引用字段拷贝到 ManagedObjects → 存入 mStack
2. **Unbox**: 从 `ILTypeInstance.Primitives` → `CopyBlock` 到目标 byte 区域 + 引用字段拷贝

**依赖**: Step 2（需要 ILTypeInstance 正确工作）

**验证方式**:
- 单元测试：IL struct box 后能正确 unbox 回原值
- 含引用字段的 struct box/unbox 引用字段不丢失

---

## Step 6: 基本算术/分支/常量加载指令

**目标**: 在 Neo 解释器中实现基础计算能力。

**内容**:
1. 常量加载：`Ldc_I4`, `Ldc_I8`, `Ldc_R4`, `Ldc_R8` — 直接写入 `*(T*)(frameBase + offset)`
2. 数据移动：`Move`（标量）— 帧内 byte 拷贝
3. 算术：`Add_I4`, `Sub_I4`, `Mul_I4`, `Div_I4`, `Add_I8`, `Add_R4`, `Add_R8` 等
4. 比较：`Ceq`, `Clt`, `Cgt` 等
5. 分支：`Br`, `Brtrue`, `Brfalse`, `Beq`, `Blt`, `Bgt` 等
6. 类型转换：`Conv_I4`, `Conv_I8`, `Conv_R4`, `Conv_R8` 等

**关键变化**: 所有 Register 字段语义从"StackObject 数组索引"改为"帧内 byte 偏移"。

**依赖**: Step 4（需要 byte* 帧布局工作）

**验证方式**:
- 纯算术方法：`int Add(int a, int b) { return a + b; }`
- 带分支：`int Max(int a, int b) { return a > b ? a : b; }`
- 循环：`int Sum(int n) { int s = 0; for(int i=0;i<n;i++) s+=i; return s; }`
- 浮点/长整型运算

---

## Step 7: ManagedStack 批量分配 + 引用类型基础

**目标**: 实现 Neo 帧中引用类型 slot 的生命周期管理。

**内容**:
1. 帧入口：`frameRefBase = mStack.Count; mStack.Count += method.TotalRefSize`
2. 帧出口：`mStack.Count = frameRefBase`（O(1) 截断）
3. 引用类型 local/temp 的读写：
   - 帧 byte 区域存 mStack index（-1 = null）
   - 读：`if (index == -1) null else mStack[index]`
   - 写：`mStack[refSlotIndex] = obj; *(int*)(frameBase + offset) = refSlotIndex`
4. `Ldnull` → 写 -1 到帧 byte 位置
5. `Ldstr` → 字符串存入 mStack，index 写到帧
6. 堆对象字段访问（`Ldfld_*` / `Stfld_*` 非 Inline 变体）：
   - 从帧读 mStack index → 取出 ILTypeInstance → 通过 Primitives/ManagedObjects 读写字段

### 7z. Step 6 cctor 回归恢复

Step 6 smoke 临时在 [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) 的 `InitializeMethods` / `StaticInstance` 两处条件编译跳过了 `appdomain.Invoke(staticConstructor)`，原因是 JITCompiler 在 Neo 模式下会为静态构造函数生成 `Stfld_I8` / `Ldfld_*` 等 Step 5 引入的特化字段访问指令，但 Step 5 当时只重构了对象模型，没把这些 case 接到解释器。

本步骤实现完整的 `Stfld_*` / `Ldfld_*` case handler 之后：
1. 删除 [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) 中两处 `#if ENABLE_NEO_MODE` 跳过 cctor 的分支，恢复无条件调用
2. 重跑 Step 6 整组 smoke（`NeoStep6Test` 14 个用例）确认 cctor 触发不破坏任何已通过的算术/分支/常量加载场景
3. 加跑一个新的最小 cctor 用例：定义 `class C { static int X = 42; }`，验证 `C.X` 读到 42（端到端验证 ldsfld + cctor 触发链）

**依赖**: Step 6

**验证方式**:
- 方法内使用 string 变量：赋值、传递、返回
- 验证方法退出后 mStack.Count 恢复
- 多次调用同一方法，mStack 不无限增长
- 堆对象字段读写（对象由测试框架从外部传入作为参数）
- **Step 6 smoke 回归**：14 个 `NeoStep6Test.*` 用例 + 至少一个静态字段读取用例全部通过

---

## Step 8: Neo Call 约定 — IL 方法间调用

**目标**: 实现 Caller-Write + Direct-Return 的零拷贝方法调用。

**内容**:
1. `Call_Neo` 指令实现：
   - Caller 将参数按 paramOffset 写入 esp
   - 调用 `ExecuteNeo(target, esp, retDst, retRefBase)`
2. 参数传递：
   - Primitive 参数：`*(int*)(esp + paramOffset) = *(int*)(frameBase + srcOffset)`
   - Reference 参数：拷贝 mStack index 到 esp 对应位置
3. 返回值：callee Ret 直接写入 caller 的 retDst
4. void 方法：retDst = null, retRefBase = 0

**依赖**: Step 7（需要引用类型管理）

**验证方式**:
- 简单调用：`int Foo() { return Bar(1, 2); } int Bar(int a, int b) { return a+b; }`
- 多参数、混合类型参数
- 返回引用类型
- 递归调用（验证帧不冲突）
- 调用深度测试

---

## Step 8b: 引用类型 newobj

**目标**: 实现 IL 引用类型对象的创建，使后续步骤能完整验证引用类型场景。

**内容**:
1. `Newobj` 指令在 ExecuteNeo 中的引用类型路径：
   - 分配 `new ILTypeInstance(type)` → 存入 mStack 预分配的 ref slot
   - this 的 mStack index 写入 callee 帧 param0（作为第一个参数）
   - 按 Step 8 的 Call 约定调用构造函数（构造函数用 `call` 非 `callvirt`，不需要 VTable）
   - 构造函数返回后，caller 的目标 ref slot 已持有新对象的 index
2. CLR 引用类型 newobj：通过 CLRMethod 构造（依赖 Step 9 完成后补充）

**说明**: 无需新增指令。`Newobj` 操作码不变，operand 仍为目标构造方法引用，只是在 `ExecuteNeo` 的 case handler 中按 Neo 帧布局执行。

**为什么不需要 VTable**: C# 构造函数始终通过 `call`（非虚调用）分派，不经过 vtable。只需要能解析到具体的 ILMethod 构造函数即可。

**依赖**: Step 8（需要 Call 约定）

**验证方式**:
- `var obj = new MyILClass();` — 默认构造函数，对象成功创建
- `var obj = new MyILClass(42, "hello");` — 带参数构造函数，字段初始化正确
- 构造函数中 `this.field = value` 赋值生效
- 创建对象后读写字段
- 对象作为参数传递给其他方法

---

## Step 9: CLRRedirectionDelegateNeo + CLR 方法调用

**目标**: 建立 Neo 模式下调用 CLR 方法的基础设施。

**内容**:
1. 定义 `CLRRedirectionDelegateNeo` 委托类型：
   ```csharp
   unsafe delegate void CLRRedirectionDelegateNeo(
       ILIntepreter intp, byte* frameBase, AutoList mStack, CLRMethod method, bool isNewObj,
       byte* retDst, int retRefBase);
   ```
2. `CLRMethod` 新增 Neo Redirection 注册入口
3. 互斥保护：Neo 模式下注册旧 delegate 抛异常
4. Neo 解释器中 Call CLR 方法的分派逻辑：
   - 有 Redirection → 直接调用
   - 无 Redirection → 通过反射 Invoke（临时 fallback）
5. CLRRediretion自动代码生成器增加Neo模式的Redirection代码生成，当前仅实现到Step9已支持的feature，其他情况仅生成Redirection方法体和读取参数的stub即可

### 9z. Step 6 smoke 断言升级

Step 6 smoke 用例（[TestCases/NeoStep6Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep6Test.cs)）当前受限于 Step 6 时还没有 `Call` / `Throw` / `Ldstr` / `Newobj` 能力，用了一个 hack：检查失败时执行 `int z = 1; int d = 0; int _ = z / d;` 触发 `DivideByZeroException`，借助 harness 把任何未预期异常视为 Failed 这一点来反馈失败。这样的代价是断言粒度极粗，**只能告诉你"某个分支跑到了"，不能告诉你两个比较值具体差多少**，调试 lowering bug 时基本只能靠肉眼读 JIT dump。

Step 9 落地后，CLR 方法（包括 `Console.WriteLine`、`Assert.AreEqual` 等）可以直接被 IL 调用。本步骤完成时回填 Step 6 smoke：
1. 把 `NeoStep6Test.cs` 中所有 `if (cond) { int z = 1; int d = 0; int _ = z / d; }` 替换为：
   - 调用 `Console.WriteLine($"NeoXxx FAIL: expected={...}, actual={...}")` 输出对比值
   - 然后 `throw new Exception("NeoXxx assertion failed")` 或调用测试框架的 `Assert.Fail`
2. 移除文件头部约束注释 "no Call/Ldstr/Newobj/Console/string.Format/Ldsfld"
3. 整组 smoke 重跑，验证升级后断言仍然全绿（同时也是 Step 9 的端到端验证 — `Console.WriteLine` 的 Neo Redirection 工作正常）
4. 在 Step 6 checklist 中把这条工作勾掉

注：Step 6 期间为绕开 cctor 已经禁用 `Ldsfld`，所以测试用例不会触发任何 cctor。但 `Console.WriteLine(string)` 不需要 cctor（CLR 方法），所以即使在 Step 7 cctor 恢复之前也可以做这次回填，只要 Step 9 的 Redirection 已经覆盖 `Console.WriteLine` 即可。

**依赖**: Step 8（需要 Call 约定已建立）

**验证方式**:
- IL 方法调用 CLR 方法：`Console.WriteLine("hello")`
- 带返回值的 CLR 调用：`Math.Max(a, b)`
- 带引用类型参数的 CLR 调用
- **Step 6 smoke 断言升级回归**：`NeoStep6Test` 14 个用例改用 `Console.WriteLine` + throw 后仍全绿

---

## Step 10: VTable 构建 + 虚方法分派

**目标**: 实现编译时 VTable 构建和运行时 O(1) 虚方法分派。

**内容**:
1. `ILType` 新增 `IMethod[] vtable` 字段
2. Slot 编号算法：
   - 基类虚方法占 slot 0..N-1
   - 本类新增虚方法占 slot N..N+M-1
   - override 复用被 override 方法的 slot
3. VTable 构建时机：类型加载时递归构建
4. `Callvirt_IL` 指令：`instance.Type.VTable[slotIndex]` → ExecuteNeo
5. `Callvirt_CLR` 指令：直接调 CLRMethod
6. `Callvirt` 通用指令：运行时判断 this 类型后分派

**后续 TODO（签名匹配）**:
- Step 10 JIT 实现当前可用 `IMethod.SignatureString` 懒加载缓存作为过渡 key，但这不是最终签名匹配方案
- 后续需要参考 Legacy `ILType.GetVirtualMethod()` / `GetMethod(... exactMatch: true)` 的成熟逻辑
- 泛型类型参数、泛型实例方法、`ref` / `out` byref 参数、显式接口实现、CLR/IL 跨模型方法匹配必须由结构化签名比较器或元数据 identity 处理，不能长期依赖字符串拼接
- Step 11 接口分派和 AOT `.neo` VTableTemplate 必须复用同一套签名匹配规则

**依赖**: Step 9（需要 CLR 方法调用能力）

**验证方式**:
- 简单继承：Base.Foo() virtual → Derived.Foo() override → 正确分派
- 多层继承链
- CLR 类型虚方法调用（ToString 等）
- 混合场景（Object 变量持有 IL 或 CLR 对象）

---

## Step 11: 接口方法分派

**目标**: 实现接口的 VTable offset 映射和分派。

**内容**:
1. `ILType` 新增 `InterfaceEntry[] interfaceMap`
2. 接口 slot 编号：每个接口独立 0-based 编号
3. 类型加载时计算 interfaceOffsets（接口方法在 vtable 中的起始 slot）
4. `Callvirt_Interface` 指令：
   ```
   baseSlot = instance.Type.GetInterfaceVTableOffset(interfaceTypeIndex)
   actualMethod = vtable[baseSlot + interfaceMethodSlot]
   ```

**依赖**: Step 10

**验证方式**:
- IL 类实现 IL 接口，通过接口变量调用
- IL 类实现 CLR 接口（如 `IDisposable`）
- 多接口实现
- 接口继承链

---

## Step 12: 帧内值类型 + Inline 字段访问

**目标**: 值类型在帧上以 flat bytes 内联，消除 ValueTypeObjectReference。

**内容**:
1. 值类型 local/temp 在帧 byte 区域中按自然对齐分配连续空间
2. 不再使用 `AllocValueType` / `ValueTypeObjectReference` 描述符模型
3. 新增 `Ldfld_*_Inline` / `Stfld_*_Inline` 指令：
   ```csharp
   case Ldfld_I4_Inline:
       *(int*)(frameBase + ip->Register1) = *(int*)(frameBase + ip->Operand2);
       break;
   ```
4. JIT 编译器根据静态类型（帧内值类型 vs 堆对象引用）生成不同指令变体
5. `Initobj` 对帧内值类型 → memset 0（编译期已知偏移和大小）
6. 帧内值类型字段访问 = 纯指针算术，零分支

**依赖**: Step 6（需要基本指令框架）

**验证方式**:
- `Vector3 v; v.x = 1; v.y = 2; float r = v.x + v.y;`
- 嵌套值类型：`struct Inner { int x; } struct Outer { Inner i; int y; }`
- 含引用字段的值类型在帧上正确存储

---

## Step 12b: Move_Vt + LowerMove Pass

**目标**: 实现值类型赋值/拷贝语义。

**内容**:
1. JIT 编译器新增 `LowerMove` pass（在所有优化 pass 之后运行）：
   - 扫描残留 Move 指令
   - 目标 slot 为值类型 → 替换为 `Move_Vt`
   - 编码 `primitiveSize` 和 `refCount` 到 Operand
2. `Move_Vt` 执行体：
   - `CopyBlock` primitive 部分
   - `refCount > 0` 时逐个拷贝 mStack 引用
3. 确保 BCP/FCP 不受影响（LowerMove 在优化之后）

**依赖**: Step 12

**验证方式**:
- `Vector3 a = b;`（纯 primitive，退化为 CopyBlock）
- 含引用字段的 struct 赋值（引用独立拷贝）
- 方法参数传递值类型
- 验证 BCP/FCP 仍能正常消除 Move

---

## Step 13: Box/Unbox 完整实现

**目标**: 完善所有 Boxing/Unboxing 场景，含 CLR 值类型。

**内容**:
1. IL 值类型 Box/Unbox（Step 5 已做基础，此处完善与帧内值类型的交互）
2. CLR 值类型分两种路径：
   - **有 ValueTypeBinder**: 帧上 flat bytes ↔ CLR 对象 memcpy
   - **无 ValueTypeBinder**: 保持 boxed 在 mStack，方法调用用 `Unsafe.Unbox<T>` in-place
3. `constrained callvirt` 编译期特化
4. Binding 代码生成改造：`Unsafe.Unbox<T>` + 直接调用模式（消除 WriteBackInstance）
5. **CLRMethod 参数区接入统一 Neo slot layout**：
   - CLRMethod 的临时 callee 参数区不得定义独立 ABI；必须复用 `AllocateSlotForType` / `StackSlotInfo` 同一套 primitive/ref slot 规则
   - CLRBinding 生成代码与 `CLRMethod.Invoke(byte*)` 必须通过非泛型读取 helper（如 `ReadNeoInt16` / `ReadNeoBoolean`）按 slot 实际宽度读取参数，不能各自手写宽度推进
   - 移除 [Optimizer.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Neo.cs) 中 CLR 非 primitive struct 参数沿用 caller temp slot 的临时 fallback
   - 为 CLR 值类型参数生成稳定的 callee 参数布局：有 Binder 走 flat bytes，无 Binder 走 boxed / `Unsafe.Unbox<T>` 约定
   - 覆盖 CLR struct by value 参数、返回值、实例方法 `this`、泛型 CLR struct 参数

**依赖**: Step 5, Step 12（帧内值类型）

**验证方式**:
- IL struct box 后存入 object 变量，再 unbox 回来
- `foreach(List<int>)` 验证无 per-iteration GC alloc（无 Binder 路径）
- 有 ValueTypeBinder 的 struct（如 Vector3）box/unbox 正确
- `constrained` 场景：`T.ToString()` where T is struct
- CLR struct 参数调用：`TaskAwaiter.GetResult()` / 自定义 CLR struct by value 方法不依赖 caller temp slot fallback

---

## Step 14: 异常处理 Neo 适配

**目标**: 在 Neo 解释器中完整支持 try/catch/finally。

**内容**:
1. 执行循环外层 C# try-catch 保持不变
2. catch handler 进入时 mStack 恢复：`mStack.Count = frameRefBase + method.TotalRefSize`
3. 异常对象存储：写入 catch 块预分配的 ref slot
4. Frames 栈维护：异常未处理时不 Pop，HandleException 找到匹配后批量 Pop
5. `Leave` / `Endfinally` 指令实现
6. 复用 `GetCorrespondingExceptionHandler` 核心逻辑

**依赖**: Step 8（需要方法调用能力来测试跨帧异常）

**验证方式**:
- 基本 try-catch：`try { throw new Exception(); } catch (Exception e) { ... }`
- try-finally：验证 finally 必定执行
- 嵌套 try-catch
- 跨方法异常传播：callee throw → caller catch
- catch 中访问异常对象

---

## Step 15: isinst / castclass

**目标**: 实现类型检查指令，含编译期 peephole 优化。

**内容**:
1. 编译期 peephole：检测 `box T; isinst U` 模式，静态消解
2. 泛型参数场景：加入 patch 表（`PatchKind.IsinstResult`）
3. 运行时路径（操作数为 object/接口类型时）：
   - `ILTypeInstance` → `CanAssignTo`
   - CLR 对象 → `IsAssignableFrom`
4. 不涉及帧内 flat bytes（检查对象已在 mStack 中）

**依赖**: Step 9（需要 CLR 类型交互）

**验证方式**:
- `obj is MyClass` 为 true/false 两种情况
- `obj as IMyInterface`
- 值类型 box 后 isinst
- 继承链中的类型检查

---

## Step 16: 数组元素访问

**目标**: 实现 ldelem/stelem/ldelema 全类型支持。

**内容**:
1. CLR 基本类型数组（int[], float[] 等）：直接 CLR 数组操作
2. IL 引用类型数组（`MyClass[]` = `object[]`）：元素为 mStack index
3. IL 值类型数组（`MyStruct[]` = `ILTypeInstance[]`）：
   - `ldelem`: CopyBlock from element.Primitives → 帧 byte 区域
   - `stelem`: CopyBlock from 帧 → element.Primitives
4. `ldelema`：产生 Ref Slot `(arrayMStackIndex, elementIndex)`
5. `Newarr` 指令

**依赖**: Step 7（需要引用类型管理）

**验证方式**:
- `int[] arr = new int[10]; arr[0] = 42; return arr[0];`
- 引用类型数组：存取对象
- IL 值类型数组：存取 struct
- 数组越界异常

---

## Step 17: Ref/Out 参数 + ldloca/ldflda

**目标**: 实现统一的 Ref Slot 表示和 stind/ldind 分派。

**内容**:
1. Ref Slot 定义：8 bytes = `(objectIndex: int, offset: int)`
   - objectIndex == -1 → 帧内非托管内存
   - objectIndex >= 0 → mStack 中的对象
2. `Ldloca` 指令：产生 `(-1, absoluteFrameOffset)`
3. `Ldflda` 指令：
   - 堆 IL 对象 → `(objMStackIndex, fieldPrimitiveOffset)`
   - 帧内值类型 → `(-1, absoluteFrameOffset + fieldOffset)`
   - CLR 对象 → `(objMStackIndex, fieldHash)`
4. `stind` / `ldind` 分派：
   - objectIndex == -1 → `*(T*)(nativePointer + offset)`
   - ILTypeInstance → pin Primitives 后读写
   - CLR 对象 → GetFieldValue/SetFieldValue
5. JIT 编译器为 byref slot 分配 8 bytes

**依赖**: Step 4（需要 byte* 帧）

**验证方式**:
- `void Increment(ref int x) { x++; }` — 帧内引用
- `ref field` — 堆对象字段引用
- `out` 参数
- ref 传递帧内值类型的字段

---

## Step 18: 值类型 newobj + CLR 类型 newobj

**目标**: 补全 Step 8b 未覆盖的 newobj 路径（IL 值类型、CLR 类型）。

**内容**:
1. **IL 值类型 newobj**：
   - 目标 slot 在帧上（编译期分配），zero-init
   - this 以 Ref Slot 传递给构造函数（ldloca 产生帧内引用）
   - 构造函数直接操作帧内数据
2. **CLR 类型 newobj**（补充 Step 8b 中的占位）：
   - 有 Redirection → 走 Neo Redirection
   - 无 Redirection → CLRMethod.Invoke 反射创建

**依赖**: Step 8b（引用类型 newobj 基础）, Step 17（Ref Slot，值类型构造函数需要 ref this）

**验证方式**:
- `new MyILStruct(args)` — 帧上值正确
- `new List<int>()` — CLR 类型创建
- 值类型构造函数中 `this.field = value` 赋值生效（通过 Ref Slot 回写帧）

---

## Step 19: 委托 (Delegate)

**目标**: 在 Neo 模式下完整支持委托创建和调用。

**内容**:
1. `ldftn` / `ldvirtftn`：IMethod 引用存入 mStack
2. Delegate 创建：DelegateManager 创建 DelegateAdapter，绑定 instance + method
3. `DelegateAdapter.InvokeILMethod` 适配 Neo calling convention：
   - CLR → IL 方向：将 CLR 参数写入 byte* 帧 → ExecuteNeo → 读返回值
4. 多播委托：沿用 next 链表

**依赖**: Step 8（Call 约定）, Step 9（CLR 互调）, Step 10（虚方法用于 ldvirtftn）

**验证方式**:
- `Action a = Foo; a();`
- `Func<int,int> f = Bar; int r = f(42);`
- 多播：`a += Baz; a();`
- 实例方法委托
- 虚方法委托

---

## Step 20: Async/Await

**目标**: 实现 Builder 全量重定向 + ILAsyncContext 的异步执行模型。

**内容**:
1. `AsyncValueTaskMethodBuilder<T>` / `AsyncTaskMethodBuilder<T>` / `AsyncTaskMethodBuilder` 所有方法注册 Neo Redirection
2. `Start<TSM>(ref sm)` → 直接调 sm.MoveNext()（通过 Ref Slot）
3. `AwaitUnsafeOnCompleted` → 帧→堆拷贝（`new ILTypeInstance(initializeCLRInstance:false)`）+ 创建 ILAsyncContext
4. 实现 `ILAsyncContext<T>` : `IValueTaskSource<T>, IAsyncStateMachine`
5. SetResult / SetException 分同步/异步路径
6. Task getter 分同步完成/异步路径

**依赖**: Step 13（Box/Unbox，state machine 搬堆）, Step 18（Ref/Out）, Step 19（委托，continuation）

**验证方式**:
- 同步完成的 async 方法（所有 await 的 task 已完成）→ 零分配
- 真正异步挂起的方法 → 正确恢复并返回结果
- ValueTask<T> 和 Task<T> 两种返回类型
- 异步方法中的异常传播
- 嵌套 async 调用

---

## Step 21: JIT 编译器 Neo 模式完整改造

**目标**: JIT 编译器能为 Neo 模式生成正确的指令序列。

**说明**: 此步骤贯穿 Step 4-20，随各功能步骤同步进行。此处列出总体改造点供跟踪。

**内容**:
1. **寄存器语义**: Register 字段从 StackObject 索引 → 帧内 byte 偏移
2. **统一 Slot 模型**: 不再区分 local 和 stackReg，统一为 StackSlotInfo
3. **对齐分配**: `AllocateLocalStackSpaces` 按自然对齐分配 slot offset
4. **Calling Convention**: Call 指令编码参数写入 `esp + paramOffset`
5. **字段指令生成**: 编译期区分堆对象/帧内值类型 → `Ldfld_I4` vs `Ldfld_I4_Inline`
6. **值类型初始化**: 不再生成 `AllocValueType`，帧入口统一 InitBlock
7. **LowerMove pass**: 最后阶段替换残留 Move → Move_Vt
8. **peephole 优化**: `box T; isinst U` 模式消解

**依赖**: 贯穿各步骤

**验证方式**: 各步骤的验证同时验证 JIT 输出正确性

---

## Step 22: 泛型方法模板 + Patch

**目标**: 实现泛型方法的模板存储和运行时实例化。

**内容**:
1. 定义 `PatchEntry` 结构（instructionIndex, fieldToPath, PatchKind, genericParamIndex）
2. 泛型方法编译时产生 `templateBody + patches[]`
3. 运行时实例化逻辑：
   - `patches.Length == 0` → 直接复用模板（引用类型泛型参数）
   - 否则 → `CloneAndPatch(templateBody, patches, typeArgs)`
4. 缓存在 ILMethod 实例的 BodyRegister 字段

**依赖**: Step 21（JIT 完整改造）

**验证方式**:
- `List<T>.Add` 对 `int` 和 `string` 实例化产生不同结果
- 引用类型泛型参数共享同一份 OpCodeR[]
- 值类型泛型参数 patch 后帧大小正确

---

## Step 23: NeoAssembly 二进制格式定义

**目标**: 定义并实现 .neo 文件的序列化/反序列化。

**内容**:
1. 文件头：Magic ("ILRN") + Version + Table offsets
2. StringTable / TypeRefTable / MethodRefTable / FieldRefTable
3. TypeDefTable（含 fields[], VTableTemplate, TotalPrimitiveSize, TotalReferenceCount）
4. MethodDefTable（含 OpCodeR[], StackSlotInfo[], ExceptionHandlers, 帧元数据）
5. GenericMethodTemplate（templateBody + patches[]）
6. 序列化器实现（BinaryWriter based）
7. 反序列化器实现（BinaryReader based）

**依赖**: Step 21, Step 22

**验证方式**:
- 序列化→反序列化往返测试（roundtrip）
- 验证所有表的索引引用正确
- 格式版本兼容性检查

---

## Step 24: ilrt_neoc 预编译工具

**目标**: 实现独立的预编译命令行工具。

**内容**:
1. CLI 工具框架（参数解析：输入 DLL、输出 .neo、引用程序集路径）
2. Cecil 加载程序集
3. 对每个方法运行 JITCompiler.Compile()（Neo 模式）
4. 泛型方法生成 templateBody + patches
5. 收集类型元数据（VTable、字段布局）
6. 调用 Step 23 的序列化器输出 .neo 文件
7. 错误报告和诊断信息

**依赖**: Step 23

**验证方式**:
- 对测试 DLL 运行 ilrt_neoc，产生 .neo 文件
- .neo 文件大小合理
- 对比 JIT 编译结果与预编译结果一致

---

## Step 25: 运行时加载器 + Cecil 解耦

**目标**: 实现 .neo 文件的加载，使 `ILType`/`ILMethod` 等运行时类型能脱离 Cecil 元数据独立工作。

**核心挑战**: 当前 `ILType`、`ILMethod`、`ILField` 等在运行时大量访问 Cecil 对象（`TypeDefinition`、`MethodDefinition` 等）。AOT 模式下没有 Cecil DLL，这些类需要支持从 .neo 元数据初始化的替代路径。

**内容**:

### 25a. ILType/ILMethod 的双路径初始化

使 `ILType`、`ILMethod` 支持两种构造方式：
- **JIT 路径**（现有）：从 Cecil TypeDefinition/MethodDefinition 构造
- **AOT 路径**（新增）：从 .neo TypeDefTable/MethodDefTable 构造

需要解耦的运行时 Cecil 依赖：

| 当前 Cecil 依赖 | AOT 替代数据来源 |
|----------------|-----------------|
| `ILType.TypeDefinition.BaseType` | .neo TypeDefTable 存储 baseTypeIndex |
| `ILType.TypeDefinition.Interfaces` | .neo TypeDefTable 存储 interfaceIndices[] |
| `ILType.TypeDefinition.FullName` | .neo StringTable |
| `ILMethod.Definition.Parameters` | .neo MethodDefTable 存储参数类型签名 |
| `ILMethod.Definition.ReturnType` | .neo MethodDefTable 存储返回类型 |
| `ILMethod.Definition.GenericParameters` | .neo 泛型参数描述 |
| 字段元数据（类型、名称） | .neo FieldDefTable |
| ExceptionHandler 范围 | .neo MethodDefTable 内 ExceptionHandlers[] |

### 25b. 加载流程

1. 反序列化类型表 → 创建 ILType 实例（不依赖 Cecil，从 .neo 填充继承链、接口列表、字段偏移）
2. 反序列化方法表 → 方法体直接指向 OpCodeR[]（无需 JIT）
3. CLR 类型引用解析：TypeRefTable → 绑定到 CLR Type
4. CLR 方法引用解析：MethodRefTable → 绑定到 CLRMethod
5. VTable 绑定（一次性 O(slotCount)）
6. 泛型模板注册
7. 类型层次关系构建（BaseType 链、接口实现关系）— 供 isinst/castclass 使用

### 25c. 可选：抽象 ILType/ILMethod 的元数据访问

根据实际代码量决定策略：
- **方案 A**：给 ILType/ILMethod 新增字段存储从 .neo 加载的元数据（冗余存储），AOT 路径不设置 Cecil 引用，运行时按需检查哪个可用
- **方案 B**：将元数据访问抽象为接口/虚方法，Cecil 和 .neo 各自实现
- **推荐方案 A**：改动小，ILType 在 JIT 模式下两套数据都有（Cecil 用于 JIT 编译，自有字段用于运行时），AOT 模式下只有自有字段

**依赖**: Step 23, Step 24

**验证方式**:
- 加载 .neo 文件后执行方法，结果与 JIT 模式一致
- 加载后 `ILType.TypeDefinition` 为 null（证明不依赖 Cecil）
- 类型引用解析正确（CLR 类型能调通）
- isinst/castclass 在 AOT 加载的类型上正确工作
- 反射 API（typeof 等）在 AOT 模式下可用
- 性能对比：加载速度 vs JIT 编译速度

---

## Step 26: 性能验证与边界完善

**目标**: 全面性能基准测试和边界情况处理。

**内容**:
1. 基准测试套件：
   - 字段访问（逐字段读写 vs 批量操作）
   - 方法调用（浅调用 vs 深递归）
   - 值类型操作（创建、拷贝、字段访问）
   - 虚方法分派
   - 数组操作
2. 边界情况：
   - 反射 API 适配（`typeof(T)`、`MethodInfo` 等）
   - 跨域调用
   - 多线程安全性
3. 调试支持：
   - DebugService 读取 Neo 帧变量（通过 CompiledFrame.LocalInfos）
   - 断点、单步执行适配
4. Cross-binding adapter 适配

**依赖**: 所有步骤

**验证方式**:
- 性能数据量化对比（Neo vs Legacy）
- 完整测试套件通过率
- DebugService 能正确显示变量值

---

## 建议执行顺序

```
第一阶段（基础设施）:     Step 1 → Step 2 → Step 3
第二阶段（Neo 帧核心）:   Step 4 → Step 6 → Step 7 → Step 8 → Step 8b
第三阶段（CLR 互操作）:   Step 9 → Step 10 → Step 11
第四阶段（值类型完整）:   Step 12 → Step 12b → Step 5 → Step 13
第五阶段（高级特性）:     Step 14 → Step 15 → Step 16 → Step 17 → Step 18
第六阶段（OOP 完整）:    Step 19 → Step 20
第七阶段（工具链）:       Step 22 → Step 23 → Step 24 → Step 25
第八阶段（收尾）:         Step 26
```

注意事项：
- Step 8b（引用类型 newobj）紧跟 Step 8，此后所有步骤都可创建 IL 对象进行完整验证
- Step 17（Ref Slot）在 Step 18（值类型 newobj）之前，因为值类型构造函数需要 ref this
- Step 21（JIT 改造）贯穿第二至第六阶段，随各功能步骤同步推进

---

## 各步骤预估工作量

| Step | 预估复杂度 | 说明 |
|------|-----------|------|
| 1 | 低 | 宏替换 + 文件整理 |
| 2 | 中-高 | Legacy 恢复 + Neo 新实现，方法多但单个不复杂 |
| 3 | 低 | 与 Step 2 模式相同 |
| 4 | 高 | 核心架构变更，涉及内存管理 |
| 5 | 中 | Box/Unbox 基础场景 |
| 6 | 高 | 大量指令实现（可分批） |
| 7 | 中 | mStack 管理逻辑变更 + 堆对象字段访问 |
| 8 | 高 | 核心 calling convention |
| 8b | 低-中 | 在 Call 基础上增加对象分配，逻辑清晰 |
| 9 | 高 | CLR 互操作基础设施 |
| 10 | 高 | VTable 算法 + 分派指令 |
| 11 | 中 | 在 VTable 基础上扩展 |
| 12 | 高 | 彻底改变值类型存储模型 |
| 12b | 中 | 编译器 pass + 一条指令 |
| 13 | 高 | 多种 CLR 值类型路径 |
| 14 | 中 | 复用已有逻辑，适配简化 |
| 15 | 低 | peephole + 简单分派 |
| 16 | 中 | 多种数组类型 |
| 17 | 高 | Ref Slot 涉及面广 |
| 18 | 中 | 值类型 newobj + CLR newobj（引用类型已在 8b 完成）|
| 19 | 中 | DelegateAdapter 适配 |
| 20 | 高 | 异步模型复杂 |
| 21 | — | 贯穿各步骤 |
| 22 | 中 | 泛型实例化逻辑 |
| 23 | 中 | 格式设计 + 序列化器 |
| 24 | 中 | CLI 工具 |
| 25 | 中 | 加载 + 绑定 |
| 26 | 中 | 测试 + 调优 |
