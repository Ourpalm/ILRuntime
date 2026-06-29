# Tasks

- [ ] Task 1: 新增 OpCodeREnum 类型化变体（仅 _I8/_R4/_R8 后缀）
  - [ ] 在 [OpCodeREnum.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCodeREnum.cs) 末尾追加：
    - 三槽算术 _I8: Add_I8/Sub_I8/Mul_I8/Div_I8/Div_Un_I8/Rem_I8/Rem_Un_I8/And_I8/Or_I8/Xor_I8/Shl_I8/Shr_I8/Shr_Un_I8
    - 三槽算术 _R4/_R8（仅 Add/Sub/Mul/Div/Rem 各 5 个）
    - 单目: Neg_I8/Not_I8/Neg_R4/Neg_R8
    - 比较 _I8/_R4/_R8（Ceq/Cgt/Cgt_Un/Clt/Clt_Un）
    - 类型化分支 _I8/_R4/_R8（Beq/Bne_Un/Blt/Bgt/Ble/Bge + _Un 整数）
    - 立即数算术 _I8 完整 + _R4/_R8（仅 Add/Sub/Mul/Div/Rem）
    - 立即数比较 _I8/_R4/_R8
    - 立即数分支 _I8/_R4/_R8
  - [ ] 全部加 `// Neo only` 注释，按类别分组
  - [ ] **不**新增 _I4 后缀变体；I4 直接复用 Legacy `Add`/`Addi`/`Beq`/`Beqi`/`Ceqi`/...
  - [ ] 不删除任何已有 enum 值

- [ ] Task 2: OpCodeR union 新增 ushort 别名（最小化）
  - [ ] 在 [OpCode.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/OpCodes/OpCode.cs) 的 OpCodeR struct 内 `#if ENABLE_NEO_MODE` 添加：
    - `[FieldOffset(4)] public ushort DstOffset` （覆盖 Register1）
    - `[FieldOffset(6)] public ushort SrcOffset` （覆盖 Register2）
  - [ ] **不**新增其它别名字段；Register3/Operand/OperandLong/OperandFloat/OperandDouble/Operand2/Operand3/Operand4 全部沿用 Legacy 名
  - [ ] 验证 OpCodeR 总大小仍为 24 字节
  - [ ] 添加注释：Neo 模式下 Register1/Register2/Register3 存 byte 偏移（按 ushort 解读）而非 slot 索引；Legacy 模式不变

- [ ] Task 3: NeoPrimitiveTypeTag 与 InferPrimTag helper
  - [ ] 在 [JITCompiler.cs](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs) 内声明 `internal enum NeoPrimitiveTypeTag : int { I4=0, U4=1, I8=2, U8=3, R4=4, R8=5 }`，仅 `#if ENABLE_NEO_MODE`
  - [ ] 实现 `static NeoPrimitiveTypeTag InferPrimTag(IType t, AppDomain appdomain)`：
    - byte/sbyte/short/ushort/char/bool/int → I4
    - uint → U4；long → I8；ulong → U8；float → R4；double → R8
    - IntPtr/UIntPtr → 按 IntPtr.Size
    - enum → underlying type 的 tag
  - [ ] fallback 默认返回 I4

- [ ] Task 4: JITCompiler virtualStackTypes 维护
  - [ ] 在 Neo 编译路径分配 `IType[] virtualStackTypes`
  - [ ] 在每个修改 baseRegIdx 的地方同步更新
  - [ ] Ldarg/Ldloc/Ldc_*/算术结果/比较结果/Conv 等同步推送类型
  - [ ] 立即数折叠时同步推导
  - [ ] 仅 `#if ENABLE_NEO_MODE`

- [ ] Task 5: JITCompiler 算术/比较/分支/Conv Neo 分流
  - [ ] 三槽算术（Add/Sub/...）、单目（Neg/Not）、比较（Ceq/Cgt/Clt/Un）、类型化分支（Beq/.../Bge_Un）：
    - I4 → 沿用 Legacy 现有 enum（不变）
    - I8/R4/R8 → 选择对应后缀变体
  - [ ] 立即数变体（Addi/Beqi/Ceqi/...）：
    - I4 → 沿用 Legacy 现有 enum
    - I8/R4/R8 → 后缀变体
  - [ ] **指令字段编排沿用 Legacy 完全不变**——JIT 不需要为 Neo 重排寄存器/立即数/target 字段
  - [ ] Conv 在 Neo 模式下设置 `op.Operand2 = (int)NeoPrimitiveTypeTag.<src>`
  - [ ] Br/Brtrue/Brfalse 不变
  - [ ] Legacy 路径完全不变

