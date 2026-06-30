#if ENABLE_NEO_MODE
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter.RegisterVM;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Intepreter
{
    public unsafe partial class ILIntepreter
    {
        internal unsafe byte* ExecuteNeo(ILMethod method, byte* esp, byte* retDst, int retRefBase, out bool unhandledException)
        {
#if DEBUG
            if (method == null)
                throw new NullReferenceException();
#endif
#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)

#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.BeginSample(method.ToString());
#else
                UnityEngine.Profiler.BeginSample(method.ToString());
#endif

#endif
            unhandledException = false;

            OpCodeR[] body = method.NeoFrame.NeoExecuteBody;
            AutoList mStack = stack.ManagedStack;
            ref readonly var nf = ref method.NeoFrame;
            int frameSize = nf.TotalStructSize;
            int totalRefSize = nf.TotalRefSize;
            int returnPrimitiveSize = nf.ReturnPrimitiveSize;
            int returnRefCount = nf.ReturnRefCount;
            var localInfos = nf.LocalInfos;
            var localIsRef = nf.LocalIsReference;

            byte* frameBase = esp;
            byte* newEsp = esp + frameSize;
            // TODO: stack overflow check vs stack.StackBase upper bound; will be added in Step 14 / 26

            // Zero locals primitive region
            if (nf.LocalsPrimitiveSize > 0)
                Unsafe.InitBlock(frameBase + nf.ParamPrimitiveSize, 0, (uint)nf.LocalsPrimitiveSize);
            if (localInfos != null && localIsRef != null)
            {
                for (int i = 0; i < localInfos.Length; i++)
                {
                    if (localIsRef[i])
                    {
                        *(int*)(frameBase + localInfos[i].Offset) = -1;
                    }
                }
            }

            // Managed stack reservation for this frame's reference slots
            int frameRefBase = mStack.Count;
            for (int i = 0; i < totalRefSize; i++)
                mStack.Add(null);

            // Frames stack placeholder: keep existing StackFrame plumbing alive.
            // BasePointer is interpreted as byte* via reinterpret cast; full debugger
            // adaptation is deferred to Step 14/26.
            StackFrame frame;
            stack.InitializeFrame(method, (StackObject*)frameBase, out frame);
            frame.IsRegister = true;
            frame.ManagedStackBase = frameRefBase;
            stack.PushFrame(ref frame);

            int finallyEndAddress = 0;
            Exception lastCaughtEx = null;
            var ehs = method.ExceptionHandlerRegister;

            fixed (OpCodeR* ptr = body)
            {
                OpCodeR* ip = ptr;
                bool returned = false;
                // Shared locals across case blocks. Declared at method scope so
                // IL2CPP / non-O3 builds reuse the same stack slot for every case.
                IType t;
                ILType ilType;
                ILTypeInstance ins;
                object obj;
                int sz, refCnt, srcIdx, dstIdx, srcRefOffset, dstRefOffset;
                while (!returned)
                {
                    try
                    {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                        if (ShouldBreak)
                            Break();
                        var insOffset = (int)(ip - ptr);
                        frame.Address.Value = insOffset;
                        AppDomain.DebugService.CheckShouldBreak(method, this, insOffset);
#endif
                        OpCodeREnum code = ip->Code;
                        switch (code)
                        {
                            case OpCodeREnum.Ldc_I4_M1:
                                *(int*)(frameBase + ip->DstOffset) = -1;
                                break;
                            case OpCodeREnum.Ldc_I4_0:
                                *(int*)(frameBase + ip->DstOffset) = 0;
                                break;
                            case OpCodeREnum.Ldc_I4_1:
                                *(int*)(frameBase + ip->DstOffset) = 1;
                                break;
                            case OpCodeREnum.Ldc_I4_2:
                                *(int*)(frameBase + ip->DstOffset) = 2;
                                break;
                            case OpCodeREnum.Ldc_I4_3:
                                *(int*)(frameBase + ip->DstOffset) = 3;
                                break;
                            case OpCodeREnum.Ldc_I4_4:
                                *(int*)(frameBase + ip->DstOffset) = 4;
                                break;
                            case OpCodeREnum.Ldc_I4_5:
                                *(int*)(frameBase + ip->DstOffset) = 5;
                                break;
                            case OpCodeREnum.Ldc_I4_6:
                                *(int*)(frameBase + ip->DstOffset) = 6;
                                break;
                            case OpCodeREnum.Ldc_I4_7:
                                *(int*)(frameBase + ip->DstOffset) = 7;
                                break;
                            case OpCodeREnum.Ldc_I4_8:
                                *(int*)(frameBase + ip->DstOffset) = 8;
                                break;
                            case OpCodeREnum.Ldc_I4:
                            case OpCodeREnum.Ldc_I4_S:
                                *(int*)(frameBase + ip->DstOffset) = ip->Operand;
                                break;
                            case OpCodeREnum.Ldc_I8:
                                *(long*)(frameBase + ip->DstOffset) = ip->OperandLong;
                                break;
                            case OpCodeREnum.Ldc_R4:
                                *(float*)(frameBase + ip->DstOffset) = ip->OperandFloat;
                                break;
                            case OpCodeREnum.Ldc_R8:
                                *(double*)(frameBase + ip->DstOffset) = ip->OperandDouble;
                                break;
                            case OpCodeREnum.Ldnull:
                                *(int*)(frameBase + ip->DstOffset) = -1;
                                break;
                            case OpCodeREnum.Ldstr:
                                dstIdx = frameRefBase + ip->Operand;
                                mStack[dstIdx] = AppDomain.GetString(ip->OperandLong);
                                *(int*)(frameBase + ip->DstOffset) = dstIdx;
                                break;
                            case OpCodeREnum.Move:
                                Unsafe.CopyBlock(frameBase + ip->DstOffset, frameBase + ip->SrcOffset, (uint)ip->Operand2);
                                if (ip->Operand == 1)
                                {
                                    dstRefOffset = ip->Operand3;
                                    srcIdx = *(int*)(frameBase + ip->SrcOffset);
                                    dstIdx = frameRefBase + dstRefOffset;
                                    if (srcIdx >= 0)
                                    {
                                        mStack[dstIdx] = mStack[srcIdx];
                                        *(int*)(frameBase + ip->DstOffset) = dstIdx;
                                    }
                                    else
                                    {
                                        mStack[dstIdx] = null;
                                        *(int*)(frameBase + ip->DstOffset) = -1;
                                    }
                                }
                                break;
                            case OpCodeREnum.Add:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) + *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Sub:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) - *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Mul:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) * *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Div:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) / *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Div_Un:
                                *(int*)(frameBase + ip->DstOffset) = (int)(*(uint*)(frameBase + ip->SrcOffset) / *(uint*)(frameBase + ip->OperandOffset));
                                break;
                            case OpCodeREnum.Rem:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) % *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Rem_Un:
                                *(int*)(frameBase + ip->DstOffset) = (int)(*(uint*)(frameBase + ip->SrcOffset) % *(uint*)(frameBase + ip->OperandOffset));
                                break;
                            case OpCodeREnum.And:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) & *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Or:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) | *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Xor:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) ^ *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Shl:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) << *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Shr:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) >> *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Shr_Un:
                                *(int*)(frameBase + ip->DstOffset) = (int)(*(uint*)(frameBase + ip->SrcOffset) >> *(int*)(frameBase + ip->OperandOffset));
                                break;
                            case OpCodeREnum.Neg:
                                *(int*)(frameBase + ip->DstOffset) = -*(int*)(frameBase + ip->SrcOffset);
                                break;
                            case OpCodeREnum.Not:
                                *(int*)(frameBase + ip->DstOffset) = ~*(int*)(frameBase + ip->SrcOffset);
                                break;
                            case OpCodeREnum.Add_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) + *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Sub_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) - *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Mul_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) * *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Div_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) / *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Div_Un_I8:
                                *(long*)(frameBase + ip->DstOffset) = (long)(*(ulong*)(frameBase + ip->SrcOffset) / *(ulong*)(frameBase + ip->OperandOffset));
                                break;
                            case OpCodeREnum.Rem_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) % *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Rem_Un_I8:
                                *(long*)(frameBase + ip->DstOffset) = (long)(*(ulong*)(frameBase + ip->SrcOffset) % *(ulong*)(frameBase + ip->OperandOffset));
                                break;
                            case OpCodeREnum.And_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) & *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Or_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) | *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Xor_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) ^ *(long*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Shl_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) << *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Shr_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) >> *(int*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Shr_Un_I8:
                                *(long*)(frameBase + ip->DstOffset) = (long)(*(ulong*)(frameBase + ip->SrcOffset) >> *(int*)(frameBase + ip->OperandOffset));
                                break;
                            case OpCodeREnum.Neg_I8:
                                *(long*)(frameBase + ip->DstOffset) = -*(long*)(frameBase + ip->SrcOffset);
                                break;
                            case OpCodeREnum.Not_I8:
                                *(long*)(frameBase + ip->DstOffset) = ~*(long*)(frameBase + ip->SrcOffset);
                                break;
                            case OpCodeREnum.Add_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) + *(float*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Sub_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) - *(float*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Mul_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) * *(float*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Div_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) / *(float*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Rem_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) % *(float*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Neg_R4:
                                *(float*)(frameBase + ip->DstOffset) = -*(float*)(frameBase + ip->SrcOffset);
                                break;
                            case OpCodeREnum.Add_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) + *(double*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Sub_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) - *(double*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Mul_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) * *(double*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Div_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) / *(double*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Rem_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) % *(double*)(frameBase + ip->OperandOffset);
                                break;
                            case OpCodeREnum.Neg_R8:
                                *(double*)(frameBase + ip->DstOffset) = -*(double*)(frameBase + ip->SrcOffset);
                                break;
                            case OpCodeREnum.Ceq:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) == *(int*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgt:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) > *(int*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgt_Un:
                                *(int*)(frameBase + ip->DstOffset) = *(uint*)(frameBase + ip->SrcOffset) > *(uint*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Clt:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) < *(int*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Clt_Un:
                                *(int*)(frameBase + ip->DstOffset) = *(uint*)(frameBase + ip->SrcOffset) < *(uint*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Ceq_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) == *(long*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgt_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) > *(long*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgt_Un_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(ulong*)(frameBase + ip->SrcOffset) > *(ulong*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Clt_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) < *(long*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Clt_Un_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(ulong*)(frameBase + ip->SrcOffset) < *(ulong*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Ceq_R4:
                                *(int*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) == *(float*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Ceq_R8:
                                *(int*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) == *(double*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgt_R4:
                            case OpCodeREnum.Cgt_Un_R4:
                                *(int*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) > *(float*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgt_R8:
                            case OpCodeREnum.Cgt_Un_R8:
                                *(int*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) > *(double*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Clt_R4:
                            case OpCodeREnum.Clt_Un_R4:
                                *(int*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) < *(float*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Clt_R8:
                            case OpCodeREnum.Clt_Un_R8:
                                *(int*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) < *(double*)(frameBase + ip->OperandOffset) ? 1 : 0;
                                break;
                            case OpCodeREnum.Br:
                            case OpCodeREnum.Br_S:
                                ip = ptr + ip->Operand;
                                continue;
                            case OpCodeREnum.Brtrue:
                            case OpCodeREnum.Brtrue_S:
                                if (ip->Operand2 == 8 ? *(long*)(frameBase + ip->DstOffset) != 0 : *(int*)(frameBase + ip->DstOffset) != 0)
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Brfalse:
                            case OpCodeREnum.Brfalse_S:
                                if (ip->Operand2 == 8 ? *(long*)(frameBase + ip->DstOffset) == 0 : *(int*)(frameBase + ip->DstOffset) == 0)
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Beq:
                                if (*(int*)(frameBase + ip->DstOffset) == *(int*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bne_Un:
                                if (*(int*)(frameBase + ip->DstOffset) != *(int*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blt:
                                if (*(int*)(frameBase + ip->DstOffset) < *(int*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgt:
                                if (*(int*)(frameBase + ip->DstOffset) > *(int*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Ble:
                                if (*(int*)(frameBase + ip->DstOffset) <= *(int*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bge:
                                if (*(int*)(frameBase + ip->DstOffset) >= *(int*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blt_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) < *(uint*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgt_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) > *(uint*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Ble_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) <= *(uint*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bge_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) >= *(uint*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Beq_I8:
                                if (*(long*)(frameBase + ip->DstOffset) == *(long*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bne_Un_I8:
                                if (*(long*)(frameBase + ip->DstOffset) != *(long*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blt_I8:
                                if (*(long*)(frameBase + ip->DstOffset) < *(long*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgt_I8:
                                if (*(long*)(frameBase + ip->DstOffset) > *(long*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Ble_I8:
                                if (*(long*)(frameBase + ip->DstOffset) <= *(long*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bge_I8:
                                if (*(long*)(frameBase + ip->DstOffset) >= *(long*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blt_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) < *(ulong*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgt_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) > *(ulong*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Ble_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) <= *(ulong*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bge_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) >= *(ulong*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Beq_R4:
                                if (*(float*)(frameBase + ip->DstOffset) == *(float*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bne_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) != *(float*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blt_R4:
                            case OpCodeREnum.Blt_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) < *(float*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgt_R4:
                            case OpCodeREnum.Bgt_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) > *(float*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Ble_R4:
                            case OpCodeREnum.Ble_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) <= *(float*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bge_R4:
                            case OpCodeREnum.Bge_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) >= *(float*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Beq_R8:
                                if (*(double*)(frameBase + ip->DstOffset) == *(double*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bne_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) != *(double*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blt_R8:
                            case OpCodeREnum.Blt_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) < *(double*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgt_R8:
                            case OpCodeREnum.Bgt_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) > *(double*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Ble_R8:
                            case OpCodeREnum.Ble_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) <= *(double*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bge_R8:
                            case OpCodeREnum.Bge_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) >= *(double*)(frameBase + ip->SrcOffset))
                                {
                                    ip = ptr + ip->Operand;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Addi:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) + ip->Operand;
                                break;
                            case OpCodeREnum.Subi:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) - ip->Operand;
                                break;
                            case OpCodeREnum.Muli:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) * ip->Operand;
                                break;
                            case OpCodeREnum.Divi:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) / ip->Operand;
                                break;
                            case OpCodeREnum.Divi_Un:
                                *(int*)(frameBase + ip->DstOffset) = (int)(*(uint*)(frameBase + ip->SrcOffset) / (uint)ip->Operand);
                                break;
                            case OpCodeREnum.Remi:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) % ip->Operand;
                                break;
                            case OpCodeREnum.Remi_Un:
                                *(int*)(frameBase + ip->DstOffset) = (int)(*(uint*)(frameBase + ip->SrcOffset) % (uint)ip->Operand);
                                break;
                            case OpCodeREnum.Andi:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) & ip->Operand;
                                break;
                            case OpCodeREnum.Ori:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) | ip->Operand;
                                break;
                            case OpCodeREnum.Xori:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) ^ ip->Operand;
                                break;
                            case OpCodeREnum.Shli:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) << ip->Operand;
                                break;
                            case OpCodeREnum.Shri:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) >> ip->Operand;
                                break;
                            case OpCodeREnum.Shri_Un:
                                *(int*)(frameBase + ip->DstOffset) = (int)(*(uint*)(frameBase + ip->SrcOffset) >> ip->Operand);
                                break;
                            case OpCodeREnum.Addi_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) + ip->OperandLong;
                                break;
                            case OpCodeREnum.Subi_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) - ip->OperandLong;
                                break;
                            case OpCodeREnum.Muli_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) * ip->OperandLong;
                                break;
                            case OpCodeREnum.Divi_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) / ip->OperandLong;
                                break;
                            case OpCodeREnum.Divi_Un_I8:
                                *(long*)(frameBase + ip->DstOffset) = (long)(*(ulong*)(frameBase + ip->SrcOffset) / (ulong)ip->OperandLong);
                                break;
                            case OpCodeREnum.Remi_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) % ip->OperandLong;
                                break;
                            case OpCodeREnum.Remi_Un_I8:
                                *(long*)(frameBase + ip->DstOffset) = (long)(*(ulong*)(frameBase + ip->SrcOffset) % (ulong)ip->OperandLong);
                                break;
                            case OpCodeREnum.Andi_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) & ip->OperandLong;
                                break;
                            case OpCodeREnum.Ori_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) | ip->OperandLong;
                                break;
                            case OpCodeREnum.Xori_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) ^ ip->OperandLong;
                                break;
                            case OpCodeREnum.Shli_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) << (int)ip->OperandLong;
                                break;
                            case OpCodeREnum.Shri_I8:
                                *(long*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) >> (int)ip->OperandLong;
                                break;
                            case OpCodeREnum.Shri_Un_I8:
                                *(long*)(frameBase + ip->DstOffset) = (long)(*(ulong*)(frameBase + ip->SrcOffset) >> (int)ip->OperandLong);
                                break;
                            case OpCodeREnum.Addi_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) + ip->OperandFloat;
                                break;
                            case OpCodeREnum.Subi_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) - ip->OperandFloat;
                                break;
                            case OpCodeREnum.Muli_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) * ip->OperandFloat;
                                break;
                            case OpCodeREnum.Divi_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) / ip->OperandFloat;
                                break;
                            case OpCodeREnum.Remi_R4:
                                *(float*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) % ip->OperandFloat;
                                break;
                            case OpCodeREnum.Addi_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) + ip->OperandDouble;
                                break;
                            case OpCodeREnum.Subi_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) - ip->OperandDouble;
                                break;
                            case OpCodeREnum.Muli_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) * ip->OperandDouble;
                                break;
                            case OpCodeREnum.Divi_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) / ip->OperandDouble;
                                break;
                            case OpCodeREnum.Remi_R8:
                                *(double*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) % ip->OperandDouble;
                                break;
                            case OpCodeREnum.Ceqi:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) == ip->Operand ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgti:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) > ip->Operand ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgti_Un:
                                *(int*)(frameBase + ip->DstOffset) = *(uint*)(frameBase + ip->SrcOffset) > (uint)ip->Operand ? 1 : 0;
                                break;
                            case OpCodeREnum.Clti:
                                *(int*)(frameBase + ip->DstOffset) = *(int*)(frameBase + ip->SrcOffset) < ip->Operand ? 1 : 0;
                                break;
                            case OpCodeREnum.Clti_Un:
                                *(int*)(frameBase + ip->DstOffset) = *(uint*)(frameBase + ip->SrcOffset) < (uint)ip->Operand ? 1 : 0;
                                break;
                            case OpCodeREnum.Ceqi_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) == ip->OperandLong ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgti_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) > ip->OperandLong ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgti_Un_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(ulong*)(frameBase + ip->SrcOffset) > (ulong)ip->OperandLong ? 1 : 0;
                                break;
                            case OpCodeREnum.Clti_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(long*)(frameBase + ip->SrcOffset) < ip->OperandLong ? 1 : 0;
                                break;
                            case OpCodeREnum.Clti_Un_I8:
                                *(int*)(frameBase + ip->DstOffset) = *(ulong*)(frameBase + ip->SrcOffset) < (ulong)ip->OperandLong ? 1 : 0;
                                break;
                            case OpCodeREnum.Ceqi_R4:
                                *(int*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) == ip->OperandFloat ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgti_R4:
                            case OpCodeREnum.Cgti_Un_R4:
                                *(int*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) > ip->OperandFloat ? 1 : 0;
                                break;
                            case OpCodeREnum.Clti_R4:
                            case OpCodeREnum.Clti_Un_R4:
                                *(int*)(frameBase + ip->DstOffset) = *(float*)(frameBase + ip->SrcOffset) < ip->OperandFloat ? 1 : 0;
                                break;
                            case OpCodeREnum.Ceqi_R8:
                                *(int*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) == ip->OperandDouble ? 1 : 0;
                                break;
                            case OpCodeREnum.Cgti_R8:
                            case OpCodeREnum.Cgti_Un_R8:
                                *(int*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) > ip->OperandDouble ? 1 : 0;
                                break;
                            case OpCodeREnum.Clti_R8:
                            case OpCodeREnum.Clti_Un_R8:
                                *(int*)(frameBase + ip->DstOffset) = *(double*)(frameBase + ip->SrcOffset) < ip->OperandDouble ? 1 : 0;
                                break;
                            case OpCodeREnum.Beqi:
                                if (*(int*)(frameBase + ip->DstOffset) == ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bnei_Un:
                                if (*(int*)(frameBase + ip->DstOffset) != ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blti:
                                if (*(int*)(frameBase + ip->DstOffset) < ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgti:
                                if (*(int*)(frameBase + ip->DstOffset) > ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blei:
                                if (*(int*)(frameBase + ip->DstOffset) <= ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgei:
                                if (*(int*)(frameBase + ip->DstOffset) >= ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blti_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) < (uint)ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgti_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) > (uint)ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blei_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) <= (uint)ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgei_Un:
                                if (*(uint*)(frameBase + ip->DstOffset) >= (uint)ip->Operand)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Beqi_I8:
                                if (*(long*)(frameBase + ip->DstOffset) == ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bnei_Un_I8:
                                if (*(long*)(frameBase + ip->DstOffset) != ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blti_I8:
                                if (*(long*)(frameBase + ip->DstOffset) < ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgti_I8:
                                if (*(long*)(frameBase + ip->DstOffset) > ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blei_I8:
                                if (*(long*)(frameBase + ip->DstOffset) <= ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgei_I8:
                                if (*(long*)(frameBase + ip->DstOffset) >= ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blti_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) < (ulong)ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgti_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) > (ulong)ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blei_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) <= (ulong)ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgei_Un_I8:
                                if (*(ulong*)(frameBase + ip->DstOffset) >= (ulong)ip->OperandLong)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Beqi_R4:
                                if (*(float*)(frameBase + ip->DstOffset) == ip->OperandFloat)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bnei_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) != ip->OperandFloat)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blti_R4:
                            case OpCodeREnum.Blti_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) < ip->OperandFloat)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgti_R4:
                            case OpCodeREnum.Bgti_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) > ip->OperandFloat)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blei_R4:
                            case OpCodeREnum.Blei_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) <= ip->OperandFloat)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgei_R4:
                            case OpCodeREnum.Bgei_Un_R4:
                                if (*(float*)(frameBase + ip->DstOffset) >= ip->OperandFloat)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Beqi_R8:
                                if (*(double*)(frameBase + ip->DstOffset) == ip->OperandDouble)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bnei_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) != ip->OperandDouble)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blti_R8:
                            case OpCodeREnum.Blti_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) < ip->OperandDouble)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgti_R8:
                            case OpCodeREnum.Bgti_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) > ip->OperandDouble)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Blei_R8:
                            case OpCodeREnum.Blei_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) <= ip->OperandDouble)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Bgei_R8:
                            case OpCodeREnum.Bgei_Un_R8:
                                if (*(double*)(frameBase + ip->DstOffset) >= ip->OperandDouble)
                                {
                                    ip = ptr + ip->Operand4;
                                    continue;
                                }
                                break;
                            case OpCodeREnum.Conv_I1:
                                *(int*)(frameBase + ip->DstOffset) = (sbyte)ReadConvI8(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_U1:
                                *(int*)(frameBase + ip->DstOffset) = (byte)ReadConvI8(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_I2:
                                *(int*)(frameBase + ip->DstOffset) = (short)ReadConvI8(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_U2:
                                *(int*)(frameBase + ip->DstOffset) = (ushort)ReadConvI8(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_I4:
                                *(int*)(frameBase + ip->DstOffset) = ReadConvI4(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_U4:
                                *(uint*)(frameBase + ip->DstOffset) = ReadConvU4(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_I8:
                                *(long*)(frameBase + ip->DstOffset) = ReadConvI8(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_U8:
                                *(ulong*)(frameBase + ip->DstOffset) = ReadConvU8(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_R4:
                                *(float*)(frameBase + ip->DstOffset) = ReadConvR4(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Conv_R8:
                                *(double*)(frameBase + ip->DstOffset) = ReadConvR8(frameBase, ip->SrcOffset, (NeoPrimitiveTypeTag)ip->Operand2);
                                break;
                            case OpCodeREnum.Call:
                            case OpCodeREnum.Callvirt:
                                {
                                    var targetMethod = AppDomain.GetMethod(ip->Operand2);
                                    if (targetMethod == null)
                                    {
                                        ip++;
                                        continue;
                                    }
                                    
                                    if (targetMethod is ILMethod ilm)
                                    {
                                        int callParamIdx = ip->Operand;
                                        ref var map = ref nf.NeoCallParams[callParamIdx];
                                        byte* targetBase = newEsp;
                                        
                                        if (map.PrimitiveSize != null)
                                        {
                                            for (int i = 0; i < map.PrimitiveSize.Length; i++)
                                            {
                                                Unsafe.CopyBlock(targetBase + map.PrimitiveDst[i], frameBase + map.PrimitiveSrc[i], map.PrimitiveSize[i]);
                                            }
                                        }
                                        
                                        if (map.RefSrc != null && map.RefSrc.Length > 0)
                                        {
                                            throw new NotImplementedException("Neo Call reference parameters require Step 7 RefOffset lowering");
                                        }
                                        
                                        int retSize = ilm.NeoFrame.ReturnPrimitiveSize;
                                        int targetRetRefBase = mStack.Count;
                                        byte* retDstPtr = ip->Register1 >= 0 ? frameBase + ip->DstOffset : null;
                                        
                                        ExecuteNeo(ilm, targetBase, retDstPtr, targetRetRefBase, out unhandledException);
                                        if (unhandledException)
                                            return null;
                                        
                                        ip++;
                                        continue;
                                    }
                                    else
                                    {
                                        throw new NotImplementedException("CLRMethod call not supported in Neo mode yet.");
                                    }
                                }
                                break;
                            case OpCodeREnum.Ret:
                                if (retDst != null && (returnPrimitiveSize > 0 || returnRefCount > 0))
                                {
                                    if (returnPrimitiveSize > 0)
                                        Unsafe.CopyBlock(retDst, frameBase + ip->DstOffset, (uint)returnPrimitiveSize);
                                    if (returnRefCount > 0)
                                        throw new NotImplementedException("Neo Ret reference return requires Step 7 RefOffset lowering");
                                }
                                returned = true;
                                break;
                            case OpCodeREnum.Initobj:
                                t = AppDomain.GetType(ip->Operand);
                                ilType = t as ILType;
                                if (ilType != null)
                                {
                                    refCnt = 0;
                                    if (ilType.IsEnum)
                                        sz = AppDomain.GetPrimitiveSize(ilType.FieldTypes[0]);
                                    else if (ilType.IsPrimitive)
                                        sz = AppDomain.GetPrimitiveSize(ilType);
                                    else if (ilType.IsValueType)
                                    {
                                        sz = ilType.TotalPrimitiveSize;
                                        refCnt = ilType.TotalReferenceCount;
                                    }
                                    else
                                    {
                                        // Reference type initobj → write null index (-1) into the byte slot.
                                        *(int*)(frameBase + ip->DstOffset) = -1;
                                        break;
                                    }
                                    if (sz > 0)
                                        Unsafe.InitBlock(frameBase + ip->DstOffset, 0, (uint)sz);
                                    if (refCnt > 0)
                                        throw new NotImplementedException("Neo Initobj reference fields require Step 7 RefOffset lowering");
                                    for (int i = 0; i < refCnt; i++)
                                        mStack[frameRefBase + i] = null;
                                }
                                else
                                {
                                    // TODO Step 13: CLR value type Initobj (with/without ValueTypeBinder)
                                    throw new NotImplementedException("CLR value type Initobj: Step 13");
                                }
                                break;
                            case OpCodeREnum.Box:
                                dstRefOffset = ip->Operand3 >> 16;
                                srcRefOffset = (short)(ip->Operand3 & 0xffff);
                                t = AppDomain.GetType(ip->Operand);
                                ilType = t as ILType;
                                if (ilType != null)
                                {
                                    if (ilType.IsEnum)
                                    {
                                        ins = new ILEnumTypeInstance(ilType);
                                        sz = AppDomain.GetPrimitiveSize(ilType.FieldTypes[0]);
                                        if (sz > 0)
                                        {
                                            ref byte dstP = ref MemoryMarshal.GetReference(ins.Primitives.AsSpan());
                                            Unsafe.CopyBlock(ref dstP, ref *(frameBase + ip->SrcOffset), (uint)sz);
                                        }
                                    }
                                    else if (ilType.IsPrimitive)
                                    {
                                        // Boxing a primitive IL type isn't a regular path
                                        // (compiler usually boxes CLR primitives), but handle for completeness.
                                        ins = ilType.Instantiate(false);
                                        sz = AppDomain.GetPrimitiveSize(ilType);
                                        if (sz > 0 && ins.Primitives != null)
                                        {
                                            ref byte dstP = ref MemoryMarshal.GetReference(ins.Primitives.AsSpan());
                                            Unsafe.CopyBlock(ref dstP, ref *(frameBase + ip->SrcOffset), (uint)sz);
                                        }
                                    }
                                    else if (ilType.IsValueType)
                                    {
                                        ins = ilType.Instantiate(false);
                                        CopyFrameToIL(frameBase, ip->SrcOffset, srcRefOffset,
                                                      ilType.TotalPrimitiveSize, ilType.TotalReferenceCount,
                                                      mStack, frameRefBase, ins);
                                    }
                                    else
                                    {
                                        // Boxing a reference type is a no-op: the same instance flows through.
                                        srcIdx = *(int*)(frameBase + ip->SrcOffset);
                                        obj = srcIdx >= 0 ? mStack[srcIdx] : null;
                                        dstIdx = frameRefBase + dstRefOffset;
                                        mStack[dstIdx] = obj;
                                        *(int*)(frameBase + ip->DstOffset) = obj != null ? dstIdx : -1;
                                        break;
                                    }
                                    ins.Boxed = true;
                                    dstIdx = frameRefBase + dstRefOffset;
                                    mStack[dstIdx] = ins;
                                    *(int*)(frameBase + ip->DstOffset) = dstIdx;
                                }
                                else
                                {
                                    // TODO Step 13: CLR value type Box (with/without ValueTypeBinder)
                                    throw new NotImplementedException("CLR value type Box: Step 13");
                                }
                                break;
                            case OpCodeREnum.Ldfld_I1:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(int*)(frameBase + ip->DstOffset) = (sbyte)ins.Primitives[ip->Operand2];
                                break;
                            case OpCodeREnum.Ldfld_U1:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(int*)(frameBase + ip->DstOffset) = ins.Primitives[ip->Operand2];
                                break;
                            case OpCodeREnum.Ldfld_I2:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<short>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_U2:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ushort>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_I4:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<int>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_U4:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(uint*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<uint>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_I8:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(long*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<long>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_U8:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(ulong*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ulong>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_R4:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(float*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<float>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_R8:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                *(double*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<double>(ref ins.Primitives[ip->Operand2]);
                                break;
                            case OpCodeREnum.Ldfld_Ref:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                obj = ins.ManagedObjects[ip->Operand3];
                                dstIdx = frameRefBase + ip->Operand;
                                mStack[dstIdx] = obj;
                                *(int*)(frameBase + ip->DstOffset) = obj != null ? dstIdx : -1;
                                break;
                            case OpCodeREnum.Stfld_I1:
                            case OpCodeREnum.Stfld_U1:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                ins.Primitives[ip->Operand2] = *(byte*)(frameBase + ip->SrcOffset);
                                break;
                            case OpCodeREnum.Stfld_I2:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(short*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_U2:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(ushort*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_I4:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(int*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_U4:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(uint*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_I8:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(long*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_U8:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(ulong*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_R4:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(float*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_R8:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                Unsafe.WriteUnaligned(ref ins.Primitives[ip->Operand2], *(double*)(frameBase + ip->SrcOffset));
                                break;
                            case OpCodeREnum.Stfld_Ref:
                                ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                srcIdx = *(int*)(frameBase + ip->SrcOffset);
                                ins.ManagedObjects[ip->Operand3] = srcIdx >= 0 ? mStack[srcIdx] : null;
                                break;
                            case OpCodeREnum.Unbox:
                            case OpCodeREnum.Unbox_Any:
                                dstRefOffset = ip->Operand3 >> 16;
                                t = AppDomain.GetType(ip->Operand);
                                srcIdx = *(int*)(frameBase + ip->SrcOffset);
                                if (srcIdx < 0)
                                    throw new NullReferenceException();
                                obj = mStack[srcIdx];
                                if (obj == null)
                                    throw new NullReferenceException();
                                ilType = t as ILType;
                                if (ilType != null)
                                {
                                    ins = obj as ILTypeInstance;
                                    if (ins == null)
                                        throw new InvalidCastException();
                                    if (ilType.IsEnum)
                                    {
                                        sz = AppDomain.GetPrimitiveSize(ilType.FieldTypes[0]);
                                        if (sz > 0)
                                        {
                                            ref byte srcP = ref MemoryMarshal.GetReference(ins.Primitives.AsSpan());
                                            Unsafe.CopyBlock(ref *(frameBase + ip->DstOffset), ref srcP, (uint)sz);
                                        }
                                    }
                                    else if (ilType.IsPrimitive)
                                    {
                                        sz = AppDomain.GetPrimitiveSize(ilType);
                                        if (sz > 0 && ins.Primitives != null)
                                        {
                                            ref byte srcP = ref MemoryMarshal.GetReference(ins.Primitives.AsSpan());
                                            Unsafe.CopyBlock(ref *(frameBase + ip->DstOffset), ref srcP, (uint)sz);
                                        }
                                    }
                                    else if (ilType.IsValueType)
                                    {
                                        CopyILToFrame(ins,
                                                      frameBase, ip->DstOffset, dstRefOffset,
                                                      ilType.TotalPrimitiveSize, ilType.TotalReferenceCount,
                                                      mStack, frameRefBase);
                                    }
                                    else
                                    {
                                        throw new InvalidCastException();
                                    }
                                }
                                else
                                {
                                    // TODO Step 13: CLR value type Unbox (with/without ValueTypeBinder)
                                    throw new NotImplementedException("CLR value type Unbox: Step 13");
                                }
                                break;
                            default:
                                throw new NotImplementedException(string.Format("Neo: opcode {0} not yet implemented (Step 6)", code));
                        }
                        ip++;
                    }
                    catch (Exception ex)
                    {
                        var oriESP = (StackObject*)newEsp;
                        StackObject* tmpEsp = oriESP;
                        bool isJmp = HandleException(ex, ref tmpEsp, ehs, method, (int)(ip - ptr), ref frame, ref lastCaughtEx, ref unhandledException, ref finallyEndAddress, out int jmpTarget, out bool isCatch);
                        if (isCatch)
                        {
                            // Truncate mStack back to this frame's reserved region
                            int targetCount = frameRefBase + totalRefSize;
                            if (mStack.Count > targetCount)
                            {
                                mStack.RemoveRange(targetCount, mStack.Count - targetCount);
                            }
                            // TODO: write exception object into the catch handler's slot (Step 14)
                        }
                        if (isJmp)
                        {
                            ip = ptr + jmpTarget;
                            continue;
                        }
                        if (unhandledException)
                        {
                            throw;
                        }
                        unhandledException = true;
                        returned = true;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                        if (!AppDomain.DebugService.Break(this, ex))
#endif
                        {
                            var newEx = new ILRuntimeException(ex.Message, this, method, oriESP, ex);
                            throw newEx;
                        }
                    }
                }
            }

            // Unwind: pop frame, truncate mStack back to entry baseline.
            // Frames stack popping: best-effort (BasePointer compares by pointer).
            if (stack.Frames.Count > 0 && stack.Frames.Peek().BasePointer == frame.BasePointer)
            {
                stack.Frames.Pop();
            }
            if (mStack.Count > frameRefBase)
            {
                mStack.RemoveRange(frameRefBase, mStack.Count - frameRefBase);
            }

#if DEBUG && !NO_PROFILER
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == AppDomain.UnityMainThreadID)
#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.EndSample();
#else
                UnityEngine.Profiler.EndSample();
#endif
#endif
            return frameBase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ILTypeInstance GetNeoILInstance(AutoList mStack, int objIndex)
        {
            if (objIndex < 0)
                throw new NullReferenceException();
            ILTypeInstance ins = mStack[objIndex] as ILTypeInstance;
            if (ins == null)
                throw new InvalidCastException();
            return ins;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe int ReadConvI4(byte* frameBase, ushort offset, NeoPrimitiveTypeTag tag)
        {
            unchecked
            {
                switch (tag)
                {
                    case NeoPrimitiveTypeTag.U4:
                        return (int)*(uint*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I8:
                        return (int)*(long*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.U8:
                        return (int)*(ulong*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R4:
                        return (int)*(float*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R8:
                        return (int)*(double*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I4:
                    default:
                        return *(int*)(frameBase + offset);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe uint ReadConvU4(byte* frameBase, ushort offset, NeoPrimitiveTypeTag tag)
        {
            unchecked
            {
                switch (tag)
                {
                    case NeoPrimitiveTypeTag.U4:
                        return *(uint*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I8:
                        return (uint)*(long*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.U8:
                        return (uint)*(ulong*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R4:
                        return (uint)*(float*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R8:
                        return (uint)*(double*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I4:
                    default:
                        return (uint)*(int*)(frameBase + offset);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe long ReadConvI8(byte* frameBase, ushort offset, NeoPrimitiveTypeTag tag)
        {
            unchecked
            {
                switch (tag)
                {
                    case NeoPrimitiveTypeTag.U4:
                        return *(uint*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I8:
                        return *(long*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.U8:
                        return (long)*(ulong*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R4:
                        return (long)*(float*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R8:
                        return (long)*(double*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I4:
                    default:
                        return *(int*)(frameBase + offset);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe ulong ReadConvU8(byte* frameBase, ushort offset, NeoPrimitiveTypeTag tag)
        {
            unchecked
            {
                switch (tag)
                {
                    case NeoPrimitiveTypeTag.U4:
                        return *(uint*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I8:
                        return (ulong)*(long*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.U8:
                        return *(ulong*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R4:
                        return (ulong)*(float*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.R8:
                        return (ulong)*(double*)(frameBase + offset);
                    case NeoPrimitiveTypeTag.I4:
                    default:
                        return (ulong)*(int*)(frameBase + offset);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float ReadConvR4(byte* frameBase, ushort offset, NeoPrimitiveTypeTag tag)
        {
            switch (tag)
            {
                case NeoPrimitiveTypeTag.U4:
                    return *(uint*)(frameBase + offset);
                case NeoPrimitiveTypeTag.I8:
                    return *(long*)(frameBase + offset);
                case NeoPrimitiveTypeTag.U8:
                    return *(ulong*)(frameBase + offset);
                case NeoPrimitiveTypeTag.R4:
                    return *(float*)(frameBase + offset);
                case NeoPrimitiveTypeTag.R8:
                    return (float)*(double*)(frameBase + offset);
                case NeoPrimitiveTypeTag.I4:
                default:
                    return *(int*)(frameBase + offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe double ReadConvR8(byte* frameBase, ushort offset, NeoPrimitiveTypeTag tag)
        {
            switch (tag)
            {
                case NeoPrimitiveTypeTag.U4:
                    return *(uint*)(frameBase + offset);
                case NeoPrimitiveTypeTag.I8:
                    return *(long*)(frameBase + offset);
                case NeoPrimitiveTypeTag.U8:
                    return *(ulong*)(frameBase + offset);
                case NeoPrimitiveTypeTag.R4:
                    return *(float*)(frameBase + offset);
                case NeoPrimitiveTypeTag.R8:
                    return *(double*)(frameBase + offset);
                case NeoPrimitiveTypeTag.I4:
                default:
                    return *(int*)(frameBase + offset);
            }
        }

        // Copies frame byte region + frame mStack refs into an ILTypeInstance.
        // Used by Box (frame → boxed instance).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void CopyFrameToIL(byte* frameBase, int primOffset, int refOffset,
                                         int primSize, int refCount,
                                         AutoList mStack, int frameRefBase,
                                         ILTypeInstance dst)
        {
            if (primSize > 0 && dst.Primitives != null)
            {
                ref byte dstP = ref MemoryMarshal.GetReference(dst.Primitives.AsSpan());
                Unsafe.CopyBlock(ref dstP, ref *(frameBase + primOffset), (uint)primSize);
            }
            if (refCount > 0)
            {
                var dstRefs = dst.ManagedObjects;
                int srcBase = frameRefBase + refOffset;
                for (int i = 0; i < refCount; i++)
                    dstRefs[i] = mStack[srcBase + i];
            }
        }

        // Copies ILTypeInstance contents back to the frame byte region + frame mStack refs.
        // Used by Unbox / Unbox_Any (boxed instance → frame).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void CopyILToFrame(ILTypeInstance src,
                                         byte* frameBase, int primOffset, int refOffset,
                                         int primSize, int refCount,
                                         AutoList mStack, int frameRefBase)
        {
            if (primSize > 0 && src.Primitives != null)
            {
                ref byte srcP = ref MemoryMarshal.GetReference(src.Primitives.AsSpan());
                Unsafe.CopyBlock(ref *(frameBase + primOffset), ref srcP, (uint)primSize);
            }
            if (refCount > 0)
            {
                var srcRefs = src.ManagedObjects;
                int dstBase = frameRefBase + refOffset;
                for (int i = 0; i < refCount; i++)
                    mStack[dstBase + i] = srcRefs[i];
            }
        }

        // Step 6 entry shim: convert raw bytes the Neo interpreter wrote into
        // retDst back into a CLR object the outer Run() pipeline can consume.
        // Step 8 will replace this whole code path with the proper Neo call
        // convention; for now it's only invoked from the top-level Invoke entry
        // for primitive return types.
        static unsafe object NeoBoxReturnValue(IType returnType, byte* retDst, int retSize)
        {
            var clr = returnType.TypeForCLR;
            if (clr == typeof(int))
                return *(int*)retDst;
            if (clr == typeof(uint))
                return *(uint*)retDst;
            if (clr == typeof(long))
                return *(long*)retDst;
            if (clr == typeof(ulong))
                return *(ulong*)retDst;
            if (clr == typeof(short))
                return *(short*)retDst;
            if (clr == typeof(ushort))
                return *(ushort*)retDst;
            if (clr == typeof(byte))
                return *retDst;
            if (clr == typeof(sbyte))
                return *(sbyte*)retDst;
            if (clr == typeof(bool))
                return *retDst != 0;
            if (clr == typeof(char))
                return *(char*)retDst;
            if (clr == typeof(float))
                return *(float*)retDst;
            if (clr == typeof(double))
                return *(double*)retDst;
            // Fallback: raw bytes as int
            return retSize >= 4 ? (object)*(int*)retDst : (object)(int)*retDst;
        }
    }
}
#endif
