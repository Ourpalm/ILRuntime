# Neo 模式 Step 6 实施 Spec — 基本算术/分支/常量加载指令 + LowerNeoOffsets Pass

## Why
Step 1-5 已经把 Neo 模式的基础设施搭好：宏体系、ILTypeInstance/StaticInstance 对象模型、`byte*` 帧布局、`Ret` 入退帧骨架、Box/Unbox/Initobj。现在需要给 Neo 解释器赋予真正的"运算能力"——常量加载、寄存器移动、整数/浮点算术（含立即数变体）、比较、分支、类型转换——使得真正的 IL 方法（如 `int Add(int a, int b)`、循环求和等）可以端到端跑通。

为了贯彻 Neo 模式"极致性能"目标，本 Spec 引入 `LowerNeoOffsets` pass：在所有 Optimizer pass 跑完后，**直接把 `Register1/2/3` 的值从 slot 索引替换为 `localInfos[reg].Offset`（byte 偏移）**——指令编排（哪个寄存器在哪个字段）完全沿用 Legacy。解释器读 `ip->Register1`/`ip->Register2`/`ip->Register3` 不再去查 LocalInfos，而是直接当作 byte 偏移使用。

为了语义可读，仅引入 `DstOffset`（short，覆盖 Register1）和 `SrcOffset`（short，覆盖 Register2）两个别名；其余位置（Register3/Operand/OperandLong/OperandFloat/OperandDouble/Operand2/Operand3/Operand4）一律沿用 Legacy 字段名与 Legacy 字段编排。

最终解释器形如：
```csharp
case OpCodeREnum.Add:        // I4 三槽：r1=dst, r2=src1, r3=src2（与 Legacy 编排完全一致）
    *(int*)(frameBase + ip->DstOffset) =          // r1 byte off
        *(int*)(frameBase + ip->SrcOffset) +      // r2 byte off
        *(int*)(frameBase + ip->Register3);       // r3 byte off（沿用 short Register3 直接当 byte 偏移）
    break;

case OpCodeREnum.Addi:       // I4 立即数：r1=dst, r2=src, Operand=imm32（与 Legacy 编排完全一致）
    *(int*)(frameBase + ip->DstOffset) =
        *(int*)(frameBase + ip->SrcOffset) + ip->Operand;
    break;
```

## What Changes

### 6.1 OpCodeR union 扩展（最小化）

仅新增两个 ushort 别名：

```csharp
#if ENABLE_NEO_MODE
[FieldOffset(4)] public ushort DstOffset;  // 与 Register1 共址：dst byte 偏移；
                                            // 对无 dst 指令（Brtrue/Beq/Ret 等）当作"主操作数槽"
[FieldOffset(6)] public ushort SrcOffset;  // 与 Register2 共址：src byte 偏移；
                                            // 三槽指令的 src1，单源/双源比较的 src2 等
#endif
```

**Register3、Operand、OperandLong、OperandFloat、OperandDouble、Operand2、Operand3、Operand4 完全不变**，沿用 Legacy 名称。Neo 解释器把 short 字段（Register1/2/3）的内存重新当 ushort 解释为 byte 偏移而非 slot 索引——byte offset 由 JIT 从 0 单调累加，永远非负，ushort 无符号语义最贴切。Register3 在 Neo 路径上用 `(ushort)op.Register3` 显式转换读取。

**OpCodeR 大小不变**（24 字节）；Legacy 路径完全不受影响（Legacy 仍按 short 读 Register1/2/3，且 Legacy 写入的 slot 索引远小于 short.MaxValue）。

**byte 偏移范围**：ushort 最大 65535（64 KB），远大于实际帧大小。JIT 编译完成后必须断言 `frame.TotalStructSize <= ushort.MaxValue`，超限直接抛 `NotSupportedException("Neo: method frame size {N} exceeds 65535 bytes")`——**不**回退到 Legacy 解释器（调用约定不兼容，无法跨方法混用）。出现此异常的方法极罕见（需要超大值类型 local 或极深虚拟栈 × 大 struct），用户应拆分方法解决。

### 6.2 类型化操作码命名约定（最小化）

**复用 Legacy 现有无后缀指令作为 I4 默认变体**，不为 I4 新增 `_I4` 后缀。仅为 I8/R4/R8 新增后缀变体。立即数指令（`Addi/Subi/Beqi/...`）同样把现有 enum 名当作 I4 变体使用。

