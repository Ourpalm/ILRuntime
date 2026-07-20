#if ENABLE_NEO_MODE
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Other;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadNeoInt32(byte* frameBase, ref int curPrim)
        {
            int res = *(int*)(frameBase + curPrim);
            curPrim += 4;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadNeoUInt32(byte* frameBase, ref int curPrim)
        {
            uint res = *(uint*)(frameBase + curPrim);
            curPrim += 4;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadNeoInt16(byte* frameBase, ref int curPrim)
        {
            short res = *(short*)(frameBase + curPrim);
            curPrim += 2;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadNeoUInt16(byte* frameBase, ref int curPrim)
        {
            ushort res = *(ushort*)(frameBase + curPrim);
            curPrim += 2;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadNeoUInt8(byte* frameBase, ref int curPrim)
        {
            byte res = *(byte*)(frameBase + curPrim);
            curPrim += 1;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadNeoInt8(byte* frameBase, ref int curPrim)
        {
            sbyte res = *(sbyte*)(frameBase + curPrim);
            curPrim += 1;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadNeoBoolean(byte* frameBase, ref int curPrim)
        {
            bool res = *(byte*)(frameBase + curPrim) != 0;
            curPrim += 1;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadNeoInt64(byte* frameBase, ref int curPrim)
        {
            long res = *(long*)(frameBase + curPrim);
            curPrim += 8;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadNeoUInt64(byte* frameBase, ref int curPrim)
        {
            ulong res = *(ulong*)(frameBase + curPrim);
            curPrim += 8;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadNeoFloat(byte* frameBase, ref int curPrim)
        {
            float res = *(float*)(frameBase + curPrim);
            curPrim += 4;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadNeoDouble(byte* frameBase, ref int curPrim)
        {
            double res = *(double*)(frameBase + curPrim);
            curPrim += 8;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ReadNeoChar(byte* frameBase, ref int curPrim)
        {
            char res = (char)*(int*)(frameBase + curPrim);
            curPrim += 4;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ReadNeoReference(byte* frameBase, ref int curPrim, AutoList mStack)
        {
            int idx = *(int*)(frameBase + curPrim);
            curPrim += 4;
            return idx < 0 ? null : mStack[idx];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyNeoCallArguments(ref NeoCallParamMap map, byte* frameBase, byte* targetBase)
        {
            if (map.PrimitiveSize != null)
            {
                for (int i = 0; i < map.PrimitiveSize.Length; i++)
                {
                    Unsafe.CopyBlock(targetBase + map.PrimitiveDst[i], frameBase + map.PrimitiveSrc[i], map.PrimitiveSize[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyNeoCallRefs(ref NeoCallParamMap map, AutoList mStack, int callerRefBase, int calleeRefBase)
        {
            if (map.RefSrc != null)
            {
                for (int i = 0; i < map.RefSrc.Length; i++)
                {
                    mStack[calleeRefBase + map.RefDst[i]] = mStack[callerRefBase + map.RefSrc[i]];
                }
            }
        }

        // Prepares a Neo call frame in one shot:
        //   1. copies primitive argument bytes from caller frame to callee frame,
        //   2. for IL targets, reserves the callee's full ref-slot region in mStack
        //      via a single O(1) ExpandBySize (no per-slot Add(null) loop), and
        //      copies argument refs from caller into their callee positions.
        // Returns calleeRefBase for IL targets, -1 for CLR targets (they don't own
        // a Neo ref region).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PrepareNeoCallFrame(
            ref NeoCallParamMap map, AutoList mStack,
            byte* frameBase, byte* targetBase,
            int callerRefBase, IMethod targetMethod)
        {
            CopyNeoCallArguments(ref map, frameBase, targetBase);
            if (targetMethod is ILMethod ilm)
            {
                int calleeRefBase = mStack.Count;
                mStack.ExpandBySize(ilm.CompiledFrame.TotalRefSize);
                CopyNeoCallRefs(ref map, mStack, callerRefBase, calleeRefBase);
                return calleeRefBase;
            }
            return -1;
        }

        bool InvokeNeoCallTarget(IMethod targetMethod, bool isNewobj, byte* targetBase, AutoList mStack, byte* retDstPtr, int targetRetRefBase, out bool unhandledException, int calleeRefBase = -1)
        {
            unhandledException = false;
            if (targetMethod is ILMethod ilm)
            {
                ExecuteNeo(ilm, targetBase, retDstPtr, targetRetRefBase, out unhandledException, calleeRefBase);
                return !unhandledException;
            }
            else if (targetMethod is CLRMethod clrMethod)
            {
                InvokeNeoClrMethod(clrMethod, isNewobj, targetBase, mStack, retDstPtr, targetRetRefBase);
                return true;
            }

            throw new NotImplementedException("Unknown method type in Neo mode.");
        }

        void InvokeNeoClrMethod(CLRMethod clrMethod, bool isNewobj, byte* targetBase, AutoList mStack, byte* retDstPtr, int targetRetRefBase)
        {
            var redirectNeo = clrMethod.RedirectionNeo;
            if (redirectNeo != null)
            {
                redirectNeo(this, targetBase, mStack, clrMethod, isNewobj, retDstPtr, targetRetRefBase);
                return;
            }

            object res = clrMethod.Invoke(targetBase, mStack, isNewobj);

            if (isNewobj || retDstPtr == null)
                return;

            IType retType = clrMethod.ReturnType;
            if (retType == null || retType == AppDomain.VoidType)
                return;

            if (retType.TypeForCLR.IsPrimitive || retType.TypeForCLR.IsEnum)
            {
                WriteNeoPrimitive(retDstPtr, retType, res);
            }
            else if (retType.IsValueType)
            {
                throw new NotImplementedException("CLR value type return in reflection fallback: Step 13");
            }
            else
            {
                // targetRetRefBase is always a caller-preallocated slot; the >=Count
                // branch is a defensive relic that should never fire in practice.
                if (targetRetRefBase >= mStack.Count)
                    mStack.Add(res);
                else
                    mStack[targetRetRefBase] = res;

                *(int*)retDstPtr = targetRetRefBase;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static object ReadNeoCallThis(OpCodeR* ip, byte* targetBase, AutoList mStack)
        {
            int thisArgOffset = (int)((uint)ip->Operand4 >> 16);
            int thisIdx = *(int*)(targetBase + thisArgOffset);
            if (thisIdx < 0 || thisIdx >= mStack.Count)
                throw new NullReferenceException("Neo callvirt this is null.");

            object thisObj = mStack[thisIdx];
            if (thisObj == null)
                throw new NullReferenceException("Neo callvirt this is null.");

            return thisObj;
        }

        static IMethod ResolveNeoCallvirtILTarget(OpCodeR* ip, IMethod declaredMethod, byte* targetBase, AutoList mStack)
        {
            object thisObj = ReadNeoCallThis(ip, targetBase, mStack);
            if (thisObj is ILTypeInstance instance)
            {
                int slot = ip->Operand4 & 0xffff;
                if (slot == 0xffff)
                {
                    if (!instance.Type.TryGetNeoVTableSlot(declaredMethod, out slot))
                        throw new MissingMethodException(string.Format("Neo callvirt cannot resolve VTable slot for {0} on {1}.", declaredMethod, instance.Type.FullName));
                }

                var vtable = instance.Type.NeoVTable;
                if (slot < 0 || slot >= vtable.Length)
                    throw new MissingMethodException(string.Format("Neo callvirt VTable slot {0} is missing on {1}.", slot, instance.Type.FullName));

                IMethod actual = vtable[slot];
                if (actual == null)
                    throw new MissingMethodException(string.Format("Neo callvirt VTable slot {0} is null on {1}.", slot, instance.Type.FullName));

                return actual;
            }

            throw new InvalidOperationException(string.Format("Neo Callvirt_IL requires ILTypeInstance this, got {0}.", thisObj.GetType().FullName));
        }

        static CLRMethod ResolveNeoCallvirtCLRTarget(OpCodeR* ip, IMethod declaredMethod, byte* targetBase, AutoList mStack)
        {
            ReadNeoCallThis(ip, targetBase, mStack);
            if (declaredMethod is CLRMethod clrMethod)
                return clrMethod;

            throw new InvalidOperationException(string.Format("Neo Callvirt_CLR requires CLRMethod, got {0}.", declaredMethod));
        }

        static IMethod ResolveNeoGenericCallvirtTarget(OpCodeR* ip, IMethod declaredMethod, byte* targetBase, AutoList mStack)
        {
            object thisObj = ReadNeoCallThis(ip, targetBase, mStack);
            if (thisObj is ILTypeInstance instance)
            {
                // 接口方法快路径：Operand4 低 16 = 接口内 slot；DeclearingType 提供接口 identity
                if (declaredMethod.DeclearingType is ILType declaringIL && declaringIL.IsInterface)
                {
                    if (!instance.Type.TryGetNeoInterfaceOffset(declaringIL, out int baseSlot))
                        throw new MissingMethodException(string.Format(
                            "Neo callvirt: type {0} does not implement interface {1}.",
                            instance.Type.FullName, declaringIL.FullName));

                    int methodSlot = ip->Operand4 & 0xffff;
                    if (methodSlot == 0xffff)
                    {
                        // 兜底：lowering 期没查到 slot（不应发生），运行时补查
                        if (!declaringIL.TryGetInterfaceMethodSlot(declaredMethod, out methodSlot))
                            throw new MissingMethodException(string.Format(
                                "Neo callvirt: interface {0} does not contain method {1}.",
                                declaringIL.FullName, declaredMethod));
                    }

                    int actualSlot = baseSlot + methodSlot;
                    var vtable = instance.Type.NeoVTable;
                    if (actualSlot < 0 || actualSlot >= vtable.Length)
                        throw new MissingMethodException(string.Format(
                            "Neo callvirt: interface slot {0} out of range on {1}.",
                            actualSlot, instance.Type.FullName));
                    IMethod actual = vtable[actualSlot];
                    if (actual == null)
                        throw new MissingMethodException(string.Format(
                            "Neo callvirt: interface slot {0} is null on {1}.",
                            actualSlot, instance.Type.FullName));
                    return actual;
                }

                return ResolveNeoCallvirtILTarget(ip, declaredMethod, targetBase, mStack);
            }
            if (declaredMethod is CLRMethod)
                return declaredMethod;

            throw new InvalidOperationException(string.Format("Neo generic callvirt cannot dispatch non-IL object {0} to {1}.", thisObj.GetType().FullName, declaredMethod));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ref byte ResolveNeoFrameTarget(byte* stackBase, int offset)
        {
            // FRAME_REF: 帧内非托管内存，直接指针读写。
            return ref *(byte*)(stackBase + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ref byte ResolveNeoILTarget(ILTypeInstance instance, int offset)
        {
            // IL 堆对象：每次重取 Primitives managed ref（GC-safe）。
#if DEBUG
            if (instance.Primitives == null || offset < 0 || offset >= instance.Primitives.Length)
                throw new IndexOutOfRangeException("Neo indirect IL field offset is out of range.");
#endif
            return ref Unsafe.Add(
                ref MemoryMarshal.GetReference(instance.Primitives.AsSpan()), offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        CLRType ResolveNeoClrOwnerType(object owner)
        {
            var t = AppDomain.GetType(owner.GetType()) as CLRType;
#if DEBUG
            if (t == null)
                throw new TypeLoadException("Neo indirect CLR owner type could not be resolved.");
#endif
            return t;
        }

        // Ldfld/Stfld receiver 三态解析（design §2.5 / §15，Step 12b）。
        //   receiverKind >  0  → same-frame inline direct，值 = struct.RefOffset + 1
        //                        primitive 目标 = frame slot(SrcOffset/DstOffset) + fieldPrimitiveOffset
        //                        ref 目标 = mStack[frameRefBase + (receiverKind-1) + field.ReferenceOffset]
        //                        （caller 直接用 receiverKind - 1 作为 structRefOffset）
        //   receiverKind == 0  → heap mStack index，receiverSlot 存 index；receiver 可能是
        //                        ILTypeInstance 或 CLR 对象；无 struct.RefOffset 概念。
        //   receiverKind <  0  → Ref-Slot receiver（8-byte Ref Slot），值 = -1 - structRefOffset
        //                        receiverSlot 存 (objectIndex, byteOffset)：
        //                          objectIndex == -1 → FRAME_REF，primitive 目标 = stackBase+offset+fieldPrimOff
        //                                              ref 目标 = mStack[frameRefBase + structRefOffset
        //                                                                 + field.ReferenceOffset]
        //                          objectIndex >=  0 → 与 heap 相同（IL / CLR 两分派）
        // NOTE: 该方法只做 receiver 识别；primitive/ref field 目标构造由 caller 在 case 内完成。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ResolveNeoFieldReceiver(
            byte* receiverSlot, int receiverKind, AutoList mStack,
            out int objectIndex, out int baseOffset,
            out object owner, out ILTypeInstance ilOwner, out CLRType ownerType)
        {
            objectIndex = 0;
            baseOffset = 0;
            owner = null;
            ilOwner = null;
            ownerType = null;
            if (receiverKind > 0)
            {
                // inline direct：caller 通过 receiverKind-1 拿到 struct.RefOffset，
                // primitive 直接用 SrcOffset/DstOffset；此方法不需要额外工作。
                return;
            }
            if (receiverKind < 0)
            {
                objectIndex = *(int*)receiverSlot;
                baseOffset = *(int*)(receiverSlot + 4);
                if (objectIndex == -1)
                    return;
            }
            else
            {
                objectIndex = *(int*)receiverSlot;
            }
#if DEBUG
            if (objectIndex < 0 || objectIndex >= mStack.Count)
                throw new NullReferenceException("Neo field receiver is null.");
#endif
            owner = mStack[objectIndex];
#if DEBUG
            if (owner == null)
                throw new NullReferenceException("Neo field receiver is null.");
#endif
            ilOwner = owner as ILTypeInstance;
            if (ilOwner != null)
                return;
            ownerType = ResolveNeoClrOwnerType(owner);
        }

        // Ldind/Stind receiver 分派状态：
        //   objectIndex == -1 && ilOwner == null    → FRAME_REF, target 通过 stackBase+offset 直接构造
        //   objectIndex >=  0 && ilOwner != null    → IL 堆对象, target 通过 ilOwner.Primitives+offset 构造
        //   objectIndex >=  0 && ownerType != null  → CLR 对象, 走 GetFieldValue / SetFieldValue(offset)
        //   otherwise (Array)                       → Step 16
        // NOTE: 该方法只解析 receiver，不返回 primitive target；caller 拿到 objectIndex/ilOwner/ownerType 后
        //       在 case 内根据具体类型再构造 ref byte，避免跨方法边界携带 ref。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ResolveNeoIndirectReceiver(
            byte* refSlot, AutoList mStack,
            out int objectIndex, out int offset,
            out object owner, out ILTypeInstance ilOwner, out CLRType ownerType)
        {
            objectIndex = *(int*)refSlot;
            offset = *(int*)(refSlot + 4);
            owner = null;
            ilOwner = null;
            ownerType = null;
            if (objectIndex == -1)
                return;
#if DEBUG
            if (objectIndex < 0 || objectIndex >= mStack.Count)
                throw new NullReferenceException("Neo indirect reference is null.");
#endif
            owner = mStack[objectIndex];
#if DEBUG
            if (owner == null)
                throw new NullReferenceException("Neo indirect reference is null.");
#endif
            ilOwner = owner as ILTypeInstance;
            if (ilOwner != null)
                return;
            if (owner is Array)
                throw new NotImplementedException("Neo indirect array access: Step 16");
            ownerType = ResolveNeoClrOwnerType(owner);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void LoadNeoStaticPrimitive(ref byte dst, ref byte src, NeoStaticFieldKind kind)
        {
            switch (kind)
            {
                case NeoStaticFieldKind.I1: Unsafe.WriteUnaligned(ref dst, (int)Unsafe.ReadUnaligned<sbyte>(ref src)); break;
                case NeoStaticFieldKind.U1:
                case NeoStaticFieldKind.Boolean: Unsafe.WriteUnaligned(ref dst, (int)Unsafe.ReadUnaligned<byte>(ref src)); break;
                case NeoStaticFieldKind.I2: Unsafe.WriteUnaligned(ref dst, (int)Unsafe.ReadUnaligned<short>(ref src)); break;
                case NeoStaticFieldKind.U2: Unsafe.WriteUnaligned(ref dst, (int)Unsafe.ReadUnaligned<ushort>(ref src)); break;
                case NeoStaticFieldKind.I4: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<int>(ref src)); break;
                case NeoStaticFieldKind.U4: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<uint>(ref src)); break;
                case NeoStaticFieldKind.I8: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<long>(ref src)); break;
                case NeoStaticFieldKind.U8: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<ulong>(ref src)); break;
                case NeoStaticFieldKind.R4: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<float>(ref src)); break;
                case NeoStaticFieldKind.R8: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<double>(ref src)); break;
                default: throw new InvalidOperationException("Neo static field is not primitive.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void StoreNeoStaticPrimitive(ref byte dst, ref byte src, NeoStaticFieldKind kind)
        {
            switch (kind)
            {
                case NeoStaticFieldKind.I1:
                case NeoStaticFieldKind.U1:
                case NeoStaticFieldKind.Boolean: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<byte>(ref src)); break;
                case NeoStaticFieldKind.I2:
                case NeoStaticFieldKind.U2: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<ushort>(ref src)); break;
                case NeoStaticFieldKind.I4:
                case NeoStaticFieldKind.U4: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<uint>(ref src)); break;
                case NeoStaticFieldKind.I8:
                case NeoStaticFieldKind.U8: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<ulong>(ref src)); break;
                case NeoStaticFieldKind.R4: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<float>(ref src)); break;
                case NeoStaticFieldKind.R8: Unsafe.WriteUnaligned(ref dst, Unsafe.ReadUnaligned<double>(ref src)); break;
                default: throw new InvalidOperationException("Neo static field is not primitive.");
            }
        }

        internal unsafe byte* ExecuteNeo(ILMethod method, byte* esp, byte* retDst, int retRefBase, out bool unhandledException, int preAllocatedRefBase = -1)
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

            OpCodeR[] body = method.CompiledFrame.NeoExecuteBody;
            AutoList mStack = stack.ManagedStack;
            ref readonly var nf = ref method.CompiledFrame;
            int frameSize = nf.TotalStructSize;
            int totalRefSize = nf.TotalRefSize;
            int returnPrimitiveSize = nf.ReturnPrimitiveSize;
            int returnRefCount = nf.ReturnRefCount;
            var localInfos = nf.LocalInfos;
            var localIsRef = nf.LocalIsReference;

            byte* frameBase = esp;
            byte* newEsp = esp + frameSize;
            // Absolute base of the runtime stack's unmanaged memory. Ref Slots with
            // objectIndex == -1 (FRAME_REF) store an absolute byte offset relative to
            // this base, so cross-frame managed pointers resolve as stackBase + offset
            // regardless of the callee frameBase (design §2.5 / §15.2).
            byte* stackBase = (byte*)stack.StackBase;
            // TODO: stack overflow check vs stack.StackBase upper bound; will be added in Step 14 / 26

            // Zero locals primitive region
            if (nf.LocalsPrimitiveSize > 0)
                Unsafe.InitBlock(frameBase + nf.ParamPrimitiveSize, 0, (uint)nf.LocalsPrimitiveSize);
            if (localInfos != null && localIsRef != null)
            {
                for (int i = 0; i < localInfos.Length; i++)
                {
                    if (localIsRef[i] && localInfos[i].Offset >= nf.ParamPrimitiveSize)
                    {
                        *(int*)(frameBase + localInfos[i].Offset) = -1;
                    }
                }
            }

            // Managed stack reservation for this frame's reference slots
            int frameRefBase;
            if (preAllocatedRefBase >= 0)
            {
                frameRefBase = preAllocatedRefBase;
            }
            else
            {
                frameRefBase = mStack.Count;
                mStack.ExpandBySize(totalRefSize);
            }

            // Frames stack placeholder: keep existing StackFrame plumbing alive.
            // BasePointer is interpreted as byte* via reinterpret cast; full debugger
            // adaptation is deferred to Step 14/26.
            StackFrame frame = new StackFrame();
            frame.LocalVarPointer = (StackObject*)frameBase;
            frame.BasePointer = (StackObject*)frameBase;
            frame.Method = method;
            frame.IsRegister = true;
            frame.ManagedStackBase = frameRefBase;
            frame.ValueTypeBasePointer = stack.ValueTypeStackPointer;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
            frame.Address = new IntegerReference();
#endif
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
                ILTypeInstance fieldIns;
                CLRType clrType;
                object obj = null;
                int sz, refCnt, srcIdx, dstIdx, srcRefOffset, dstRefOffset;
                // Ref Slot (managed pointer) working locals, shared across all
                // ldloca/ldarga/ldflda/ldsflda/ldind/stind and Ref-Slot-receiver
                // ldfld/stfld cases. Declared here (not per-case) for iOS/non-O3
                // stack safety (spec core principle 1).
                int objIndex = -1;
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
                            case OpCodeREnum.Ldloca:
                            case OpCodeREnum.Ldloca_S:
                            case OpCodeREnum.Ldarga:
                            case OpCodeREnum.Ldarga_S:
                                *(int*)(frameBase + ip->DstOffset) = -1;
                                *(int*)(frameBase + ip->DstOffset + 4) =
                                    (int)(frameBase - stackBase) + ip->SrcOffset;
                                break;
                            case OpCodeREnum.Ldflda:
                                t = AppDomain.GetType(ip->Operand);
#if DEBUG
                                if (t == null)
                                    throw new TypeLoadException("Neo Ldflda field type could not be resolved.");
#endif
                                if (!t.IsPrimitive && !t.IsValueType)
                                    throw new NotImplementedException("Neo Ldflda for reference fields requires the objectIndex=-2 marker (Step 17).");
                                if (ip->Operand4 != 0)
                                {
                                    // Ref-Slot receiver：延续原 (objIndex, offset) 累加 field primitive offset。
                                    objIndex = *(int*)(frameBase + ip->SrcOffset);
                                    dstIdx = *(int*)(frameBase + ip->SrcOffset + 4);
                                    if (objIndex == -1)
                                    {
                                        *(int*)(frameBase + ip->DstOffset) = -1;
                                        *(int*)(frameBase + ip->DstOffset + 4) = dstIdx + ip->Operand2;
                                        break;
                                    }
#if DEBUG
                                    if (objIndex < 0 || objIndex >= mStack.Count)
                                        throw new NullReferenceException("Neo Ldflda receiver is null.");
#endif
                                    obj = mStack[objIndex];
                                    if (!(obj is ILTypeInstance))
                                        throw new NotImplementedException("Neo nested Ldflda through a CLR field Ref Slot: Step 17");
                                    *(int*)(frameBase + ip->DstOffset) = objIndex;
                                    *(int*)(frameBase + ip->DstOffset + 4) = dstIdx + ip->Operand2;
                                }
                                else
                                {
                                    // heap receiver：SrcOffset 保存 mStack index。
                                    objIndex = *(int*)(frameBase + ip->SrcOffset);
#if DEBUG
                                    if (objIndex < 0 || objIndex >= mStack.Count)
                                        throw new NullReferenceException("Neo Ldflda receiver is null.");
#endif
                                    *(int*)(frameBase + ip->DstOffset) = objIndex;
                                    *(int*)(frameBase + ip->DstOffset + 4) = ip->Operand2;
                                }
                                break;
                            case OpCodeREnum.Ldsfld:
                                t = AppDomain.GetType(ip->Operand);
                                ilType = AppDomain.GetType(ip->Operand2) as ILType;
                                if (ilType == null)
                                    throw new NotImplementedException("Neo Ldsfld for CLR static fields: Step 12b CLR follow-up.");
                                ins = ilType.StaticInstance;
#if DEBUG
                                if (ins == null)
                                    throw new MissingFieldException(ilType.FullName, "static field");
#endif
                                sz = ip->Operand4 & 0xFF;
                                srcRefOffset = (ip->Operand3 >> 16) & 0xFFFF;
                                srcIdx = ip->Operand3 & 0xFFFF;
                                if (sz == (int)NeoStaticFieldKind.Reference)
                                {
                                    obj = ins.ManagedObjects[srcRefOffset];
                                    dstIdx = frameRefBase + (ushort)ip->Register2;
                                    mStack[dstIdx] = obj;
                                    *(int*)(frameBase + ip->DstOffset) = obj != null ? dstIdx : -1;
                                }
                                else if (sz == (int)NeoStaticFieldKind.Value)
                                {
                                    refCnt = (ip->Operand4 >> 8) & 0xFF;
                                    sz = (int)((uint)ip->Operand4 >> 16);
                                    if (sz > 0)
                                    {
                                        Unsafe.CopyBlockUnaligned(
                                            ref *(byte*)(frameBase + ip->DstOffset),
                                            ref ResolveNeoILTarget(ins, srcIdx),
                                            (uint)sz);
                                    }
                                    dstIdx = frameRefBase + (ushort)ip->Register2;
                                    for (srcIdx = 0; srcIdx < refCnt; srcIdx++)
                                        mStack[dstIdx + srcIdx] = ins.ManagedObjects[srcRefOffset + srcIdx];
                                }
                                else
                                {
                                    LoadNeoStaticPrimitive(
                                        ref *(byte*)(frameBase + ip->DstOffset),
                                        ref ResolveNeoILTarget(ins, srcIdx),
                                        (NeoStaticFieldKind)sz);
                                }
                                break;
                            case OpCodeREnum.Stsfld:
                                t = AppDomain.GetType(ip->Operand);
                                ilType = AppDomain.GetType(ip->Operand2) as ILType;
                                if (ilType == null)
                                    throw new NotImplementedException("Neo Stsfld for CLR static fields: Step 12b CLR follow-up.");
                                ins = ilType.StaticInstance;
#if DEBUG
                                if (ins == null)
                                    throw new MissingFieldException(ilType.FullName, "static field");
#endif
                                sz = ip->Operand4 & 0xFF;
                                dstRefOffset = (ip->Operand3 >> 16) & 0xFFFF;
                                dstIdx = ip->Operand3 & 0xFFFF;
                                if (sz == (int)NeoStaticFieldKind.Reference)
                                {
                                    srcIdx = *(int*)(frameBase + ip->SrcOffset);
                                    ins.ManagedObjects[dstRefOffset] = srcIdx >= 0 ? mStack[srcIdx] : null;
                                }
                                else if (sz == (int)NeoStaticFieldKind.Value)
                                {
                                    refCnt = (ip->Operand4 >> 8) & 0xFF;
                                    sz = (int)((uint)ip->Operand4 >> 16);
                                    if (sz > 0)
                                    {
                                        Unsafe.CopyBlockUnaligned(
                                            ref ResolveNeoILTarget(ins, dstIdx),
                                            ref *(byte*)(frameBase + ip->SrcOffset),
                                            (uint)sz);
                                    }
                                    srcIdx = frameRefBase + (ushort)ip->Register2;
                                    for (dstIdx = 0; dstIdx < refCnt; dstIdx++)
                                        ins.ManagedObjects[dstRefOffset + dstIdx] = mStack[srcIdx + dstIdx];
                                }
                                else
                                {
                                    StoreNeoStaticPrimitive(
                                        ref ResolveNeoILTarget(ins, dstIdx),
                                        ref *(byte*)(frameBase + ip->SrcOffset),
                                        (NeoStaticFieldKind)sz);
                                }
                                break;
                            case OpCodeREnum.Ldsflda:
                                t = AppDomain.GetType(ip->Operand);
#if DEBUG
                                if (t == null)
                                    throw new TypeLoadException("Neo Ldsflda field type could not be resolved.");
#endif
                                if (!t.IsPrimitive && !t.IsValueType)
                                    throw new NotImplementedException("Neo Ldsflda for reference fields requires the objectIndex=-2 marker (Step 17).");
                                ilType = AppDomain.GetType(ip->Operand3) as ILType;
                                if (ilType == null)
                                    throw new NotImplementedException("Neo Ldsflda for CLR static fields: Step 17");
                                ins = ilType.StaticInstance;
#if DEBUG
                                if (ins == null)
                                    throw new MissingFieldException(ilType.FullName, "static field");
#endif
                                dstIdx = frameRefBase + ip->Operand4;
                                mStack[dstIdx] = ins;
                                *(int*)(frameBase + ip->DstOffset) = dstIdx;
                                *(int*)(frameBase + ip->DstOffset + 4) = ip->Operand2;
                                break;
                            case OpCodeREnum.Ldind_I1:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<sbyte>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<sbyte>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_U1:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<byte>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<byte>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_I2:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<short>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<short>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_U2:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<ushort>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<ushort>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_I:
                            case OpCodeREnum.Ldind_I4:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<int>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(int*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<int>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_U4:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(uint*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<uint>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(uint*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<uint>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_I8:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(long*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<long>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(long*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<long>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_R4:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(float*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<float>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(float*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<float>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_R8:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    *(double*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<double>(ref ResolveNeoFrameTarget(stackBase, dstIdx));
                                else if (fieldIns != null)
                                    *(double*)(frameBase + ip->DstOffset) =
                                        Unsafe.ReadUnaligned<double>(ref ResolveNeoILTarget(fieldIns, dstIdx));
                                else
                                    clrType.CopyFieldToNeoFrame(dstIdx, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                break;
                            case OpCodeREnum.Ldind_Ref:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->SrcOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                {
                                    // FRAME_REF 目标是 primitive slot（一个 mStack index），不携带 IL ref
                                    // 语义。ldind_ref 在此路径下读到的是被解引用变量当前持有的 index，
                                    // 直接搬到消费者 ref-region 即可。
                                    srcIdx = Unsafe.ReadUnaligned<int>(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx));
#if DEBUG
                                    if (srcIdx < -1 || srcIdx >= mStack.Count)
                                        throw new NullReferenceException("Neo indirect reference target is invalid.");
#endif
                                    obj = srcIdx >= 0 ? mStack[srcIdx] : null;
                                }
                                else if (fieldIns != null)
                                {
                                    // IL 堆对象的引用字段无法通过 primitive offset 直达 ManagedObjects
                                    // （design §15.6 需要 objectIndex=-2 marker），归 Step 17。
                                    throw new NotImplementedException(
                                        "Neo Ldind_Ref for IL reference fields requires the objectIndex=-2 marker (Step 17).");
                                }
                                else
                                {
                                    obj = clrType.GetFieldValue(dstIdx, obj);
                                }
                                dstIdx = frameRefBase + ip->Operand;
                                mStack[dstIdx] = obj;
                                *(int*)(frameBase + ip->DstOffset) = obj != null ? dstIdx : -1;
                                break;
                            case OpCodeREnum.Stind_I1:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->DstOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx),
                                        *(sbyte*)(frameBase + ip->SrcOffset));
                                else if (fieldIns != null)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoILTarget(fieldIns, dstIdx),
                                        *(sbyte*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    clrType.AssignFieldFromNeoFrame(dstIdx, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                    mStack[objIndex] = obj;
                                }
                                break;
                            case OpCodeREnum.Stind_I2:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->DstOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx),
                                        *(short*)(frameBase + ip->SrcOffset));
                                else if (fieldIns != null)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoILTarget(fieldIns, dstIdx),
                                        *(short*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    clrType.AssignFieldFromNeoFrame(dstIdx, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                    mStack[objIndex] = obj;
                                }
                                break;
                            case OpCodeREnum.Stind_I:
                            case OpCodeREnum.Stind_I4:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->DstOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx),
                                        *(int*)(frameBase + ip->SrcOffset));
                                else if (fieldIns != null)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoILTarget(fieldIns, dstIdx),
                                        *(int*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    clrType.AssignFieldFromNeoFrame(dstIdx, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                    mStack[objIndex] = obj;
                                }
                                break;
                            case OpCodeREnum.Stind_I8:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->DstOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx),
                                        *(long*)(frameBase + ip->SrcOffset));
                                else if (fieldIns != null)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoILTarget(fieldIns, dstIdx),
                                        *(long*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    clrType.AssignFieldFromNeoFrame(dstIdx, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                    mStack[objIndex] = obj;
                                }
                                break;
                            case OpCodeREnum.Stind_R4:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->DstOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx),
                                        *(float*)(frameBase + ip->SrcOffset));
                                else if (fieldIns != null)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoILTarget(fieldIns, dstIdx),
                                        *(float*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    clrType.AssignFieldFromNeoFrame(dstIdx, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                    mStack[objIndex] = obj;
                                }
                                break;
                            case OpCodeREnum.Stind_R8:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->DstOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                if (objIndex == -1)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx),
                                        *(double*)(frameBase + ip->SrcOffset));
                                else if (fieldIns != null)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoILTarget(fieldIns, dstIdx),
                                        *(double*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    clrType.AssignFieldFromNeoFrame(dstIdx, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                    mStack[objIndex] = obj;
                                }
                                break;
                            case OpCodeREnum.Stind_Ref:
                                ResolveNeoIndirectReceiver(
                                    frameBase + ip->DstOffset, mStack,
                                    out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                srcIdx = *(int*)(frameBase + ip->SrcOffset);
#if DEBUG
                                if (srcIdx < -1 || srcIdx >= mStack.Count)
                                    throw new NullReferenceException("Neo indirect reference value is invalid.");
#endif
                                if (objIndex == -1)
                                    Unsafe.WriteUnaligned(
                                        ref ResolveNeoFrameTarget(stackBase, dstIdx), srcIdx);
                                else if (fieldIns != null)
                                {
                                    throw new NotImplementedException(
                                        "Neo Stind_Ref for IL reference fields requires the objectIndex=-2 marker (Step 17).");
                                }
                                else
                                {
                                    clrType.AssignFieldFromNeoFrame(dstIdx, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                    mStack[objIndex] = obj;
                                }
                                break;
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
                                refCnt = ip->Operand;
                                if (refCnt > 0)
                                {
                                    dstRefOffset = ip->Operand3;
                                    dstIdx = frameRefBase + dstRefOffset;
                                    if (ip->Operand4 < 0)
                                    {
                                        srcIdx = *(int*)(frameBase + ip->SrcOffset);
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
                                    else
                                    {
                                        srcRefOffset = ip->Operand4;
                                        srcIdx = frameRefBase + srcRefOffset;
                                        for (dstRefOffset = 0; dstRefOffset < refCnt; dstRefOffset++)
                                            mStack[dstIdx + dstRefOffset] = mStack[srcIdx + dstRefOffset];
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
                                {
                                    var targetMethod = AppDomain.GetMethod(ip->Operand2);
                                    if (targetMethod == null)
                                    {
                                        ip++;
                                        continue;
                                    }

                                    int callParamIdx = ip->Operand;
                                    ref var map = ref nf.NeoCallParams[callParamIdx];
                                    byte* targetBase = newEsp;
                                    int calleeRefBase = PrepareNeoCallFrame(ref map, mStack, frameBase, targetBase, frameRefBase, targetMethod);

                                    byte* retDstPtr = null;
                                    int targetRetRefBase = -1;

                                    if (ip->Register1 >= 0)
                                    {
                                        retDstPtr = frameBase + ip->DstOffset;
                                        targetRetRefBase = frameRefBase + ip->Operand3;
                                    }

                                    if (!InvokeNeoCallTarget(targetMethod, false, targetBase, mStack, retDstPtr, targetRetRefBase, out unhandledException, calleeRefBase))
                                        return null;

                                    ip++;
                                    continue;
                                }
                            case OpCodeREnum.Newobj:
                                {
                                    var targetMethod = AppDomain.GetMethod(ip->Operand2);
                                    if (targetMethod == null)
                                    {
                                        ip++;
                                        continue;
                                    }

                                    var newobjType = targetMethod.DeclearingType as ILType;
                                    if (newobjType == null)
                                        throw new NotImplementedException("Neo Newobj CLR type is not implemented (Step 9)");
                                    if (newobjType.IsDelegate)
                                        throw new NotImplementedException("Neo Newobj delegate is not implemented");

                                    bool isValueTypeCtor = newobjType.IsValueType && !newobjType.IsEnum;
                                    if (isValueTypeCtor)
                                    {
                                        // Value-type constructors require the destination slot to be
                                        // converted to a Ref Slot before invocation. The old path copied
                                        // the struct bytes into the callee `this`, which violates the
                                        // managed-pointer ABI. Implemented with the full newobj flow in Step 18.
                                        throw new NotImplementedException(
                                            "Neo value-type Newobj Ref Slot this: Step 18");
                                    }

                                    dstRefOffset = ip->Operand3;
                                    int newobjDstIdx = frameRefBase + dstRefOffset;

                                    int callParamIdx = ip->Operand;
                                    ref var map = ref nf.NeoCallParams[callParamIdx];
                                    byte* targetBase = newEsp;

                                    int calleeRefBase = -1;
                                    byte* retDstPtr = null;
                                    int targetRetRefBase = -1;

                                    ins = newobjType.Instantiate(false);
                                    mStack[newobjDstIdx] = ins;
                                    *(int*)(frameBase + ip->DstOffset) = newobjDstIdx;

                                    *(int*)targetBase = newobjDstIdx;
                                    // Reference-type newobj: this is the freshly instantiated object,
                                    // not a caller register, so the JIT-emitted NeoCallParamMap does
                                    // not describe it. Seed callee r0 explicitly before CopyRefs
                                    // fills in the actual argument refs.
                                    CopyNeoCallArguments(ref map, frameBase, targetBase);
                                    if (targetMethod is ILMethod ilmNewobj)
                                    {
                                        calleeRefBase = mStack.Count;
                                        mStack.ExpandBySize(ilmNewobj.CompiledFrame.TotalRefSize);
                                        mStack[calleeRefBase] = mStack[newobjDstIdx];
                                        CopyNeoCallRefs(ref map, mStack, frameRefBase, calleeRefBase);
                                    }

                                    if (!InvokeNeoCallTarget(targetMethod, true, targetBase, mStack, retDstPtr, targetRetRefBase, out unhandledException, calleeRefBase))
                                        return null;

                                    ip++;
                                    continue;
                                }
                            case OpCodeREnum.Callvirt_IL:
                                {
                                    var targetMethod = AppDomain.GetMethod(ip->Operand2);
                                    if (targetMethod == null)
                                    {
                                        ip++;
                                        continue;
                                    }

                                    int callParamIdx = ip->Operand;
                                    ref var map = ref nf.NeoCallParams[callParamIdx];
                                    byte* targetBase = newEsp;
                                    CopyNeoCallArguments(ref map, frameBase, targetBase);

                                    byte* retDstPtr = ip->Register1 >= 0 ? frameBase + ip->DstOffset : null;
                                    int targetRetRefBase = ip->Register1 >= 0 ? frameRefBase + ip->Operand3 : -1;
                                    IMethod actualMethod = ResolveNeoCallvirtILTarget(ip, targetMethod, targetBase, mStack);

                                    // Ref allocation is deferred until after virtual dispatch since
                                    // the actual callee's TotalRefSize can differ from targetMethod's.
                                    int calleeRefBase = -1;
                                    if (actualMethod is ILMethod ilmCallvirt)
                                    {
                                        calleeRefBase = mStack.Count;
                                        mStack.ExpandBySize(ilmCallvirt.CompiledFrame.TotalRefSize);
                                        CopyNeoCallRefs(ref map, mStack, frameRefBase, calleeRefBase);
                                    }

                                    if (!InvokeNeoCallTarget(actualMethod, false, targetBase, mStack, retDstPtr, targetRetRefBase, out unhandledException, calleeRefBase))
                                        return null;

                                    ip++;
                                    continue;
                                }
                            case OpCodeREnum.Callvirt_CLR:
                                {
                                    var targetMethod = AppDomain.GetMethod(ip->Operand2);
                                    if (targetMethod == null)
                                    {
                                        ip++;
                                        continue;
                                    }

                                    int callParamIdx = ip->Operand;
                                    ref var map = ref nf.NeoCallParams[callParamIdx];
                                    byte* targetBase = newEsp;
                                    CopyNeoCallArguments(ref map, frameBase, targetBase);

                                    byte* retDstPtr = ip->Register1 >= 0 ? frameBase + ip->DstOffset : null;
                                    int targetRetRefBase = ip->Register1 >= 0 ? frameRefBase + ip->Operand3 : -1;
                                    CLRMethod clrMethod = ResolveNeoCallvirtCLRTarget(ip, targetMethod, targetBase, mStack);
                                    InvokeNeoClrMethod(clrMethod, false, targetBase, mStack, retDstPtr, targetRetRefBase);

                                    ip++;
                                    continue;
                                }
                            case OpCodeREnum.Callvirt:
                                {
                                    var targetMethod = AppDomain.GetMethod(ip->Operand2);
                                    if (targetMethod == null)
                                    {
                                        ip++;
                                        continue;
                                    }

                                    int callParamIdx = ip->Operand;
                                    ref var map = ref nf.NeoCallParams[callParamIdx];
                                    byte* targetBase = newEsp;
                                    CopyNeoCallArguments(ref map, frameBase, targetBase);

                                    byte* retDstPtr = ip->Register1 >= 0 ? frameBase + ip->DstOffset : null;
                                    int targetRetRefBase = ip->Register1 >= 0 ? frameRefBase + ip->Operand3 : -1;
                                    IMethod actualMethod = ResolveNeoGenericCallvirtTarget(ip, targetMethod, targetBase, mStack);

                                    // Ref allocation is deferred until after virtual dispatch since
                                    // the actual callee's TotalRefSize can differ from targetMethod's.
                                    int calleeRefBase = -1;
                                    if (actualMethod is ILMethod ilmGeneric)
                                    {
                                        calleeRefBase = mStack.Count;
                                        mStack.ExpandBySize(ilmGeneric.CompiledFrame.TotalRefSize);
                                        CopyNeoCallRefs(ref map, mStack, frameRefBase, calleeRefBase);
                                    }

                                    if (!InvokeNeoCallTarget(actualMethod, false, targetBase, mStack, retDstPtr, targetRetRefBase, out unhandledException, calleeRefBase))
                                        return null;

                                    ip++;
                                    continue;
                                }
                            case OpCodeREnum.Ret:
                                if (retDst != null && (returnPrimitiveSize > 0 || returnRefCount > 0))
                                {
                                    if (returnPrimitiveSize > 0)
                                        Unsafe.CopyBlock(retDst, frameBase + ip->DstOffset, (uint)returnPrimitiveSize);
                                    if (returnRefCount > 0)
                                    {
                                        IType returnType = method.ReturnType;
                                        bool isSingleReferenceReturn = returnType != null &&
                                            !returnType.IsPrimitive &&
                                            !returnType.IsValueType &&
                                            returnPrimitiveSize == 4 &&
                                            returnRefCount == 1;
                                        if (isSingleReferenceReturn)
                                        {
                                            int retSrcIdx = *(int*)(frameBase + ip->DstOffset);
                                            if (retSrcIdx >= 0)
                                            {
                                                mStack[retRefBase] = mStack[retSrcIdx];
                                                *(int*)retDst = retRefBase;
                                            }
                                            else
                                            {
                                                mStack[retRefBase] = null;
                                                *(int*)retDst = -1;
                                            }
                                        }
                                        else
                                        {
                                            srcRefOffset = ip->Operand3;
                                            srcIdx = frameRefBase + srcRefOffset;
                                            for (dstRefOffset = 0; dstRefOffset < returnRefCount; dstRefOffset++)
                                                mStack[retRefBase + dstRefOffset] = mStack[srcIdx + dstRefOffset];
                                        }
                                    }
                                }
                                mStack.RemoveRange(frameRefBase, mStack.Count - frameRefBase);
                                returned = true;
                                continue;
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
                                    if (ip->Operand4 > 0)
                                    {
                                        // same-frame inline direct: Operand4 - 1 = struct.RefOffset
                                        if (sz > 0)
                                            Unsafe.InitBlock(frameBase + ip->DstOffset, 0, (uint)sz);
                                        dstRefOffset = ip->Operand3;
                                        for (int i = 0; i < refCnt; i++)
                                            mStack[frameRefBase + dstRefOffset + i] = null;
                                    }
                                    else if (ip->Operand4 < 0)
                                    {
                                        // Ref-Slot receiver: -1 - structRefOffset
                                        dstRefOffset = -1 - ip->Operand4;
                                        objIndex = *(int*)(frameBase + ip->DstOffset);
                                        dstIdx = *(int*)(frameBase + ip->DstOffset + 4);
                                        if (objIndex == -1)
                                        {
                                            if (sz > 0)
                                                Unsafe.InitBlockUnaligned(
                                                    ref ResolveNeoFrameTarget(stackBase, dstIdx),
                                                    0, (uint)sz);
                                            for (int i = 0; i < refCnt; i++)
                                                mStack[frameRefBase + dstRefOffset + i] = null;
                                        }
                                        else
                                        {
#if DEBUG
                                            if (objIndex < 0 || objIndex >= mStack.Count)
                                                throw new NullReferenceException("Neo Initobj receiver is null.");
#endif
                                            obj = mStack[objIndex];
                                            if (obj is ILTypeInstance ilOwner)
                                            {
                                                if (sz > 0)
                                                    Unsafe.InitBlockUnaligned(
                                                        ref ResolveNeoILTarget(ilOwner, dstIdx),
                                                        0, (uint)sz);
                                                for (int i = 0; i < refCnt; i++)
                                                    ilOwner.ManagedObjects[ip->Operand3 + i] = null;
                                            }
                                            else
                                                throw new NotImplementedException(
                                                    "Neo Initobj through a CLR Ref Slot: Step 13/17");
                                        }
                                    }
                                    else
                                    {
                                        throw new NotImplementedException("Initobj boxed: Step 13");
                                    }
                                }
                                else
                                {
                                    // TODO Step 13: CLR value type Initobj (with/without ValueTypeBinder)
                                    throw new NotImplementedException("CLR value type Initobj: Step 13");
                                }
                                break;
                            case OpCodeREnum.Box:
                                dstRefOffset = ip->Operand3;
                                srcRefOffset = (short)ip->Operand4;
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
                            case OpCodeREnum.Ldfld:
                            case OpCodeREnum.Stfld:
                                // CLR value-type fields deliberately stay as the generic opcode
                                // during JIT prewarm. Fail only if execution reaches the unsupported
                                // runtime path; primitive/reference fields are specialized above.
                                throw new NotImplementedException(
                                    "Neo CLR value-type field access: Step 13/17");
                            case OpCodeREnum.Ldfld_I1:
                                if (ip->Operand4 > 0)
                                    *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<sbyte>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<sbyte>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<sbyte>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                    {
                                        obj = clrType.GetFieldValue(ip->Operand2, obj);
                                        *(int*)(frameBase + ip->DstOffset) =
                                            obj is bool ? ((bool)obj ? 1 : 0) : Convert.ToSByte(obj);
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldfld_U1:
                                if (ip->Operand4 > 0)
                                    *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<byte>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<byte>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<byte>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_I2:
                                if (ip->Operand4 > 0)
                                    *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<short>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<short>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<short>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_U2:
                                if (ip->Operand4 > 0)
                                    *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ushort>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ushort>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ushort>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_I4:
                                if (ip->Operand4 > 0)
                                    *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<int>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<int>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(int*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<int>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_U4:
                                if (ip->Operand4 > 0)
                                    *(uint*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<uint>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(uint*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<uint>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(uint*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<uint>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_I8:
                                if (ip->Operand4 > 0)
                                    *(long*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<long>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(long*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<long>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(long*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<long>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_U8:
                                if (ip->Operand4 > 0)
                                    *(ulong*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ulong>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(ulong*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ulong>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(ulong*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<ulong>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_R4:
                                if (ip->Operand4 > 0)
                                    *(float*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<float>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(float*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<float>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(float*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<float>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_R8:
                                if (ip->Operand4 > 0)
                                    *(double*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<double>(
                                        ref *(byte*)(frameBase + ip->SrcOffset + ip->Operand2));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        *(double*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<double>(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2));
                                    else if (fieldIns != null)
                                        *(double*)(frameBase + ip->DstOffset) = Unsafe.ReadUnaligned<double>(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2));
                                    else
                                        clrType.CopyFieldToNeoFrame(ip->Operand2, obj, this, frameBase + ip->DstOffset, 0, mStack);
                                }
                                break;
                            case OpCodeREnum.Ldfld_Ref:
                                if (ip->Operand4 > 0)
                                {
                                    // inline direct: struct.RefOffset + 1
                                    srcIdx = frameRefBase + (ip->Operand4 - 1) + ip->Operand3;
                                    obj = mStack[srcIdx];
                                }
                                else if (ip->Operand4 < 0)
                                {
                                    // Ref-Slot receiver: -1 - structRefOffset
                                    srcRefOffset = -1 - ip->Operand4;
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                    {
                                        // FRAME_REF：帧内 struct 的引用字段位于 mStack 的定位 ref 区。
                                        srcIdx = frameRefBase + srcRefOffset + ip->Operand3;
                                        obj = mStack[srcIdx];
                                    }
                                    else if (fieldIns != null)
                                    {
                                        throw new NotImplementedException(
                                            "Neo Ldfld_Ref through an IL Ref Slot: Step 17");
                                    }
                                    else
                                    {
                                        obj = clrType.GetFieldValue(ip->Operand2, obj);
                                    }
                                }
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, 0, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (fieldIns != null)
                                        obj = fieldIns.ManagedObjects[ip->Operand3];
                                    else
                                        obj = clrType.GetFieldValue(ip->Operand2, obj);
                                }
                                dstIdx = frameRefBase + ip->Operand;
                                mStack[dstIdx] = obj;
                                *(int*)(frameBase + ip->DstOffset) = obj != null ? dstIdx : -1;
                                break;
                            case OpCodeREnum.Stfld_I1:
                            case OpCodeREnum.Stfld_U1:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(byte*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(byte*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(byte*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_I2:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(short*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(short*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(short*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_U2:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(ushort*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(ushort*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(ushort*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_I4:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(int*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(int*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(int*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_U4:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(uint*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(uint*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(uint*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_I8:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(long*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(long*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(long*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_U8:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(ulong*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(ulong*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(ulong*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_R4:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(float*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(float*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(float*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_R8:
                                if (ip->Operand4 > 0)
                                    Unsafe.WriteUnaligned(
                                        ref *(byte*)(frameBase + ip->DstOffset + ip->Operand2),
                                        *(double*)(frameBase + ip->SrcOffset));
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoFrameTarget(stackBase, dstIdx + ip->Operand2),
                                            *(double*)(frameBase + ip->SrcOffset));
                                    else if (fieldIns != null)
                                        Unsafe.WriteUnaligned(
                                            ref ResolveNeoILTarget(fieldIns, ip->Operand2),
                                            *(double*)(frameBase + ip->SrcOffset));
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Stfld_Ref:
                                srcIdx = *(int*)(frameBase + ip->SrcOffset);
                                if (ip->Operand4 > 0)
                                {
                                    // inline direct: struct.RefOffset + 1
                                    dstIdx = frameRefBase + (ip->Operand4 - 1) + ip->Operand3;
                                    mStack[dstIdx] = srcIdx >= 0 ? mStack[srcIdx] : null;
                                }
                                else if (ip->Operand4 < 0)
                                {
                                    // Ref-Slot receiver: -1 - structRefOffset
                                    dstRefOffset = -1 - ip->Operand4;
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                    {
                                        dstIdx = frameRefBase + dstRefOffset + ip->Operand3;
                                        mStack[dstIdx] = srcIdx >= 0 ? mStack[srcIdx] : null;
                                    }
                                    else if (fieldIns != null)
                                    {
                                        throw new NotImplementedException(
                                            "Neo Stfld_Ref through an IL Ref Slot: Step 17");
                                    }
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                else
                                {
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, 0, mStack,
                                        out objIndex, out dstIdx, out obj, out fieldIns, out clrType);
                                    if (fieldIns != null)
                                        fieldIns.ManagedObjects[ip->Operand3] =
                                            srcIdx >= 0 ? mStack[srcIdx] : null;
                                    else
                                    {
                                        clrType.AssignFieldFromNeoFrame(ip->Operand2, ref obj, this, frameBase + ip->SrcOffset, mStack);
                                        mStack[objIndex] = obj;
                                    }
                                }
                                break;
                            case OpCodeREnum.Ldfld_Value:
                                sz = ip->Operand;
                                srcRefOffset = ip->Operand2 & 0xFFFF;
                                srcIdx = (ip->Operand2 >> 16) & 0xFFFF;
                                dstRefOffset = ip->Operand3 & 0xFFFF;
                                refCnt = (ip->Operand3 >> 16) & 0xFFFF;
                                if (ip->Operand4 > 0)
                                {
                                    // inline direct: dst struct.RefOffset + 1
                                    dstIdx = ip->Operand4 - 1;
                                    if (sz > 0)
                                        Unsafe.CopyBlockUnaligned(
                                            ref *(byte*)(frameBase + ip->DstOffset),
                                            ref *(byte*)(frameBase + ip->SrcOffset + srcRefOffset),
                                            (uint)sz);
                                    if (refCnt > 0)
                                        mStack.CopyTo(frameRefBase + dstIdx + dstRefOffset, mStack, frameRefBase + srcIdx, refCnt);
                                }
                                else if (ip->Operand4 < 0)
                                {
                                    // Ref-Slot receiver: -1 - structRefOffset
                                    dstIdx = -1 - ip->Operand4;
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->SrcOffset, ip->Operand4, mStack,
                                        out objIndex, out dstRefOffset, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                    {
                                        // FRAME_REF: primitive block copy + ref-region 定位
                                        if (sz > 0)
                                            Unsafe.CopyBlockUnaligned(
                                                ref *(byte*)(frameBase + ip->DstOffset),
                                                ref ResolveNeoFrameTarget(stackBase, dstRefOffset + srcRefOffset),
                                                (uint)sz);
                                        if (refCnt > 0)
                                            mStack.CopyTo(frameRefBase + dstIdx + (ip->Operand3 & 0xFFFF), mStack, frameRefBase + srcIdx, refCnt);
                                    }
                                    else if (fieldIns != null)
                                    {
                                        if (refCnt != 0 && (ip->Operand2 & 0xFFFF) != 0)
                                            throw new NotImplementedException(
                                                "Neo nested Ldfld_Value with managed references through an IL Ref Slot: Step 17");
                                        if (sz > 0)
                                            Unsafe.CopyBlockUnaligned(
                                                ref *(byte*)(frameBase + ip->DstOffset),
                                                ref ResolveNeoILTarget(fieldIns, srcRefOffset),
                                                (uint)sz);
                                        if (refCnt > 0)
                                            fieldIns.ManagedObjects.CopyTo(ip->Operand3 & 0xFFFF, mStack, frameRefBase + srcIdx, refCnt);
                                    }
                                    else
                                    {
                                        throw new NotImplementedException(
                                            "Neo Ldfld_Value through a CLR Ref Slot: Step 13/17");
                                    }
                                }
                                else
                                {
                                    ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->SrcOffset));
                                    if (sz > 0)
                                        Unsafe.CopyBlockUnaligned(
                                            ref *(byte*)(frameBase + ip->DstOffset),
                                            ref ResolveNeoILTarget(ins, srcRefOffset),
                                            (uint)sz);
                                    if (refCnt > 0)
                                        ins.ManagedObjects.CopyTo(dstRefOffset, mStack, frameRefBase + srcIdx, refCnt);
                                }
                                break;
                            case OpCodeREnum.Stfld_Value:
                                sz = ip->Operand;
                                srcRefOffset = ip->Operand2 & 0xFFFF;
                                srcIdx = (ip->Operand2 >> 16) & 0xFFFF;
                                dstRefOffset = ip->Operand3 & 0xFFFF;
                                refCnt = (ip->Operand3 >> 16) & 0xFFFF;
                                if (ip->Operand4 > 0)
                                {
                                    // inline direct: receiver struct.RefOffset + 1
                                    dstIdx = ip->Operand4 - 1;
                                    if (sz > 0)
                                        Unsafe.CopyBlockUnaligned(
                                            ref *(byte*)(frameBase + ip->DstOffset + srcRefOffset),
                                            ref *(byte*)(frameBase + ip->SrcOffset),
                                            (uint)sz);
                                    if (refCnt > 0)
                                        mStack.CopyTo(frameRefBase + srcIdx, mStack, frameRefBase + dstIdx + dstRefOffset, refCnt);
                                }
                                else if (ip->Operand4 < 0)
                                {
                                    // Ref-Slot receiver: -1 - structRefOffset
                                    dstIdx = -1 - ip->Operand4;
                                    ResolveNeoFieldReceiver(
                                        frameBase + ip->DstOffset, ip->Operand4, mStack,
                                        out objIndex, out dstRefOffset, out obj, out fieldIns, out clrType);
                                    if (objIndex == -1)
                                    {
                                        if (sz > 0)
                                            Unsafe.CopyBlockUnaligned(
                                                ref ResolveNeoFrameTarget(stackBase, dstRefOffset + srcRefOffset),
                                                ref *(byte*)(frameBase + ip->SrcOffset),
                                                (uint)sz);
                                        if (refCnt > 0)
                                            mStack.CopyTo(frameRefBase + srcIdx, mStack, frameRefBase + dstIdx + (ip->Operand3 & 0xFFFF), refCnt);
                                    }
                                    else if (fieldIns != null)
                                    {
                                        if (refCnt != 0 && (ip->Operand2 & 0xFFFF) != 0)
                                            throw new NotImplementedException(
                                                "Neo nested Stfld_Value with managed references through an IL Ref Slot: Step 17");
                                        if (sz > 0)
                                            Unsafe.CopyBlockUnaligned(
                                                ref ResolveNeoILTarget(fieldIns, srcRefOffset),
                                                ref *(byte*)(frameBase + ip->SrcOffset),
                                                (uint)sz);
                                        if (refCnt > 0)
                                            mStack.CopyTo(frameRefBase + srcIdx, fieldIns.ManagedObjects, ip->Operand3 & 0xFFFF, refCnt);
                                    }
                                    else
                                    {
                                        throw new NotImplementedException(
                                            "Neo Stfld_Value through a CLR Ref Slot: Step 13/17");
                                    }
                                }
                                else
                                {
                                    ins = GetNeoILInstance(mStack, *(int*)(frameBase + ip->DstOffset));
                                    if (sz > 0)
                                        Unsafe.CopyBlockUnaligned(
                                            ref ResolveNeoILTarget(ins, srcRefOffset),
                                            ref *(byte*)(frameBase + ip->SrcOffset),
                                            (uint)sz);
                                    if (refCnt > 0)
                                        mStack.CopyTo(frameRefBase + srcIdx, ins.ManagedObjects, dstRefOffset, refCnt);
                                }
                                break;
                            case OpCodeREnum.Unbox:
                            case OpCodeREnum.Unbox_Any:
                                dstRefOffset = ip->Operand3;
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
    }
}
#endif
