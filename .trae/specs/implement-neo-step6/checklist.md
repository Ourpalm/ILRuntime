# Step 6 Checklist

## 编译验证
- [ ] `dotnet build -c Debug` 通过（Legacy 行为不受影响），0 错误 0 警告
- [ ] `dotnet build -c Debug_Neo` 通过，0 错误 0 警告
- [ ] Neo 模式下所有现有测试套件保持通过

## OpCodeR union 扩展（最小化）
- [ ] 仅新增 `DstOffset` (ushort, FieldOffset 4) 与 `SrcOffset` (ushort, FieldOffset 6) 两个别名，均 `#if ENABLE_NEO_MODE`
- [ ] **不**新增其它别名字段
- [ ] 验证 `sizeof(OpCodeR) == 24`
- [ ] 字段注释说明：Neo 模式下 Register1/2/3 存 byte 偏移（按 ushort 解读）而非 slot 索引

## OpCodeREnum 扩展（仅 _I8/_R4/_R8 后缀）
- [ ] **I4 默认变体**：直接复用现有 `Add`/`Addi`/`Beq`/`Beqi`/`Ceqi`... 等 enum 名，不新增 _I4 后缀
- [ ] 三槽算术 _I8 完整 + _R4/_R8（仅 Add/Sub/Mul/Div/Rem 5 个）
- [ ] 单目 _I8（Neg/Not）+ _R4/_R8（仅 Neg）
- [ ] 比较 _I8/_R4/_R8 完整
- [ ] 类型化分支 _I8/_R4/_R8（含 _Un 整数）
- [ ] 立即数算术 _I8 完整 + _R4/_R8（仅 Add/Sub/Mul/Div/Rem）
- [ ] 立即数比较 _I8/_R4/_R8
- [ ] 立即数分支 _I8/_R4/_R8（含 _Un 整数）
- [ ] enum 值在 Legacy 路径下从未被生成

## JIT 改造（最小化）
- [ ] `NeoPrimitiveTypeTag` 与 `InferPrimTag` helper 实现，仅 Neo 可见
- [ ] `virtualStackTypes` 类型推导栈在 JIT Neo 路径正确维护
- [ ] **指令字段编排沿用 Legacy 完全不变**（JIT 不重排立即数/target）
- [ ] 三槽算术/比较/分支：I4 沿用 Legacy enum，I8/R4/R8 → 后缀变体
- [ ] 立即数算术/比较/分支：I4 沿用 Legacy enum，I8/R4/R8 → 后缀变体
- [ ] Conv 指令在 Neo 模式下设置 `op.Operand2 = (int)NeoPrimitiveTypeTag.<src>`
- [ ] Br/Brtrue/Brfalse 不变
- [ ] Legacy 路径生成的指令完全未变

## LowerNeoOffsets pass
- [ ] Pass 在所有 Optimizer pass（BCP/FCP/InlineMethod/ELDC/RegisterCleanup）之后调用
- [ ] **入口断言** `frame.TotalStructSize <= ushort.MaxValue`，超限抛 NotSupportedException（不回退 Legacy）
- [ ] **三槽算术/比较**：r1/r2/r3 都替换为 byte offset（r1/r2 经 DstOffset/SrcOffset，r3 经 (short) 写入位模式即 ushort）
- [ ] **r1+r2 类指令**（单目/Move/Conv/立即数算术/立即数比较/双源分支）：r1/r2 替换为 byte offset；其它字段不动
- [ ] **仅 r1 类指令**（Brtrue/Brfalse/立即数分支/Initobj/Ldc_*）：仅 r1 替换
- [ ] **Brtrue/Brfalse**：额外写入 `op.Operand2 = localInfos[r1].Size`
- [ ] **Br/Br_S**：什么都不动
- [ ] **Ret**：r1 ≥ 0 才替换
- [ ] **Box/Unbox/Unbox_Any**：r1+r2 替换；额外写 Operand3 = (r1.RefOffset << 16) | (r2.RefOffset & 0xFFFF)
- [ ] 写入顺序保证安全（先把所需 LocalInfos 字段全部读到本地变量再覆写）
- [ ] DEBUG 校验：lowering 后未识别 opcode 输出 warning

