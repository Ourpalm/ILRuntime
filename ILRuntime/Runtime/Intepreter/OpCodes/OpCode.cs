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
        [FieldOffset(6)]
        public short Register2;
        [FieldOffset(8)]
        public int Operand;
        [FieldOffset(8)]
        public float OperandFloat;
        [FieldOffset(8)]
        public short Register3;
        [FieldOffset(10)]
        public short Register4;
        [FieldOffset(12)]
        public int Operand2;
        [FieldOffset(16)]
        public int Operand3;
        [FieldOffset(12)]
        public long OperandLong;
        [FieldOffset(12)]
        public double OperandDouble;

        public override string ToString()
        {
            string param = null;
            switch (Code)
            {
                case OpCodeREnum.Move:
                case OpCodeREnum.Ldind_I:
                case OpCodeREnum.Ldind_I1:
                case OpCodeREnum.Ldind_I2:
                case OpCodeREnum.Ldind_I4:
                case OpCodeREnum.Ldind_R4:
                case OpCodeREnum.Ldind_R8:
                case OpCodeREnum.Ldind_U1:
                case OpCodeREnum.Ldind_U2:
                case OpCodeREnum.Ldind_U4:
                case OpCodeREnum.Stind_I:
                case OpCodeREnum.Stind_I1:
                case OpCodeREnum.Stind_I2:
                case OpCodeREnum.Stind_I4:
                case OpCodeREnum.Stind_I8:
                case OpCodeREnum.Stind_R4:
                case OpCodeREnum.Stind_R8:
                case OpCodeREnum.Ldloca:
                case OpCodeREnum.Ldloca_S:
                    param = string.Format("r{0},r{1}", Register1, Register2);
                    break;
                case OpCodeREnum.Box:
                    param = string.Format("r{0},r{1},{2}", Register1, Register2, Operand);
                    break;

                case OpCodeREnum.Stfld:
                case OpCodeREnum.Ldfld:
                    param = string.Format("r{0},r{1},0x{2:X8}", Register1, Register2, OperandLong);
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
                case OpCodeREnum.Stelem_I1:
                case OpCodeREnum.Stelem_I2:
                case OpCodeREnum.Stelem_I4:
                case OpCodeREnum.Stelem_R4:
                case OpCodeREnum.Stelem_R8:
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
                case OpCodeREnum.Call:
                case OpCodeREnum.Callvirt:
                case OpCodeREnum.Newobj:
                    param = string.Format("r{0}, {1}", Register1, Operand);
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
                    param = string.Format("r{0}, r{1}, {2}", Register1, Register2, Operand);
                    break;
                case OpCodeREnum.Br:
                case OpCodeREnum.Br_S:
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
                    param = string.Format("r{0},0x{1:X}", Register1, OperandLong);
                    break;
                case OpCodeREnum.Newarr:
                    param = string.Format("r{0}, r{1}", Register1, Register2);
                    break;
            }
            return string.Format("{0} {1}", Code.ToString().ToLower().Replace('_', '.'), param);
        }
    }
}
