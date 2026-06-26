# ILRuntime 新 Object Model 设计文档

> 分支: `features/object-model-overhaul`  
> 起始提交: `a330817b` (First working model, 2025-12-08)  
> 最新提交: `91cc26a` (Local stack space allocation, 2025-12-16)  
> 作者: liiir1985  
>  
> **相关文档**: [Neo 解释器 & 完善设计方案](./object-model-neo-design.md) — 下一步设计讨论与实现路线图

---

## 1. 设计目标

将 `ILTypeInstance` 的字段存储从基于 `StackObject[]` 的统一存储模型，重构为 **原始字节数组 (`byte[]`) + 托管对象列表 (`AutoList`) 的分离存储模型**，以实现：

- 更紧凑的内存布局（消除每字段 12 字节的 StackObject 开销）
- 类型特化的字段访问指令（避免运行时类型判断）
- 直接内存读写（通过 `Unsafe` API 实现零开销访问）

通过预编译符号 `USE_OLD_OBJ_MODEL` 进行条件编译控制，允许新旧模型并存。

---

## 2. 整体架构

```
CIL 字节码 (.dll)
    │
    ▼
JITCompiler.Compile()
    ├── CodeBasicBlock 构建
    ├── Translate(): 逐条 CIL → OpCodeR (栈模拟 → 寄存器分配)
    ├── 优化器 (FCP, BCP, ELDC, InlineMethod, RegisterCleanup)
    └── AllocateLocalStackSpaces(): 计算局部变量栈空间布局
    │
    ▼
CompiledFrame (OpCodeR[], StackSlotInfo[])
    │
    ▼
ILIntepreter.ExecuteR() 解释执行
    ├── 寄存器寻址: r + registerIndex
    ├── 堆对象字段: byte[] 直接内存访问
    ├── 栈值类型字段: StackObject* 指针算术
    └── 函数调用: 寄存器参数 + Push → 递归 ExecuteR
```

---

## 3. 内存布局

### 3.1 旧模型（`USE_OLD_OBJ_MODEL`）

```
ILTypeInstance:
  StackObject[] fields    // 每字段 12 字节 (ObjectType + Value + ValueLow)
  AutoList managedObjs    // 引用对象关联列表
```

每个字段不分类型统一占用一个 StackObject 槽位。

### 3.2 新模型

```
ILTypeInstance:
  byte[] fields           // 所有原始类型字段紧凑排列（通过 Primitives 属性访问）
  AutoList managedObjs    // 所有引用类型字段（通过 ManagedObjects 属性访问）
```

#### 字段偏移结构

```csharp
internal struct ILTypeFieldOffset
{
    public int PrimitiveOffset;   // 在 byte[] 中的字节偏移
    public int ReferenceOffset;   // 在 managedObjs 中的索引
}
```

#### 类型大小计算

每种基本类型在 `byte[]` 中的占用字节数（`AppDomain.GetPrimitiveSize`）：

| 类型 | 大小 |
|------|------|
| byte / sbyte / bool | 1 |
| short / ushort | 2 |
| int / uint / float / char | 4 |
| long / ulong / double / IntPtr | 8 |

#### 字段布局初始化 (`ILType.InitializeFields`)

维护两个累加器 `primitiveOffset` 和 `referenceOffset`，对每个非静态实例字段：

- **原始类型字段**: `PrimitiveOffset = primitiveOffset`，然后 `primitiveOffset += GetPrimitiveSize(type)`
- **IL 值类型字段**: `PrimitiveOffset = primitiveOffset`，然后递归累加子类型的 `TotalPrimitiveSize` 和 `TotalReferenceCount`
- **引用类型字段**: `ReferenceOffset = referenceOffset`，然后 `referenceOffset++`

最终 `TotalPrimitiveSize` = 所有原始数据的总字节数，`TotalReferenceCount` = 所有引用字段的总数量。继承场景下递归包含基类字段。

#### 实例初始化

```csharp
// ILTypeInstance 构造函数
int pSize = type.TotalPrimitiveSize;
int mCnt = type.TotalReferenceCount;
if (pSize > 0) fields = new byte[pSize];
if (mCnt > 0) { managedObjs = new AutoList(mCnt); /* fill nulls */ }
```