| Legacy enum（I4 默认） | I8 变体 | R4 变体 | R8 变体 |
|---|---|---|---|
| `Add`/`Sub`/`Mul`/`Div`/`Div_Un`/`Rem`/`Rem_Un` | `Add_I8` ... `Rem_Un_I8` | `Add_R4`/`Sub_R4`/`Mul_R4`/`Div_R4`/`Rem_R4` | `Add_R8`/`Sub_R8`/`Mul_R8`/`Div_R8`/`Rem_R8` |
| `And`/`Or`/`Xor`/`Shl`/`Shr`/`Shr_Un` | `And_I8` ... `Shr_Un_I8` | — | — |
| `Neg`/`Not` | `Neg_I8`/`Not_I8` | `Neg_R4` | `Neg_R8` |
| `Ceq`/`Cgt`/`Cgt_Un`/`Clt`/`Clt_Un` | `Ceq_I8`...`Clt_Un_I8` | `Ceq_R4`...`Clt_Un_R4` | `Ceq_R8`...`Clt_Un_R8` |
| `Beq`/`Bne_Un`/`Blt`/`Bgt`/`Ble`/`Bge`/`Blt_Un`/`Bgt_Un`/`Ble_Un`/`Bge_Un` | `Beq_I8`...`Bge_Un_I8` | `Beq_R4`...`Bge_Un_R4` | `Beq_R8`...`Bge_Un_R8` |
| `Addi`/`Subi`/`Muli`/`Divi`/`Divi_Un`/`Remi`/`Remi_Un`/`Andi`/`Ori`/`Xori`/`Shli`/`Shri`/`Shri_Un` | `Addi_I8`...`Shri_Un_I8` | `Addi_R4`/`Subi_R4`/`Muli_R4`/`Divi_R4`/`Remi_R4` | `Addi_R8`/`Subi_R8`/`Muli_R8`/`Divi_R8`/`Remi_R8` |
| `Ceqi`/`Cgti`/`Cgti_Un`/`Clti`/`Clti_Un` | `Ceqi_I8`...`Clti_Un_I8` | `Ceqi_R4`...`Clti_Un_R4` | `Ceqi_R8`...`Clti_Un_R8` |
| `Beqi`/`Bnei_Un`/`Blti`/`Bgti`/`Blei`/`Bgei`/`Blti_Un`/`Bgti_Un`/`Blei_Un`/`Bgei_Un` | `Beqi_I8`...`Bgei_Un_I8` | `Beqi_R4`...`Bgei_Un_R4` | `Beqi_R8`...`Bgei_Un_R8` |
| `Br`/`Brtrue`/`Brfalse`（含 _S）| 无类型变体（通用） | — | — |
| `Conv_I1/U1/I2/U2/I4/U4/I8/U8/R4/R8` | 不新增 enum，源类型 tag 写入 `Operand2` | — | — |

### 6.3 字段编排沿用 Legacy（关键设计）

**Neo 模式不重排字段**，所有指令字段编排与 Legacy 完全一致。lowering pass 唯一做的事：把 Register1/2/3 中的 slot 索引替换为对应的 byte 偏移，**字段位置不动**。

| 指令类别 | r1 (DstOffset @4) | r2 (SrcOffset @6) | r3 (Register3 @8) | Operand 系列 / Operand4 |
|---|---|---|---|---|
| 三槽算术（Add/Sub/.../+_I8/_R4/_R8） | dst | src1 | src2 | — |
| 单目（Neg/Not/+_I8/_R4/_R8） | dst | src | — | — |
| 二槽比较（Ceq/Cgt/Clt/+_I8/_R4/_R8） | dst | src1 | src2 | — |
| 二源类型化分支（Beq/.../+_I8/_R4/_R8） | src1 | src2 | — | Operand = target |
| 单源分支（Brtrue/Brfalse + _S） | src | — | — | Operand = target；Operand2 = src size (4/8) |
| 无源分支（Br/Br_S） | — | — | — | Operand = target |
| Move | dst | src | — | Operand2 = primitive 字节数 |
| Conv_* | dst | src | — | Operand2 = NeoPrimitiveTypeTag |
| Ldc_I4/I4_S/I4_M1..I4_8 | dst | — | — | Operand = imm |
| Ldc_I8 | dst | — | — | OperandLong = imm |
| Ldc_R4 | dst | — | — | OperandFloat = imm |
| Ldc_R8 | dst | — | — | OperandDouble = imm |
| 立即数算术 I4/R4（Addi/Subi/Muli/...） | dst | src | — | Operand/OperandFloat = imm |
| 立即数算术 I8/R8（Addi_I8/Addi_R8/...） | dst | src | — | OperandLong/OperandDouble = imm |
| 立即数比较 I4/R4（Ceqi/...） | dst | src | — | Operand/OperandFloat = imm |
| 立即数比较 I8/R8 | dst | src | — | OperandLong/OperandDouble = imm |
| 立即数分支 I4/R4（Beqi/...） | src | — | — | Operand/OperandFloat = imm；Operand4 = target |
| 立即数分支 I8/R8 | src | — | — | OperandLong/OperandDouble = imm；Operand4 = target |
| Ret | src（返回值源） | — | — | — |
| Initobj | dst | — | — | Operand = type token |
| Box/Unbox/Unbox_Any | dst | src | — | Operand = type token；Operand3 = RefOffset 编码（高 16 = dst.RefOffset，低 16 = src.RefOffset） |

