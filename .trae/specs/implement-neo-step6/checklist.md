# Step 6 Checklist

## 编译验证
- [x] `dotnet build -c Debug` 通过（Legacy 行为不受影响），0 错误 0 警告
- [x] `dotnet build -c Debug_Neo` 通过，0 错误 0 警告
- [ ] Neo 模式下所有现有测试套件保持通过

## OpCodeR union 扩展（最小化）
- [x] 仅新增 `DstOffset` (ushort, FieldOffset 4) 与 `SrcOffset` (ushort, FieldOffset 6) 两个别名，均 `#if ENABLE_NEO_MODE`
- [x] **不**新增其它别名字段
- [ ] 验证 `sizeof(OpCodeR) == 24`
- [x] 字段注释说明：Neo 模式下 Register1/2/3 存 byte 偏移（按 ushort 解读）而非 slot 索引

## OpCodeREnum 扩展（仅 _I8/_R4/_R8 后缀）
- [x] **I4 默认变体**：直接复用现有 `Add`/`Addi`/`Beq`/`Beqi`/`Ceqi`... 等 enum 名，不新增 _I4 后缀
- [x] 三槽算术 _I8 完整 + _R4/_R8（仅 Add/Sub/Mul/Div/Rem 5 个）
- [x] 单目 _I8（Neg/Not）+ _R4/_R8（仅 Neg）
- [x] 比较 _I8/_R4/_R8 完整
- [x] 类型化分支 _I8/_R4/_R8（含 _Un 整数）
- [x] 立即数算术 _I8 完整 + _R4/_R8（仅 Add/Sub/Mul/Div/Rem）
- [x] 立即数比较 _I8/_R4/_R8
- [x] 立即数分支 _I8/_R4/_R8（含 _Un 整数）
- [x] enum 值在 Legacy 路径下从未被生成

## JIT 改造（最小化）
- [x] `NeoPrimitiveTypeTag` 与 `InferPrimTag` helper 实现，仅 Neo 可见
- [x] `virtualStackTypes` 类型推导栈在 JIT Neo 路径正确维护
- [x] **指令字段编排沿用 Legacy 完全不变**（JIT 不重排立即数/target）
- [x] 三槽算术/比较/分支：I4 沿用 Legacy enum，I8/R4/R8 → 后缀变体
- [x] 立即数算术/比较/分支：I4 沿用 Legacy enum，I8/R4/R8 → 后缀变体
- [x] Conv 指令在 Neo 模式下设置 `op.Operand2 = (int)NeoPrimitiveTypeTag.<src>`
- [x] Br/Brtrue/Brfalse 不变
- [x] Legacy 路径生成的指令完全未变

## LowerNeoOffsets pass
- [x] Pass 在所有 Optimizer pass（BCP/FCP/InlineMethod/ELDC/RegisterCleanup）之后调用
- [x] **入口断言** `frame.TotalStructSize <= ushort.MaxValue`，超限抛 NotSupportedException（不回退 Legacy）
- [x] **三槽算术/比较**：r1/r2/r3 都替换为 byte offset（r1/r2 经 DstOffset/SrcOffset，r3 经 (short) 写入位模式即 ushort）
- [x] **r1+r2 类指令**（单目/Move/Conv/立即数算术/立即数比较/双源分支）：r1/r2 替换为 byte offset；其它字段不动
- [x] **仅 r1 类指令**（Brtrue/Brfalse/立即数分支/Initobj/Ldc_*）：仅 r1 替换
- [x] **Brtrue/Brfalse**：额外写入 `op.Operand2 = localInfos[r1].Size`
- [x] **Br/Br_S**：什么都不动
- [x] **Ret**：r1 ≥ 0 才替换
- [x] **Box/Unbox/Unbox_Any**：r1+r2 替换；额外写 Operand3 = (r1.RefOffset << 16) | (r2.RefOffset & 0xFFFF)
- [x] 写入顺序保证安全（先把所需 LocalInfos 字段全部读到本地变量再覆写）
- [x] DEBUG 校验：lowering 后未识别 opcode 输出 warning

