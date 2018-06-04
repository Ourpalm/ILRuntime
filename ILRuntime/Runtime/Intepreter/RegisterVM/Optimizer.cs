using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    class Optimizer
    {
        public static void ForwardCopyPropagation(List<CodeBasicBlock> blocks, bool hasReturn,short stackRegisterBegin)
        {
            foreach (var b in blocks)
            {
                var lst = b.FinalInstructions;
                HashSet<int> canRemove = b.CanRemove;
                HashSet<int> pendingFCP = b.PendingFCP;

                for (int i = 0; i < lst.Count; i++)
                {
                    OpCodeR X = lst[i];
                    if (X.Code == OpCodeREnum.Nop)
                    {
                        canRemove.Add(i);
                        continue;
                    }
                    if (X.Code == OpCodeREnum.Move)
                    {
                        short xSrc, xSrc2, xSrc3, xDst;
                        GetOpcodeSourceRegister(ref X, hasReturn, out xSrc, out xSrc2, out xSrc3);
                        GetOpcodeDestRegister(ref X, out xDst);
                        if (xDst == xSrc)
                        {
                            canRemove.Add(i);
                            continue;
                        }
                        //Only deal with local->stack, local->local, stack->stack
                        if (xSrc >= stackRegisterBegin && xDst < stackRegisterBegin)
                            continue;
                        bool postPropagation = false;
                        bool ended = false;
                        for (int j = i + 1; j < lst.Count; j++)
                        {
                            OpCodeR Y = lst[j];
                            short ySrc, ySrc2, ySrc3;
                            if (GetOpcodeSourceRegister(ref Y, hasReturn, out ySrc, out ySrc2, out ySrc3))
                            {
                                bool replaced = false;
                                if (ySrc > 0 && ySrc == xDst)
                                {
                                    if (postPropagation)
                                    {
                                        postPropagation = false;
                                        ended = true;
                                        break;
                                    }
                                    ReplaceOpcodeSource(ref Y, 0, xSrc);
                                    replaced = true;
                                }
                                if (ySrc2 > 0 && ySrc2 == xDst)
                                {
                                    if (postPropagation)
                                    {
                                        postPropagation = false;
                                        ended = true;
                                        break;
                                    }
                                    ReplaceOpcodeSource(ref Y, 1, xSrc);
                                    replaced = true;
                                }
                                if (ySrc3 > 0 && ySrc3 == xDst)
                                {
                                    if (postPropagation)
                                    {
                                        postPropagation = false;
                                        ended = true;
                                        break;
                                    }
                                    ReplaceOpcodeSource(ref Y, 2, xSrc);
                                    replaced = true;
                                }

                                if (replaced)
                                    lst[j] = Y;
                            }
                            short yDst;
                            if (GetOpcodeDestRegister(ref Y, out yDst))
                            {
                                if (xSrc == yDst)
                                {
                                    postPropagation = true;
                                }
                                if (xDst == yDst)
                                {
                                    postPropagation = false;
                                    canRemove.Add(i);
                                    ended = true;
                                    break;
                                }
                            }

                            if(Y.Code == OpCodeREnum.Ret)
                            {
                                postPropagation = false;
                                canRemove.Add(i);
                                ended = true;
                                break;
                            }
                        }

                        if (postPropagation || !ended)
                        {
                            if (xDst >= stackRegisterBegin)
                                pendingFCP.Add(i);
                        }
                    }
                }
            }

            foreach(var b in blocks)
            {
                var pendingFCP = b.PendingFCP;

                if (pendingFCP.Count > 0)
                {
                    var originBlock = b;
                    HashSet<CodeBasicBlock> processedBlocks = new HashSet<CodeBasicBlock>();
                    Queue<CodeBasicBlock> pendingBlocks = new Queue<CodeBasicBlock>();

                    foreach (var idx in pendingFCP)
                    {
                        var X = originBlock.FinalInstructions[idx];
                        short xDst;
                        GetOpcodeDestRegister(ref X, out xDst);
                        pendingBlocks.Clear();
                        bool cannotRemove = false;
                        bool isAbort = false;
                        foreach (var nb in originBlock.NextBlocks)
                            pendingBlocks.Enqueue(nb);
                        while (pendingBlocks.Count > 0)
                        {
                            var cur = pendingBlocks.Dequeue();

                            var ins = cur.FinalInstructions;
                            var canRemove = cur.CanRemove;

                            for (int j = 0; j < ins.Count; j++)
                            {
                                if (canRemove.Contains(j))
                                    continue;
                                if(cur == originBlock && j == idx)
                                {
                                    isAbort = true;
                                    break;
                                }
                                var Y = ins[j];

                                short ySrc, ySrc2, ySrc3, yDst;
                                if (GetOpcodeSourceRegister(ref Y, hasReturn, out ySrc, out ySrc2, out ySrc3))
                                {
                                    if (ySrc == xDst || ySrc2 == xDst || ySrc3 == xDst)
                                    {
                                        cannotRemove = true;
                                        break;
                                    }
                                }
                                if(GetOpcodeDestRegister(ref Y, out yDst))
                                {
                                    if(yDst == xDst)
                                    {
                                        isAbort = true;
                                        break;
                                    }
                                }
                            }

                            if (cannotRemove)
                                break;

                            processedBlocks.Add(cur);
                            if (!isAbort)
                            {
                                foreach (var nb in cur.NextBlocks)
                                {
                                    if (!processedBlocks.Contains(nb))
                                        pendingBlocks.Enqueue(nb);
                                }
                            }
                        }
                        if(!cannotRemove)
                        {
                            originBlock.CanRemove.Add(idx);
                        }
                    }
                    pendingFCP.Clear();
                }
            }
        }

        static bool GetOpcodeSourceRegister(ref OpCodeR op, bool hasReturn, out short r1, out short r2, out short r3)
        {
            r1 = -1;
            r2 = -1;
            r3 = -1;
            switch (op.Code)
            {
                case OpCodeREnum.Move:
                    r1 = op.Register2;
                    return true;
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
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                case OpCodeREnum.Ldc_I8:
                case OpCodeREnum.Ldc_R4:
                case OpCodeREnum.Ldc_R8:
                case OpCodeREnum.Ldstr:
                    return false;
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Br:
                case OpCodeREnum.Nop:
                    return false;
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
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
                    r1 = op.Register1;
                    return true;
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
                case OpCodeREnum.Clt:
                case OpCodeREnum.Clt_Un:
                case OpCodeREnum.Cgt:
                case OpCodeREnum.Cgt_Un:
                case OpCodeREnum.Ceq:
                    r1 = op.Register2;
                    r2 = op.Register3;
                    return true;
                case OpCodeREnum.Ret:
                    if (hasReturn)
                    {
                        r1 = op.Register1;
                        return true;
                    }
                    else
                        return false;
                default:
                    throw new NotImplementedException();
            }
        }

        static bool GetOpcodeDestRegister(ref OpCodes.OpCodeR op, out short r1)
        {
            r1 = -1;
            switch (op.Code)
            {
                case OpCodes.OpCodeREnum.Move:
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
                case OpCodeREnum.Ldc_I4:
                case OpCodeREnum.Ldc_I4_S:
                case OpCodeREnum.Ldc_I8:
                case OpCodeREnum.Ldc_R4:
                case OpCodeREnum.Ldc_R8:
                case OpCodeREnum.Ldstr:
                    r1 = op.Register1;
                    return true;
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Br:
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
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
                case OpCodeREnum.Nop:
                case OpCodeREnum.Ret:
                    return false;
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
                case OpCodeREnum.Clt:
                case OpCodeREnum.Clt_Un:
                case OpCodeREnum.Cgt:
                case OpCodeREnum.Cgt_Un:
                case OpCodeREnum.Ceq:
                    r1 = op.Register1;
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        static void ReplaceOpcodeSource(ref OpCodes.OpCodeR op, int idx, short src)
        {
            switch (op.Code)
            {
                case OpCodes.OpCodeREnum.Move:
                    op.Register2 = src;
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
                case OpCodeREnum.Clt:
                case OpCodeREnum.Clt_Un:
                case OpCodeREnum.Cgt:
                case OpCodeREnum.Cgt_Un:
                case OpCodeREnum.Ceq:
                    switch (idx)
                    {
                        case 0:
                            op.Register2 = src;
                            break;
                        case 1:
                            op.Register3 = src;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
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
                    op.Register1 = src;
                    break;
                case OpCodeREnum.Ret:
                    op.Register1 = src;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
