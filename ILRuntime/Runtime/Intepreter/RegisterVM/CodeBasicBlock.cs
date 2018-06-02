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
    class CodeBasicBlock
    {
        List<Instruction> instructions = new List<Instruction>();
        List<OpCodeR> finalInstructions = new List<OpCodeR>();
        HashSet<int> canRemove = new HashSet<int>();
        HashSet<short> pendingRegister = new HashSet<short>();
        Instruction entry;
        public List<Instruction> Instructions { get { return instructions; } }

        public List<OpCodeR> FinalInstructions { get { return finalInstructions; } }

        public HashSet<int> CanRemove { get { return canRemove; } }

        public HashSet<short> PendingRegister { get { return pendingRegister; } }

        public void AddInstruction(Instruction op)
        {
            if (instructions.Count == 0)
                entry = op;
            instructions.Add(op);
        }

        public static List<CodeBasicBlock> BuildBasicBlocks(MethodBody body, out Dictionary<Instruction, CodeBasicBlock> entryMapping)
        {
            entryMapping = new Dictionary<Instruction, CodeBasicBlock>();
            HashSet<Instruction> branchTargets = new HashSet<Instruction>();
            foreach (var i in body.Instructions)
            {
                switch (i.OpCode.OperandType)
                {
                    case OperandType.InlineBrTarget:
                    case OperandType.ShortInlineBrTarget:
                        branchTargets.Add((Instruction)i.Operand);
                        break;
                    case OperandType.InlineSwitch:
                        {
                            var arr = i.Operand as Instruction[];
                            foreach (var j in arr)
                                branchTargets.Add(j);
                        }
                        break;
                }
            }

            List<CodeBasicBlock> res = new List<CodeBasicBlock>();
            CodeBasicBlock cur = new CodeBasicBlock();
            res.Add(cur);
            foreach (var i in body.Instructions)
            {
                if (branchTargets.Contains(i))
                {
                    if(cur.entry != null && cur.entry != i)
                    {
                        entryMapping[cur.entry] = cur;
                        cur = new CodeBasicBlock();
                        res.Add(cur);
                    }
                }
                cur.AddInstruction(i);
                if (i.OpCode.Code == Code.Switch || i.OpCode.OperandType == OperandType.InlineBrTarget || i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                {
                    if (cur.entry != null)
                    {
                        if (i.OpCode.OperandType == OperandType.InlineBrTarget || i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                        {
                            if (cur.entry != (Instruction)i.Operand)
                            {
                                entryMapping[cur.entry] = cur;
                                cur = new CodeBasicBlock();
                                res.Add(cur);
                            }
                        }
                        else
                        {
                            if (cur.entry != (Instruction)i.Operand)
                            {
                                entryMapping[cur.entry] = cur;
                                cur = new CodeBasicBlock();
                                res.Add(cur);
                            }
                        }
                    }
                }
            }
            entryMapping[cur.entry] = cur;

            return res;
        }
    }
}
