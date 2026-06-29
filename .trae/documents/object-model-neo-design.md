# ILRuntime 新 Object Model & Neo 解释器 — 完善设计方案

> 基于 `features/object-model-overhaul` 分支现有实现的下一步设计讨论  
> 本文档仅为设计讨论，不涉及代码修改  
>  
> **相关文档**: [Object Model 现有实现总结](./object-model-design.md) — 已完成部分的架构与代码参照

---

## 1. 当前架构的性能瓶颈分析

### 1.1 Call 指令的模式转换开销

当前 `ExecuteR` 中 Call/Callvirt 的执行流程：

```
1. 判断目标方法是 ILMethod 还是 CLRMethod
2. 判断 useRegister = isILMethod && method.ShouldUseRegisterVM
3. 将最多 3 个寄存器参数 CopyToStack(esp, reg, mStack) → esp++
4. 如果 callvirt，还要解析虚方法，可能发现 shouldFix（新旧模式不一致）
5. if (useRegister) → ExecuteR(ilm, esp) 
   else → Execute(ilm, esp)  // 回退到栈式 VM
6. 返回后 PopToRegister 取返回值
```

**核心开销**：
- `CopyToStack` 对每个参数：检查 ValueTypeObjectReference → 分配/复制值类型；否则复制 StackObject + mStack.Add
- 被调方 `ExecuteR` 入口又要逆向处理参数：检查值类型克隆、初始化所有寄存器/mStack 槽位
- `shouldFix` 的 mStack 补偿逻辑：Register VM 多出的 null 占位
- 每次 call 都有 `mStack.Count` 的读取和 `mStack.Add(null)` 的循环

**量化**：一次简单的 `void Foo(int a, int b)` 调用，需要：
- 2× CopyToStack (各含1次 mStack.Add)
- 帧初始化 (InitializeFrame + PushFrame)
- locCnt + stackRegCnt 次 mStack.Add(null)
- 参数的值类型克隆检查循环
- 返回后 PopFrame

### 1.2 字段访问的分支开销

即使使用了类型特化指令（如 `Ldfld_I4`），执行时仍有：
```csharp
if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference) {
    // 栈值类型路径：指针解引用 + domain.GetTypeByIndex + 偏移计算
} else {
    // 堆对象路径：RetriveObject + MemoryMarshal + Unsafe.ReadUnaligned
}
```

堆对象路径中 `RetriveObject` = `mStack[objRef->Value]` + 类型转换。

### 1.3 值类型栈上操作的开销

栈上值类型使用 `ValueTypeObjectReference` + 描述符方式：
- 每次字段访问要先解引用 `*(StackObject**)&objRef->Value` 得到描述符
- 然后 `dst - (fieldIndex + 1)` 计算字段地址
- 初始化时 `AllocValueType` 要在 ValueType 栈上分配连续空间
- 复制时 `CopyStackValueType` 要逐字段拷贝

### 1.4 ManagedStack 的间接开销

每个对象引用都通过 `mStack[index]` 间接访问：
- 寄存器存的是 index，不是直接引用
- 每次获取对象要先取 index，再从 List 中查找
- mStack 持续增长，调用链深时占用大量内存

---

## 2. Neo 解释器设计目标

### 2.1 与当前 ExecuteR 的核心区别

| 方面 | ExecuteR（当前） | ExecuteNeo（目标）|
|------|-----------------|-------------------|
| 输入格式 | DLL → Cecil 解析 → JIT 编译 | 预编译二进制 → 直接加载 OpCodeR[] |
| 方法调用 | 可能 call 到栈式 VM 的方法 | 所有 IL 方法均为 Register VM |
| 模式转换 | 需要 shouldFix 兼容逻辑 | 不存在 |
| 参数传递 | CopyToStack → 被调方再初始化 | 可直接使用调用者栈上的值（零拷贝）|
| 元数据来源 | Cecil MethodDefinition | 自定义序列化格式 |
| 值类型布局 | 混合：栈上用 StackObject[]，堆上用 byte[] | 统一的 byte[] 布局 |

### 2.2 消除模式转换的收益

当前 Call 指令的开销来自于要适配两种模式。在 Neo 中：
- 不再需要 `ShouldUseRegisterVM` 判断
- 不再需要 `CopyToStack`/`PopToRegister` 的参数传递
- 不再需要 `shouldFix` 的 mStack 补偿

---

## 3. Neo 解释器的 Call 设计方案（已确定）

### 3.1 核心设计：Caller-Write + Direct-Return

**基本原则**：调用者直接将参数写入 `esp`（即被调方的帧起始位置），被调方从帧头部直接读取参数开始执行。引用类型参数以 mStack index 形式传递（与 `ILTypeInstance` 中引用字段在 byte[] 中的存储方式一致），callee 无需额外操作即可直接使用。返回值由 callee 直接写入 caller 指定的目标位置（通过 `ExecuteNeo` 参数传入），零中间拷贝。

**帧布局规则**：

```
帧内偏移:
| param0 | param1 | ... | local0 | local1 | ... | stack_regs... |
^                        ^                        ^
0 (= frameBase)         ParamPrimitiveSize       locals 区
                         (= locals 起始偏移)
```

- 参数在帧头部，偏移固定，由方法签名决定
- 参数区大小在所有虚方法实现中相同（因为签名相同）
- locals 和 stackRegs 在参数之后，大小因实现不同而异

### 3.2 完整 Call 流程

```
ExecuteNeo 签名:
  void ExecuteNeo(ILMethod method, byte* esp, byte* retDst, int retRefBase)
    - esp:        callee 帧起始位置（参数已写好）
    - retDst:     返回值 primitive 部分的目标地址（caller 帧内）
    - retRefBase: 返回值 引用部分的 mStack 绝对起始 index（caller 分配）

Caller:
  1. 将参数按目标方法的 param offset 写入 esp
     - primitive 参数：直接写值  *(int*)(esp + paramOffset) = value
     - reference 参数：写 mStack index  *(int*)(esp + paramOffset) = srcMStackIndex
  2. 调用 ExecuteNeo(targetMethod, esp, frameBase + ip->Register1, dstRefBase)
     - retDst = caller 的目标 temp slot 的 byte 地址
     - retRefBase = 该 slot 对应的 mStack ref 起始位置
  3. 调用返回后，返回值已在 caller 的 temp slot 中，无需额外操作
  4. esp 不变（callee 自行管理了 esp 的推进和回退）

Callee:
  1. frameBase = 传入的 esp
     （params 已由 caller 写好，直接可用）
  2. esp += method.FrameSize （推进到自己帧尾部）
  3. 为自己的引用类型 locals 分配 mStack 槽:
     localRefBase = mStack.Count
     mStack.Count += method.TotalRefSize
     将新槽 index 写入对应 local 的帧位置
  4. Unsafe.InitBlock(frameBase + ParamPrimitiveSize, 0, LocalsPrimitiveSize)
     （仅清零 locals 区，params 保留 caller 写入的值）
  5. 执行方法体 ...
  6. Ret:
     - 将返回值直接写到 caller 指定的目标位置：
       CopyBlock(retDst, frameBase + srcOffset, retPrimitiveSize)
       for i in 0..retRefCount: mStack[retRefBase + i] = mStack[srcRefBase + i]
     - mStack.Count = localRefBase （释放 locals 的引用槽）
     - return
```

对于 void 方法，retDst 传 null，retRefBase 传 0，Ret 不做任何写入。

### 3.3 引用类型参数为什么不需要额外处理

帧 byte* 区域中，引用类型槽位存储的是 **mStack 中的 index**（-1 表示 null）。这与 `ILTypeInstance.Primitives` 中引用字段的存储方式一致——byte 区域存 index，实际对象在 mStack（或 ManagedObjects）中。

因此：
- Caller 传引用参数 = 把自己持有的 mStack index 复制到 callee 帧的对应 byte 位置
- Callee 读引用参数 = 从自己帧的 byte 位置读出 index，直接 `mStack[index]` 取对象
- 无需 caller 帮 callee 预分配 mStack 空间
- 无需任何 mStack.Add 操作

### 3.4 虚方法的处理

虚方法不同实现的帧大小不同（locals 数量不同），但**参数区布局完全相同**（由方法签名决定）。因此：
- Caller 只需要知道目标方法签名 → 写参数到 esp
- Callee 自己知道自己的 FrameSize → 自行推进 esp

虚分派逻辑：
```csharp
case Call_Neo:
    // 写参数到 esp ...
    ExecuteNeo(resolvedMethod, esp, frameBase + ip->Register1, dstRefBase);
    break;

case Callvirt_Neo:
    // 写参数到 esp ...
    // 解析虚方法（通过 this 的类型查 vtable）
    actualMethod = ResolveVirtual(*(int*)(esp + thisOffset), declaredMethod);
    ExecuteNeo(actualMethod, esp, frameBase + ip->Register1, dstRefBase);
    break;
```

虚分派的唯一额外开销 = vtable 查找，无任何帧布局兼容性问题。

### 3.5 返回值约定