**这就是 Legacy 的字段编排**，Neo 只是改了 Register1/2/3 的"值"含义。

### 6.4 LowerNeoOffsets Pass

在 [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) 优化流水线末尾（BCP/FCP/InlineMethod/ELDC/RegisterCleanup 之后）调用 `LowerNeoOffsets`。仅 `#if ENABLE_NEO_MODE`。

**核心逻辑非常简单**：根据 opcode 决定 Register1/2/3 中哪些是寄存器索引，把它们替换为 byte offset；首先检查帧大小是否超出 ushort 上限：

```csharp
void LowerNeoOffsets(ref CompiledFrame frame)
{
    if (frame.TotalStructSize > ushort.MaxValue)
        throw new NotSupportedException(
            $"Neo: method frame size {frame.TotalStructSize} exceeds {ushort.MaxValue} bytes; please split the method");

    var localInfos = frame.LocalInfos;
    for (int i = 0; i < frame.CodeBody.Length; i++)
    {
        ref var op = ref frame.CodeBody[i];
        switch (op.Code)
        {
            // 三槽算术/比较：r1, r2, r3 都是寄存器
            case OpCodeREnum.Add: case OpCodeREnum.Sub: case OpCodeREnum.Mul:
            ... case OpCodeREnum.Add_I8: ... case OpCodeREnum.Add_R4: ... case OpCodeREnum.Add_R8: ...
            case OpCodeREnum.Ceq: case OpCodeREnum.Cgt: case OpCodeREnum.Clt:
            ...
                op.DstOffset = (ushort)localInfos[op.Register1].Offset;
                op.SrcOffset = (ushort)localInfos[op.Register2].Offset;
                op.Register3 = (short)localInfos[op.Register3].Offset; // 位模式即 ushort，读回 (ushort)op.Register3
                break;

            // 单目 / Conv / Move / 立即数算术 / 立即数比较：r1, r2 是寄存器
            case OpCodeREnum.Neg: case OpCodeREnum.Not:
            case OpCodeREnum.Move:
            case OpCodeREnum.Conv_I4: ...
            case OpCodeREnum.Addi: case OpCodeREnum.Subi: ...
            case OpCodeREnum.Addi_I8: ...
            case OpCodeREnum.Ceqi: ...
                op.DstOffset = (ushort)localInfos[op.Register1].Offset;
                op.SrcOffset = (ushort)localInfos[op.Register2].Offset;
                break;

            // 二源类型化分支：r1, r2 是寄存器；Operand=target
            case OpCodeREnum.Beq: case OpCodeREnum.Bne_Un: ...
            case OpCodeREnum.Beq_I8: ...
                op.DstOffset = (ushort)localInfos[op.Register1].Offset;
                op.SrcOffset = (ushort)localInfos[op.Register2].Offset;
                break;

            // 单源分支 / 立即数分支 / Ret / Initobj / Ldc_*：仅 r1 是寄存器
            case OpCodeREnum.Brtrue: case OpCodeREnum.Brfalse:
            case OpCodeREnum.Brtrue_S: case OpCodeREnum.Brfalse_S:
            case OpCodeREnum.Beqi: case OpCodeREnum.Bnei_Un: ...
            case OpCodeREnum.Beqi_I8: ...
            case OpCodeREnum.Initobj:
            case OpCodeREnum.Ldc_I4_M1: ... case OpCodeREnum.Ldc_R8:
                op.DstOffset = (ushort)localInfos[op.Register1].Offset;
                break;

            // Ret 特殊：可能没有返回值（r1 = -1 占位）
            case OpCodeREnum.Ret:
                if (op.Register1 >= 0)
                    op.DstOffset = (ushort)localInfos[op.Register1].Offset;
                break;

            // 单源 Brtrue/Brfalse 额外写入 size 到 Operand2
            // （上面已经处理了 Register1，这里补一步 Operand2）
            // — 改为另一个 case 块或紧随其后写入

            // 无源分支 Br：什么都不动
            case OpCodeREnum.Br: case OpCodeREnum.Br_S:
                break;

            // Box/Unbox/Unbox_Any: r1, r2 是寄存器；额外写 Operand3 (RefOffset 编码)
            case OpCodeREnum.Box:
            case OpCodeREnum.Unbox:
            case OpCodeREnum.Unbox_Any:
                {
                    int dstRefOff = localInfos[op.Register1].RefOffset;
                    int srcRefOff = localInfos[op.Register2].RefOffset;
                    int dstOff = localInfos[op.Register1].Offset;
                    int srcOff = localInfos[op.Register2].Offset;
                    op.DstOffset = (ushort)dstOff;
                    op.SrcOffset = (ushort)srcOff;
                    op.Operand3 = (dstRefOff << 16) | (srcRefOff & 0xFFFF);
                }
                break;

            default:
                // 其它 opcode（Push/Call/Newobj/Ldfld_* 等 Step 7+）暂不处理
                break;
        }
    }

    // Brtrue/Brfalse 的 Operand2 = src size (4/8)：
    // 在主 switch 中处理时需要在改写 Register1 之前先记下原 slot 索引
    // → 拆分为两段：先取 size，再改 Register1。或单独 case 处理。
}
```