---

## 4. 指令系统

### 4.1 OpCodeR 指令格式

使用 `[StructLayout(LayoutKind.Explicit)]` 实现 24 字节的 union 结构：

| Offset | 字段 | 用途 |
|--------|------|------|
| 0 | `Code` (OpCodeREnum) | 操作码 |
| 4 | `Register1` (short) | 目标寄存器 |
| 6 | `Register2` (short) | 源寄存器 1 |
| 8 | `Register3` (short) | 源寄存器 2（与 Operand 重叠）|
| 10 | `Register4` (short) | 源寄存器 3 |
| 8 | `Operand` (int) | 整数操作数 / TypeIndex |
| 8 | `OperandFloat` (float) | 浮点操作数 |
| 12 | `Operand2` (int) | PrimitiveOffset |
| 16 | `Operand3` (int) | ReferenceOffset |
| 12 | `OperandLong` (long) | 64 位操作数（与 Operand2+3 重叠）|
| 20 | `Operand4` (int) | 第四操作数 |

### 4.2 新增的类型特化字段指令

**加载指令** (`Ldfld_*`)：

| 操作码 | 数据类型 | 读取来源 |
|--------|----------|----------|
| `Ldfld_I1` | int8 | byte[] |
| `Ldfld_I2` | int16 | byte[] |
| `Ldfld_I4` | int32 | byte[] |
| `Ldfld_I8` | int64 | byte[] |
| `Ldfld_U1` | uint8 | byte[] |
| `Ldfld_U2` | uint16 | byte[] |
| `Ldfld_U4` | uint32 | byte[] |
| `Ldfld_U8` | uint64 | byte[] |
| `Ldfld_R4` | float32 | byte[] |
| `Ldfld_R8` | float64 | byte[] |
| `Ldfld_Ref` | 引用类型 | managedObjs |
| `Ldfld_Value` | IL 值类型 | byte[] + managedObjs |

**存储指令** (`Stfld_*`)：与加载对称，完整 12 个变体。

### 4.3 指令操作数编码

对于 `Ldfld_I4 r1, r2, typeIndex(primitiveOffset, referenceOffset)`:
- `Register1` = 目标寄存器（加载结果）
- `Register2` = 对象所在寄存器
- `Operand` = 类型 TypeIndex
- `Operand2` = PrimitiveOffset（byte[] 中字节偏移）
- `Operand3` = ReferenceOffset（managedObjs 中索引）
- `OperandLong` = 用于值类型分支（高32位=类型Hash，低32位=字段索引）

---

## 5. JIT 编译器

### 5.1 寄存器分配方案

```
| this | param0 | param1 | ... | local0 | local1 | ... | stack_reg0 | stack_reg1 | ... |
  ^                               ^                        ^
  reg 0                      locVarRegStart           baseRegStart (临时寄存器)
```

- 前 N 个寄存器分配给参数（含 this）
- 紧跟着是局部变量
- 剩余为栈临时寄存器（模拟 CIL 求值栈）
- `CallRegisterParamCount = 3`：函数调用时最多通过 Register2/3/4 传递 3 个参数

### 5.2 字段访问编译

JIT 编译器在遇到 `Ldfld`/`Stfld` CIL 指令时：
1. 调用 `AppDomain.GetFieldOffset(token)` 获取 `ILTypeFieldOffset`
2. 根据字段类型调用 `GetLdfldCodeForType`/`GetStfldCodeForType` 选择特化操作码
3. 将 `PrimitiveOffset` 和 `ReferenceOffset` 编码到指令的 `Operand2`/`Operand3`

### 5.3 本地栈空间分配 (`AllocateLocalStackSpaces`)

```csharp
struct StackSlotInfo {
    public int Offset;      // 原始数据在帧局部空间的字节偏移
    public int RefOffset;   // 引用类型在引用数组中的偏移
    public int Size;        // 值类型占用的原始数据大小
}
```

