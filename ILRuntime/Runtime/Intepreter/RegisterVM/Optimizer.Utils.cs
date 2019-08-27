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
    partial class Optimizer
    {
        public static bool IsBranching(OpCodeREnum op)
        {
            switch (op)
            {
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
                    return true;
            }
            return false;
        }
        static bool GetOpcodeSourceRegister(ref OpCodeR op, bool hasReturn, out short r1, out short r2, out short r3)
        {
            r1 = -1;
            r2 = -1;
            r3 = -1;
            switch (op.Code)
            {
                case OpCodeREnum.Move:
                case OpCodeREnum.MoveRef:
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
                case OpCodeREnum.Ldobj:
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
                case OpCodeREnum.Ldind_I:
                case OpCodeREnum.Ldind_I1:
                case OpCodeREnum.Ldind_I2:
                case OpCodeREnum.Ldind_I4:
                case OpCodeREnum.Ldind_I8:
                case OpCodeREnum.Ldind_R4:
                case OpCodeREnum.Ldind_R8:
                case OpCodeREnum.Ldind_Ref:
                case OpCodeREnum.Ldind_U1:
                case OpCodeREnum.Ldind_U2:
                case OpCodeREnum.Ldind_U4:
                case OpCodeREnum.Not:
                case OpCodeREnum.Neg:
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Starg:
                case OpCodeREnum.Starg_S:
                case OpCodeREnum.Ldlen:
                case OpCodeREnum.Newarr:
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldflda:
                case OpCodeREnum.Ldvirtftn:
                case OpCodeREnum.Isinst:
                    r2 = op.Register2;
                    return true;
                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Stind_Ref:
                case OpCodeREnum.Stfld:
                case OpCodeREnum.Stobj:
                    r1 = op.Register1;
                    r2 = op.Register2;
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
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Call:
                case OpCodeREnum.Newobj:
                case OpCodeREnum.Ldtoken:
                case OpCodeREnum.Ldftn:
                case OpCodeREnum.Ldsfld:
                case OpCodeREnum.Ldsflda:
                    return false;
                case OpCodeREnum.Br_S:
                case OpCodeREnum.Br:
                case OpCodeREnum.Nop:
                case OpCodeREnum.InlineStart:
                case OpCodeREnum.InlineEnd:
                case OpCodeREnum.Castclass:
                    return false;
                case OpCodeREnum.Brtrue:
                case OpCodeREnum.Brtrue_S:
                case OpCodeREnum.Brfalse:
                case OpCodeREnum.Brfalse_S:
                case OpCodeREnum.Push:
                case OpCodeREnum.Throw:
                case OpCodeREnum.Stsfld:
                    r1 = op.Register1;
                    return true;
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
                    r2 = op.Register2;
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
                    r2 = op.Register2;
                    r3 = op.Register3;
                    return true;
                case OpCodeREnum.Stelem_I:
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_I8:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
                case OpCodeREnum.Stelem_Ref:
                case OpCodeREnum.Stelem_Any:
                    r1 = op.Register1;
                    r2 = op.Register2;
                    r3 = op.Register3;
                    return true;
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_U1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_U2:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_U4:
                case OpCodeREnum.Ldelem_I8:
                case OpCodeREnum.Ldelem_I:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelema:
                    r2 = op.Register2;
                    r3 = op.Register3;
                    return true;
                case OpCodeREnum.Ret:
                    if (hasReturn)
                    {
                        r1 = op.Register1;
                        return true;
                    }
                    else
                        return false;
                case OpCodeREnum.Initobj:
                    r1 = op.Register1;
                    r2 = op.Register2;
                    return true;
                case OpCodeREnum.Constrained:
                    return false;
                case OpCodeREnum.Switch:
                    r1 = op.Register1;
                    return true;
                default:
                    throw new NotImplementedException(op.Code.ToString());
            }
        }

        static bool GetOpcodeDestRegister(ref OpCodes.OpCodeR op, out short r1)
        {
            bool reference;
            return GetOpcodeDestRegister(ref op, out r1, out reference);
        }
        static bool GetOpcodeDestRegister(ref OpCodes.OpCodeR op, out short r1, out bool reference)
        {
            r1 = -1;
            switch (op.Code)
            {
                case OpCodeREnum.Move:
                case OpCodeREnum.MoveRef:
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
                case OpCodeREnum.Ldobj:
                case OpCodeREnum.Box:
                case OpCodeREnum.Unbox:
                case OpCodeREnum.Unbox_Any:
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
                case OpCodeREnum.Newobj:
                case OpCodeREnum.Ldind_I:
                case OpCodeREnum.Ldind_I1:
                case OpCodeREnum.Ldind_I2:
                case OpCodeREnum.Ldind_I4:
                case OpCodeREnum.Ldind_I8:
                case OpCodeREnum.Ldind_R4:
                case OpCodeREnum.Ldind_R8:
                case OpCodeREnum.Ldind_Ref:
                case OpCodeREnum.Ldind_U1:
                case OpCodeREnum.Ldind_U2:
                case OpCodeREnum.Ldind_U4:
                case OpCodeREnum.Not:
                case OpCodeREnum.Neg:
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                case OpCodeREnum.Ldarga:
                case OpCodeREnum.Ldarga_S:
                case OpCodeREnum.Starg:
                case OpCodeREnum.Starg_S:
                case OpCodeREnum.Ldlen:
                case OpCodeREnum.Newarr:
                case OpCodeREnum.Ldfld:
                case OpCodeREnum.Ldflda:
                case OpCodeREnum.Ldtoken:
                case OpCodeREnum.Isinst:
                case OpCodeREnum.Ldsfld:
                case OpCodeREnum.Ldsflda:
                case OpCodeREnum.Ldftn:
                case OpCodeREnum.Ldvirtftn:
                    r1 = op.Register1;
                    reference = false;
                    return true;
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Call:
                    r1 = op.Register1;
                    reference = false;
                    return op.Flag != 0;
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
                case OpCodeREnum.Push:
                case OpCodeREnum.InlineStart:
                case OpCodeREnum.InlineEnd:
                    reference = false;
                    return false;
                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Stind_Ref:
                case OpCodeREnum.Stelem_I:
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_I8:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
                case OpCodeREnum.Stelem_Ref:
                case OpCodeREnum.Stelem_Any:
                case OpCodeREnum.Stfld:
                case OpCodeREnum.Stsfld:
                case OpCodeREnum.Stobj:
                    r1 = op.Register1;
                    reference = true;
                    return false;
                case OpCodeREnum.Throw:
                case OpCodeREnum.Castclass:
                    reference = false;
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
                    r1 = op.Register1;
                    reference = false;
                    return true;
                case OpCodeREnum.Initobj:
                    r1 = op.Register1;
                    reference = true;
                    return false;
                case OpCodeREnum.Ldelem_I1:
                case OpCodeREnum.Ldelem_U1:
                case OpCodeREnum.Ldelem_I2:
                case OpCodeREnum.Ldelem_U2:
                case OpCodeREnum.Ldelem_I4:
                case OpCodeREnum.Ldelem_U4:
                case OpCodeREnum.Ldelem_I8:
                case OpCodeREnum.Ldelem_I:
                case OpCodeREnum.Ldelem_R4:
                case OpCodeREnum.Ldelem_R8:
                case OpCodeREnum.Ldelem_Ref:
                case OpCodeREnum.Ldelem_Any:
                case OpCodeREnum.Ldelema:
                    r1 = op.Register1;
                    reference = false;
                    return true;
                case OpCodeREnum.Constrained:
                    reference = false;
                    return false;
                case OpCodeREnum.Switch:
                    reference = false;
                    return false;
                default:
                    throw new NotImplementedException(op.Code.ToString());
            }
        }

        static void ReplaceOpcodeSource(ref OpCodes.OpCodeR op, int idx, short src)
        {
            if(src == -1)
            {
                var iiii = 0;
            }
            switch (idx)
            {
                case 0:
                    op.Register1 = src;
                    break;
                case 1:
                    op.Register2 = src;
                    break;
                case 2:
                    op.Register3 = src;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        static void ReplaceOpcodeDest(ref OpCodes.OpCodeR op, short dst)
        {
            op.Register1 = dst;
            if(dst == -1)
            {
                var iiiii = 0;
            }
        }
    }
}