**关键约束**：
- Register1/2/3 是 short，处理时必须先读取原 slot 索引再写入 byte offset；先读完所有需要的 LocalInfos 字段（比如 Box/Unbox 还要读 RefOffset），再覆写 Register1/2/3。
- Brtrue/Brfalse 单独处理：先用 `localInfos[op.Register1].Size` 取出 size 写入 `op.Operand2`，再把 `op.Register1` 替换为 byte offset。

### 6.5 新增类型化操作码（仅非 I4 后缀）

详见 §6.2 表格。所有新增 enum 只是 OpCodeREnum 的 _I8/_R4/_R8 变体；I4 全部沿用 Legacy 名。

`NeoPrimitiveTypeTag` 仅 Neo 模式可见：
```csharp
internal enum NeoPrimitiveTypeTag : int { I4 = 0, U4 = 1, I8 = 2, U8 = 3, R4 = 4, R8 = 5 }
```

Conv_Ovf_* 暂不实现（Step 26）。

### 6.6 解释器实现（lowering 之后）

```csharp
// 三槽算术（I4 默认变体）
case OpCodeREnum.Add:
    *(int*)(frameBase + ip->DstOffset) =
        *(int*)(frameBase + ip->SrcOffset) +
        *(int*)(frameBase + (ushort)ip->Register3);   // src2 byte off（ushort 解读）
    break;

case OpCodeREnum.Add_I8:
    *(long*)(frameBase + ip->DstOffset) =
        *(long*)(frameBase + ip->SrcOffset) +
        *(long*)(frameBase + (ushort)ip->Register3);
    break;

case OpCodeREnum.Add_R8:
    *(double*)(frameBase + ip->DstOffset) =
        *(double*)(frameBase + ip->SrcOffset) +
        *(double*)(frameBase + (ushort)ip->Register3);
    break;

// I4 立即数算术（沿用 Legacy 编排）
case OpCodeREnum.Addi:
    *(int*)(frameBase + ip->DstOffset) =
        *(int*)(frameBase + ip->SrcOffset) + ip->Operand;
    break;

case OpCodeREnum.Addi_I8:
    *(long*)(frameBase + ip->DstOffset) =
        *(long*)(frameBase + ip->SrcOffset) + ip->OperandLong;
    break;

// 二源类型化分支（I4 默认）：r1=src1, r2=src2, Operand=target
case OpCodeREnum.Beq:
    if (*(int*)(frameBase + ip->DstOffset) ==     // r1 在 DstOffset 位置（语义实为 src1）
        *(int*)(frameBase + ip->SrcOffset))
    {
        ip = ptr + ip->Operand;
        continue;
    }
    break;

case OpCodeREnum.Beq_I8:
    if (*(long*)(frameBase + ip->DstOffset) ==
        *(long*)(frameBase + ip->SrcOffset))
    {
        ip = ptr + ip->Operand;
        continue;
    }
    break;

// I4 立即数分支：r1=src, Operand=imm, Operand4=target（与 Legacy 一致）
case OpCodeREnum.Beqi:
    if (*(int*)(frameBase + ip->DstOffset) == ip->Operand)
    {
        ip = ptr + ip->Operand4;
        continue;
    }
    break;

case OpCodeREnum.Beqi_I8:
    if (*(long*)(frameBase + ip->DstOffset) == ip->OperandLong)
    {
        ip = ptr + ip->Operand4;
        continue;
    }
    break;

// 单源分支：r1=src, Operand=target, Operand2=size (4/8)
case OpCodeREnum.Brtrue:
case OpCodeREnum.Brtrue_S:
    if (ip->Operand2 == 8
            ? *(long*)(frameBase + ip->DstOffset) != 0
            : *(int*)(frameBase + ip->DstOffset) != 0)
    {
        ip = ptr + ip->Operand;
        continue;
    }
    break;

case OpCodeREnum.Br:
case OpCodeREnum.Br_S:
    ip = ptr + ip->Operand;
    continue;

// Move
case OpCodeREnum.Move:
    Unsafe.CopyBlock(
        frameBase + ip->DstOffset,
        frameBase + ip->SrcOffset,
        (uint)ip->Operand2);
    break;

// 常量
case OpCodeREnum.Ldc_I4: case OpCodeREnum.Ldc_I4_S:
    *(int*)(frameBase + ip->DstOffset) = ip->Operand;
    break;

case OpCodeREnum.Ldc_I8:
    *(long*)(frameBase + ip->DstOffset) = ip->OperandLong;
    break;

// Conv：r1=dst, r2=src, Operand2 = src 类型 tag
case OpCodeREnum.Conv_I8:
    {
        switch ((NeoPrimitiveTypeTag)ip->Operand2)
        {
            case NeoPrimitiveTypeTag.I4: *(long*)(frameBase + ip->DstOffset) = (long)*(int*)(frameBase + ip->SrcOffset); break;
            case NeoPrimitiveTypeTag.U4: *(long*)(frameBase + ip->DstOffset) = (long)*(uint*)(frameBase + ip->SrcOffset); break;
            case NeoPrimitiveTypeTag.I8: *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset); break;
            case NeoPrimitiveTypeTag.R4: *(long*)(frameBase + ip->DstOffset) = (long)*(float*)(frameBase + ip->SrcOffset); break;
            case NeoPrimitiveTypeTag.R8: *(long*)(frameBase + ip->DstOffset) = (long)*(double*)(frameBase + ip->SrcOffset); break;
        }
    }
    break;
```