返回值由 callee 通过 `retDst` / `retRefBase` 参数直接写入 caller 指定的目标位置：
- Caller 的接收 slot 是普通的 temp slot，编译器在 `AllocateLocalStackSpaces` 中分配
- 该 slot 的 ref 空间属于 caller 的 TotalRefSize（callee 截断 mStack 不影响它）
- Ret 本质上是一次跨帧的 Move_Vt：CopyBlock primitive + 拷贝引用字段
- void 方法不写任何东西（retDst = null）
- 无引用字段时（int, float, Vector3 等）退化为一次 CopyBlock，无 mStack 操作

### 3.6 与当前 ExecuteR Call 的性能对比

| 操作 | 当前 ExecuteR | Neo |
|------|-------------|-----|
| 参数准备 | CopyToStack × N（含 mStack.Add、ObjectType 判断、值类型克隆） | 直接内存写 × N |
| 帧初始化 | InitializeFrame + PushFrame + 参数克隆循环 + mStack.Add(null) × (loc+stackReg) | esp += size + InitBlock(locals 区) |
| mStack 管理 | 逐个 Add(null) 为每个寄存器 | 仅 Count += localRefCount |
| 模式兼容 | shouldFix、ShouldUseRegisterVM 判断 | 不存在 |
| 返回值 | CopyToStack → PopToRegister | Ret 直接写 caller slot（零中间拷贝）|
| 退帧 | PopFrame（含 mStack 清理） | mStack.Count = localRefBase |

一次 `void Foo(int a, int b)` 调用：当前约 20+ 次函数调用/分支判断；Neo 下约 2 次内存写 + 1 次 InitBlock + 1 次 Count 操作。

---

## 4. Neo 帧内存布局：彻底摆脱 StackObject

### 4.1 核心决策

**在 Neo 解释器中，局部变量不再使用 `StackObject` 格式，而是与 `ILTypeInstance` 相同的紧凑 byte 布局。**

理由：
- 每个局部变量/寄存器的类型在预编译期已完全确定
- 类型特化指令（`Ldfld_I4`、`Add` 等）已经编码了数据类型，不需要运行时 `ObjectType` 标记
- 统一布局后，栈上值类型和堆对象字段访问共用同一代码路径

### 4.2 内存来源

**直接复用 `RuntimeStack.nativePointer`**（`AllocHGlobal` 分配的非托管堆内存），不使用 `stackalloc`。

原因：当前 `ExecuteR`/`ExecuteNeo` 是巨大的 switch-case 方法，单方法的线程栈帧已经很大。在 iOS/Xcode 不开 O3 时，递归 10 多层就会爆线程栈。`stackalloc` 从线程栈分配，会加剧这个问题。而 `nativePointer` 在进程堆上，不受线程栈大小限制。

### 4.3 Neo 帧布局：统一的 Slot 模型

**在 Neo 中不存在"栈寄存器"与"局部变量"的区分。** 所有变量（用户声明的 locals + 编译器生成的临时值）统一为帧 byte 区域中有确定偏移和大小的 slot。

**内存布局原则**：Neo 中值类型（无论在帧上还是在 `ILTypeInstance.Primitives` 中）的字段布局必须与 CLR 运行时的 `StructLayout` 一致——字段按自然对齐排列，必要时插入 padding。这保证了：
1. 所有指针解引用都是自然对齐的（`int` 对齐到 4，`long`/`double` 对齐到 8），无需 `ReadUnaligned`
2. 帧上值类型与堆对象 `ILTypeInstance.Primitives` 布局完全一致，memcpy 互通无需格式转换
3. 有 `ValueTypeBinder` 的 CLR 值类型可以在栈 byte 区域与 CLR 对象之间直接 memcpy（blittable 时）

`AllocateLocalStackSpaces` 在分配每个 slot 的 offset 时，按该 slot 类型的对齐要求向上对齐（与 CLR field layout 算法一致）。这是编译期行为，运行时零开销。

```
RuntimeStack.nativePointer 内的布局:
                                                                      
低地址 ────────────────────────────────────────────────────── 高地址
| 调用者帧 ... | 当前帧 primitives                    | esp →
                ^                                      ^
                frameBase                              frameBase + frameSize

当前帧 primitives 的内部结构（编译期确定布局）:
| param0 | param1 | ... | slot0 | slot1 | slot2 | ... | slotN |
  4B       8B              所有 locals 和 temps 按自然对齐排列（可能有 padding）
```

- `esp` 语义变为 `byte*`（纯数据指针），不再是 `StackObject*`
- 帧大小 = `CompiledFrame.TotalStructSize`（编译期已知的所有原始数据总字节数，含对齐 padding）
- 每个 slot（无论是用户 local 还是编译器 temp）通过 `StackSlotInfo.Offset` 确定在帧内的字节偏移
- 退帧时 `esp -= frameSize` 回退即可
- BCP/FCP 优化减少 temp slot 数量 → 减小帧大小（但不影响正确性）

### 4.4 为什么不再区分栈寄存器和局部变量

在旧 `StackObject` 模型中，栈寄存器和局部变量在格式上都是 12 字节的 StackObject，只是生命周期不同。但在 Neo 的 byte* 帧中：

- `int` 变量 = 4 bytes
- `Vector3` 值类型 = 12 bytes
- `object` 引用 = 4 bytes（mStack index）

所有类型都按实际大小占用空间。"临时中间值"和"局部变量"在内存模型上没有任何区别——都是帧中一段有固定偏移的 bytes。区别仅在于：

- **用户声明的 local** → 生命周期覆盖整个方法
- **编译器生成的 temp** → 生命周期短（可通过 liveness 分析复用空间，但非必须）

编译器在 `AllocateLocalStackSpaces` 时统一为所有 slot 分配偏移。

### 4.5 引用类型的存储

原始类型（int/float/long/值类型字段）存在 `nativePointer` 的 byte 区域。

引用类型**必须**存在 GC 可见的地方（否则会被回收），因此仍使用 ManagedStack（`AutoList`）：
- 每个帧的引用槽通过 `StackSlotInfo.RefOffset` 确定在 mStack 中的索引
- 帧入口时批量预分配 `mStack.Count += TotalRefSize`
- 退帧时 `mStack.Count = frameRefBase`（O(1)）

**Null 约定**：引用类型 slot 在 byte 区域中存储的 mStack index 使用 **-1 表示 null**。读取引用时先判断 index == -1，是则为 null，否则 `mStack[index]` 取对象。帧入口 `InitBlock` 清零 locals 区后 index 为 0，因此需要在初始化时将引用类型 local 的 byte 位置显式写入 -1（或 InitBlock 改为 memset 0xFF 再单独清零 primitive 部分——实践中逐个写 -1 更简单，因为引用类型 local 通常很少）。

### 4.6 值类型与基本类型统一

在此方案下，**值类型局部变量不再需要特殊处理**：
- `int local0` → 在帧 byte 区域偏移 0 处占 4 字节
- `long local1` → 偏移 4 处占 8 字节
- `MyStruct local2`（含 int x, int y）→ 偏移 12 处占 8 字节，字段 x 在偏移 12，字段 y 在偏移 16
- `object local3` → 在 byte 区域占 4 字节（存 mStack index），实际对象在 mStack[refOffset] 中

值类型不需要描述符、不需要 `AllocValueType`、不需要 `ValueTypeObjectReference`——它们就是帧中连续的一段 bytes。

### 4.7 与堆对象的对应关系

```
堆对象 (ILTypeInstance):
  byte[] Primitives     → MemoryMarshal.GetArrayDataReference() 得到 baseAddr
  AutoList ManagedObjects → refs[index]

栈局部变量 (Neo 帧):
  byte* frameBase       → frameBase + slotInfo.Offset 得到 baseAddr
  AutoList mStack       → mStack[frameRefBase + slotInfo.RefOffset]
```

两者结构完全同构，字段访问指令只需获取 `baseAddr` 的方式不同。

---

## 5. 统一字段访问指令

### 5.1 目标

消除 `Ldfld_I4` 中的 `if (ValueTypeObjectReference) ... else ...` 分支。

### 5.2 设计

在 Neo 中，`Register1`/`Register2` 的语义变为**帧内 byte 偏移**（不再是 StackObject 数组索引）。对于字段访问，编译期已知目标是局部值类型还是堆对象引用，生成不同指令：

```
Ldfld_I4          → 堆对象字段访问（对象引用存在帧中，值为 mStack 绝对 index）
Ldfld_I4_Inline   → 帧内值类型字段访问（数据直接内联在帧 byte 区域）
```

命名规则：`_Inline` 后缀表示字段数据直接嵌入帧的 byte 区域（值类型），对应的 Stfld 同理（如 `Stfld_I4_Inline`）。

帧内值类型路径：
```csharp
case Ldfld_I4_Inline:
    // ip->Operand2 = 帧内绝对偏移 (slotInfo.Offset + fieldOffset.PrimitiveOffset)
    // ip->Register1 = 目标变量的帧内 byte 偏移
    *(int*)(frameBase + ip->Register1) = *(int*)(frameBase + ip->Operand2);
    break;
```

堆对象路径：
```csharp
case Ldfld_I4:
    // ip->Register2 = 对象变量在帧中的 byte 偏移（该位置存储 mStack 绝对 index）
    int objIndex = *(int*)(frameBase + ip->Register2);
    ilInstance = (ILTypeInstance)mStack[objIndex];
    byte* baseAddr = Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(ilInstance.Primitives));
    *(int*)(frameBase + ip->Register1) = *(int*)(baseAddr + ip->Operand2);
    break;
```