- 为每个局部变量计算 `StackSlotInfo`
- 栈寄存器分配统一的最大值类型空间（取所有使用到的值类型中最大的 PrimitiveSize 和 ReferenceCount）
- 输出 `CompiledFrame.TotalStructSize` 和 `TotalRefSize` 供运行时分配帧空间

### 5.4 编译输出 (`CompiledFrame`)

```csharp
struct CompiledFrame {
    public OpCodeR[] CodeBody;           // 编译后的指令序列
    public int StackRegisterCount;       // 栈临时寄存器数量
    public Dictionary<int, int[]> SwitchTargets;  // switch 跳转表
    public Dictionary<int, RegisterVMSymbol> Symbols; // 调试符号
    public StackSlotInfo[] LocalInfos;   // 局部变量布局信息
    public int TotalStructSize;          // 帧所需的值类型总空间
    public int TotalRefSize;             // 帧所需的引用总数
}
```

---

## 6. 解释器执行

### 6.1 帧结构 (`RegisterFrameInfo`)

```csharp
unsafe struct RegisterFrameInfo {
    public ILIntepreter Intepreter;
    public int FrameManagedBase;        // 当前帧在 ManagedStack 中的起始索引
    public int LocalManagedBase;        // 局部变量在 ManagedStack 中的起始索引
    public StackObject* StackBase;      // 求值栈底
    public StackObject* RegisterStart;  // 寄存器区域起始 (= r)
    public StackObject* StackRegisterStart; // 栈寄存器区域起始
    public StackObject* RegisterEnd;    // 寄存器区域结束 (= esp)
    public AutoList ManagedStack;       // 托管栈引用
}
```

### 6.2 字段访问执行路径

以 `Ldfld_I4` 为例：

```csharp
reg2 = r + ip->Register2;  // 对象寄存器
objRef = GetObjectAndResolveReference(reg2);

if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference) {
    // 栈上值类型: 通过 StackObject* 指针偏移
    dst = *(StackObject**)&objRef->Value;
    val = dst - ((int)ip->OperandLong + 1);  // 字段地址 = 描述符 - (fieldIndex + 1)
    CopyToRegister(ref info, ip->Register1, val);
} else {
    // 堆上对象: byte[] 直接内存访问
    ilInstance = (ILTypeInstance)RetriveObject(objRef, mStack, false);
    ref byte baseAddr = ref MemoryMarshal.GetArrayDataReference(ilInstance.Primitives);
    ref byte tarAddr = ref Unsafe.Add(ref baseAddr, ip->Operand2);
    PushInt32(reg1, Unsafe.ReadUnaligned<int>(ref tarAddr));
}
```

以 `Stfld_I4` 为例：

```csharp
objRef = GetObjectAndResolveReference(r + ip->Register1);
reg2 = r + ip->Register2;  // 值寄存器

if (objRef->ObjectType == ObjectTypes.ValueTypeObjectReference) {
    // 栈上值类型
    dst = ILIntepreter.ResolveReference(objRef);
    CopyToValueTypeField(dst, (int)ip->OperandLong, reg2, mStack);
} else {
    // 堆上对象
    ilInstance = (ILTypeInstance)RetriveObject(objRef, mStack, false);
    ref byte baseAddr = ref MemoryMarshal.GetArrayDataReference(ilInstance.Primitives);
    ref byte tarAddr = ref Unsafe.Add(ref baseAddr, ip->Operand2);
    Unsafe.WriteUnaligned(ref tarAddr, reg2->Value);
}
```

引用字段 (`Ldfld_Ref` / `Stfld_Ref`)：
- 读取: `obj = ilInstance.ManagedObjects[ip->Operand3]`
- 写入: `ilInstance.ManagedObjects[ip->Operand3] = mStack[reg2->Value]`

### 6.3 值类型的两种存在形式

| 形式 | 存储位置 | 字段寻址方式 |
|------|----------|-------------|
| **栈上** (`ValueTypeObjectReference`) | 求值栈 StackObject 区域 | 描述符指针 - (fieldIndex + 1) |
| **堆上** (`ILTypeInstance`) | `byte[] Primitives` + `AutoList ManagedObjects` | `Unsafe.Add(baseAddr, offset)` |

