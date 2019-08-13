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
        public static void BackwardsCopyPropagation(List<CodeBasicBlock> blocks, bool hasReturn,short stackRegisterBegin)
        {
            foreach (var b in blocks)
            {
                var lst = b.FinalInstructions;
                HashSet<int> canRemove = b.CanRemove;
                //HashSet<int> pendingBCP = b.PendingCP;
                bool isInline = false;

                for (int i = lst.Count - 1; i >= 0; i--)
                {
                    if (canRemove.Contains(i))
                        continue;
                    OpCodeR X = lst[i];
                    if (X.Code == OpCodeREnum.InlineStart)
                    {
                        isInline = true;
                        continue;
                    }
                    if (X.Code == OpCodeREnum.InlineEnd)
                    {
                        isInline = false;
                        continue;
                    }
                    if (isInline)
                        continue;
                    if (X.Code == OpCodeREnum.Nop)
                    {
                        canRemove.Add(i);
                        continue;
                    }
                    if (X.Code == OpCodeREnum.Move)
                    {
                        short xSrc, xDst;
                        if (true)
                        {
                            short xSrc2, xSrc3;
                            GetOpcodeSourceRegister(ref X, hasReturn, out xSrc2, out xSrc, out xSrc3);
                            GetOpcodeDestRegister(ref X, out xDst);
                        }
                        if (xDst == xSrc)
                        {
                            canRemove.Add(i);
                            continue;
                        }
                        //Only deal with stack->local
                        if (xSrc < stackRegisterBegin || xDst >= stackRegisterBegin)
                            continue;
                        //bool ended = false;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (canRemove.Contains(j))
                                continue;
                            OpCodeR Y = lst[j];

                            short yDst;
                            bool reference;
                            if (GetOpcodeDestRegister(ref Y, out yDst,out reference))
                            {
                                if (xSrc == yDst)
                                {
                                    ReplaceOpcodeDest(ref Y, xDst);
                                    canRemove.Add(i);
                                    //ended = true;
                                    lst[j] = Y;
                                    break;
                                }
                                if (xDst == yDst)
                                {
                                    //ended = true;
                                    break;
                                }
                            }
                            short ySrc, ySrc2, ySrc3;
                            if (GetOpcodeSourceRegister(ref Y, hasReturn, out ySrc, out ySrc2, out ySrc3))
                            {
                                if (ySrc >= 0 && ySrc == xDst)
                                {
                                    //ended = true;
                                    break;
                                }
                                if (ySrc2 >= 0 && ySrc2 == xDst)
                                {
                                    //ended = true;
                                    break;
                                }
                                if (ySrc3 >= 0 && ySrc3 == xDst)
                                {
                                    //ended = true;
                                    break;
                                }
                            }
                            if (reference)
                            {
                                if (xSrc == yDst)
                                {
                                    ReplaceOpcodeDest(ref Y, xDst);
                                    canRemove.Add(i);
                                    //ended = true;
                                    lst[j] = Y;
                                }
                            }
                        }

                        /*if (!ended)
                        {
                            if (xDst < stackRegisterBegin)
                            {
                                pendingBCP.Add(i);
                                throw new NotImplementedException();
                            }
                        }*/
                    }
                }
            }
        }
    }
}