整数除以零仍抛 `DivideByZeroException`。浮点 NaN/除零按 IEEE 754。

Step 5 既有 handler 同步重构（Ret/Initobj/Box/Unbox/Unbox_Any）：
```csharp
case OpCodeREnum.Ret:
    if (retDst != null && returnPrimitiveSize > 0)
        Unsafe.CopyBlock(retDst, frameBase + ip->DstOffset, (uint)returnPrimitiveSize);
    if (returnRefCount > 0)
    {
        // RefOffset 暂保留 LocalInfos 查询或编码到 Operand3，详见 Step 7
    }
    returned = true;
    break;
```

Box/Unbox 的 RefOffset 编码到 Operand3（高 16 = dst.RefOffset, 低 16 = src.RefOffset）。

### 6.7 JIT 改造（Neo 模式专用路径）

1. **新增 `NeoPrimitiveTypeTag` 与 `InferPrimTag(IType)`**（仅 Neo）。
2. **`virtualStackTypes` 类型推导栈**：在 Neo 编译路径中分配 `IType[]`，按 `baseRegIdx` 同步。
3. **算术/比较生成**：根据栈顶类型 tag：
   - I4 → 沿用 `Add/Sub/.../Ceq/Cgt/Clt`
   - I8/R4/R8 → 选择 `Add_I8/Add_R4/Add_R8` 等
4. **类型化分支生成**：同算术，I4 沿用现有，I8/R4/R8 后缀变体。
5. **立即数指令生成**（Legacy FCP/BCP "Add+Ldc 折叠为 Addi"）：I4 沿用，I8/R4/R8 后缀。
6. **Conv 生成**：在 Neo 模式下设置 `op.Operand2 = (int)NeoPrimitiveTypeTag.<src>`。
7. **指令字段编排沿用 Legacy**：JIT 不需要为 Neo 改变寄存器在哪个字段——只是 lowering 把寄存器索引换成 byte offset。
8. **LowerNeoOffsets pass**：在所有 Optimizer pass 之后调用一次。

