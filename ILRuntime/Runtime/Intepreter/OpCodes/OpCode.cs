using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Intepreter.OpCodes
{
  
    /// <summary>
    /// IL指令
    /// </summary>
    struct OpCode
    {
        /// <summary>
        /// 当前指令
        /// </summary>
        public OpCodeEnum Code;

        /// <summary>
        ///  Int32 操作数
        /// </summary>
        public int TokenInteger;

        /// <summary>
        /// Int64 操作数
        /// </summary>
        public long TokenLong;
    }

    /// <summary>
    /// Register machine opcode
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct OpCodeR
    {
        [FieldOffset(0)]
        public OpCodeREnum Code;
        [FieldOffset(4)]
        public short Register1;
        // Neo mode: byte offsets in frame. Register1/2/3 reinterpreted as
        // ushort byte offsets (0..65535) post-LowerNeoOffsets pass.
        // DstOffset shares Register1, SrcOffset shares Register2.
        // Register3 used directly as byte offset via (ushort)op.Register3.
        [FieldOffset(4)]
        public ushort DstOffset;
        [FieldOffset(6)]
        public short Register2;
        [FieldOffset(6)]
        public ushort SrcOffset;
        [FieldOffset(8)]
        public short Register3;
        [FieldOffset(8)]
        public ushort OperandOffset;
        [FieldOffset(10)]
        public short Register4;
        [FieldOffset(8)]
        public int Operand;
        [FieldOffset(8)]
        public float OperandFloat;
        [FieldOffset(12)]
        public int Operand2;
        [FieldOffset(16)]
        public int Operand3;
        [FieldOffset(12)]
        public long OperandLong;
        [FieldOffset(12)]
        public double OperandDouble;
        [FieldOffset(20)]
        public int Operand4;

        public override string ToString()
        {

            return ToString(null, null, false);
        }

        public string ToString(Enviorment.AppDomain domain)
        {
            return ToString(domain, null, false);
        }

        // Neo-aware pretty-print entrypoint. Passing a non-null `method` and `isNeoMode=true`
        // enables receiver-slot type lookup so field-access opcodes can append `; Class.Field`.
        public string ToString(ILMethod method, bool isNeoMode = false)
        {
            return ToString(method?.AppDomain, method, isNeoMode);
        }

        static bool IsStoreFieldOpcode(OpCodeREnum code)
        {
            switch (code)
            {
                case OpCodeREnum.Stfld_I1:
                case OpCodeREnum.Stfld_I2:
                case OpCodeREnum.Stfld_I4:
                case OpCodeREnum.Stfld_I8:
                case OpCodeREnum.Stfld_U1:
                case OpCodeREnum.Stfld_U2:
                case OpCodeREnum.Stfld_U4:
                case OpCodeREnum.Stfld_U8:
                case OpCodeREnum.Stfld_R4:
                case OpCodeREnum.Stfld_R8:
                    return true;
                default:
                    return false;
            }
        }

        // Look up the field name behind a Neo-lowered Ldfld/Stfld primOff. `receiverReg` is the
        // register-index form of the receiver operand pre-lowering; when the dump path has
        // already lowered offsets it's actually the receiver's byte offset. We handle both by
        // scanning LocalInfos for a matching Offset (byte-offset form) then falling back to
        // treating the input as a register index. Returns null when the lookup cannot resolve
        // to a concrete field name.
        static string TryLookupFieldName(ILMethod method, int receiverIdx, int fieldPrimitiveOffset, bool isNeoLowered)
        {
#if ENABLE_NEO_MODE
            if (method == null)
                return null;
            ref readonly var frame = ref method.CompiledFrame;
            var locals = frame.LocalInfos;
            var types = frame.LocalTypes;
            if (locals == null || types == null)
                return null;

            int reg = -1;
            if (isNeoLowered)
            {
                // Register fields hold byte offsets after LowerNeoOffsets; map back to reg idx.
                for (int i = 0; i < locals.Length; i++)
                {
                    if (locals[i].Offset == receiverIdx)
                    {
                        reg = i;
                        break;
                    }
                }
            }
            else
            {
                reg = receiverIdx;
            }
            if (reg < 0 || reg >= types.Length)
                return null;
            var recvType = types[reg];
            if (recvType is CLR.TypeSystem.ILType ilType &&
                ilType.TryGetFieldNameByPrimitiveOffset(fieldPrimitiveOffset, out string fname))
            {
                return string.Format("{0}.{1}", ilType.TypeDefinition.Name, fname);
            }
            return null;
#else
            return null;
#endif
        }

        string ToString(Enviorment.AppDomain domain, ILMethod method, bool isNeoMode)
        {
            string param = null;
            string nameSuffix = "";
            switch (Code)
            {
                case OpCodeREnum.Move:
                    if (Operand == 0)
                    {
                        param = string.Format("r{0}, r{1}, size={2}", Register1, Register2, Operand2);
                    }
                    else if (Operand == 1)
                    {
                        param = string.Format("r{0}, r{1}", Register1, Register2);
                    }
                    else
                    {
                        param = string.Format("r{0}, r{1}, vt(sz={2}, refs={3})", Register1, Register2, Operand2, Operand);
                    }
                    break;
                case OpCodeREnum.Ldind_I:
                case OpCodeREnum.Ldind_I1:
                case OpCodeREnum.Ldind_I2:
                case OpCodeREnum.Ldind_I4:
                case OpCodeREnum.Ldind_I8:
                case OpCodeREnum.Ldind_R4:
                case OpCodeREnum.Ldind_R8:
                case OpCodeREnum.Ldind_U1:
                case OpCodeREnum.Ldind_U2:
                case OpCodeREnum.Ldind_U4:
                case OpCodeREnum.Ldind_Ref:
                case OpCodeREnum.Ldobj:
                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Stind_Ref:
                case OpCodeREnum.Stobj:
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Conv_I:
                case OpCodeREnum.Conv_I1:
                case OpCodeREnum.Conv_I2:
                case OpCodeREnum.Conv_I4:
                case OpCodeREnum.Conv_I8:
                case OpCodeREnum.Conv_Ovf_I:
                case OpCodeREnum.Conv_Ovf_I1:
                case OpCodeREnum.Conv_Ovf_I1_Un:
                case OpCodeREnum.Conv_Ovf_I2:
                case OpCodeREnum.Conv_Ovf_I2_Un:
                case OpCodeREnum.Conv_Ovf_I4:
                case OpCodeREnum.Conv_Ovf_I4_Un:
                case OpCodeREnum.Conv_Ovf_I8:
                case OpCodeREnum.Conv_Ovf_I8_Un:
                case OpCodeREnum.Conv_Ovf_I_Un:
                case OpCodeREnum.Conv_Ovf_U:
                case OpCodeREnum.Conv_Ovf_U1:
                case OpCodeREnum.Conv_Ovf_U1_Un:
                case OpCodeREnum.Conv_Ovf_U2:
                case OpCodeREnum.Conv_Ovf_U2_Un:
                case OpCodeREnum.Conv_Ovf_U4:
                case OpCodeREnum.Conv_Ovf_U4_Un:
                case OpCodeREnum.Conv_Ovf_U8:
                case OpCodeREnum.Conv_Ovf_U8_Un:
                case OpCodeREnum.Conv_Ovf_U_Un:
                case OpCodeREnum.Conv_R4:
                case OpCodeREnum.Conv_R8:
                case OpCodeREnum.Conv_R_Un:
                case OpCodeREnum.Conv_U:
                case OpCodeREnum.Conv_U1:
                case OpCodeREnum.Conv_U2:
                case OpCodeREnum.Conv_U4:
                case OpCodeREnum.Conv_U8:
                case OpCodeREnum.Not:
                case OpCodeREnum.Neg:
#if ENABLE_NEO_MODE
                case OpCodeREnum.Neg_I8:
                case OpCodeREnum.Not_I8:
                case OpCodeREnum.Neg_R4:
                case OpCodeREnum.Neg_R8:
#endif
                    param = string.Format("r{0}, r{1}", Register1, Register2);
                    break;
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
                case OpCodeREnum.Isinst:
                case OpCodeREnum.Castclass:
                    if (domain == null)
                        param = string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand);
                    else
                    {
                        var type = domain.GetType(Operand);
                        param = string.Format("r{0}, r{1}, {2}", Register1, Register2, type);
                    }
                    break;

                case OpCodeREnum.Stfld:
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldflda:
                    param = string.Format("r{0}, r{1}, 0x{2:X8}", Register1, Register2, OperandLong);
                    break;
                case OpCodeREnum.Ldfld_I1:
                case OpCodeREnum.Ldfld_I2:
                case OpCodeREnum.Ldfld_I4:
                case OpCodeREnum.Ldfld_I8:
                case OpCodeREnum.Ldfld_U1:
                case OpCodeREnum.Ldfld_U2:
                case OpCodeREnum.Ldfld_U4:
                case OpCodeREnum.Ldfld_U8:
                case OpCodeREnum.Ldfld_R4:
                case OpCodeREnum.Ldfld_R8:
                case OpCodeREnum.Stfld_I1:
                case OpCodeREnum.Stfld_I2:
                case OpCodeREnum.Stfld_I4:
                case OpCodeREnum.Stfld_I8:
                case OpCodeREnum.Stfld_U1:
                case OpCodeREnum.Stfld_U2:
                case OpCodeREnum.Stfld_U4:
                case OpCodeREnum.Stfld_U8:
                case OpCodeREnum.Stfld_R4:
                case OpCodeREnum.Stfld_R8:
                    if (isNeoMode && Operand4 < 0)
                    {
                        nameSuffix = ".ref";
                        param = string.Format("r{0}, r{1}, primOff={2}", Register1, Register2, Operand2);
                    }
                    else if (isNeoMode && Operand4 > 0)
                    {
                        nameSuffix = ".inline";
                        param = string.Format("r{0}, r{1}, primOff={2}", Register1, Register2, Operand2);
                    }
                    else
                    {
                        if (domain != null)
                            param = string.Format("r{0}, r{1}, 0x{2:X8}, {3}({4},{5})", Register1, Register2, OperandLong, domain.GetType(Operand), Operand2, Operand3);
                        else
                            param = string.Format("r{0}, r{1}, 0x{2:X8}, {3}({4},{5})", Register1, Register2, OperandLong, Operand, Operand2, Operand3);
                    }
                    if (isNeoMode && method != null && Operand4 != 0)
                    {
                        int recv = IsStoreFieldOpcode(Code) ? Register1 : Register2;
                        string fname = TryLookupFieldName(method, recv, Operand2, isNeoLowered: true);
                        if (fname != null)
                            param = string.Format("{0}  ; {1}", param, fname);
                    }
                    break;
                case OpCodeREnum.Ldfld_Ref:
                case OpCodeREnum.Stfld_Ref:
                    if (isNeoMode && Operand4 < 0)
                    {
                        nameSuffix = ".ref";
                        param = string.Format("r{0}, r{1}, primOff={2},refOff={3}", Register1, Register2, Operand2, Operand3);
                    }
                    else if (isNeoMode && Operand4 > 0)
                    {
                        nameSuffix = ".inline";
                        param = string.Format("r{0}, r{1}, primOff={2},refOff={3},slotRO={4}", Register1, Register2, Operand2, Operand3, Operand4 - 1);
                    }
                    else
                    {
                        if (domain != null)
                            param = string.Format("r{0}, r{1}, 0x{2:X8}, {3}({4},{5})", Register1, Register2, OperandLong, domain.GetType(Operand), Operand2, Operand3);
                        else
                            param = string.Format("r{0}, r{1}, 0x{2:X8}, {3}({4},{5})", Register1, Register2, OperandLong, Operand, Operand2, Operand3);
                    }
                    if (isNeoMode && method != null && Operand4 != 0)
                    {
                        int recv = Code == OpCodeREnum.Stfld_Ref ? Register1 : Register2;
                        string fname = TryLookupFieldName(method, recv, Operand2, isNeoLowered: true);
                        if (fname != null)
                            param = string.Format("{0}  ; {1}", param, fname);
                    }
                    break;
                case OpCodeREnum.Ldfld_Value:
                case OpCodeREnum.Stfld_Value:
                    if (isNeoMode && Operand4 < 0)
                        nameSuffix = ".ref";
                    else if (isNeoMode && Operand4 > 0)
                        nameSuffix = ".inline";
                    param = string.Format("r{0}, r{1}, sz={2}(fpo=0x{3:x},fro={4},o2h={5},refCnt={6})",
                        Register1, Register2, Operand,
                        Operand2 & 0xFFFF, Operand3 & 0xFFFF,
                        (Operand2 >> 16) & 0xFFFF, (Operand3 >> 16) & 0xFFFF);
                    if (isNeoMode && method != null && Operand4 != 0)
                    {
                        int recv = Code == OpCodeREnum.Stfld_Value ? Register1 : Register2;
                        string fname = TryLookupFieldName(method, recv, Operand2 & 0xFFFF, isNeoLowered: true);
                        if (fname != null)
                            param = string.Format("{0}  ; {1}", param, fname);
                    }
                    break;
                case OpCodeREnum.Stsfld:
                case OpCodeREnum.Ldsfld:
                case OpCodeREnum.Ldsflda:
                    param = string.Format("r{0}, 0x{1:X8}", Register1, OperandLong);
                    break;

                case OpCodeREnum.Beqi:
                case OpCodeREnum.Bgei:
                case OpCodeREnum.Bgei_Un:
                case OpCodeREnum.Bgti:
                case OpCodeREnum.Bgti_Un:
                case OpCodeREnum.Bnei_Un:
                case OpCodeREnum.Blei:
                case OpCodeREnum.Blei_Un:
                case OpCodeREnum.Blti:
                case OpCodeREnum.Blti_Un:
#if ENABLE_NEO_MODE
                case OpCodeREnum.Beqi_I8:
                case OpCodeREnum.Bgei_I8:
                case OpCodeREnum.Bgei_Un_I8:
                case OpCodeREnum.Bgti_I8:
                case OpCodeREnum.Bgti_Un_I8:
                case OpCodeREnum.Bnei_Un_I8:
                case OpCodeREnum.Blei_I8:
                case OpCodeREnum.Blei_Un_I8:
                case OpCodeREnum.Blti_I8:
                case OpCodeREnum.Blti_Un_I8:
                case OpCodeREnum.Beqi_R4:
                case OpCodeREnum.Bgei_R4:
                case OpCodeREnum.Bgei_Un_R4:
                case OpCodeREnum.Bgti_R4:
                case OpCodeREnum.Bgti_Un_R4:
                case OpCodeREnum.Bnei_Un_R4:
                case OpCodeREnum.Blei_R4:
                case OpCodeREnum.Blei_Un_R4:
                case OpCodeREnum.Blti_R4:
                case OpCodeREnum.Blti_Un_R4:
                case OpCodeREnum.Beqi_R8:
                case OpCodeREnum.Bgei_R8:
                case OpCodeREnum.Bgei_Un_R8:
                case OpCodeREnum.Bgti_R8:
                case OpCodeREnum.Bgti_Un_R8:
                case OpCodeREnum.Bnei_Un_R8:
                case OpCodeREnum.Blei_R8:
                case OpCodeREnum.Blei_Un_R8:
                case OpCodeREnum.Blti_R8:
                case OpCodeREnum.Blti_Un_R8:
#endif
                    if (Operand != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, Operand, Operand4);
                    }
                    else if (OperandLong != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, OperandLong, Operand4);
                    }
                    else if (OperandFloat != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, OperandFloat, Operand4);
                    }
                    else if (OperandDouble != 0)
                    {
                        param = string.Format("r{0},{1},{2}", Register1, OperandDouble, Operand4);
                    }
                    else
                    {
                        param = string.Format("r{0},0,{1}", Register1, Operand4);
                    }
                    break;
                case OpCodeREnum.Ceqi:
                case OpCodeREnum.Cgti:
                case OpCodeREnum.Cgti_Un:
                case OpCodeREnum.Clti:
                case OpCodeREnum.Clti_Un:
                case OpCodeREnum.Addi:
                case OpCodeREnum.Subi:
                case OpCodeREnum.Muli:
                case OpCodeREnum.Divi:
                case OpCodeREnum.Remi:
                case OpCodeREnum.Remi_Un:
                case OpCodeREnum.Andi:
                case OpCodeREnum.Ori:
                case OpCodeREnum.Xori:
                case OpCodeREnum.Shli:
                case OpCodeREnum.Shri:
                case OpCodeREnum.Shri_Un:
#if ENABLE_NEO_MODE
                case OpCodeREnum.Ceqi_I8:
                case OpCodeREnum.Cgti_I8:
                case OpCodeREnum.Cgti_Un_I8:
                case OpCodeREnum.Clti_I8:
                case OpCodeREnum.Clti_Un_I8:
                case OpCodeREnum.Ceqi_R4:
                case OpCodeREnum.Cgti_R4:
                case OpCodeREnum.Cgti_Un_R4:
                case OpCodeREnum.Clti_R4:
                case OpCodeREnum.Clti_Un_R4:
                case OpCodeREnum.Ceqi_R8:
                case OpCodeREnum.Cgti_R8:
                case OpCodeREnum.Cgti_Un_R8:
                case OpCodeREnum.Clti_R8:
                case OpCodeREnum.Clti_Un_R8:
                case OpCodeREnum.Addi_I8:
                case OpCodeREnum.Subi_I8:
                case OpCodeREnum.Muli_I8:
                case OpCodeREnum.Divi_I8:
                case OpCodeREnum.Divi_Un_I8:
                case OpCodeREnum.Remi_I8:
                case OpCodeREnum.Remi_Un_I8:
                case OpCodeREnum.Andi_I8:
                case OpCodeREnum.Ori_I8:
                case OpCodeREnum.Xori_I8:
                case OpCodeREnum.Shli_I8:
                case OpCodeREnum.Shri_I8:
                case OpCodeREnum.Shri_Un_I8:
                case OpCodeREnum.Addi_R4:
                case OpCodeREnum.Subi_R4:
                case OpCodeREnum.Muli_R4:
                case OpCodeREnum.Divi_R4:
                case OpCodeREnum.Remi_R4:
                case OpCodeREnum.Addi_R8:
                case OpCodeREnum.Subi_R8:
                case OpCodeREnum.Muli_R8:
                case OpCodeREnum.Divi_R8:
                case OpCodeREnum.Remi_R8:
