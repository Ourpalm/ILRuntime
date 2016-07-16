using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Intepreter.OpCodes
{
    struct OpCode
    {
        public OpCodeEnum Code;
        public int TokenInteger;
        public long TokenLong;
        public float TokenFloat;
        public double TokenDouble;
    }
}