### 6.8 性能验证目标

```csharp
// 之前（间接）：3 次 LocalInfos 数组寻址 + 3 个临时 struct 拷贝
StackSlotInfo dst = localInfos[ip->Register1];
StackSlotInfo s1  = localInfos[ip->Register2];
StackSlotInfo s2  = localInfos[ip->Register3];
*(int*)(frameBase + dst.Offset) =
    *(int*)(frameBase + s1.Offset) + *(int*)(frameBase + s2.Offset);
```
```csharp
// 之后（直接 byte offset）：纯指针算术
*(int*)(frameBase + ip->DstOffset) =
    *(int*)(frameBase + ip->SrcOffset) +
    *(int*)(frameBase + (ushort)ip->Register3);
```

### 6.9 向后兼容性

- Legacy 模式（不定义 `ENABLE_NEO_MODE`）路径**完全不变**：新增 enum 不会被 Legacy JIT 生成；新增 DstOffset/SrcOffset 别名不影响 Legacy 解释器（Legacy 仍用 Register1/Register2 名字读 short）。
- LowerNeoOffsets pass 仅在 Neo 编译路径调用。
- I4 enum 名（`Add/Addi/Beq/Beqi` 等）在 Legacy 与 Neo 解释器中**含义不同**——通过 `#if ENABLE_NEO_MODE` 编译期分隔解释器实现。

## Impact

- [OpCode.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCode.cs) — 在 OpCodeR struct 上新增 `DstOffset`/`SrcOffset` 两个 short 别名（覆盖 Register1/Register2）
- [OpCodeREnum.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCodeREnum.cs) — 仅新增 _I8/_R4/_R8 后缀变体
- [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) — 新增 NeoPrimitiveTypeTag、InferPrimTag、virtualStackTypes、LowerNeoOffsets pass；I8/R4/R8 路径生成后缀变体
- [ILIntepreter.Neo.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs) — 实现所有 case handler；同步重构 Step 5 既有 Ret/Initobj/Box/Unbox

## 设计决策

1. **指令字段编排沿用 Legacy**：lowering 唯一动作就是把 Register1/2/3 中的 slot 索引替换为 byte offset。字段位置不变，立即数/target 位置不变，无任何字段搬迁。
2. **OpCodeR 别名最小化**：仅 `DstOffset`/`SrcOffset` 两个 ushort 别名（覆盖 Register1/Register2）。Register3 直接用现有名字，Neo 路径 `(ushort)op.Register3` 解读。
3. **I4 默认复用现有 enum 名**：避免新增 _I4 后缀指令；I4 高频场景直接沿用 `Add`/`Addi` 等。
4. **DstOffset 一词在无 dst 指令上的语义**：单源分支/比较分支等"Legacy r1 = src"场景下，DstOffset 实际是 src 偏移——这是为节省命名而做的轻微名实不符；解释器代码里加注释说明即可。
5. **byte offset 用 ushort（无符号 16 位）**：byte offset 由 JIT 从 0 单调累加，永远非负，无符号语义最贴切。上限 65535 字节（64 KB）对游戏脚本绰绰有余。lowering 入口断言 `frame.TotalStructSize <= ushort.MaxValue`，超限抛 `NotSupportedException`，**不**回退 Legacy（调用约定不兼容，无法跨方法混用）。
6. **Lowering pass 在所有优化器之后**：Optimizer pass 都按 Register 索引工作，lowering 必须在它们之后。
7. **Conv 源类型 tag 用 Operand2**：复用现有字段。
8. **Brtrue/Brfalse 用 Operand2 区分 4/8 字节**：lowering 时按 `localInfos[Register1].Size` 写入。
9. **Box/Unbox RefOffset 编码到 Operand3**（高 16/低 16）：临时方案，Step 7 引入完整 RefOffset 字段时统一调整。
10. **本 Spec 不实现的内容**：Ldstr / Ldnull 写帧（Step 7）、Switch（后补）、Ldfld_*/Stfld_*（Step 1-5 部分实现）、Ldind_*/Stind_*/Ldloca/Ldflda（Step 17）、Conv_Ovf_*（Step 26）、Call/Newobj（Step 8/8b）。

## ADDED Requirements

### Requirement: OpCodeR Neo 偏移别名字段
The system SHALL extend `OpCodeR` (under `#if ENABLE_NEO_MODE`) with two `ushort` alias fields `DstOffset` at `[FieldOffset(4)]` (sharing `Register1`) and `SrcOffset` at `[FieldOffset(6)]` (sharing `Register2`). The struct's total size SHALL remain 24 bytes.