#endif
                    if (Operand != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, Operand);
                    }
                    else if (OperandLong != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, OperandLong);
                    }
                    else if (OperandFloat != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, OperandFloat);
                    }
                    else if (OperandDouble != 0)
                    {
                        param = string.Format("r{0},r{1},{2}", Register1, Register2, OperandDouble);
                    }
                    else
                    {
                        param = string.Format("r{0},r{1},0", Register1, Register2);
                    }
                    break;
                case OpCodeREnum.Add:
                case OpCodeREnum.Add_Ovf:
                case OpCodeREnum.Add_Ovf_Un:
                case OpCodeREnum.Sub:
                case OpCodeREnum.Sub_Ovf:
                case OpCodeREnum.Sub_Ovf_Un:
                case OpCodeREnum.Mul:
                case OpCodeREnum.Mul_Ovf:
                case OpCodeREnum.Mul_Ovf_Un:
                case OpCodeREnum.Div:
                case OpCodeREnum.Div_Un:
                case OpCodeREnum.Rem:
                case OpCodeREnum.Rem_Un:
                case OpCodeREnum.Xor:
                case OpCodeREnum.And:
                case OpCodeREnum.Or:
                case OpCodeREnum.Shl:
                case OpCodeREnum.Shr:
                case OpCodeREnum.Shr_Un:
                case OpCodeREnum.Clt:
                case OpCodeREnum.Clt_Un:
                case OpCodeREnum.Cgt:
                case OpCodeREnum.Cgt_Un:
                case OpCodeREnum.Ceq:
#if ENABLE_NEO_MODE
                case OpCodeREnum.Add_I8:
                case OpCodeREnum.Sub_I8:
                case OpCodeREnum.Mul_I8:
                case OpCodeREnum.Div_I8:
                case OpCodeREnum.Div_Un_I8:
                case OpCodeREnum.Rem_I8:
                case OpCodeREnum.Rem_Un_I8:
                case OpCodeREnum.And_I8:
                case OpCodeREnum.Or_I8:
                case OpCodeREnum.Xor_I8:
                case OpCodeREnum.Shl_I8:
                case OpCodeREnum.Shr_I8:
                case OpCodeREnum.Shr_Un_I8:
                case OpCodeREnum.Add_R4:
                case OpCodeREnum.Sub_R4:
                case OpCodeREnum.Mul_R4:
                case OpCodeREnum.Div_R4:
                case OpCodeREnum.Rem_R4:
                case OpCodeREnum.Add_R8:
                case OpCodeREnum.Sub_R8:
                case OpCodeREnum.Mul_R8:
                case OpCodeREnum.Div_R8:
                case OpCodeREnum.Rem_R8:
                case OpCodeREnum.Ceq_I8:
                case OpCodeREnum.Cgt_I8:
                case OpCodeREnum.Cgt_Un_I8:
                case OpCodeREnum.Clt_I8:
                case OpCodeREnum.Clt_Un_I8:
                case OpCodeREnum.Ceq_R4:
                case OpCodeREnum.Cgt_R4:
                case OpCodeREnum.Cgt_Un_R4:
                case OpCodeREnum.Clt_R4:
                case OpCodeREnum.Clt_Un_R4:
                case OpCodeREnum.Ceq_R8:
                case OpCodeREnum.Cgt_R8:
                case OpCodeREnum.Cgt_Un_R8:
                case OpCodeREnum.Clt_R8:
                case OpCodeREnum.Clt_Un_R8:
#endif
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
                case OpCodeREnum.Stelem_Any:
                case OpCodeREnum.Stelem_Ref:
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_I:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelema:
                    param = string.Format("r{0},r{1},r{2}", Register1, Register2, Register3);
                    break;
                case OpCodeREnum.Ldc_I4_0:
                case OpCodeREnum.Ldc_I4_1:
                case OpCodeREnum.Ldc_I4_2:
                case OpCodeREnum.Ldc_I4_3:
                case OpCodeREnum.Ldc_I4_4:
                case OpCodeREnum.Ldc_I4_5:
                case OpCodeREnum.Ldc_I4_6:
                case OpCodeREnum.Ldc_I4_7:
                case OpCodeREnum.Ldc_I4_8:
                case OpCodeREnum.Ldc_I4_M1:
                case OpCodeREnum.Ldnull:
                case OpCodeREnum.Ret:
                case OpCodeREnum.Push:
                    param = string.Format("r{0}", Register1);
                    break;
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
                case OpCodeREnum.Switch:
                    param = string.Format("r{0}, {1}", Register1, Operand);
                    break;
                case OpCodeREnum.Ldftn:
                    if (domain == null)
                    {
                        param = string.Format("r{0}, {1}", Register1, Operand2);
                    }
                    else
                    {
                        IMethod m = domain.GetMethod(Operand2);
                        if (m is CLR.Method.CLRMethod)
                            param = m != null ? string.Format("r{0}, {1}::{2}", Register1, m.DeclearingType.FullName, m) : string.Format("r{0}, {1}", Register1, Operand2);
                        else
                            param = m != null ? string.Format("r{0}, {1}", Register1, m) : string.Format("r{0}, {1}", Register1, Operand2);
                    }
                    break;

                case OpCodeREnum.Ldvirtftn:
                    if (domain == null)
                    {
                        param = string.Format("r{0}, r{1} {2}", Register1, Register2, Operand2);
                    }
                    else
                    {
                        IMethod m = domain.GetMethod(Operand2);
                        if (m is CLR.Method.CLRMethod)
                            param = m != null ? string.Format("r{0}, r{1}, {2}::{3}", Register1, Register2, m.DeclearingType.FullName, m) : string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand2);
                        else
                            param = m != null ? string.Format("r{0}, r{1}, {2}", Register1, Register2, m) : string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand2);
                    }
                    break;
                case OpCodeREnum.Constrained:
                    {
                        if (domain == null)
                        {
                            param = Operand.ToString();
                        }
                        else
                        {
                            var m = domain.GetType(Operand);
                            param = m != null ? m.ToString() : Operand.ToString();
                        }
                    }
                    break;

                case OpCodeREnum.Call_Redirect:
                    {
                        string retR = Register1 >= 0 ? "r" + Register1 : "-";
                        if (Register2 >= 0)
                            retR += ", r" + Register2;
                        if (Register3 >= 0)
                            retR += ", r" + Register3;
                        if (Register4 >= 0)
                            retR += ", r" + Register4;

                        string flags = "";
                        if ((Operand4 & 0x2) == 0x2)
                            flags = "c";
                        else
                            flags = "m";
                        if ((Operand4 & 0x1) == 0x1)
                            flags += "t";
                        if ((Operand4 & 0x4) == 0x4)
                            flags += "r";
                        if (domain == null)
                        {
                            param = string.Format("{0}, {1}({2})", retR, Operand2, flags);
                        }
                        else
                        {
                            IMethod m = domain.GetMethod(Operand2);
                            if (m is CLR.Method.CLRMethod)
                                param = m != null ? string.Format("{0}, {1}::{2}({3})", retR, m.DeclearingType.FullName, m, flags) : string.Format("{0}, {1}({2})", retR, Operand2, flags);
                            else
                                param = m != null ? string.Format("{0}, {1}({2})", retR, m, flags) : string.Format("{0}, {1}({2})", retR, Operand2, flags);
                        }
                    }
                    break;
                case OpCodeREnum.Call:
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Callvirt_IL:
                case OpCodeREnum.Callvirt_CLR:
                case OpCodeREnum.Newobj:
                    {
                        string retR = Register1 >= 0 ? "r" + Register1 : "-";
                        if (Register2 >= 0)
                            retR += ", r" + Register2;
                        if (Register3 >= 0)
                            retR += ", r" + Register3;
                        if (Register4 >= 0)
                            retR += ", r" + Register4;

                        if (domain == null)
                        {
                            if (Code == OpCodeREnum.Callvirt_IL || Code == OpCodeREnum.Callvirt_CLR || Code == OpCodeREnum.Callvirt)
                                param = string.Format("{0}, {1}, vslot={2}, thisArg={3}", retR, Operand2, Operand4 & 0xffff, (int)((uint)Operand4 >> 16));
                            else
                                param = string.Format("{0}, {1}", retR, Operand2);
                        }
                        else
                        {
                            IMethod m = domain.GetMethod(Operand2);
                            if (m is CLR.Method.CLRMethod)
                                param = m != null ? string.Format("{0}, {1}::{2}", retR, m.DeclearingType.FullName, m) : string.Format("{0}, {1}", retR, Operand2);
                            else
                                param = m != null ? string.Format("{0}, {1}", retR, m) : string.Format("{0}, {1}", retR, Operand2);
                            if (Code == OpCodeREnum.Callvirt_IL || Code == OpCodeREnum.Callvirt_CLR || Code == OpCodeREnum.Callvirt)
                                param = string.Format("{0}, vslot={1}, thisArg={2}", param, Operand4 & 0xffff, (int)((uint)Operand4 >> 16));
                        }
                    }
                    break;
                case OpCodeREnum.Blt:
                case OpCodeREnum.Blt_S:
                case OpCodeREnum.Blt_Un:
                case OpCodeREnum.Blt_Un_S:
                case OpCodeREnum.Ble:
                case OpCodeREnum.Ble_S:
                case OpCodeREnum.Ble_Un:
                case OpCodeREnum.Ble_Un_S:
                case OpCodeREnum.Bgt:
                case OpCodeREnum.Bgt_S:
                case OpCodeREnum.Bgt_Un:
                case OpCodeREnum.Bgt_Un_S:
                case OpCodeREnum.Bge:
                case OpCodeREnum.Bge_S:
                case OpCodeREnum.Bge_Un:
                case OpCodeREnum.Bge_Un_S:
                case OpCodeREnum.Beq:
                case OpCodeREnum.Beq_S:
                case OpCodeREnum.Bne_Un:
                case OpCodeREnum.Bne_Un_S:
#if ENABLE_NEO_MODE
                case OpCodeREnum.Blt_I8:
                case OpCodeREnum.Blt_Un_I8:
                case OpCodeREnum.Ble_I8:
                case OpCodeREnum.Ble_Un_I8:
                case OpCodeREnum.Bgt_I8:
                case OpCodeREnum.Bgt_Un_I8:
                case OpCodeREnum.Bge_I8:
                case OpCodeREnum.Bge_Un_I8:
                case OpCodeREnum.Beq_I8:
                case OpCodeREnum.Bne_Un_I8:
                case OpCodeREnum.Blt_R4:
                case OpCodeREnum.Blt_Un_R4:
                case OpCodeREnum.Ble_R4:
                case OpCodeREnum.Ble_Un_R4:
                case OpCodeREnum.Bgt_R4:
                case OpCodeREnum.Bgt_Un_R4:
                case OpCodeREnum.Bge_R4:
                case OpCodeREnum.Bge_Un_R4:
                case OpCodeREnum.Beq_R4:
                case OpCodeREnum.Bne_Un_R4:
                case OpCodeREnum.Blt_R8:
                case OpCodeREnum.Blt_Un_R8:
                case OpCodeREnum.Ble_R8:
                case OpCodeREnum.Ble_Un_R8:
                case OpCodeREnum.Bgt_R8:
                case OpCodeREnum.Bgt_Un_R8:
                case OpCodeREnum.Bge_R8:
                case OpCodeREnum.Bge_Un_R8:
                case OpCodeREnum.Beq_R8:
                case OpCodeREnum.Bne_Un_R8:
#endif
                    param = string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand);
                    break;
                case OpCodeREnum.Br:
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Leave:
                case OpCodeREnum.Leave_S:
                    param = string.Format("{0}", Operand);
                    break;
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                    param = string.Format("r{0},{1}", Register1, Operand);
                    break;
                case OpCodeREnum.Ldc_I8:
                    param = string.Format("r{0},{1}", Register1, OperandLong);
                    break;
                case OpCodeREnum.Ldc_R4:
                    param = string.Format("r{0},{1}", Register1, OperandFloat);
                    break;
                case OpCodeREnum.Ldc_R8:
                    param = string.Format("r{0},{1}", Register1, OperandDouble);
                    break;
                case OpCodeREnum.Ldstr:
                    if (domain == null)
                        param = string.Format("r{0},0x{1:X}", Register1, OperandLong);
                    else
                        param = string.Format("r{0},\"{1}\"", Register1, domain.GetString(OperandLong));
                    break;
                case OpCodeREnum.Ldtoken:
                    if (domain == null)
                        param = string.Format("r{0},0x{1:X}", Register1, OperandLong);
                    else
                    {
                        switch (Operand)
                        {
                            case 0:
                                {
                                    var type = domain.GetType((int)(OperandLong >> 32));
                                    int fieldIdx = (int)OperandLong;
                                    param = string.Format("r{0},{1}.{2}", Register1, type.FullName, (type is CLR.TypeSystem.ILType) ? ((CLR.TypeSystem.ILType)type).TypeDefinition.Fields[fieldIdx].Name : ((CLR.TypeSystem.CLRType)type).Fields[fieldIdx].Name);                                    
                                }
                                break;
                            case 1:
                                {
                                    var type = domain.GetType((int)OperandLong);
                                    param = string.Format("r{0},\"{1}\"", Register1, type);
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    break;
                case OpCodeREnum.Initobj:
                    if (domain == null)
                        param = string.Format("r{0}, {1}", Register1, Operand);
                    else
                    {
                        var type = domain.GetType(Operand);
                        param = string.Format("r{0}, {1}", Register1, type);
                    }
                    break;
                case OpCodeREnum.Newarr:
                    if (domain == null)
                        param = string.Format("r{0}, r{1}", Register1, Register2);
                    else
                    {
                        var type = domain.GetType(Operand);
                        param = string.Format("r{0}, {2}, r{1}", Register1, Register2, type);
                    }
                    break;
            }
            return string.Format("{0}{1} {2}", Code.ToString().ToLower().Replace('_', '.'), nameSuffix, param);
        }
    }
}
