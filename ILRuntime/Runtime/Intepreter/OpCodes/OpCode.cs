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
        [FieldOffset(12)]
        public short Register5;
        [FieldOffset(14)]
        public short Register6;
        [FieldOffset(16)]
        public int Operand3;
        [FieldOffset(12)]
        public long OperandLong;
        [FieldOffset(12)]
        public double OperandDouble;
    }
}