#### Scenario: 别名字段共享内存
- **WHEN** lowering pass 写入 `op.DstOffset = 16`
- **THEN** 同一 OpCodeR 实例的 `Register1` 也是 16

#### Scenario: 帧大小超出 ushort 上限
- **WHEN** JIT 编译完成后 `frame.TotalStructSize > 65535`
- **THEN** LowerNeoOffsets pass 抛 `NotSupportedException`，不回退 Legacy

### Requirement: I4 默认变体复用 Legacy enum
Under `#if ENABLE_NEO_MODE`, the Neo interpreter SHALL implement existing Legacy enum names (`Add`/`Sub`/.../`Ceq`/`Cgt`/`Clt`/.../`Beq`/.../`Addi`/.../`Beqi`/.../`Ceqi`/...) as the I4 typed variants, without introducing `_I4`-suffixed enums.

### Requirement: 仅 I8/R4/R8 后缀变体
The system SHALL add new `_I8` / `_R4` / `_R8` suffixed enum values for non-I4 typed variants per §6.2 table. Bitwise/shift opcodes (And/Or/Xor/Shl/Shr/Shr_Un) and Div_Un/Rem_Un do NOT have R4/R8 variants.

### Requirement: LowerNeoOffsets Pass — 沿用 Legacy 字段编排
The JIT compiler SHALL, under `#if ENABLE_NEO_MODE`, run a `LowerNeoOffsets` pass after all optimizer passes (BCP/FCP/InlineMethod/ELDC/RegisterCleanup), translating per-instruction `Register1/2/3` slot indices into `localInfos[reg].Offset` byte offsets **in-place**, without moving immediate or target fields.

#### Scenario: Add 指令 lowering
- **WHEN** lowering pass 处理 `Add`，原 Register1=2/Register2=0/Register3=1，LocalInfos[2].Offset=12/LocalInfos[0].Offset=0/LocalInfos[1].Offset=4
- **THEN** 处理后 `op.Register1 == 12`、`op.Register2 == 0`、`op.Register3 == 4`（即 DstOffset=12, SrcOffset=0, Register3=4）

#### Scenario: Addi 指令 lowering
- **WHEN** lowering pass 处理 `Addi`（dst=r1, src=r2, imm=Operand），LocalInfos[r1].Offset=8, LocalInfos[r2].Offset=4
- **THEN** 处理后 `op.Register1 == 8`、`op.Register2 == 4`、`op.Operand` 仍是 imm

#### Scenario: Beqi 指令 lowering
- **WHEN** lowering pass 处理 `Beqi`（src=r1, imm=Operand, target=Operand4）
- **THEN** 处理后 `op.Register1 == LocalInfos[r1].Offset`、`op.Operand` 仍是 imm、`op.Operand4` 仍是 target

#### Scenario: Beqi_I8 指令 lowering
- **WHEN** lowering pass 处理 `Beqi_I8`（src=r1, imm=OperandLong, target=Operand4）
- **THEN** 处理后 `op.Register1 == LocalInfos[r1].Offset`、`op.OperandLong` 仍是 imm、`op.Operand4` 仍是 target（**字段不动**）

### Requirement: Neo 类型化算术/比较/分支指令
The system SHALL provide typed arithmetic / comparison / branch handlers reading source slots via `*(T*)(frameBase + ip->DstOffset)` / `ip->SrcOffset` / `(ushort)ip->Register3`, writing destination via `ip->DstOffset`, or jumping to `ptr + ip->Operand`.

#### Scenario: I4 加法直接寻址
- **WHEN** Neo 解释器执行 lowered `Add`
- **THEN** dst 写入 `*(int*)(frameBase + ip->SrcOffset) + *(int*)(frameBase + (ushort)ip->Register3)`

#### Scenario: 浮点 NaN 比较不跳转
- **WHEN** Neo 执行 lowered `Beq_R8`，src1 或 src2 为 NaN
- **THEN** 不跳转

### Requirement: Neo 类型化立即数指令
The system SHALL provide typed immediate-form handlers (Addi/Subi/.../Ceqi/.../Beqi/... + _I8/_R4/_R8) that read src via `ip->SrcOffset`, immediate from `Operand`/`OperandLong`/`OperandFloat`/`OperandDouble`, write dst via `ip->DstOffset`, jump via `ip->Operand4` (for branches).