- [ ] Task 6: 实现 LowerNeoOffsets pass
  - [ ] 在 JITCompiler 优化流水线末尾（BCP/FCP/InlineMethod/ELDC/RegisterCleanup 之后）调用 `LowerNeoOffsets(ref CompiledFrame frame)`
  - [ ] 仅 `#if ENABLE_NEO_MODE`
  - [ ] **入口断言**：`if (frame.TotalStructSize > ushort.MaxValue) throw new NotSupportedException(...)`，**不**回退 Legacy
  - [ ] **核心逻辑**：根据 opcode 把 Register1/2/3 中作为寄存器索引的 short 替换为 byte offset。r1/r2 用 `op.DstOffset`/`op.SrcOffset = (ushort)localInfos[reg].Offset`；r3 用 `op.Register3 = (short)localInfos[reg].Offset`（位模式即 ushort）
  - [ ] 分组覆盖（按"Register 字段哪些是寄存器索引"分类）：
    - **r1 + r2 + r3 都是寄存器**：三槽算术/比较 I4 + _I8/_R4/_R8 后缀
    - **r1 + r2 是寄存器**：单目（Neg/Not）+ 后缀；Move；Conv_*；立即数算术/比较 I4 + 后缀
    - **r1 + r2 是寄存器（双源分支）**：Beq/Bne_Un/Blt/.../Bge_Un 及 _I8/_R4/_R8 后缀（target 在 Operand 不动）
    - **仅 r1 是寄存器**：Brtrue/Brfalse(+_S)（额外写 `op.Operand2 = localInfos[r1].Size` 区分 4/8）；立即数分支 Beqi/.../Bgei_Un + 后缀；Initobj；Ldc_*
    - **Ret 特殊**：r1 ≥ 0 才替换（无返回值时为占位）
    - **无源分支 Br/Br_S**：什么都不动
    - **Box/Unbox/Unbox_Any**：r1+r2 替换为 byte offset；额外写 `op.Operand3 = (r1.RefOffset << 16) | (r2.RefOffset & 0xFFFF)`
  - [ ] 关键约束：每条指令内**先把所需 LocalInfos 字段（Offset、Size、RefOffset）全部读到本地变量**再覆写 Register1/2/3
  - [ ] 添加 `[Conditional("DEBUG")]` 校验：lowering 后未识别 opcode 输出 warning

- [ ] Task 7: ILIntepreter.Neo.cs 实现常量与 Move
  - [ ] Ldc_I4_M1..Ldc_I4_8 各 case：写常量到 `frameBase + ip->DstOffset`
  - [ ] Ldc_I4/Ldc_I4_S → 写 `ip->Operand`
  - [ ] Ldc_I8 → 写 `ip->OperandLong`
  - [ ] Ldc_R4 → 写 `ip->OperandFloat`；Ldc_R8 → 写 `ip->OperandDouble`
  - [ ] Move → `Unsafe.CopyBlock(frameBase + ip->DstOffset, frameBase + ip->SrcOffset, (uint)ip->Operand2)`

- [ ] Task 8: ILIntepreter.Neo.cs 实现三槽算术与单目（I4 默认 + _I8/_R4/_R8）
  - [ ] Add/Sub/Mul/Div/Div_Un/Rem/Rem_Un (I4)：`*(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) op *(int*)(frameBase + (ushort)ip->Register3)`
  - [ ] Add_I8/.../Rem_Un_I8 用 long*；Add_R4/.../Rem_R4 用 float*；Add_R8/.../Rem_R8 用 double*
  - [ ] And/Or/Xor (I4) + _I8 变体
  - [ ] Shl/Shr/Shr_Un (I4) + _I8（移位计数从 `(ushort)ip->Register3` 处读 32 位）
  - [ ] Neg (I4) + Neg_I8/_R4/_R8；Not (I4) + Not_I8（DstOffset/SrcOffset 寻址）
  - [ ] Div/Div_I8 整数除以零抛 DivideByZeroException
  - [ ] R4/R8 按 IEEE 754

- [ ] Task 9: ILIntepreter.Neo.cs 实现比较与分支
  - [ ] Ceq/Cgt/Cgt_Un/Clt/Clt_Un (I4) + _I8/_R4/_R8 写 0/1 (int) 到 DstOffset
  - [ ] Beq/Bne_Un/Blt/Bgt/Ble/Bge + _Un (I4) + 后缀：读 DstOffset(=src1) / SrcOffset(=src2)，满足条件 `ip = ptr + ip->Operand; continue;`
  - [ ] Br：`ip = ptr + ip->Operand; continue;`
  - [ ] Brtrue/Brfalse(+_S)：按 `ip->Operand2` (4/8) 读 DstOffset(=src)，与 0 比较；满足条件跳到 `ip->Operand`
  - [ ] 浮点 NaN 比较不跳转

