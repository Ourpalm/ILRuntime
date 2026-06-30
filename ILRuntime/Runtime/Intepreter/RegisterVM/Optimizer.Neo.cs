using ILRuntime.Runtime.Intepreter.OpCodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    partial class Optimizer
    {
        public static void LowerNeoOffsets(ref CompiledFrame frame)
        {
            if (frame.TotalStructSize > ushort.MaxValue)
            {
                throw new NotSupportedException(string.Format("Neo frame primitive size {0} exceeds maximum byte offset {1}.", frame.TotalStructSize, ushort.MaxValue));
            }

            var localInfos = frame.LocalInfos;
            var body = frame.NeoExecuteBody;
            for (int i = 0; i < body.Length; i++)
            {
                OpCodeR op = body[i];
                bool handled = true;
                switch (op.Code)
                {
                    case OpCodeREnum.Add:
                    case OpCodeREnum.Sub:
                    case OpCodeREnum.Mul:
                    case OpCodeREnum.Div:
                    case OpCodeREnum.Div_Un:
                    case OpCodeREnum.Rem:
                    case OpCodeREnum.Rem_Un:
                    case OpCodeREnum.And:
                    case OpCodeREnum.Or:
                    case OpCodeREnum.Xor:
                    case OpCodeREnum.Shl:
                    case OpCodeREnum.Shr:
                    case OpCodeREnum.Shr_Un:
                    case OpCodeREnum.Ceq:
                    case OpCodeREnum.Cgt:
                    case OpCodeREnum.Cgt_Un:
                    case OpCodeREnum.Clt:
                    case OpCodeREnum.Clt_Un:
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
                        LowerR1R2R3(ref op, localInfos);
                        break;

                    case OpCodeREnum.Neg:
                    case OpCodeREnum.Not:
                    case OpCodeREnum.Neg_I8:
                    case OpCodeREnum.Not_I8:
                    case OpCodeREnum.Neg_R4:
                    case OpCodeREnum.Neg_R8:
                        LowerR1R2(ref op, localInfos);
                        break;
                    case OpCodeREnum.Move:
                        // Move emits Unsafe.CopyBlock at runtime, so it needs the
                        // copy size (in bytes) in Operand2. Use min(src,dst) so a
                        // wide stack register (8 bytes) copied into a narrow local
                        // (e.g. int = 4 bytes) doesn't clobber neighbouring slots.
                        {
                            int srcReg = op.Register2;
                            int dstReg = op.Register1;
                            int srcSz = (srcReg >= 0 && srcReg < localInfos.Length) ? localInfos[srcReg].Size : 0;
                            int dstSz = (dstReg >= 0 && dstReg < localInfos.Length) ? localInfos[dstReg].Size : 0;
                            int sz;
                            if (srcSz > 0 && dstSz > 0)
                                sz = srcSz < dstSz ? srcSz : dstSz;
                            else
                                sz = srcSz > 0 ? srcSz : dstSz;
                            LowerR1R2(ref op, localInfos);
                            op.Operand2 = sz;
                        }
                        break;
                    case OpCodeREnum.Conv_I:
                    case OpCodeREnum.Conv_I1:
                    case OpCodeREnum.Conv_I2:
                    case OpCodeREnum.Conv_I4:
                    case OpCodeREnum.Conv_I8:
                    case OpCodeREnum.Conv_R4:
                    case OpCodeREnum.Conv_R8:
                    case OpCodeREnum.Conv_R_Un:
                    case OpCodeREnum.Conv_U:
                    case OpCodeREnum.Conv_U1:
                    case OpCodeREnum.Conv_U2:
                    case OpCodeREnum.Conv_U4:
                    case OpCodeREnum.Conv_U8:
                    case OpCodeREnum.Addi:
                    case OpCodeREnum.Subi:
                    case OpCodeREnum.Muli:
                    case OpCodeREnum.Divi:
                    case OpCodeREnum.Divi_Un:
                    case OpCodeREnum.Remi:
                    case OpCodeREnum.Remi_Un:
                    case OpCodeREnum.Andi:
                    case OpCodeREnum.Ori:
                    case OpCodeREnum.Xori:
                    case OpCodeREnum.Shli:
                    case OpCodeREnum.Shri:
                    case OpCodeREnum.Shri_Un:
                    case OpCodeREnum.Ceqi:
                    case OpCodeREnum.Cgti:
                    case OpCodeREnum.Cgti_Un:
                    case OpCodeREnum.Clti:
                    case OpCodeREnum.Clti_Un:
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
                        LowerR1R2(ref op, localInfos);
                        break;

                    case OpCodeREnum.Beq:
                    case OpCodeREnum.Bne_Un:
                    case OpCodeREnum.Blt:
                    case OpCodeREnum.Blt_Un:
                    case OpCodeREnum.Bgt:
                    case OpCodeREnum.Bgt_Un:
                    case OpCodeREnum.Ble:
                    case OpCodeREnum.Ble_Un:
                    case OpCodeREnum.Bge:
                    case OpCodeREnum.Bge_Un:
                    case OpCodeREnum.Beq_I8:
                    case OpCodeREnum.Bne_Un_I8:
                    case OpCodeREnum.Blt_I8:
                    case OpCodeREnum.Blt_Un_I8:
                    case OpCodeREnum.Bgt_I8:
                    case OpCodeREnum.Bgt_Un_I8:
                    case OpCodeREnum.Ble_I8:
                    case OpCodeREnum.Ble_Un_I8:
                    case OpCodeREnum.Bge_I8:
                    case OpCodeREnum.Bge_Un_I8:
                    case OpCodeREnum.Beq_R4:
                    case OpCodeREnum.Bne_Un_R4:
                    case OpCodeREnum.Blt_R4:
                    case OpCodeREnum.Blt_Un_R4:
                    case OpCodeREnum.Bgt_R4:
                    case OpCodeREnum.Bgt_Un_R4:
                    case OpCodeREnum.Ble_R4:
                    case OpCodeREnum.Ble_Un_R4:
                    case OpCodeREnum.Bge_R4:
                    case OpCodeREnum.Bge_Un_R4:
                    case OpCodeREnum.Beq_R8:
                    case OpCodeREnum.Bne_Un_R8:
                    case OpCodeREnum.Blt_R8:
                    case OpCodeREnum.Blt_Un_R8:
                    case OpCodeREnum.Bgt_R8:
                    case OpCodeREnum.Bgt_Un_R8:
                    case OpCodeREnum.Ble_R8:
                    case OpCodeREnum.Ble_Un_R8:
                    case OpCodeREnum.Bge_R8:
                    case OpCodeREnum.Bge_Un_R8:
                        LowerR1R2(ref op, localInfos);
                        break;

                    case OpCodeREnum.Brtrue:
                    case OpCodeREnum.Brtrue_S:
                    case OpCodeREnum.Brfalse:
                    case OpCodeREnum.Brfalse_S:
                        {
                            short r1 = op.Register1;
                            int size = localInfos[r1].Size;
                            int off1 = localInfos[r1].Offset;
                            op.Operand2 = size;
                            op.DstOffset = (ushort)off1;
                        }
                        break;
                    case OpCodeREnum.Beqi:
                    case OpCodeREnum.Bnei_Un:
                    case OpCodeREnum.Blti:
                    case OpCodeREnum.Blti_Un:
                    case OpCodeREnum.Bgti:
                    case OpCodeREnum.Bgti_Un:
                    case OpCodeREnum.Blei:
                    case OpCodeREnum.Blei_Un:
                    case OpCodeREnum.Bgei:
                    case OpCodeREnum.Bgei_Un:
                    case OpCodeREnum.Beqi_I8:
                    case OpCodeREnum.Bnei_Un_I8:
                    case OpCodeREnum.Blti_I8:
                    case OpCodeREnum.Blti_Un_I8:
                    case OpCodeREnum.Bgti_I8:
                    case OpCodeREnum.Bgti_Un_I8:
                    case OpCodeREnum.Blei_I8:
                    case OpCodeREnum.Blei_Un_I8:
                    case OpCodeREnum.Bgei_I8:
                    case OpCodeREnum.Bgei_Un_I8:
                    case OpCodeREnum.Beqi_R4:
                    case OpCodeREnum.Bnei_Un_R4:
                    case OpCodeREnum.Blti_R4:
                    case OpCodeREnum.Blti_Un_R4:
                    case OpCodeREnum.Bgti_R4:
                    case OpCodeREnum.Bgti_Un_R4:
                    case OpCodeREnum.Blei_R4:
                    case OpCodeREnum.Blei_Un_R4:
                    case OpCodeREnum.Bgei_R4:
                    case OpCodeREnum.Bgei_Un_R4:
                    case OpCodeREnum.Beqi_R8:
                    case OpCodeREnum.Bnei_Un_R8:
                    case OpCodeREnum.Blti_R8:
                    case OpCodeREnum.Blti_Un_R8:
                    case OpCodeREnum.Bgti_R8:
                    case OpCodeREnum.Bgti_Un_R8:
                    case OpCodeREnum.Blei_R8:
                    case OpCodeREnum.Blei_Un_R8:
                    case OpCodeREnum.Bgei_R8:
                    case OpCodeREnum.Bgei_Un_R8:
                    case OpCodeREnum.Initobj:
                    case OpCodeREnum.Ldc_I4_M1:
                    case OpCodeREnum.Ldc_I4_0:
                    case OpCodeREnum.Ldc_I4_1:
                    case OpCodeREnum.Ldc_I4_2:
                    case OpCodeREnum.Ldc_I4_3:
                    case OpCodeREnum.Ldc_I4_4:
                    case OpCodeREnum.Ldc_I4_5:
                    case OpCodeREnum.Ldc_I4_6:
                    case OpCodeREnum.Ldc_I4_7:
                    case OpCodeREnum.Ldc_I4_8:
                    case OpCodeREnum.Ldc_I4:
                    case OpCodeREnum.Ldc_I4_S:
                    case OpCodeREnum.Ldc_I8:
                    case OpCodeREnum.Ldc_R4:
                    case OpCodeREnum.Ldc_R8:
                        LowerR1(ref op, localInfos);
                        break;
                    case OpCodeREnum.Ret:
                        if (op.Register1 >= 0)
                            LowerR1(ref op, localInfos);
                        break;
                    case OpCodeREnum.Box:
                    case OpCodeREnum.Unbox:
                    case OpCodeREnum.Unbox_Any:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            int off1 = localInfos[r1].Offset;
                            int off2 = localInfos[r2].Offset;
                            int ref1 = localInfos[r1].RefOffset;
                            int ref2 = localInfos[r2].RefOffset;
                            op.DstOffset = (ushort)off1;
                            op.SrcOffset = (ushort)off2;
                            op.Operand3 = (ref1 << 16) | (ref2 & 0xffff);
                        }
                        break;
                    case OpCodeREnum.Br:
                    case OpCodeREnum.Br_S:
                    case OpCodeREnum.Nop:
                        break;
                    default:
                        handled = false;
                        break;
                }
                WarnUnhandledNeoLoweringOpcode(op.Code, handled);
                body[i] = op;
            }
        }

        static void LowerR1(ref OpCodeR op, StackSlotInfo[] localInfos)
        {
            short r1 = op.Register1;
            int off1 = localInfos[r1].Offset;
            op.DstOffset = (ushort)off1;
        }

        static void LowerR1R2(ref OpCodeR op, StackSlotInfo[] localInfos)
        {
            short r1 = op.Register1;
            short r2 = op.Register2;
            int off1 = localInfos[r1].Offset;
            int off2 = localInfos[r2].Offset;
            op.DstOffset = (ushort)off1;
            op.SrcOffset = (ushort)off2;
        }

        static void LowerR1R2R3(ref OpCodeR op, StackSlotInfo[] localInfos)
        {
            short r1 = op.Register1;
            short r2 = op.Register2;
            short r3 = op.Register3;
            int off1 = localInfos[r1].Offset;
            int off2 = localInfos[r2].Offset;
            int off3 = localInfos[r3].Offset;
            op.DstOffset = (ushort)off1;
            op.SrcOffset = (ushort)off2;
            op.OperandOffset = (ushort)off3;
        }

        static void WarnUnhandledNeoLoweringOpcode(OpCodeREnum code, bool handled)
        {
            if (!handled)
                throw new NotSupportedException(string.Format("Neo lowering skipped opcode {0}.", code));
        }
    }
}
