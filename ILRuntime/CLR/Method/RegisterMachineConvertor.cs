using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter.OpCodes;
namespace ILRuntime.CLR.Method
{
    class RegisterMachineConvertor
    {
        public static OpCodeR[] ConvertInstructions(OpCode[] input)
        {
            OpCodeR[] translated = new OpCodeR[input.Length];

            return null;
        }

        static OpCodeR Convert(OpCode input)
        {
            OpCodeR res = new OpCodeR();
            res.Code = input.Code;

            return res;
        }
    }
}
