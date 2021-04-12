using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter.OpCodes;

namespace ILRuntime.Runtime.Intepreter.RegisterVM
{
    partial class Optimizer
    {
        public const int MaximalInlineInstructionCount = 10;
        public static void InlineMethod(CodeBasicBlock block, ILMethod method,RegisterVMSymbolLink symbolLink, short baseRegIdx, bool hasReturn)
        {
            var ins = block.FinalInstructions;
            var body = method.BodyRegister;
            OpCodeR start = new OpCodeR();
            start.Code = OpCodeREnum.InlineStart;
            ins.Add(start);
            int branchStart = ins.Count;
            int branchOffset = 0;
            List<int> reloc = new List<int>();
            for(int i=0;i<body.Length;i++)
            {
                var opcode = body[i];
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

                if (opcode.Code == OpCodeREnum.Ret)
                {
                    if (hasReturn)
                    {
                        opcode.Code = OpCodeREnum.Move;
                        opcode.Register2 = opcode.Register1;
                        opcode.Register1 = baseRegIdx;
                        ins.Add(opcode);
                        branchOffset++;
                    }
                    if (i < body.Length - 1)
                    {
                        if (hasReturn)
                        {
                            for (int j = 0; j < ins.Count; j++)
                            {
                                var op2 = ins[j];
                                if (IsBranching(op2.Code))
                                {
                                    if (op2.Operand >= i)
                                    {
                                        op2.Operand++;
                                        ins[j] = op2;
                                    }
                                }
                            }
                        }
                        reloc.Add(ins.Count);
                        opcode.Code = OpCodeREnum.Br;
                        ins.Add(opcode);
                    }
                    continue;
                }

                if (IsBranching(opcode.Code))
                {
                    opcode.Operand += branchOffset;
                }
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                RegisterVMSymbol oriIns;
                if (method.RegisterVMSymbols.TryGetValue(i, out oriIns))
                {
                    oriIns.ParentSymbol = symbolLink;
                    block.InstructionMapping.Add(ins.Count, oriIns);
                }
#endif
                ins.Add(opcode);
            }

            foreach(var i in reloc)
            {
                var opcode = ins[i];
                opcode.Operand = ins.Count - branchStart;
                ins[i] = opcode;
            }
            start.Code = OpCodeREnum.InlineEnd;
            ins.Add(start);
        }
    }
}
