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
        public static void ForwardCopyPropagation(List<CodeBasicBlock> blocks, bool hasReturn)
        {
            foreach (var b in blocks)
            {
                var lst = b.FinalInstructions;
                HashSet<int> canRemove = b.CanRemove;
                HashSet<short> pendingRegister = b.PendingRegister;

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
                        for (int j = i + 1; j < lst.Count; j++)
                        {
                            OpCodeR Y = lst[j];
                            short ySrc, ySrc2, ySrc3;
                            if (GetOpcodeSourceRegister(ref Y, hasReturn, out ySrc, out ySrc2, out ySrc3))
                            {
                                bool replaced = false;
                                if (ySrc > 0 && ySrc == xDst)
                                {
                                    ReplaceOpcodeSource(ref Y, 0, xSrc);
                                    replaced = true;
                                }
                                if (ySrc2 > 0 && ySrc2 == xDst)
                                {
                                    ReplaceOpcodeSource(ref Y, 1, xSrc);
                                    replaced = true;
                                }
                                if (ySrc3 > 0 && ySrc3 == xDst)
                                {
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
                                    pendingRegister.Add(yDst);
                                    break;
                                }
                                if (xDst == yDst)
                                {
                                    canRemove.Add(i);
                                    break;
                                }
                            }
                        }
                    }
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