### 5.3 更激进的统一方案

如果将栈值类型也包装为 `ILTypeInstance`（生命周期 = 帧），则两者完全统一。但会引入堆分配。
权衡后，**编译期区分指令变体**是更务实的选择——零运行时分支，纯指针算术。

---

## 6. Neo 解释器的 ManagedStack 改进

### 6.1 当前问题

ManagedStack（`AutoList` = `List<object>` 或 `UncheckedList<object>`）：
- 持续 Add/RemoveRange，容量反复扩张
- 每个寄存器都需要一个 mStack 槽位（即使是纯 int 值）
- GC 扫描压力大

### 6.2 Neo 的做法（与第 4 节帧布局一致）

由于 Neo 帧中原始类型变量只存在 `nativePointer` 的 byte 区域，**mStack 只为引用类型变量服务**：

- 帧入口：`int frameRefBase = mStack.Count; mStack.Count += TotalRefSize;`（一次批量扩展）
- 帧退出：`mStack.Count = frameRefBase;`（O(1) 截断，无逐个清理）
- int/float/long/值类型局部变量**不占** mStack 槽位

这大幅减少了 mStack 的操作次数和 GC 扫描范围。

---

## 7. CLR Redirection 接口（已确定）

### 7.1 新旧模式不兼容

Neo 帧是 `byte*` 紧凑布局，旧模式帧是 `StackObject*`，两种 Redirection 实现无法互用。

**规则**：
- Neo 模式下注册旧版 `CLRRedirectionDelegate` → 抛异常
- 旧模式下注册 Neo 版 `CLRRedirectionDelegateNeo` → 抛异常
- 两套 Redirection 各自独立注册、独立调用

### 7.2 新签名

```csharp
// 旧（保留不动）:
unsafe delegate StackObject* CLRRedirectionDelegate(
    ILIntepreter intp, StackObject* esp, AutoList mStack, CLRMethod method, bool isNewObj);

// Neo（新增）:
unsafe delegate void CLRRedirectionDelegateNeo(
    ILIntepreter intp, byte* frameBase, AutoList mStack, CLRMethod method, bool isNewObj,
    byte* retDst, int retRefBase);
```

### 7.3 签名差异说明

| 方面 | 旧签名 | Neo 签名 |
|------|--------|----------|
| 返回类型 | `StackObject*`（新 esp 位置）| `void`（返回值已写入 retDst）|
| 帧指针 | `StackObject* esp`（参数在 esp 下方）| `byte* frameBase`（参数在 frameBase 起始）|
| 读 int 参数 | `(esp - N)->Value` | `*(int*)(frameBase + paramOffset)` |
| 读 ref 参数 | `mStack[(esp - N)->Value]` | `mStack[*(int*)(frameBase + paramRefOffset)]` |
| 写返回值 | push 到 esp 然后返回 esp+1 | 写到 `retDst`（primitive）或 `mStack[retRefBase] = obj; *(int*)retDst = retRefBase`（reference）|

### 7.4 Neo Redirection 实现示例

```csharp
// 示例: int Mathf.Max(int a, int b) 的 Neo Redirection
unsafe static void MathfMax_Neo(ILIntepreter intp, byte* frameBase, AutoList mStack, CLRMethod method, bool isNewObj,
    byte* retDst, int retRefBase)
{
    int a = *(int*)(frameBase + 0);   // param0 offset
    int b = *(int*)(frameBase + 4);   // param1 offset
    *(int*)retDst = Math.Max(a, b);   // 返回值直接写入 caller 目标
}
```

### 7.5 自动生成绑定代码

由于 Neo Redirection 的参数读写是基于偏移的，绑定代码生成器需要在生成时计算每个参数的 `paramOffset`。这可以从方法签名静态推导（按参数类型依次累加 size）。

---

## 8. 预编译二进制格式设计

### 8.1 与 HybridPatch 格式的关系

HybridPatch 已有的序列化基础：
- 类型/方法/字段/字符串引用均索引化
- `MethodPatchInfo` 包含 CodeBody（OpCode[]）、LocalVariables、ExceptionHandlers
- ExceptionHandlerPatchInfo 包含完整的 try/catch/finally 范围

Neo 的预编译格式是 HybridPatch 的超集：
- HybridPatch 保存的是 CIL OpCode[]（栈式指令）
- Neo 需要保存的是 OpCodeR[]（寄存器指令）+ CompiledFrame 元数据

### 8.2 Neo 预编译格式草案

```
NeoAssembly 文件结构:
├── Header
│   ├── Magic ("ILRN")
│   ├── Version
│   └── Table offsets
├── StringTable        // 所有字符串常量，索引化
├── TypeRefTable       // 类型引用表（含 IL 和 CLR 类型）
├── MethodRefTable     // 方法引用表
├── FieldRefTable      // 字段引用表
├── TypeDefTable       // IL 类型定义
│   ├── Fields[]       // 字段定义 + ILTypeFieldOffset
│   ├── TotalPrimitiveSize
│   └── TotalReferenceCount
├── MethodDefTable     // IL 方法定义
│   ├── OpCodeR[]      // 预编译的寄存器指令
│   ├── StackSlotInfo[]// 局部变量栈布局
│   ├── ExceptionHandlers[]
│   ├── ParameterCount
│   ├── TotalStructSize   // 帧 byte 区域总大小
│   └── TotalRefSize      // 引用类型 slot 总数
└── InitializerTable   // 静态构造器列表
```

### 8.3 预编译工具流程

```
原始 DLL
    │
    ▼
ILRuntimeNeoCompiler (ilrt_neoc)
    ├── Cecil 加载程序集
    ├── 对每个方法运行 JITCompiler.Compile()
    ├── 泛型方法生成 templateBody + patches[]
    ├── 序列化 CompiledFrame + 类型元数据
    └── 输出 .neo 文件
    │
    ▼
运行时加载:
    ├── 反序列化类型/方法/字段表
    ├── 绑定 CLR 类型引用
    ├── 方法体直接指向 OpCodeR[]（无需 JIT）
    └── ExecuteNeo() 直接执行
```

注意：`ilrt_neoc` 与 `PatchTool` 是独立的工具。PatchTool 专门用于 HybridPatch 的 AOP 补丁生成，不负责 Neo 预编译。

### 8.4 关键设计决策

1. **引用解析时机**：类型/方法引用在加载时一次性解析为直接指针/索引，执行时不再查表
2. **泛型实例化**：泛型方法/类型需要在预编译时展开所有使用到的实例化组合，或保留模板 + 运行时实例化
3. **CLR 互操作**：CLR 方法调用仍需要通过 Redirection 或 binding，但参数传递无需模式转换

---

## 9. 实现路线图（建议优先级）

### Phase 1：完善堆对象的新 Object Model

- [ ] 完成 `ILTypeInstance` 被注释掉的方法重写（Clone、Equals、GetHashCode 等）
- [ ] 静态字段的新模型适配（`ILTypeStaticInstance`）
- [ ] Box/Unbox 适配新内存布局
- [ ] 数组元素的新模型支持
- [ ] 在现有 `ExecuteR` 中验证正确性

### Phase 2：栈上值类型的新布局

- [ ] 实现帧级平坦内存池（`localPrimitives` + `localRefs`）
- [ ] 修改 `Initobj` 指令使用新的值类型初始化
- [ ] 统一 `Ldfld_*`/`Stfld_*` 对栈值类型和堆对象的处理
- [ ] 值类型参数传递的新方式

### Phase 3：Neo 解释器核心

- [ ] 设计并实现新的 Call 指令（Caller-Allocate 方案）
- [ ] 实现 ManagedStack 批量预分配
- [ ] 去除所有模式转换代码
- [ ] Ret 指令直接写入调用者寄存器

### Phase 4：预编译工具链

- [ ] 定义 NeoAssembly 二进制格式
- [ ] 实现 ilrt_neoc 预编译工具（独立于 PatchTool）
- [ ] 实现运行时加载器
- [ ] 泛型实例化处理

### Phase 5：性能验证与完善

- [ ] 基准测试对比（字段访问、方法调用、值类型操作）
- [ ] 边界情况处理（反射、委托、跨域调用）
- [ ] 调试支持适配

---

## 10. 讨论主题索引

| 节 | 主题 | 状态 |
|----|------|------|
| 11 | 泛型方法预编译策略 | 已确定 |
| 12 | ValueType Copy 语义与 Move_Vt 指令 | 已确定 |
| 13 | 虚方法表 (VTable) 设计 | 已确定 |
| 14 | 编译模式与兼容性策略 | 已确定 |
| 15 | Ref/Out 参数与 ldloca/ldflda | 已确定 |
| 16 | CLR 类型对象的通盘考虑 | 已确定 |
| 17 | 异常处理（try/catch/finally） | 已确定 |
| 18 | Boxing/Unboxing | 已确定 |
| 19 | newobj 的返回值处理 | 已确定 |
| 20 | 栈溢出保护 | 已确定 |
| 21 | 委托（Delegate） | 已确定 |
| 22 | 数组元素访问 | 已确定 |
| 23 | 静态字段 | 已确定 |
| 24 | isinst/castclass 适配 | 已确定 |
| 25 | ldelema 与 CLR 值类型嵌套字段引用 | 已确定 |
| 26 | Async/Await 与 ValueTask | 已确定 |

