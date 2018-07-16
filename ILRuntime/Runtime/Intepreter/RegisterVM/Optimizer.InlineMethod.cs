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
        public const int MaximalInlineInstructionCount = 10;
        public static void InlineMethod(List<OpCodeR> ins, ILMethod method, short baseRegIdx, bool hasReturn)
        {
            var body = method.BodyRegister;
            OpCodeR start = new OpCodeR();
            start.Code = OpCodeREnum.InlineStart;
            ins.Add(start);
            foreach (var i in body)
            {
                var opcode = i;
                short r1 = 0;
                short r2 = 0;
                short r3 = 0;
                if (GetOpcodeSourceRegister(ref opcode, hasReturn, out r1, out r2, out r3))
                {
                    if (r1 >= 0)
                    {
                        ReplaceOpcodeSource(ref opcode, 0, (short)(r1 + baseRegIdx));
                    }
                    if (r2 >= 0)
                    {
                        ReplaceOpcodeSource(ref opcode, 1, (short)(r2 + baseRegIdx));
                    }
                    if (r3 >= 0)
                    {
                        ReplaceOpcodeSource(ref opcode, 2, (short)(r3 + baseRegIdx));
                    }
                }
                if (GetOpcodeDestRegister(ref opcode, out r1))
                {
                    ReplaceOpcodeDest(ref opcode, (short)(r1 + baseRegIdx));
                }

                if (opcode.Code == OpCodeREnum.Ret && hasReturn)
                {
                    opcode.Code = OpCodeREnum.Move;
                    opcode.Register2 = opcode.Register1;
                    opcode.Register1 = baseRegIdx;
                }

                ins.Add(opcode);
            }

            start.Code = OpCodeREnum.InlineEnd;
            ins.Add(start);
        }
    }
}
