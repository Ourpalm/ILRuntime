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
        public static void ForwardCopyPropagation(List<CodeBasicBlock> blocks, bool hasReturn,short stackRegisterBegin)
        {
            foreach (var b in blocks)
            {
                var lst = b.FinalInstructions;
                HashSet<int> canRemove = b.CanRemove;
                HashSet<int> pendingFCP = b.PendingCP;
                bool isInline = false;
                for (int i = 0; i < lst.Count; i++)
                {
                    if (canRemove.Contains(i))
                        continue;
                    OpCodeR X = lst[i];
                    if(X.Code == OpCodeREnum.InlineStart)
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
                    if (X.Code == OpCodeREnum.Nop || X.Code == OpCodeREnum.Castclass)
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
                        //Only deal with local->stack, local->local, stack->stack
                        if (xSrc >= stackRegisterBegin && xDst < stackRegisterBegin)
                            continue;
                        bool postPropagation = false;
                        bool ended = false;
                        //bool propagationInline = false;
                        for (int j = i + 1; j < lst.Count; j++)
                        {
                            OpCodeR Y = lst[j];
                            //if (Y.Code == OpCodeREnum.InlineStart)
                            //    propagationInline = true;
                            //else if (Y.Code == OpCodeREnum.InlineEnd)
                            //{
                            //    propagationInline = false;
                            //    if (ended)
                            //        break;
                            //}
                            short ySrc, ySrc2, ySrc3;
                            if (GetOpcodeSourceRegister(ref Y, hasReturn, out ySrc, out ySrc2, out ySrc3))
                            {
                                bool replaced = false;
                                if (ySrc >= 0 && ySrc == xDst)
                                {
                                    if (postPropagation)
                                    {
                                        postPropagation = false;
                                        ended = true;
                                        break;
                                    }
                                    ReplaceOpcodeSource(ref Y, 0, xSrc);
                                    replaced = true;
                                }
                                if (ySrc2 >= 0 && ySrc2 == xDst)
                                {
                                    if (postPropagation)
                                    {
                                        postPropagation = false;
                                        ended = true;
                                        break;
                                    }
                                    ReplaceOpcodeSource(ref Y, 1, xSrc);
                                    replaced = true;
                                }
                                if (ySrc3 >= 0 && ySrc3 == xDst)
                                {
                                    if (postPropagation)
                                    {
                                        postPropagation = false;
                                        ended = true;
                                        break;
                                    }
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
                                    postPropagation = true;
                                }
                                if (xDst == yDst)
                                {
                                    postPropagation = false;
                                    canRemove.Add(i);
                                    ended = true;
                                    //if (!propagationInline)
                                    break;
                                }
                            }

                            if(Y.Code == OpCodeREnum.Ret)
                            {
                                postPropagation = false;
                                canRemove.Add(i);
                                ended = true;
                                break;
                            }
                        }

                        if (postPropagation || !ended)
                        {
                            if (xDst >= stackRegisterBegin)
                                pendingFCP.Add(i);
                        }
                    }
                }
            }

            foreach(var b in blocks)
            {
                var pendingFCP = b.PendingCP;

                if (pendingFCP.Count > 0)
                {
                    var originBlock = b;
                    HashSet<CodeBasicBlock> processedBlocks = new HashSet<CodeBasicBlock>();
                    Queue<CodeBasicBlock> pendingBlocks = new Queue<CodeBasicBlock>();

                    foreach (var idx in pendingFCP)
                    {
                        var X = originBlock.FinalInstructions[idx];
                        short xDst;
                        GetOpcodeDestRegister(ref X, out xDst);
                        pendingBlocks.Clear();
                        bool cannotRemove = false;
                        bool isAbort = false;
                        foreach (var nb in originBlock.NextBlocks)
                            pendingBlocks.Enqueue(nb);
                        while (pendingBlocks.Count > 0)
                        {
                            var cur = pendingBlocks.Dequeue();

                            var ins = cur.FinalInstructions;
                            var canRemove = cur.CanRemove;

                            for (int j = 0; j < ins.Count; j++)
                            {
                                if (canRemove.Contains(j))
                                    continue;
                                if(cur == originBlock && j == idx)
                                {
                                    isAbort = true;
                                    break;
                                }
                                var Y = ins[j];

                                short ySrc, ySrc2, ySrc3, yDst;
                                if (GetOpcodeSourceRegister(ref Y, hasReturn, out ySrc, out ySrc2, out ySrc3))
                                {
                                    if (ySrc == xDst || ySrc2 == xDst || ySrc3 == xDst)
                                    {
                                        cannotRemove = true;
                                        break;
                                    }
                                }
                                if(GetOpcodeDestRegister(ref Y, out yDst))
                                {
                                    if(yDst == xDst)
                                    {
                                        isAbort = true;
                                        break;
                                    }
                                }
                            }

                            if (cannotRemove)
                                break;

                            processedBlocks.Add(cur);
                            if (!isAbort)
                            {
                                foreach (var nb in cur.NextBlocks)
                                {
                                    if (!processedBlocks.Contains(nb))
                                        pendingBlocks.Enqueue(nb);
                                }
                            }
                        }
                        if(!cannotRemove)
                        {
                            originBlock.CanRemove.Add(idx);
                        }
                    }
                    pendingFCP.Clear();
                }
            }
        }

        
    }
}