## Neo 解释器实现
- [x] 常量加载 Ldc_* 全部 case：写到 `frameBase + ip->DstOffset`
- [x] Move：CopyBlock by `ip->Operand2` size
- [x] **三槽算术 I4 默认**（Add/Sub/Mul/Div/Div_Un/Rem/Rem_Un/And/Or/Xor/Shl/Shr/Shr_Un/Neg/Not）case 实现，使用 DstOffset/SrcOffset/`(ushort)Register3`
- [x] **三槽算术 _I8/_R4/_R8** 后缀变体 case 实现
- [x] **比较 I4 默认**（Ceq/Cgt/Cgt_Un/Clt/Clt_Un）+ _I8/_R4/_R8 后缀
- [x] **类型化分支 I4 默认**（Beq/Bne_Un/Blt/Bgt/Ble/Bge + _Un）+ _I8/_R4/_R8 后缀，跳转 `ip = ptr + ip->Operand`
- [x] 通用分支（Br/Brtrue/Brfalse）case 实现
- [x] **立即数算术 I4 默认**（Addi/Subi/.../Shri_Un）+ _I8/_R4/_R8 后缀，src 走 SrcOffset
- [x] **立即数比较 I4 默认**（Ceqi/Cgti/...）+ _I8/_R4/_R8 后缀
- [x] **立即数分支 I4 默认**（Beqi/Bnei_Un/.../Bgei_Un）+ _I8/_R4/_R8 后缀，src 走 DstOffset，target 走 `ip->Operand4`
- [x] 整数除以零抛 DivideByZeroException
- [x] 浮点 NaN 比较不跳转
- [x] 浮点除以零返回 Infinity/NaN
- [x] Conv_I1/U1/I2/U2/I4/U4/I8/U8/R4/R8 全部实现，按 `ip->Operand2` (NeoPrimitiveTypeTag) 编码源读取
- [x] default 分支 NotImplementedException 包含 OpCode 名称
- [x] **case body 内不再出现 `localInfos[...]` 索引（Step 5 RefOffset 部分除外）**

## Step 5 既有 handler 重构
- [x] Ret handler 改用 `ip->DstOffset` 直接寻址
- [x] Initobj handler 改用 `ip->DstOffset`；type token 走 `ip->Operand`
- [x] Box/Unbox/Unbox_Any handler 改用 `ip->DstOffset` (dst) / `ip->SrcOffset` (src)；type token 走 `ip->Operand`；RefOffset 从 Operand3 解码
- [x] Step 5 既有端到端测试用例保持通过

## 端到端 Smoke 测试
> 测试代码：[TestCases/NeoStep6Test.cs](file:///f:/SVN/ILRuntime/TestCases/NeoStep6Test.cs)；`ILRuntimeTestCLI` 增加 `nameFilter` 参数以便聚焦 `NeoStep6Test`，并在 `Main` 外层加 try/catch 输出未处理异常，避免被 .NET 进程直接吞掉退出码。
>
> **临时跳过 cctor**：`ENABLE_NEO_MODE` 下 [ILType.cs](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs) 的 `InitializeMethods` / `StaticInstance` 两处条件编译跳过了 `appdomain.Invoke(staticConstructor)`，避免触发尚未实现的 `Stfld_*` / `Ldfld_*` case。Step 7（引用类型寄存器拷贝 + `Ldfld_*` / `Stfld_*` 完整实现）落地时一并恢复。
>
> 顺便发现并修复的 Step 3-4 阻塞：[ILType.cs#L317-L373](file:///f:/SVN/ILRuntime/ILRuntime/CLR/TypeSystem/ILType.cs#L317-L373) 中 `TotalPrimitiveSize`/`TotalReferenceCount`/`StaticTotalPrimitiveSize`/`StaticTotalReferenceCount` 用 `< 0` 作为未初始化哨兵，循环字段图（如 struct 静态字段间接引用自身）会重入 `InitializeFields()` 导致栈溢出。已改为按 `fieldMapping == null` 判定。
>
> 顺便发现并修复的 Move lowering bug：[JITCompiler.cs Move case](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1130-L1148) 之前只取 `src.Size`，当 stack register (`maxSize=8`) 写回 int local (`size=4`) 时会 `CopyBlock(8)` 覆盖相邻 slot，导致循环计数器被破坏（`NeoSumI4` 死循环）。改为 `min(src.Size, dst.Size)`。

- [x] Neo `int Add(int a, int b)` 正确（I4 三槽 Add）— NeoAddI4
- [x] Neo `int AddConst(int a) { return a + 5; }` 正确（I4 立即数 Addi）— NeoAddiI4
- [x] Neo `int Max(int a, int b)` 正确 — NeoMaxI4
- [x] Neo `int Sum(int n)` 求和正确（I4 立即数分支 Blti）— NeoSumI4（Move lowering fix 后通过）
- [x] Neo `long AddL(long a, long b)` 正确（Add_I8）— NeoAddI8
- [x] Neo `long AddConstL(long a) { return a + 100L; }` 正确（Addi_I8）— NeoAddiI8
- [x] Neo `bool EqL(long a) { return a == 42L; }` 正确（Ceqi_I8）— NeoEqI8
- [x] Neo `for (long i = 0; i < n; i++) {}` 正确（Blti_I8）— NeoSumI8
- [x] Neo `double DivD(double a, double b)` 与 Legacy 一致 — NeoDivR8
- [x] Neo R8 NaN 比较不跳转 — NeoNaNR8
- [x] Neo `long ShiftL(long v, int n)` 与 Legacy 一致 — NeoShlI8
- [x] Neo `int Convert(double d)` 与 Legacy 一致 — NeoConvR8ToI4
- [x] Neo Conv I4→I8 符号扩展 — NeoConvI4ToI8
- [x] Neo 整数除零抛 DivideByZeroException — NeoDivByZeroI4
- [ ] **帧大小溢出**：构造 TotalStructSize > 65535 的方法（巨型 fixed 数组 struct local），LowerNeoOffsets 抛 NotSupportedException