栈上值类型结构：
```
高地址 → [描述符 StackObject: type index + field count]
          [field N-1]
          [field N-2]
          ...
低地址 → [field 0]
```

---

## 7. 异常处理重构

提交 `1361f6d` 将异常处理逻辑从 `ExecuteR` 中提取为独立的 `HandleException` 方法（位于 `ILIntepreter.cs`），并创建了 `ILIntepreter.Neo.cs` 作为新解释器入口的骨架。

异常寄存器编号 = `paramCount + localCount`（即 catch handler 中第一个栈寄存器位置）。

---

## 8. 类型索引系统

`AppDomain` 新增了基于索引的类型快速查找：

- `List<IType> typesByIndex` — 按索引快速查找
- `AllocTypeIndex(IType type)` — 线程安全分配索引
- `GetTypeByIndex(int index)` — O(1) 查找

用于解释器执行字段指令时，通过 `ip->Operand` (TypeIndex) 快速获取类型元数据。

---

## 9. 提交历史

| 提交 | 日期 | 内容 |
|------|------|------|
| `a330817` | 2025-12-08 | 首个可工作的新对象模型：类型布局计算、字段偏移、ILTypeInstance 新内存结构、Ldfld 特化指令 |
| `f6457e2` | 2025-12-08 | 完成全部 Stfld 特化指令实现 |
| `fd92449` | 2025-12-08 | Ldfld 指令完整工作 |
| `c762d06` | 2025-12-08 | 补充更多 Ldfld 指令变体 |
| `1361f6d` | 2025-12-12 | 重构异常处理，提取 HandleException 方法，创建 Neo 解释器骨架 |
| `91cc26a` | 2025-12-16 | 局部栈空间分配 (StackSlotInfo, AllocateLocalStackSpaces) |

---

## 10. 关键文件索引

| 文件 | 职责 |
|------|------|
| `ILRuntime/CLR/TypeSystem/ILType.cs` | 字段偏移计算、TotalPrimitiveSize/TotalReferenceCount |
| `ILRuntime/Runtime/Enviorment/AppDomain.cs` | GetFieldOffset、GetPrimitiveSize、类型索引系统 |
| `ILRuntime/Runtime/Intepreter/ILTypeInstance.cs` | 实例内存布局 (byte[] + AutoList) |
| `ILRuntime/Runtime/Intepreter/OpCodes/OpCodeREnum.cs` | 新增 Ldfld_*/Stfld_* 操作码枚举 |
| `ILRuntime/Runtime/Intepreter/OpCodes/OpCode.cs` | OpCodeR union 结构定义 |
| `ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs` | CIL→寄存器指令翻译、字段指令选择、栈空间分配 |
| `ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Register.cs` | 寄存器 VM 主解释循环、字段访问执行 |
| `ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs` | 新解释器入口骨架 |
| `ILRuntime/Runtime/Intepreter/RegisterVM/Optimizer.Utils.cs` | 优化器工具（立即数优化、寄存器分析）|

---

## 11. 当前状态与待完成项

### 已完成
- [x] 类型字段偏移计算 (`ILType.InitializeFields`)
- [x] 堆对象的 byte[] + AutoList 内存布局
- [x] 全部 12 种 Ldfld 特化指令的解释器实现
- [x] 全部 12 种 Stfld 特化指令的解释器实现
- [x] JIT 编译器生成特化字段指令
- [x] 栈上值类型的字段访问兼容
- [x] 异常处理重构
- [x] 局部变量栈空间布局预计算 (StackSlotInfo)

### 待完成（推测）
- [ ] 静态字段的新模型适配（`ILTypeStaticInstance` 构造函数在新模型下为空）
- [ ] `ILTypeInstance` 旧接口方法的重写（大量方法被注释）
- [ ] Neo 解释器的完整指令实现（目前只有 Ret）
- [ ] 值类型的 box/unbox 适配
- [ ] 值类型的 Equals/GetHashCode 适配
- [ ] 数组元素的新模型支持
- [ ] 运行时帧的实际栈空间分配（利用 TotalStructSize/TotalRefSize）
- [ ] 完整的测试覆盖