---

## 11. 泛型方法预编译策略（已确定）

### 11.1 背景

每个泛型实例化在运行时是独立的 ILMethod 实例（`List<int>.Add` 和 `List<string>.Add` 是不同对象），各自持有自己的 `BodyRegister`（OpCodeR[]）。不需要额外的 Dictionary 缓存。

### 11.2 方案：模板 + Patch 表

预编译时不做全展开，只为每个泛型方法定义存储一份模板指令 + patch 描述：

```
GenericMethodTemplate:
  OpCodeR[] templateBody;      // 模板指令（泛型相关 operand 用占位符）
  PatchEntry[] patches;        // 需要根据泛型参数填充的位置

struct PatchEntry {
  int instructionIndex;        // 哪条指令
  int fieldToPath;             // OpCodeR 中的哪个字段 (Operand/Operand2/...)
  PatchKind kind;              // TypeSize / FieldOffset / MethodToken / TypeIndex
  int genericParamIndex;       // 对应第几个泛型参数
}
```

### 11.3 运行时实例化

```csharp
// ILMethod.BodyRegister 属性访问时触发
if (patches.Length == 0)
    // 方法体完全不依赖泛型参数 → 所有实例化共享同一份 OpCodeR[]
    ilMethod.BodyRegister = templateBody;
else
    // clone + patch → 存入该 ILMethod 实例自己的 bodyRegister 字段
    ilMethod.BodyRegister = CloneAndPatch(templateBody, patches, typeArgs);
```

### 11.4 引用类型 vs 值类型

- **T 为引用类型**：帧中统一表示为 4 字节 mStack index，字段访问走动态类型 → patches 通常为空，直接共享模板
- **T 为值类型**：帧大小/字段偏移/sizeof 等随 T 变化 → 需要 clone + patch

### 11.5 开销

| 方面 | 开销 |
|------|------|
| 预编译体积 | 每个泛型方法只存 1 份模板 + patch 表 |
| 首次实例化 | Array.Copy + 遍历 patches（通常很少）|
| 后续调用 | 零额外开销（BodyRegister 已缓存在 ILMethod 实例上）|
| 引用类型实例化 | 零内存开销（直接复用模板数组引用）|

---

## 12. ValueType Copy 语义与 Move_Vt 指令（已确定）

### 12.1 问题

C# 值类型赋值/参数传递 = 深拷贝。在 Neo 的 byte* 帧中：
- 纯 primitive 值类型（如 `Vector3`）→ 直接 `CopyBlock` 即可
- 含引用字段的值类型 → `CopyBlock` 只拷贝 byte 区域（primitive + mStack index），引用对象本身需要正确拷贝到目标 slot 在 mStack 中的独立位置

### 12.2 确定方案：Move_Vt 指令 + LowerMove pass

**原则：不增加运行时查表开销，不干扰 BCP/FCP 优化。**

执行流程：
1. JIT 编译照常生成 `Move` 指令（不区分值类型）
2. BCP/FCP 优化照常运行（尽量消除 Move）
3. **所有优化 pass 结束后**，运行一个 `LowerMove` pass：
   - 扫描残留的 Move 指令
   - 根据 `StackSlotInfo` 判断目标 slot 是否为值类型
   - 如果是：替换为 `Move_Vt`，将 `primitiveSize` 和 `refCount` 编码到 Operand 中
   - 如果不是（纯 int/float/ref 等标量）：保持 Move 不变

### 12.3 Move_Vt 的执行语义

```csharp
case Move_Vt:
    // ip->Register1 = dst byte offset
    // ip->Register2 = src byte offset
    // ip->Operand  = primitiveSize（整个值类型在 byte 区域的大小）
    // ip->Operand2 = refCount（引用字段数量）
    
    // 拷贝 primitive 部分
    Unsafe.CopyBlock(frameBase + ip->Register1, frameBase + ip->Register2, (uint)ip->Operand);
    
    // 拷贝引用部分（将源 slot 引用槽中的对象拷贝到目标 slot 自己的引用槽）
    if (ip->Operand2 > 0) {
        int dstRefBase = /* 从 Register1 对应的 StackSlotInfo.RefOffset 得到 */;
        int srcRefBase = /* 从 Register2 对应的 StackSlotInfo.RefOffset 得到 */;
        for (int i = 0; i < ip->Operand2; i++)
            mStack[dstRefBase + i] = mStack[srcRefBase + i];
    }
    break;
```

对于 `refCount == 0`（大多数常用 struct：Vector3, Quaternion, Color, Rect 等），只执行一次 `CopyBlock`，零循环。

### 12.4 不含引用字段的快速路径

`refCount == 0` 时 `Move_Vt` 退化为纯 `CopyBlock`。也可以考虑不区分——因为 `if (ip->Operand2 > 0)` 是一次分支预测几乎必中的判断（大多数值类型无引用字段），开销可忽略。

### 12.5 为什么不影响 BCP/FCP

- BCP/FCP 看到的始终是普通 `Move`，正常执行消除
- `LowerMove` 是所有优化 pass 之后的最后一步（Lowering 阶段）
- 被 BCP/FCP 消除的 Move 不会出现在最终代码中，自然不需要 Lower
- 只有无法消除的残留 Move（极少数情况）才会被替换为 `Move_Vt`

### 12.6 其他指令中的值类型拷贝

除了显式赋值（Move），以下场景也涉及值类型拷贝，但**内嵌在指令执行体中**，不产生额外指令：

| 场景 | 处理方式 |
|------|---------|
| Call 传值类型参数 | Call 指令执行体内部 CopyBlock 写参数到 callee 帧 |
| Ldfld 加载值类型字段 | `Ldfld_VT` 指令内部 CopyBlock 到目标 slot |
| Stfld 写入值类型字段 | `Stfld_VT` 指令内部 CopyBlock 到对象字段区域 |
| Ret 返回值类型 | Ret 指令内部 CopyBlock 到 retDst + 引用字段拷贝到 retRefBase |

所有这些都是一对一指令替换或指令内部逻辑，**不增加指令 dispatch 次数**。

### 12.7 JITCompiler 改造总结

针对 Neo 的 JITCompiler 需要以下改造：

| 阶段 | 改造内容 |
|------|---------|
| 寄存器分配 | Register 字段含义从 StackObject 索引改为帧内 byte 偏移 |
| Calling Convention | Call 指令编码参数写入 `esp + paramOffset` 的语义 |
| Ldfld/Stfld 生成 | 编译期区分堆对象/帧内值类型，生成 `Ldfld_I4` 或 `Ldfld_I4_Inline` |
| 值类型初始化 | 不再生成 `Initobj`/`AllocValueType`，帧入口 InitBlock 清零 |
| LowerMove pass | 所有优化结束后，残留 Move 按类型替换为 Move_Vt |
| 帧布局计算 | `AllocateLocalStackSpaces` 统一为所有 slot 分配 byte 偏移和大小 |

---

## 13. 虚方法表 (VTable) 设计（已确定）

### 13.1 当前问题

ILRuntime 当前没有 vtable。每次 `callvirt` 的分派路径：
1. `Dictionary<string, List<ILMethod>>` 按方法名查找
2. 线性遍历同名方法列表，逐参数类型比对
3. 如果未找到，递归向 `BaseType.GetVirtualMethod()` 查找
4. 接口实现用 `"接口全名.方法名"` 格式作为额外 key

无任何缓存（除 ToString/Equals/GetHashCode 三个特例）。

### 13.2 Neo 方案：编译时构建 VTable 模板，加载时绑定

**ILType 新增字段**：
```csharp
IMethod[] vtable;  // vtable[slotIndex] = 实际方法
```

**Slot 编号规则（编译时由 ilrt_neoc 确定）**：
```
基类虚方法:      slot 0 .. N-1（按声明顺序，CLR 基类从引用程序集元数据获得）
本类新增虚方法:  slot N .. N+M-1
override:       不新增 slot，复用被 override 方法的 slot
```

**编译时输出**（存入 .neo 文件的 TypeDefTable）：
```
VTableTemplate:
  int slotCount;
  VTableEntry[] entries;

struct VTableEntry {
  VTableEntryKind kind;    // ILMethod / CLRMethodRef
  int methodIndex;         // IL 方法 → MethodDefTable 索引; CLR → MethodRefTable 索引
}
```

**加载时绑定**（一次性 O(slotCount)）：
```csharp
ilType.VTable = new IMethod[template.slotCount];
for (int i = 0; i < template.slotCount; i++) {
    if (entries[i].kind == ILMethod)
        ilType.VTable[i] = GetILMethod(entries[i].methodIndex);
    else
        ilType.VTable[i] = ResolveCLRMethod(entries[i].methodIndex);
}
```

### 13.3 OpCodeR 编码

```
Callvirt_Neo:
  ip->Operand  = vtable slot index（编译期确定）
  ip->Register2 = this 参数在帧中的 byte offset
```

### 13.4 运行时分派：三种 Callvirt 变体

编译期根据 this 的静态类型生成不同指令：