## Neo 解释器实现
- [ ] 常量加载 Ldc_* 全部 case：写到 `frameBase + ip->DstOffset`
- [ ] Move：CopyBlock by `ip->Operand2` size
- [ ] **三槽算术 I4 默认**（Add/Sub/Mul/Div/Div_Un/Rem/Rem_Un/And/Or/Xor/Shl/Shr/Shr_Un/Neg/Not）case 实现，使用 DstOffset/SrcOffset/`(ushort)Register3`
- [ ] **三槽算术 _I8/_R4/_R8** 后缀变体 case 实现
- [ ] **比较 I4 默认**（Ceq/Cgt/Cgt_Un/Clt/Clt_Un）+ _I8/_R4/_R8 后缀
- [ ] **类型化分支 I4 默认**（Beq/Bne_Un/Blt/Bgt/Ble/Bge + _Un）+ _I8/_R4/_R8 后缀，跳转 `ip = ptr + ip->Operand`
- [ ] 通用分支（Br/Brtrue/Brfalse）case 实现
- [ ] **立即数算术 I4 默认**（Addi/Subi/.../Shri_Un）+ _I8/_R4/_R8 后缀，src 走 SrcOffset
- [ ] **立即数比较 I4 默认**（Ceqi/Cgti/...）+ _I8/_R4/_R8 后缀
- [ ] **立即数分支 I4 默认**（Beqi/Bnei_Un/.../Bgei_Un）+ _I8/_R4/_R8 后缀，src 走 DstOffset，target 走 `ip->Operand4`
- [ ] 整数除以零抛 DivideByZeroException
- [ ] 浮点 NaN 比较不跳转
- [ ] 浮点除以零返回 Infinity/NaN
- [ ] Conv_I1/U1/I2/U2/I4/U4/I8/U8/R4/R8 全部实现，按 `ip->Operand2` (NeoPrimitiveTypeTag) 编码源读取
- [ ] default 分支 NotImplementedException 包含 OpCode 名称
- [ ] **case body 内不再出现 `localInfos[...]` 索引（Step 5 RefOffset 部分除外）**

## Step 5 既有 handler 重构
- [ ] Ret handler 改用 `ip->DstOffset` 直接寻址
- [ ] Initobj handler 改用 `ip->DstOffset`；type token 走 `ip->Operand`
- [ ] Box/Unbox/Unbox_Any handler 改用 `ip->DstOffset` (dst) / `ip->SrcOffset` (src)；type token 走 `ip->Operand`；RefOffset 从 Operand3 解码
- [ ] Step 5 既有端到端测试用例保持通过

## 端到端 Smoke 测试
- [ ] Neo `int Add(int a, int b)` 正确（I4 三槽 Add）
- [ ] Neo `int AddConst(int a) { return a + 5; }` 正确（I4 立即数 Addi）
- [ ] Neo `int Max(int a, int b)` 正确
- [ ] Neo `int Sum(int n)` 求和正确（I4 立即数分支 Blti）
- [ ] Neo `long AddL(long a, long b)` 正确（Add_I8）
- [ ] Neo `long AddConstL(long a) { return a + 100L; }` 正确（Addi_I8）
- [ ] Neo `bool EqL(long a) { return a == 42L; }` 正确（Ceqi_I8）
- [ ] Neo `for (long i = 0; i < n; i++) {}` 正确（Blti_I8）
- [ ] Neo `double DivD(double a, double b)` 与 Legacy 一致
- [ ] Neo `long ShiftL(long v, int n)` 与 Legacy 一致
- [ ] Neo `int Convert(double d)` 与 Legacy 一致
- [ ] Neo 模式下 mStack.Count 在测试方法返回后恢复到调用前
- [ ] **帧大小溢出**：构造 TotalStructSize > 65535 的方法（巨型 fixed 数组 struct local），LowerNeoOffsets 抛 NotSupportedException