#### Scenario: I4 立即数加法 r1 = r2 + 5
- **WHEN** Neo 执行 lowered `Addi`，src(SrcOffset)=10, imm(Operand)=5
- **THEN** `*(int*)(frameBase + ip->DstOffset) == 15`

#### Scenario: I4 立即数分支 r == 42 跳转
- **WHEN** Neo 执行 lowered `Beqi`，src(DstOffset)=42, imm(Operand)=42
- **THEN** `ip = ptr + ip->Operand4`

#### Scenario: I8 立即数加法
- **WHEN** Neo 执行 lowered `Addi_I8`，src(SrcOffset)=200, imm(OperandLong)=100
- **THEN** `*(long*)(frameBase + ip->DstOffset) == 300`

### Requirement: Neo 常量加载与 Move
The system SHALL implement `Ldc_I4_M1..Ldc_I4_8` / `Ldc_I4` / `Ldc_I4_S` / `Ldc_I8` / `Ldc_R4` / `Ldc_R8` / `Move` in the Neo interpreter using direct `ip->DstOffset` / `ip->SrcOffset` / `ip->Operand2` (size for Move) addressing.

### Requirement: Neo 类型转换指令
The system SHALL implement `Conv_I1`/`U1`/`I2`/`U2`/`I4`/`U4`/`I8`/`U8`/`R4`/`R8` in the Neo interpreter, dispatching on `(NeoPrimitiveTypeTag)ip->Operand2` to read the source.

### Requirement: Step 5 既有 handler 同步重构
The Neo interpreter's `Ret`、`Initobj`、`Box`、`Unbox`、`Unbox_Any` handlers SHALL be rewritten to read directly from `ip->DstOffset` / `ip->SrcOffset` / `ip->Operand` (type token) / `ip->Operand3` (RefOffset 编码) instead of `localInfos[ip->Register1/2]`.

## MODIFIED Requirements

### Requirement: ExecuteNeo case 覆盖范围
原仅支持 `Ret`、`Initobj`、`Box`、`Unbox`、`Unbox_Any`。新增覆盖：

- 常量：`Ldc_I4_M1..Ldc_I4_8`、`Ldc_I4`、`Ldc_I4_S`、`Ldc_I8`、`Ldc_R4`、`Ldc_R8`
- 移动：`Move`
- 三槽算术 I4（默认）：`Add`/`Sub`/`Mul`/`Div`/`Div_Un`/`Rem`/`Rem_Un`/`And`/`Or`/`Xor`/`Shl`/`Shr`/`Shr_Un`
- 三槽算术 I8/R4/R8：后缀变体
- 单目 I4：`Neg`/`Not`；I8/R4/R8 后缀变体
- 比较 I4（默认）：`Ceq`/`Cgt`/`Cgt_Un`/`Clt`/`Clt_Un`；I8/R4/R8 后缀变体
- 类型化分支 I4（默认）：`Beq`/`Bne_Un`/`Blt`/`Bgt`/`Ble`/`Bge`+`_Un`；I8/R4/R8 后缀变体
- 立即数算术 I4（默认）：`Addi`/`Subi`/`Muli`/`Divi`/`Divi_Un`/`Remi`/`Remi_Un`/`Andi`/`Ori`/`Xori`/`Shli`/`Shri`/`Shri_Un`；I8/R4/R8 后缀变体
- 立即数比较 I4（默认）：`Ceqi`/`Cgti`/`Cgti_Un`/`Clti`/`Clti_Un`；I8/R4/R8 后缀变体
- 立即数分支 I4（默认）：`Beqi`/`Bnei_Un`/`Blti`/`Bgti`/`Blei`/`Bgei`+`_Un`；I8/R4/R8 后缀变体
- 通用分支：`Br`、`Brtrue`、`Brfalse`（含 _S）
- 类型转换：`Conv_I1`/`Conv_U1`/`Conv_I2`/`Conv_U2`/`Conv_I4`/`Conv_U4`/`Conv_I8`/`Conv_U8`/`Conv_R4`/`Conv_R8`

未覆盖的 OpCode 走 default 抛 `NotImplementedException`。

### Requirement: Step 5 既有 handler 寻址方式
原 Step 5 实现使用 `StackSlotInfo srcSlot = localInfos[ip->Register1]; *(T*)(frameBase + srcSlot.Offset)`。**修改为**：lowering pass 完成后，handler 直接 `*(T*)(frameBase + ip->DstOffset / SrcOffset)`，type token 沿用 `Operand`，RefOffset 编码沿用 `Operand3`。

## REMOVED Requirements

无。