| 指令 | 使用条件 | 分派方式 |
|------|---------|---------|
| `Callvirt_IL` | 编译期确定 this 是 IL 类型 | 直接 `vtable[slotIndex]`，无分支 |
| `Callvirt_CLR` | 编译期确定 this 是 CLR 类型 | 直接调用 CLRMethod |
| `Callvirt` | 编译期无法确定（接口变量、Object 等） | 运行时判断后分派 |

```csharp
case Callvirt_IL:
    // 快速路径：编译期确定 this 是 IL 类型
    int thisIndex = *(int*)(frameBase + ip->Register2);
    ILTypeInstance instance = (ILTypeInstance)mStack[thisIndex];
    ILMethod actualMethod = (ILMethod)instance.Type.VTable[ip->Operand];
    ExecuteNeo(actualMethod, esp);
    break;

case Callvirt_CLR:
    // 快速路径：编译期确定 this 是 CLR 类型
    int thisIndex = *(int*)(frameBase + ip->Register2);
    object clrObj = mStack[thisIndex];
    CLRMethod clrMethod = (CLRMethod)methodTable[ip->Operand];
    InvokeCLRMethod(clrMethod, clrObj, frameBase, mStack);
    break;

case Callvirt:
    // 通用路径：运行时判断 this 类型
    int thisIndex = *(int*)(frameBase + ip->Register2);
    object thisObj = mStack[thisIndex];
    if (thisObj is ILTypeInstance instance) {
        ILMethod actualMethod = (ILMethod)instance.Type.VTable[ip->Operand];
        ExecuteNeo(actualMethod, esp);
    } else {
        CLRMethod clrMethod = (CLRMethod)methodTable[ip->Operand2];
        InvokeCLRMethod(clrMethod, thisObj, frameBase, mStack);
    }
    break;
```

大多数场景编译期能确定 this 类型（局部变量类型明确），走无分支快速路径。只有接口变量、Object 类型变量等少数情况才走 `Callvirt` 通用路径。

### 13.5 性能对比

| | 当前 | Neo |
|--|------|-----|
| 分派开销 | Dict lookup + 线性遍历 + 参数比对 + 递归基类 | `vtable[slotIndex]`（一次内存读）|
| 编译时信息 | OpCodeR 存 method token hash | OpCodeR 存 vtable slot index |
| 缓存 | 无（每次重新查找）| vtable 本身就是缓存 |

### 13.6 接口方法分派

接口的 slot 编号系统独立于类的 vtable（一个类可能实现多个接口，各接口的 slot 不能直接映射到类的 vtable slot）。

**方案：接口偏移表 (Interface Offset Table)**

每个 ILType 维护一个接口映射：
```csharp
Dictionary<ILType, int> interfaceOffsets;  // interface → 该接口在 vtable 中的起始 slot
```

或使用紧凑数组（如果接口数量少）：
```csharp
struct InterfaceEntry {
    int interfaceTypeIndex;
    int vtableOffset;      // 该接口的方法从 vtable 的哪个 slot 开始
}
InterfaceEntry[] interfaceMap;
```

接口 callvirt 分派：
```csharp
case Callvirt_Interface_Neo:
    // ip->Operand  = interface type index
    // ip->Operand2 = interface method slot（接口内编号，0-based）
    int thisIndex = *(int*)(frameBase + ip->Register2);
    ILTypeInstance instance = (ILTypeInstance)mStack[thisIndex];
    int baseSlot = instance.Type.GetInterfaceVTableOffset(ip->Operand);
    ILMethod actualMethod = (ILMethod)instance.Type.VTable[baseSlot + ip->Operand2];
    ExecuteNeo(actualMethod, esp);
    break;
```

这样接口方法也只需要：一次接口偏移查找 + 一次数组索引。

### 13.7 非虚方法调用（Call）

非虚方法不需要 vtable 分派，编译时直接确定目标 ILMethod：
```csharp
case Call_Neo:
    // ip->Operand = MethodDefTable index（编译时确定）
    ILMethod target = methodTable[ip->Operand];
    // 写参数到 esp ...
    ExecuteNeo(target, esp);
    break;
```

---

## 14. 编译模式与兼容性策略（已确定）

### 14.1 三层模式架构

Codebase 同时支持 Legacy 和 Neo 两种运行模式，Neo 模式内部支持 JIT 和 AOT 两种编译路径：

| 模式 | 编译宏 | Object Model | 解释器 | 编译方式 | 适用场景 |
|------|--------|-------------|--------|---------|---------|
| Legacy | 未定义 `ENABLE_NEO_MODE` | 旧 StackObject 模型 | ExecuteR / Execute | 现有 JIT | 默认行为，向后兼容 |
| Neo JIT | 定义 `ENABLE_NEO_MODE` | 新 byte* 紧凑模型 | ExecuteNeo | 改造后的 JIT（运行时） | 开发调试、热更新 |
| Neo AOT | 定义 `ENABLE_NEO_MODE` | 新 byte* 紧凑模型 | ExecuteNeo | ilrt_neoc 预编译 | 极致启动速度、无 Cecil 依赖 |

### 14.2 Neo JIT 与 Neo AOT 的关系

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

**设计原则**：
- Neo JIT 功能完整——不依赖 .neo 文件即可运行所有 IL 代码
- Neo AOT 是纯优化层，把 JIT 编译工作提前到构建期
- 两种路径产出的 OpCodeR[] 语义完全一致，ExecuteNeo 无需区分来源
- VTable、接口偏移表等元数据在 JIT 模式下运行时构建（从 Cecil），在 AOT 模式下从 .neo 文件加载

**Neo AOT 相对 JIT 的优势**：
- 消除首次方法调用的 JIT 编译延迟
- 运行时无需加载 Cecil DLL 和解析程序集元数据，减少内存占用
- 泛型方法使用模板+Patch 机制，减少预编译体积（多个引用类型实例化共享同一份模板）

**Cecil 依赖**：
- Neo JIT 模式下 Cecil 仍然加载，`ILType.TypeDefinition` 等引用正常存在，运行时元数据访问路径不变
- Neo AOT 模式下无 Cecil，`ILType`/`ILMethod` 通过 .neo 元数据表初始化（双路径构造）

### 14.3 编译宏规则

**宏名**：`ENABLE_NEO_MODE`

**设计原则**：用户更新 ILRuntime 版本后，默认行为不变（Legacy 模式）。只有显式定义 `ENABLE_NEO_MODE` 才启用新模式。

```csharp
// 典型用法
#if ENABLE_NEO_MODE
    // Neo 模式代码路径（JIT 和 AOT 共用）
#else
    // Legacy 模式代码路径
#endif
```

Neo JIT 和 Neo AOT 不通过编译宏区分——它们是运行时行为差异（加载 DLL 走 JIT，加载 .neo 走 AOT），代码路径在 `AppDomain.LoadAssembly` / `AppDomain.LoadNeoAssembly` 层面分叉。

### 14.4 互斥保护

`USE_OLD_OBJ_MODEL` 宏不再保留。`ENABLE_NEO_MODE` 同时控制 Object Model 和解释器：Neo 模式使用新 Object Model（byte[] Primitives）+ ExecuteNeo，Legacy 模式使用旧 StackObject[] 模型 + ExecuteR（见 14.6 节）。

运行时保护：
- Neo 模式下调用 `ExecuteR`/`Execute`（Legacy 入口）→ 抛异常
- Legacy 模式下调用 `ExecuteNeo` → 抛异常
- Neo 模式下注册旧版 `CLRRedirectionDelegate` → 抛异常
- Legacy 模式下注册 `CLRRedirectionDelegateNeo` → 抛异常

### 14.5 代码组织建议

```
ILIntepreter.cs              // 共享代码（帧管理、异常处理基础设施）
ILIntepreter.Register.cs     // Legacy 模式 ExecuteR 实现
ILIntepreter.Neo.cs          // Neo 模式 ExecuteNeo 实现（#if ENABLE_NEO_MODE）
ILTypeInstance.cs            // #if ENABLE_NEO_MODE 切换新旧 Object Model
```

### 14.6 Object Model 与解释器模式的关系

新 Object Model（byte[] Primitives + AutoList ManagedObjects）**仅在 Neo 模式下启用**。Legacy 模式保持原有的 StackObject[] 字段存储，避免性能退化。

```csharp
// ILTypeInstance.cs
#if ENABLE_NEO_MODE
    byte[] primitives;
    AutoList managedObjects;
#else
    StackObject[] fields;     // 原有模型
#endif
```

原因：新 Object Model 的值类型字段用紧凑 byte[] 存储，但 Legacy 栈上值类型格式为 StackObject[] 展开。如果混用，每次 Ldfld/Stfld 值类型字段都需要格式转换（紧凑 ↔ 展开），带来性能退化。因此两种模式各自使用匹配的 Object Model，保持内部格式一致性。

### 14.7 待清理：ILIntepreter.Register.cs 中的新 Object Model 代码

当前 `ILIntepreter.Register.cs` 中存在 `#if !USE_OLD_OBJ_MODEL` 条件下的新 Object Model 代码路径（Ldfld/Stfld 等指令的 byte[] Primitives 访问逻辑）。既然 Legacy 模式确定不使用新 Object Model，这些代码应在后续开发中清理：

