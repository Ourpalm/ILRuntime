﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿#if ENABLE_NEO_MODE
using ILRuntime.Runtime.Intepreter.OpCodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    partial class Optimizer
    {
        public static void LowerNeoOffsets(ref CompiledFrame frame, Enviorment.AppDomain domain, bool[] localIsRef = null)
        {
            if (frame.TotalStructSize > ushort.MaxValue)
            {
                throw new NotSupportedException(string.Format("Neo frame primitive size {0} exceeds maximum byte offset {1}.", frame.TotalStructSize, ushort.MaxValue));
            }

            if (localIsRef == null)
                localIsRef = frame.LocalIsReference;
            var localInfos = frame.LocalInfos;
            var body = frame.NeoExecuteBody;
            List<NeoCallParamMap> callParams = new List<NeoCallParamMap>();
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
                        {
                            int srcReg = op.Register2;
                            int dstReg = op.Register1;
                            int srcSz  = (srcReg >= 0 && srcReg < localInfos.Length) ? localInfos[srcReg].Size : 0;
                            int srcRef = (srcReg >= 0 && srcReg < localInfos.Length) ? localInfos[srcReg].RefCount : 0;
#if DEBUG
                            int dstSz  = (dstReg >= 0 && dstReg < localInfos.Length) ? localInfos[dstReg].Size : 0;
                            int dstRef = (dstReg >= 0 && dstReg < localInfos.Length) ? localInfos[dstReg].RefCount : 0;
                            if (srcSz != dstSz || srcRef != dstRef)
                                throw new System.Exception($"Move layout mismatch: src(sz={srcSz},ref={srcRef}) dst(sz={dstSz},ref={dstRef})");
#endif
                            bool isStandaloneRef = (srcRef == 1 && srcSz == 4 && (localIsRef == null || (srcReg >= 0 && srcReg < localIsRef.Length && localIsRef[srcReg])));
                            int sz = srcSz;
                            LowerR1R2(ref op, localInfos);
                            op.Operand = srcRef;
                            op.Operand2 = sz;
                            op.Operand3 = (dstReg >= 0 && dstReg < localInfos.Length) ? localInfos[dstReg].RefOffset : 0;
                            op.Operand4 = isStandaloneRef ? -1 : ((srcReg >= 0 && srcReg < localInfos.Length) ? localInfos[srcReg].RefOffset : 0);
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
                    case OpCodeREnum.Ldnull:
                    case OpCodeREnum.Ldstr:
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
                        if (op.Code == OpCodeREnum.Ldstr)
                            op.Operand = localInfos[op.Register1].RefOffset;
                        LowerR1(ref op, localInfos);
                        break;
                    case OpCodeREnum.Initobj:
                        {
                            short r1 = op.Register1;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.Operand3 = localInfos[r1].RefOffset;
                            // Initobj 三态编码同 Ldfld/Stfld（design §2.5 / §15，Step 12b）：
                            //   Operand4 == 0  → boxed（Step 13）
                            //   Operand4 >  0  → same-frame inline direct，值 = struct.RefOffset + 1
                            //   Operand4 <  0  → Ref-Slot receiver，值 = -1 - receiverStructRefOffset
                            if (localInfos[r1].IsRef)
                                op.Operand4 = -1 - localInfos[r1].RefOffset;
                            else
                                op.Operand4 = localInfos[r1].RefOffset + 1;
                        }
                        break;
                    case OpCodeREnum.Ret:
                        if (op.Register1 >= 0)
                        {
                            short r1 = op.Register1;
                            LowerR1(ref op, localInfos);
                            if (r1 < localInfos.Length)
                                op.Operand3 = localInfos[r1].RefOffset;
                        }
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
                            op.Operand3 = ref1;
                            op.Operand4 = ref2;
                        }
                        break;
                    case OpCodeREnum.Ldloca:
                    case OpCodeREnum.Ldloca_S:
                    case OpCodeREnum.Ldarga:
                    case OpCodeREnum.Ldarga_S:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                        }
                        break;
                    case OpCodeREnum.Ldsfld:
                        {
                            short r1 = op.Register1;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.Register2 = (short)localInfos[r1].RefOffset;
                        }
                        break;
                    case OpCodeREnum.Stsfld:
                        {
                            short r1 = op.Register1;
                            op.SrcOffset = (ushort)localInfos[r1].Offset;
                            op.Register2 = (short)localInfos[r1].RefOffset;
                        }
                        break;
                    case OpCodeREnum.Ldflda:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                            // Ldflda-specific receiver flag: 0 = heap object index,
                            // 1 = source slot contains an 8-byte Ref Slot.
                            op.Operand4 = localInfos[r2].IsRef ? 1 : 0;
                        }
                        break;
                    case OpCodeREnum.Ldsflda:
                        {
                            short r1 = op.Register1;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            // Keep the Ref Slot itself at Size=8/RefCount=0. The static
                            // instance needs one stable mStack anchor for the lifetime
                            // of this frame, allocated here and encoded directly.
                            op.Operand4 = frame.TotalRefSize++;
                            frame.LocalsReferenceCount++;
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
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                            if (op.Code == OpCodeREnum.Ldind_Ref)
                                op.Operand = localInfos[r1].RefOffset;
                        }
                        break;
                    case OpCodeREnum.Stind_I:
                    case OpCodeREnum.Stind_I1:
                    case OpCodeREnum.Stind_I2:
                    case OpCodeREnum.Stind_I4:
                    case OpCodeREnum.Stind_I8:
                    case OpCodeREnum.Stind_R4:
                    case OpCodeREnum.Stind_R8:
                    case OpCodeREnum.Stind_Ref:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                            if (op.Code == OpCodeREnum.Stind_Ref)
                                op.Operand = localInfos[r2].RefOffset;
                        }
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
                    case OpCodeREnum.Ldfld_Ref:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            if (op.Code == OpCodeREnum.Ldfld_Ref)
                                op.Operand = localInfos[r1].RefOffset;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                            // Ldfld/Stfld receiver 三态编码（design §2.5 / §15，Step 12b）：
                            //   Operand4 == 0  → heap mStack index
                            //   Operand4 >  0  → same-frame inline direct，值 = receiver struct 的 RefOffset + 1
                            //   Operand4 <  0  → Ref-Slot receiver，值 = -1 - receiverStructRefOffset
                            //                    receiverStructRefOffset 只在 FRAME_REF + same-method 场景有效
                            if (localInfos[r2].IsRef)
                                op.Operand4 = -1 - localInfos[r2].RefOffset;
                            else if (op.Operand4 == 1)
                                op.Operand4 = localInfos[r2].RefOffset + 1;
                            else
                                op.Operand4 = 0;
                        }
                        break;
                    case OpCodeREnum.Ldfld_Value:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                            int fpo = op.Operand2 & 0xFFFF;
                            op.Operand2 = (localInfos[r1].RefOffset << 16) | fpo;
                            if (localInfos[r2].IsRef)
                                op.Operand4 = -1 - localInfos[r2].RefOffset;
                            else if (op.Operand4 == 1)
                                op.Operand4 = localInfos[r2].RefOffset + 1;
                            else
                                op.Operand4 = 0;
                        }
                        break;
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
                    case OpCodeREnum.Stfld_Ref:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                            if (localInfos[r1].IsRef)
                                op.Operand4 = -1 - localInfos[r1].RefOffset;
                            else if (op.Operand4 == 1)
                                op.Operand4 = localInfos[r1].RefOffset + 1;
                            else
                                op.Operand4 = 0;
                        }
                        break;
                    case OpCodeREnum.Stfld_Value:
                        {
                            short r1 = op.Register1;
                            short r2 = op.Register2;
                            op.DstOffset = (ushort)localInfos[r1].Offset;
                            op.SrcOffset = (ushort)localInfos[r2].Offset;
                            int fpo = op.Operand2 & 0xFFFF;
                            op.Operand2 = (localInfos[r2].RefOffset << 16) | fpo;
                            if (localInfos[r1].IsRef)
                                op.Operand4 = -1 - localInfos[r1].RefOffset;
                            else if (op.Operand4 == 1)
                                op.Operand4 = localInfos[r1].RefOffset + 1;
                            else
                                op.Operand4 = 0;
                        }
                        break;
                    case OpCodeREnum.Br:
                    case OpCodeREnum.Br_S:
                    case OpCodeREnum.Nop:
                        break;
                    case OpCodeREnum.Call:
                    case OpCodeREnum.Callvirt:
                    case OpCodeREnum.Callvirt_IL:
                    case OpCodeREnum.Callvirt_CLR:
                    case OpCodeREnum.Newobj:
                        {
                            var targetMethod = domain.GetMethod(op.Operand2);
                            if (targetMethod == null)
                                break;
                            
                            int pCnt = targetMethod.ParameterCount;
                            if (targetMethod.HasThis && op.Code != OpCodeREnum.Newobj) pCnt++;
                            
                            // 接口 callvirt 已在 InitializeCallvirtDispatch 里把接口内 slot 编入 Operand4 低 16 位。
                            // 若同时被 constrained 判定命中会误吞 slot=1 的接口方法，因此这里显式排除接口目标。
                            bool targetIsInterface = targetMethod.DeclearingType != null && targetMethod.DeclearingType.IsInterface;
                            bool hasConstrained = op.Code != OpCodeREnum.Callvirt_IL &&
                                op.Code != OpCodeREnum.Callvirt_CLR &&
                                !(op.Code == OpCodeREnum.Callvirt && targetIsInterface) &&
                                op.Operand4 == 1;
                            int pushCnt = hasConstrained ? pCnt : Math.Max(pCnt - 3, 0);
                            int regCnt = pCnt - pushCnt;
                            
                            short[] srcRegs = new short[pCnt];
                            if (regCnt > 0) srcRegs[pCnt - regCnt] = op.Register2;
                            if (regCnt > 1) srcRegs[pCnt - regCnt + 1] = op.Register3;
                            if (regCnt > 2) srcRegs[pCnt - regCnt + 2] = op.Register4;
                            
                            int foundPushes = 0;
                            int scanIdx = i - 1;
                            while (scanIdx >= 0 && foundPushes < pushCnt)
                            {
                                if (body[scanIdx].Code == OpCodeREnum.Push)
                                {
                                    srcRegs[pushCnt - 1 - foundPushes] = body[scanIdx].Register1;
                                    // 彻底删除该指令，避免解释器 Nop 带来的 Dispatch 开销
                                    for (int j = scanIdx; j < body.Length - 1; j++)
                                    {
                                        body[j] = body[j + 1];
                                    }
                                    Array.Resize(ref body, body.Length - 1);
                                    FixBranchTargetsAfterRemove(body, scanIdx, frame.SwitchTargets, frame.Symbols);
                                    // 因为当前指令(Call)的位置前移了，我们需要更新外层循环的 i 和当前 op
                                    i--;
                                    op = body[i];
                                    foundPushes++;
                                    // 不减少 scanIdx，因为后面的指令已经补上来了，当前 scanIdx 就是前一条指令
                                    continue;
                                }
                                scanIdx--;
                            }
                            
                            if (foundPushes != pushCnt)
                                throw new Exception("Neo lowering could not find expected Push instructions for Call/Newobj.");
                            
                             StackSlotInfo[] paramInfos = null;
                            if (targetMethod is ILRuntime.CLR.Method.ILMethod ilm)
                            {
                                // 接口方法无 body，CompiledFrame.ParamInfos 为 null。
                                // 按 CLRMethod 分支思路手工分配 param slots；运行时会解析成实现方法（同签名），layout 兼容。
                                if (ilm.Definition == null || !ilm.Definition.HasBody)
                                {
                                    paramInfos = BuildParamInfosForBodylessILMethod(ilm, pCnt, op.Code, domain);
                                }
                                else
                                {
                                    paramInfos = ilm.CompiledFrame.ParamInfos;
                                }
                            }
                            else if (targetMethod is ILRuntime.CLR.Method.CLRMethod clrMethod)
                            {
                                // Generate contiguous paramInfos for CLRMethod
                                int totalParams = pCnt + (op.Code == OpCodeREnum.Newobj ? 1 : 0);
                                paramInfos = new StackSlotInfo[totalParams];
                                int curPrim = 0, curRef = 0;
                                if (op.Code == OpCodeREnum.Newobj)
                                {
                                    paramInfos[0] = new StackSlotInfo { Offset = curPrim, Size = 4, RefOffset = curRef, RefCount = 1 };
                                    curPrim += 4;
                                    curRef += 1;
                                }
                                for (int p = 0; p < pCnt; p++)
                                {
                                    int dstIndex = (op.Code == OpCodeREnum.Newobj) ? p + 1 : p;
                                    CLR.TypeSystem.IType paramType;
                                    if (targetMethod.HasThis && op.Code != OpCodeREnum.Newobj && p == 0)
                                        paramType = targetMethod.DeclearingType;
                                    else
                                        paramType = clrMethod.Parameters[p - ((targetMethod.HasThis && op.Code != OpCodeREnum.Newobj) ? 1 : 0)];

                                    if (paramType.IsValueType && !paramType.IsPrimitive && !(paramType is CLR.TypeSystem.ILType) && !(paramType.TypeForCLR != null && paramType.TypeForCLR.IsEnum))
                                    {
                                        // TODO Step 13: replace this CLR struct fallback with a real CLR value-type ABI.
                                        // For now we keep the caller temp slot shape so unsupported CLR structs (for example TaskAwaiter)
                                        // do not fail during JIT prewarm. Reference/primitive/IL value-type parameters use exact callee layout below.
                                        var srcInfo = localInfos[srcRegs[p]];
                                        paramInfos[dstIndex] = new StackSlotInfo { Offset = curPrim, Size = srcInfo.Size, RefOffset = curRef, RefCount = srcInfo.RefCount };
                                        curPrim += srcInfo.Size;
                                        curRef += srcInfo.RefCount;
                                    }
                                    else
                                    {
                                        paramInfos[dstIndex] = AllocateNeoCallParamSlot(paramType, ref curPrim, ref curRef, domain);
                                    }
                                }
                            }

                            if (paramInfos != null)
                            {
                                List<ushort> primSrc = new List<ushort>();
                                List<ushort> primDst = new List<ushort>();
                                List<ushort> primSize = new List<ushort>();
                                List<ushort> refSrc = new List<ushort>();
                                List<ushort> refDst = new List<ushort>();
                                
                                for (int p = 0; p < pCnt; p++)
                                {
                                    var srcInfo = localInfos[srcRegs[p]];
                                    int dstIndex = (op.Code == OpCodeREnum.Newobj) ? p + 1 : p;
                                    var dstInfo = paramInfos[dstIndex];

                                    if (srcInfo.IsRef != dstInfo.IsRef)
                                        throw new InvalidProgramException(
                                            "Neo call argument Ref Slot layout does not match the callee parameter ABI.");
                                    if (dstInfo.IsRef && (srcInfo.Size != 8 || dstInfo.Size != 8 ||
                                        srcInfo.RefCount != 0 || dstInfo.RefCount != 0))
                                        throw new InvalidProgramException(
                                            "Neo managed-pointer arguments must use an 8-byte Ref Slot.");
                                    
                                    if (dstInfo.Size > 0)
                                    {
                                        primSrc.Add((ushort)srcInfo.Offset);
                                        primDst.Add((ushort)dstInfo.Offset);
                                        primSize.Add((ushort)dstInfo.Size);
                                    }
                                    for (int r = 0; r < dstInfo.RefCount; r++)
                                    {
                                        refSrc.Add((ushort)(srcInfo.RefOffset + r));
                                        refDst.Add((ushort)(dstInfo.RefOffset + r));
                                    }
                                }
                                
                                NeoCallParamMap map = new NeoCallParamMap();
                                if (primSrc.Count > 0)
                                {
                                    map.PrimitiveSrc = primSrc.ToArray();
                                    map.PrimitiveDst = primDst.ToArray();
                                    map.PrimitiveSize = primSize.ToArray();
                                }
                                if (refSrc.Count > 0)
                                {
                                    map.RefSrc = refSrc.ToArray();
                                    map.RefDst = refDst.ToArray();
                                }
                                
                                op.Operand = callParams.Count;
                                callParams.Add(map);
                            }
                            
                            if (op.Code == OpCodeREnum.Newobj)
                            {
                                short r1 = op.Register1; // Destination
                                int off1 = localInfos[r1].Offset;
                                int ref1 = localInfos[r1].RefOffset;
                                op.DstOffset = (ushort)off1;
                                op.Operand3 = ref1;
                            }
                            else
                            {
                                if (op.Register1 >= 0)
                                {
                                    short r1 = op.Register1;
                                    op.Operand3 = localInfos[r1].RefOffset;
                                    LowerR1(ref op, localInfos);
                                }
                            }
                        }
                        break;
                    default:
                        handled = false;
                        break;
                }
                WarnUnhandledNeoLoweringOpcode(op.Code, handled);
                body[i] = op;
            }
            frame.NeoExecuteBody = body;
            if (callParams.Count > 0)
            {
                frame.NeoCallParams = callParams.ToArray();
            }
        }

        static void LowerR1(ref OpCodeR op, StackSlotInfo[] localInfos)
        {
            short r1 = op.Register1;
            int off1 = localInfos[r1].Offset;
            op.DstOffset = (ushort)off1;
        }

        static StackSlotInfo[] BuildParamInfosForBodylessILMethod(CLR.Method.ILMethod ilm, int pCnt, OpCodeREnum opCode, Enviorment.AppDomain domain)
        {
            // 用于接口方法（无 body）等场景：按签名手工分配 param slot 布局。
            // 运行时会解析成实现方法（同签名），layout 兼容；实现方法的 CompiledFrame.ParamInfos 应产出相同 offset/size。
            bool isNewobj = opCode == OpCodeREnum.Newobj;
            int totalParams = pCnt + (isNewobj ? 1 : 0);
            var infos = new StackSlotInfo[totalParams];
            int curPrim = 0, curRef = 0;
            if (isNewobj)
            {
                infos[0] = new StackSlotInfo { Offset = curPrim, Size = 4, RefOffset = curRef, RefCount = 1 };
                curPrim += 4;
                curRef += 1;
            }
            for (int p = 0; p < pCnt; p++)
            {
                int dstIndex = isNewobj ? p + 1 : p;
                CLR.TypeSystem.IType paramType;
                if (ilm.HasThis && !isNewobj && p == 0)
                    paramType = ilm.DeclearingType.IsValueType
                        ? ilm.DeclearingType.MakeByRefType()
                        : ilm.DeclearingType;
                else
                    paramType = ilm.Parameters[p - ((ilm.HasThis && !isNewobj) ? 1 : 0)];

                infos[dstIndex] = AllocateNeoCallParamSlot(paramType, ref curPrim, ref curRef, domain);
            }
            return infos;
        }

        static StackSlotInfo AllocateNeoCallParamSlot(CLR.TypeSystem.IType type, ref int offset, ref int refOffset, Enviorment.AppDomain domain)
        {
            StackSlotInfo slot = default;
            slot.Offset = offset;
            slot.RefOffset = refOffset;

            if (type.IsByRef)
            {
                slot.Size = 8;
                slot.RefCount = 0;
                slot.IsRef = true;
                offset += 8;
            }
            else if (type.IsPrimitive || (type.TypeForCLR != null && type.TypeForCLR.IsEnum))
            {
                slot.Size = domain.GetPrimitiveSize(type);
                offset += slot.Size;
            }
            else if (type is CLR.TypeSystem.ILType il && type.IsValueType)
            {
                slot.Size = il.TotalPrimitiveSize;
                slot.RefCount = il.TotalReferenceCount;
                offset += slot.Size;
                refOffset += slot.RefCount;
            }
            else if (type.IsValueType)
            {
                slot.Size = domain.GetPrimitiveSize(type);
                offset += slot.Size;
            }
            else
            {
                slot.Size = 4;
                slot.RefCount = 1;
                offset += 4;
                refOffset++;
            }

            return slot;
        }

        static void FixBranchTargetsAfterRemove(OpCodeR[] body, int removedIndex, Dictionary<int, int[]> jumpTables, Dictionary<int, RegisterVMSymbol> symbols)
        {
            for (int i = 0; i < body.Length; i++)
            {
                var op = body[i];
                if (IsBranching(op.Code))
                {
                    if (op.Operand > removedIndex)
                    {
                        op.Operand--;
                        body[i] = op;
                    }
                }
                else if (IsIntermediateBranching(op.Code))
                {
                    if (op.Operand4 > removedIndex)
                    {
                        op.Operand4--;
                        body[i] = op;
                    }
                }
                else if (op.Code == OpCodeREnum.Switch && jumpTables != null && jumpTables.TryGetValue(op.Operand, out var targets))
                {
                    for (int j = 0; j < targets.Length; j++)
                    {
                        if (targets[j] > removedIndex)
                            targets[j]--;
                    }
                }
            }

            if (symbols != null && symbols.Count > 0)
            {
                var oldSymbols = new List<KeyValuePair<int, RegisterVMSymbol>>(symbols);
                symbols.Clear();
                foreach (var item in oldSymbols)
                {
                    if (item.Key < removedIndex)
                    {
                        symbols[item.Key] = item.Value;
                    }
                    else if (item.Key > removedIndex)
                    {
                        symbols[item.Key - 1] = item.Value;
                    }
                }
            }
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
            // Do not throw during incremental steps because Prewarm compiles the whole assembly
        }
    }
}
#endif