- [ ] Task 10: ILIntepreter.Neo.cs 实现立即数算术/比较
  - [ ] **Addi/Subi/Muli/Divi/Divi_Un/Remi/Remi_Un/Andi/Ori/Xori/Shli/Shri/Shri_Un (I4)** + _R4 后缀：src 走 SrcOffset，imm 走 Operand/OperandFloat，dst → DstOffset
  - [ ] _I8 + _R8 后缀：imm 走 OperandLong/OperandDouble
  - [ ] Ceqi/Cgti/Cgti_Un/Clti/Clti_Un (I4) + 后缀：dst 写 0/1 (int)

- [ ] Task 11: ILIntepreter.Neo.cs 实现立即数分支
  - [ ] Beqi/Bnei_Un/Blti/Bgti/Blei/Bgei + _Un (I4) + 后缀：src 走 DstOffset，imm 走 Operand/OperandFloat/OperandLong/OperandDouble，target 走 `ip->Operand4`
  - [ ] 满足条件 `ip = ptr + ip->Operand4; continue;`
  - [ ] 浮点 NaN 立即数比较不跳转

- [ ] Task 12: ILIntepreter.Neo.cs 实现 Conv 指令
  - [ ] Conv_I4/U4/I8/U8/R4/R8 case：按 `(NeoPrimitiveTypeTag)ip->Operand2` switch；src 从 SrcOffset，dst 写 DstOffset
  - [ ] Conv_I1/U1/I2/U2 case：截断 + 符号/零扩展到 4-byte int
  - [ ] 默认（Operand2 未知）按 I4 处理

- [ ] Task 13: 重构 Step 5 既有 handler
  - [ ] **Ret**：用 `ip->DstOffset` （即原 Register1，作为返回值源 byte off）；引用拷贝部分保留 LocalInfos 查询，加 `// TODO Step 7` 注释
  - [ ] **Initobj**：用 `ip->DstOffset`；type token 走 `ip->Operand`
  - [ ] **Box/Unbox/Unbox_Any**：dst 走 `ip->DstOffset`，src 走 `ip->SrcOffset`，type token 走 `ip->Operand`，RefOffset 从 `ip->Operand3` 解码（高 16=dst.RefOffset, 低 16=src.RefOffset）
  - [ ] 验证 Step 5 测试不回退

- [ ] Task 14: default 防御
  - [ ] ILIntepreter.Neo.cs switch 末尾 `default:` 抛 `NotImplementedException($"Neo: opcode {code} not yet implemented (Step 6)")`

- [ ] Task 15: OpCodeR sizeof 验证
  - [ ] 测试中断言 `sizeof(OpCodeR) == 24`

- [ ] Task 16: 编译与端到端验证
  - [ ] `dotnet build -c Debug` 通过，0 错误 0 警告
  - [ ] `dotnet build -c Debug_Neo` 通过，0 错误 0 警告
  - [ ] 在 `ILRuntimeTest` 添加 smoke 测试：
    - `int Add(int a, int b)` （I4 三槽 Add）
    - `int AddConst(int a) { return a + 5; }` （I4 立即数 Addi）
    - `int Max(int a, int b) { return a > b ? a : b; }` （Cgt + Brtrue/Beqi）
    - `int Sum(int n)` （Blti 立即数分支循环）
    - `long AddL(long a, long b)` （Add_I8）
    - `long AddConstL(long a) { return a + 100L; }` （Addi_I8）
    - `bool EqL(long a) { return a == 42L; }` （Ceqi_I8）
    - `void LoopL(long n)` （Blti_I8）
    - `double DivD(double a, double b)` （Div_R8）
    - `long ShiftL(long v, int n)` （Shl_I8）
    - `int Convert(double d)` （Conv_I4 from R8）
  - [ ] Neo 模式输出与 Legacy 一致
  - [ ] Step 5 已有 Box/Unbox/Initobj/Ret 测试不回退
  - [ ] **帧大小溢出测试**：构造一个 `TotalStructSize > 65535` 的方法（含巨型 fixed-size struct local），验证 LowerNeoOffsets 抛 NotSupportedException

# Task Dependencies

- Task 2 是 Task 6-13 前置（OpCodeR 别名）
- Task 3 是 Task 4-5 前置
- Task 4 是 Task 5 前置
- Task 1 是 Task 5、7-13 前置
- Task 5、6 完成后 Task 7-13 才有依据
- Task 13 依赖 Task 6 lowering 已正确处理 Step 5 既有指令
- Task 14、15 是 Task 7-13 扫尾
- Task 16 验证整体；依赖 Task 1-15 全部完成

# 不在本 Spec 范围的内容（明确推迟）

- 引用类型寄存器拷贝、Ldstr、Ldnull 写帧 — Step 7
- 完整的 RefOffset 别名字段 — Step 7
- Switch 指令 Neo 适配 — 留 NotImplementedException
- Ldfld_*/Stfld_* — 已在 Step 1-5 部分实现
- Ldind_*/Stind_*/Ldloca/Ldflda — Step 17
- Conv_Ovf_* — Step 26
- Call/Callvirt/Newobj — Step 8/8b/9