- 移除 `ILIntepreter.Register.cs` 中所有 `#if !USE_OLD_OBJ_MODEL` 分支
- 移除 `USE_OLD_OBJ_MODEL` 宏本身及所有相关的 `#if`/`#else`/`#endif`
- Legacy 模式 `ILIntepreter.Register.cs` 只保留原有的 StackObject[] 字段访问路径
- 新 Object Model 的字段访问逻辑迁移到 `ILIntepreter.Neo.cs`（`#if ENABLE_NEO_MODE`）中重新实现

---

## 15. Ref/Out 参数与 ldloca/ldflda（已确定）

### 15.1 问题

`ldloca`（取局部变量地址）和 `ldflda`（取对象字段地址）产生的引用需要跨帧传递（作为 ref/out 参数传给被调方）。关键约束：

1. **堆对象字段地址不能用裸指针**：`ILTypeInstance.Primitives` 是 managed `byte[]`，GC 可能移动它（不能假定保守 GC，Unity 也在向 CoreCLR 迁移）
2. **帧内地址不能用相对 frameBase 的偏移**：被调方的 frameBase 与调用者不同
3. **CLR 对象字段**：无法直接取到内存地址，需通过 CLRType 接口访问

### 15.2 统一的 Ref Slot 表示

每个 ref slot 占 **8 bytes**，存储 `(objectIndex, offset)` 对：

```
objectIndex == -1 (FRAME_REF):
  → offset = 相对 nativePointer 起始的绝对偏移
  → 目标在非托管帧内存中，直接指针读写

objectIndex >= 0:
  → objectIndex = mStack index
  → stind/ldind 运行时按 mStack[objectIndex] 的类型分派：
    - ILTypeInstance → offset = 字段在 Primitives 中的偏移
    - CLR 对象 → offset = fieldIndex（通过 CLRType.GetFieldValue/SetFieldValue 读写）
    - Array → offset = elementIndex（数组元素读写）
```

### 15.3 各指令的实现思路

| 指令 | objectIndex | offset |
|------|-------------|--------|
| `Ldloca` | -1 | `(frameBase - nativePointer) + slotOffset` |
| `Ldflda`（堆 IL 对象）| this 的 mStack index | 字段 primitiveOffset |
| `Ldflda_Inline`（帧内值类型）| -1 | `(frameBase - nativePointer) + slotOffset + fieldOffset` |
| `Ldflda`（CLR 对象）| this 的 mStack index | 字段 hash（`FieldInfo.GetHashCode()`）|

### 15.4 stind/ldind 的分派

被调方通过 ref 读写时，根据 objectIndex 区分三种路径：
1. `objectIndex == -1` → 帧内非托管内存，直接 `*(T*)(nativePointer + offset)` 读写
2. `objectIndex >= 0` 且 `mStack[objectIndex] is ILTypeInstance` → 重新获取 `Primitives` 的 managed ref 后读写（GC safe）
3. `objectIndex >= 0` 且目标是 CLR 对象 → 通过 `CLRType.SetFieldValue/GetFieldValue(fieldHash, ...)` 读写（与 Legacy 模式一致）

CLR 对象场景需要注意值类型回写：`mStack[objectIndex] = obj`。

### 15.5 Ref Slot 的帧空间分配

Ref slot 占 8 bytes。编译器在 `AllocateLocalStackSpaces` 中对 byref 类型的 slot 分配 8 bytes。

**同一 temp slot 类型冲突的处理**：取 max size。MSIL 验证规则保证控制流汇合点类型一致，冲突只出现在生命周期不重叠的复用场景。取 max 在内存占用和执行效率上均最优，且实现简单。

### 15.6 引用类型字段的 ldflda

```csharp
struct Foo { public object bar; }
ref object x = ref foo.bar;
```

这种场景中"字段地址"指向 mStack 中的一个槽位本身。需要第三种 marker（如 objectIndex = -2），极罕见，后续遇到时再设计。

---

## 16. CLR 类型对象的通盘考虑

### 16.1 背景

ILRuntime 执行时栈上出现 CLR 类型对象（`string`、`List<T>`、Unity 组件等）是常见现象。设计各指令时必须一并考虑 CLR 对象路径，不能只考虑 ILTypeInstance。

### 16.2 各功能点中 CLR 对象的影响

| 功能点 | CLR 对象的特殊处理 |
|--------|-------------------|
| Callvirt | 三种变体：`Callvirt_IL` / `Callvirt_CLR` / `Callvirt`（通用，运行时判断）|
| Ldfld/Stfld | CLR 对象字段通过 CLRType binding 或反射访问，不走 Primitives |
| Ldflda | Ref Slot 中 offset 为字段 hash，读写走 CLRType.Get/SetFieldValue |
| Call（实例方法）| this 为 CLR 对象时，目标必然是 CLRMethod（走 Redirection 或反射）|
| Box | IL 值类型 → new ILTypeInstance；CLR 值类型 → 直接 CLR boxing |
| isinst/castclass | 需要同时处理 IL 类型和 CLR 类型的类型检查 |
| newobj | CLR 类型通过 CLRMethod constructor 创建 |

### 16.3 设计原则

1. **编译期区分**：当静态类型明确时，生成特化指令（如 `Callvirt_CLR`），避免运行时判断
2. **运行时 fallback**：当静态类型不明确时（接口、Object 等），运行时通过 `is ILTypeInstance` 判断分派
3. **保持一致性**：CLR 对象在帧中的表示与 IL 对象相同——都是 4 bytes 的 mStack index，区别仅在于 mStack 中存的对象类型不同

---

## 17. 异常处理（try/catch/finally）（已确定）

### 17.1 整体思路

Neo 的异常处理控制流与 Legacy Register VM 完全一致：C# `try-catch` 包裹执行循环，IL 的 `throw` 实际抛出 C# 异常，由执行循环的 `catch` 块调用 `HandleException` 查找匹配的 handler。`Leave`/`Endfinally` 的逻辑不变。

### 17.2 esp 恢复

esp 以参数方式传递（与 Legacy 一致），C# 异常展开自动恢复 esp，无需额外处理。

### 17.3 mStack 恢复

当 catch handler 捕获异常时，一行恢复：`mStack.Count = frameRefBase + method.TotalRefSize`。这会截断所有未正常退出的 callee 帧残留的 mStack 槽位，同时保留当前帧自己的引用槽。

### 17.4 Frames 栈维护

DebugService 通过 `RuntimeStack.Frames` 获取调用栈，Neo 需要继续维护。Push/Pop 时机与 Legacy 一致：方法入口 PushFrame，正常退出 PopFrame，异常未处理时不 Pop——由上层 HandleException 找到匹配 handler 后批量 Pop 中间帧。

### 17.5 异常对象存储

编译器为每个 catch 块的异常变量分配一个普通 temp ref slot（与其他引用类型 local/temp 统一）。ExceptionHandler 元数据中记录该 slot 的 ref offset，catch handler 进入时将异常对象写入 `mStack[frameRefBase + refOffset]`。catch 块内访问异常对象走普通引用 slot 读取，无需特殊指令。

### 17.6 HandleException 复用

核心查找逻辑（`GetCorrespondingExceptionHandler`、`FindExceptionHandlerByBranchTarget`）完全复用。栈清理部分 Neo 更简单：只需弹 Frames 栈条目 + 重置 mStack.Count，不需要 Legacy 中的 StackObject 清理和值类型释放。用条件编译或方法重载区分。

---

## 18. Boxing/Unboxing（已确定）

### 18.1 IL 值类型

- **Box**：`new ILTypeInstance(type)`，从帧 byte 区域 CopyBlock → Primitives，含引用字段时同步拷贝 ManagedObjects。结果存入 mStack，返回 index。
- **Unbox**：从 ILTypeInstance.Primitives CopyBlock → 帧 byte 区域，引用字段同步拷贝。

### 18.2 CLR 值类型：根据 ValueTypeBinder 编译期区分

| 条件 | 存储位置 | Box | Unbox | 方法调用 |
|------|---------|-----|-------|---------|
| **有 ValueTypeBinder** | Neo 栈上（flat bytes） | 从栈 bytes 重建 CLR 对象 | 拷贝到栈 | Binder 通过 CLR Redirection 直接操作栈上 byte 数据 |
| **无 ValueTypeBinder** | 保持 boxed 在 mStack | 已是 CLR 对象，无需转换 | 保持引用（不拷贝到栈） | 通过 `Unsafe.Unbox<T>` in-place 调用，零 re-box |

编译器（ilrt_neoc）在预编译时已知 Binder 注册信息，据此生成不同的指令变体。

**无 Binder 时不 unbox 到栈的原因**：没有 Binder 提供方法重定向，调用 CLR 方法时需要重新 box 回 CLR 对象，产生无意义的 unbox → re-box 循环。保持 boxed 避免此开销。

### 18.3 无 ValueTypeBinder 时的 In-Place 方法调用

Legacy 模式下，CLR struct 方法调用的自动生成 binding 使用 "unbox → call → WriteBackInstance" 模式，每次 writeback 都产生一次 re-boxing（因为赋值 struct 给 `object` = 隐式 box）。这是 `foreach(List<int>)` per-iteration GC alloc 的根源。

Neo 模式下，自动生成的 CLR Redirection 使用 `Unsafe.Unbox<T>` 直接获取 box 内部引用：

```csharp
// Neo binding 示例：List<T>.Enumerator.MoveNext()
unsafe static void Enumerator_MoveNext_Neo(ILIntepreter intp, byte* frameBase, AutoList mStack,
    CLRMethod method, bool isNewObj, byte* retDst, int retRefBase)
{
    int thisIdx = *(int*)(frameBase + 0);
    // Unsafe.Unbox<T> 返回 ref T，直接指向 box 内部数据
    ref var instance = ref Unsafe.Unbox<List<ILTypeInstance>.Enumerator>(mStack[thisIdx]);
    var result = instance.MoveNext();  // in-place 修改 box 内部
    // 无需 WriteBackInstance，无需 re-box
    *(int*)retDst = result ? 1 : 0;
}
```

**原理**：`Unsafe.Unbox<T>(object)` 返回指向 boxed 对象内部数据区域的 `ref T`。通过此 ref 调用方法等价于在 box 本体上操作，修改直接生效。

**开销对比**（以 `foreach(List<int>)` 完整遍历 N 次为例）：

| | Legacy | Neo（无 Binder） | Neo（有 Binder） |
|--|--------|-----------------|-----------------|
| GetEnumerator | 1 box | 1 box | 0（栈上） |
| per-iteration MoveNext | 1 re-box | **0** | 0 |
| per-iteration Current | 可能 1 re-box | **0** | 0 |
| 总 GC alloc | 1 + 2N | **1** | **0** |

**binding 生成工具改造**：Neo 模式下不再生成 `WriteBackInstance` 及相关的 unbox-copy-writeback 逻辑，统一改为 `Unsafe.Unbox<T>` + 直接调用模式。

### 18.4 栈上 CLR 值类型的 memcpy 策略

有 ValueTypeBinder 时，CLR struct 在 Neo 栈上的数据拷贝：
- **Blittable**（无引用字段，可直接 memcpy）：直接 CopyBlock
- **Non-blittable**：运行时计算内存布局并缓存，按布局逐字段拷贝

### 18.5 constrained callvirt

编译期特化，不保留运行时判断：
- 静态类型已知时，编译器直接生成 Call（值类型实现了该方法）或 Box + Callvirt（未实现）
- 泛型 T 在实例化（patch）时确定具体路径
- 值类型直接调用时，`this` 以 Ref Slot（第 15 节）传递

### 18.6 ValueTypeBinder 在 Neo 中的角色

与 Legacy 模式不同，Neo 中 ValueTypeBinder 的主要职责是：
- 通过 CLR Redirection 将值类型方法重定向为直接操作栈上 byte 数据
- 提供字段直接操作能力，避免 CLR 对象与栈数据之间的来回转换

---

## 19. newobj 的返回值处理（已确定）

行为由已确定的设计直接推导：

- **引用类型 newobj**：分配 ILTypeInstance → 写入预分配的 mStack ref slot → `this` 作为 mStack index 写到 callee 帧 param0 → 按第 3 节 calling convention 调构造函数 → 结果为该 ref slot 的 index
- **IL 值类型 newobj**：目标 slot 已在帧上（编译期分配），zero-init 后 `this` 以 Ref Slot（第 15 节）传递给构造函数，构造函数直接操作帧内数据
- **CLR 类型 newobj**：有 ValueTypeBinder 走 Binder，无 Binder 走 CLR Redirection/反射，存储方式遵循第 18 节规则

---

## 20. 栈溢出保护（已确定）

在帧初始化时（InitializeFrame/PushFrame 的 Neo 版本）检查 `esp + frameSize <= stackEnd`，不足则抛出 StackOverflowException。与现有机制统一，仅在 DEBUG 模式下启用。

---

## 21. 委托（Delegate）（已确定）

行为由现有架构直接推导，无额外设计决策：

- **ldftn/ldvirtftn**：IMethod 引用存入 mStack（引用类型），帧中存 mStack index
- **Delegate 创建**：Newobj 检测到 delegate 类型时，通过 DelegateManager 创建 DelegateAdapter，绑定 instance + method
- **Delegate.Invoke**：DelegateAdapter 的 InvokeILMethod 适配 Neo calling convention——将参数写入 byte* 帧 → ExecuteNeo → 读返回值。与 CLR Redirection 互为镜像（Redirection 是 IL→CLR，Delegate 是 CLR→IL）
- **多播委托**：沿用 IDelegateAdapter.next 链表，逻辑不变

---

## 22. 数组元素访问（已确定）

### 22.1 各类型数组的存储

| 数组类型 | 存储方式 |
|---------|---------|
| CLR 类型数组（int[], string[], Vector3[] 等） | CLR 原生数组 |
| IL 引用类型数组（MyILClass[]） | `object[]`，元素为 ILTypeInstance |
| IL 值类型数组（MyILStruct[]） | `ILTypeInstance[]`，元素为独立 ILTypeInstance 对象 |

### 22.2 IL 值类型数组选择 ILTypeInstance[] 的原因

紧凑 byte[] 布局虽然缓存友好，但 ILArray 不是 CLR Array，无法直接传给期望 `Array`/`T[]` 参数的 CLR API。保持 `ILTypeInstance[]` 则天然兼容所有 CLR 数组操作。新 Object Model 下每个 ILTypeInstance 已使用紧凑 byte[] Primitives，单元素字段访问的性能提升已有保障。数组级紧凑优化可作为后续优化点。

### 22.3 元素访问语义

- `ldelem` 基本类型数组（int[] 等）：直接读值写到帧 byte 区域
- `ldelem` 引用类型数组：返回元素的 mStack index
- `ldelem` IL 值类型数组：从元素 ILTypeInstance.Primitives CopyBlock 到帧 byte 区域（+ 引用字段拷贝）
- `stelem` 值类型：深拷贝（CopyBlock Primitives + 拷贝 ManagedObjects）
- `stelem` 引用类型：拷贝引用

---

## 23. 静态字段（已确定）

行为由新 Object Model 直接推导：

- **ILTypeStaticInstance**：使用新 Object Model（byte[] Primitives + AutoList ManagedObjects），与普通 ILTypeInstance 同构
- **ldsfld/stsfld**：通过 type → StaticInstance → 字段偏移访问 Primitives/ManagedObjects，与实例字段访问路径一致
- **静态构造器触发**：时机不变（首次访问类型时触发）
- **CLR 类型静态字段**：通过 CLRType binding 或反射访问，与 Legacy 一致

---

## 24. isinst/castclass 适配（已确定）

### 24.1 背景

C# 编译器对值类型做 isinst/castclass 前会先 emit `box` 指令。Legacy Register VM 的 box 对基本类型（int/float/long/double）不真的 box，保留 ObjectType 标记，因此 isinst 需要处理 `ObjectType <= Double` 的路径。Neo 帧中没有 ObjectType 标记，需要不同的策略。

### 24.2 方案：编译期 peephole 合并

在指令翻译阶段（BCP/FCP 之前），检测 `box T` + `isinst U` / `castclass U` 的固定 pattern，编译期消解：

| IL 序列 | 编译期判断 | 优化结果 |
|---------|-----------|---------|
| `box T; isinst U` | T 兼容 U | 消除两条指令，直接保留原值 |
| `box T; isinst U` | T 不兼容 U | 消除两条指令，写 null |
| `box T; isinst U; unbox.any` | T 兼容 U | 消除三条指令，直接 Move |
| `box T; castclass U` | T 兼容 U | 消除 |
| `box T; castclass U` | T 不兼容 U | 编译期报错或保留 |
| 涉及泛型参数 T | 实例化时确定 | 加入 patch 表（PatchKind.IsinstResult） |

### 24.3 运行时残留路径

编译期无法消解的情况（操作数静态类型为 object/接口等），操作数**已在 mStack 中**（是引用类型或 boxed 值类型），isinst/castclass 直接对 mStack 中的对象做类型检查：
- `ILTypeInstance` → `CanAssignTo`
- CLR 对象 → `IsAssignableFrom`

不涉及帧内 flat bytes，不需要额外 box。

---

## 25. ldelema 与 CLR 值类型嵌套字段引用（已确定）

### 25.1 问题

`ref arr[i].field.innerField` 需要跨越数组元素 + 多层值类型字段。CLR 值类型没有 byte offset 直接访问能力（无 ValueTypeBinder 时），需要逐层处理。

### 25.2 Legacy 方案

Legacy 使用 `FieldReference(mStackIndex, fieldIndex)` 和 `ArrayReference(mStackIndex, elementIndex)` 表示。嵌套 ldflda 时，`RetriveObject` 按 fieldIndex 调用 `CLRType.GetFieldValue` 取出中间值（boxed），放入 mStack，产生新的 FieldReference。每层独立，不打包多层信息。

### 25.3 Neo 方案

**Ref Slot 保持 8 字节 `(objectIndex, offset)`**，与 Legacy 的 FieldReference/ArrayReference 完全对应：

- ldflda 遇到非帧内 Ref Slot 输入时，先通过 offset 取出中间值（`CLRType.GetFieldValue` 或数组元素访问）放入 mStack，再产生新的 Ref Slot
- stind/ldind 按 `mStack[objectIndex]` 的运行时类型分派（ILTypeInstance / CLR 对象 / Array）
- ILTypeInstance 路径不做中间解引用，field offset 直接累加

```
Outer[] arr;  // CLR 值类型数组
ref int r = ref arr[2].inner.x;

ldelema arr, 2     → RefSlot = (arrIdx, 2)        // Array 引用
ldflda inner       → RetriveObject 取 arr[2]，box 入 mStack 得 tmpIdx
                     → RefSlot = (tmpIdx, innerFieldIdx)
ldflda x           → RetriveObject 取 inner，box 入 mStack 得 tmpIdx2
                     → RefSlot = (tmpIdx2, xFieldIdx)
stind_i4           → CLRType.SetFieldValue(xFieldIdx, ...) + 回写 mStack
```

### 25.4 stind 回写

与 Legacy 的 `StoreValueToFieldReference` 一致：CLR 值类型通过 `ref obj` 模式，修改后写回 `mStack[objectIndex] = obj`。

### 25.5 不需要额外字段的原因

Legacy 用 `ObjectType` 枚举区分 FieldReference/ArrayReference，Neo 不需要——stind/ldind 通过 `mStack[objectIndex]` 的 CLR 类型即可区分数组和对象。Ref Slot 保持 `(objectIndex, offset)` 两个 int32，共 8 字节。

---

## 26. Async/Await 与 ValueTask（已确定）

### 26.1 问题

C# 编译器为 async 方法生成 struct state machine + 实现 `IAsyncStateMachine` 接口。在 ILRuntime 中：
- IL struct 实现 CLR 接口 → 正常路径触发 CrossBindingAdapter 查找 + boxing
- `AsyncValueTaskMethodBuilder<T>.Start(ref TSM)` 等方法带泛型约束 `where TSM : IAsyncStateMachine` → 按接口传递 → 反复 box/unbox + writeback
- ValueTask 的零分配优势完全丧失

### 26.2 核心方案：Builder 全量重定向 + Adapter 绕过

**原则**：通过 CLR Redirection 拦截 `AsyncValueTaskMethodBuilder<T>` / `AsyncTaskMethodBuilder<T>` / `AsyncTaskMethodBuilder` 的所有关键方法，使 state machine 永远不作为 `IAsyncStateMachine` 接口传给 CLR 侧，从而完全绕过 CrossBindingAdapter。

State machine 始终作为帧上值类型存在（同步路径），或以 `initializeCLRInstance: false` 方式搬到堆上（异步路径），不触发 adapter 创建。

### 26.3 执行流程

```
IL async method Bar() 被调用:
  编译器生成的代码等价于:
    SM sm = default;
    sm.<>t__builder = AsyncValueTaskMethodBuilder<T>.Create();
    sm.<>1__state = -1;
    sm.<>t__builder.Start(ref sm);
    return sm.<>t__builder.Task;

Neo 执行时:
  sm 在 Bar() 帧上作为普通值类型 slot（flat bytes）
  所有 builder 方法被 CLR Redirection 拦截 ↓
```

### 26.4 Builder 方法重定向

| Builder 方法 | 重定向行为 |
|-------------|-----------|
| `Create()` | 返回空 builder（无操作） |
| `Start<TSM>(ref sm)` | 直接调用 sm.MoveNext()（通过 Ref Slot 传 this，ExecuteNeo），不经 CLR 泛型约束 |
| `AwaitUnsafeOnCompleted<TA, TSM>(ref awaiter, ref sm)` | 首次挂起时：帧→堆拷贝 + 创建 ILAsyncContext（见 26.5） |
| `AwaitOnCompleted<TA, TSM>(ref awaiter, ref sm)` | 同上（带 ExecutionContext capture） |
| `SetResult(T)` | 同步：标记到帧上 builder 字段；异步：`context.core.SetResult(result)` |
| `SetException(Exception)` | 同步：记录异常；异步：`context.core.SetException(ex)` |
| `Task` getter | 同步完成：`new ValueTask<T>(result)`；异步：`new ValueTask<T>(context, token)` |
| `SetStateMachine(IAsyncStateMachine)` | 空操作 |

### 26.5 异步路径：帧→堆拷贝

首次 `AwaitUnsafeOnCompleted` 被调用时（state machine 真正挂起）：

```csharp
// 在重定向 handler 中：
// sm 当前在帧上（通过 Ref Slot 访问），帧还活着，数据安全
var heapSM = new ILTypeInstance(smType, initializeCLRInstance: false);  // 不创建 adapter！
CopyBlock(heapSM.Primitives, frameSMData, smPrimitiveSize);
// 引用字段也拷贝到 heapSM.ManagedObjects
for (int i = 0; i < smRefCount; i++)
    heapSM.ManagedObjects[i] = mStack[smRefBase + i];

// 创建 context
var context = new ILAsyncContext<T>();
context.stateMachine = heapSM;
context.moveNextMethod = smType.GetMethod("MoveNext");

// 注册 continuation
awaiter.UnsafeOnCompleted(context.MoveNextDelegate);
```

关键：`initializeCLRInstance: false` 确保不触发 `CrossBindingAdaptor.CreateCLRInstance()`——heapSM 永远只被 ILAsyncContext 内部持有，不暴露给 CLR 侧。

### 26.6 ILAsyncContext<T>

```csharp
class ILAsyncContext<T> : IValueTaskSource<T>, IAsyncStateMachine
{
    ILTypeInstance stateMachine;
    ILMethod moveNextMethod;
    ManualResetValueTaskSourceCore<T> core;

    // CLR continuation 回调入口
    void IAsyncStateMachine.MoveNext()
    {
        // 从 AppDomain 的 intp 池中获取（线程安全）
        var intp = domain.GetOrCreateInterpreter();
        intp.ExecuteNeo(moveNextMethod, /* 以 heapSM 为 this */...);
        domain.ReleaseInterpreter(intp);
    }

    // IValueTaskSource<T>
    T GetResult(short token) => core.GetResult(token);  // token 不匹配直接抛异常
    ValueTaskSourceStatus GetStatus(short token) => core.GetStatus(token);
    void OnCompleted(...) => core.OnCompleted(...);
}
```

### 26.7 开销总结

| 路径 | 分配 | 与 native C# 对比 |
|------|------|------------------|
| 同步完成（所有 awaited task 已完成） | **0** | 一致 |
| 真正异步挂起 | 2（ILTypeInstance for heapSM + ILAsyncContext） | native C# 也是 1-2 次分配 |
| ValueTask 在方法间传递 | 0（值类型 CopyBlock） | 一致 |
| await ValueTask 取结果 | 0 | 一致 |

### 26.8 线程安全

- state machine（ILTypeInstance）字段无竞争：async 状态机不变量保证同一时刻只有一个 MoveNext 在跑
- `ManualResetValueTaskSourceCore` 内部用 `Interlocked.CompareExchange` 处理 SetResult 与 OnCompleted 的竞争
- ILIntepreter 获取：通过 AppDomain 的 intp 池（类似现有的 CLR→IL 回调路径：delegate、协程等）

### 26.9 覆盖范围

同一方案适用于所有 async 返回类型：

| 返回类型 | Builder | ILAsyncContext 实现 |
|---------|---------|-------------------|
| `ValueTask<T>` | `AsyncValueTaskMethodBuilder<T>` | `IValueTaskSource<T>` |
| `ValueTask` | `AsyncValueTaskMethodBuilder` | `IValueTaskSource` |
| `Task<T>` | `AsyncTaskMethodBuilder<T>` | 内部持有 `TaskCompletionSource<T>` |
| `Task` | `AsyncTaskMethodBuilder` | 内部持有 `TaskCompletionSource` |
| 自定义 Builder | 用户注册对应的 Redirection | 按具体 builder API 适配 |

### 26.10 不池化 ILAsyncContext 的原因

`IValueTaskSource` 规范要求 GetResult 只调用一次，池化归还时机理论上正确。但实际存在竞争窗口：OnCompleted 注册的 ExecutionContext 回调可能延迟执行，归还后被新使用者覆盖。为避免复杂的线程安全问题，默认不池化——异步路径已经有 2 次堆分配，多少一次不影响大局。同步路径（主要性能关注点）始终零分配。

---

## 附录：从现有设计可直接推导的指令/功能

以下主题无需额外设计决策，行为由已确定的章节直接推导：

| 主题 | 推导依据 |
|------|---------|
| **Cross-binding adapters** | 适配器模式不变（CLR 包装器持有 ILTypeInstance），IL 方法调用入口从 ExecuteR → ExecuteNeo，新 Object Model 对适配器层透明 |
| **Enum 类型** | 按 underlying type（int/byte 等）存储在 byte 区域，box/unbox 遵循第 18 节规则 |
| **ldobj/stobj** | 通过 Ref Slot（第 15 节）读写值类型：IL 值类型 CopyBlock + 引用字段拷贝，基本类型直接读写 |
| **DebugService 变量检查** | Neo 帧无 ObjectType 标记。CompiledFrame 的 StackSlotInfo[]、TotalStructSize、TotalRefSize 仅在 `#if DEBUG` 下持久化到 ILMethod（当前编译后丢弃了），DebugService 通过 ILMethod.LocalInfos（类型 + 偏移）解读帧数据。解释器执行时不需要这些字段——帧布局已编码在指令 operand 中 |
| **算术/比较/分支指令** | add/sub/ceq/beq 等直接操作帧 byte 区域，寄存器语义从 StackObject 索引统一改为 byte 偏移，所有特化变体（Add_I4 等）的 operand 编码需调整，无设计决策 |
