#if ENABLE_NEO_MODE
using System;

using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Other;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

namespace ILRuntime.Runtime.Intepreter
{
    partial class ILIntepreter
    {
        // Marshals a managed CLR call into a Neo interpreter frame:
        //   * lays out the frame's primitive region + return slot on stack.StackBase
        //   * pre-allocates return refs (visible to caller after Ret) then callee refs
        //   * accepts arguments either by explicit index or by push cursor
        //   * runs ExecuteNeo
        //   * unmarshals the return value back into a CLR object / primitive
        //
        // Lifetime is scoped by `using`: Dispose trims mStack back to the pre-invocation
        // count so both return refs and callee refs are released. Attempting to read a
        // return value after Dispose is not supported.
        internal unsafe ref struct InvocationFrame
        {
            ILIntepreter intp;
            ILMethod method;
            AutoList mStack;
            byte* frameBase;
            byte* retDst;
            int retRefBase;
            int calleeRefBase;
            int mStackBase;
            int nextParamIdx;
            bool executed;

            const string GenericStubMsg =
                "Generic non-boxing Push/Write/Read is reserved for InvocationContext migration (Step 13).";

            internal static InvocationFrame Begin(ILIntepreter intp, ILMethod method)
            {
                var stack = intp.Stack;
                var mStack = stack.ManagedStack;
                stack.ResetValueTypePointer();

                ref readonly var nf = ref method.CompiledFrame;

                InvocationFrame f;
                f.intp = intp;
                f.method = method;
                f.mStack = mStack;
                f.mStackBase = mStack.Count;

                // Neo frame lives on stack.StackBase as a flat byte region. The return
                // primitive slot is placed immediately after the frame, sharing the
                // same block; no StackObject alignment is needed because Neo addressing
                // is entirely byte-offset based.
                byte* baseAddr = (byte*)stack.StackBase;
                f.frameBase = baseAddr;
                f.retDst = nf.ReturnPrimitiveSize > 0 || nf.ReturnRefCount > 0
                    ? baseAddr + nf.TotalStructSize
                    : null;

                // Return refs come first so ExecuteNeo's Ret can RemoveRange the callee
                // block without wiping them.
                f.retRefBase = mStack.Count;
                mStack.ExpandBySize(nf.ReturnRefCount);

                f.calleeRefBase = mStack.Count;
                mStack.ExpandBySize(nf.TotalRefSize);

                f.nextParamIdx = 0;
                f.executed = false;
                return f;
            }

            // ---------------------------------------------------------------
            // Index-form writers
            // ---------------------------------------------------------------

            public void WriteObject(int index, object value)
            {
                WriteObjectInternal(index, value);
            }

            public void WriteInt32(int index, int value)
            {
                var slot = method.CompiledFrame.ParamInfos[index];
                *(int*)(frameBase + slot.Offset) = value;
            }

            public void WriteInt64(int index, long value)
            {
                var slot = method.CompiledFrame.ParamInfos[index];
                *(long*)(frameBase + slot.Offset) = value;
            }

            public void WriteSingle(int index, float value)
            {
                var slot = method.CompiledFrame.ParamInfos[index];
                *(float*)(frameBase + slot.Offset) = value;
            }

            public void WriteDouble(int index, double value)
            {
                var slot = method.CompiledFrame.ParamInfos[index];
                *(double*)(frameBase + slot.Offset) = value;
            }

            public void WriteInt32<T>(int index, T value) { throw new NotImplementedException(GenericStubMsg); }
            public void WriteInt64<T>(int index, T value) { throw new NotImplementedException(GenericStubMsg); }
            public void WriteSingle<T>(int index, T value) { throw new NotImplementedException(GenericStubMsg); }
            public void WriteDouble<T>(int index, T value) { throw new NotImplementedException(GenericStubMsg); }

            // ---------------------------------------------------------------
            // Cursor-form writers
            // ---------------------------------------------------------------

            public void PushObject(object value)
            {
                WriteObjectInternal(nextParamIdx++, value);
            }

            public void PushInt32(int value)
            {
                WriteInt32(nextParamIdx++, value);
            }

            public void PushInt64(long value)
            {
                WriteInt64(nextParamIdx++, value);
            }

            public void PushSingle(float value)
            {
                WriteSingle(nextParamIdx++, value);
            }

            public void PushDouble(double value)
            {
                WriteDouble(nextParamIdx++, value);
            }

            public void PushInt32<T>(T value) { throw new NotImplementedException(GenericStubMsg); }
            public void PushInt64<T>(T value) { throw new NotImplementedException(GenericStubMsg); }
            public void PushSingle<T>(T value) { throw new NotImplementedException(GenericStubMsg); }
            public void PushDouble<T>(T value) { throw new NotImplementedException(GenericStubMsg); }

            // ---------------------------------------------------------------
            // Execute + return-value readers
            // ---------------------------------------------------------------

            public void Execute(out bool unhandledException)
            {
                intp.ExecuteNeo(method, frameBase, retDst, retRefBase, out unhandledException, calleeRefBase);
                executed = true;
            }

            public object ReadObject()
            {
                if (!executed)
                    throw new InvalidOperationException("Execute must be called before reading the return value.");

                IType retType = method.ReturnType;
                if (retType == null || retType == intp.AppDomain.VoidType)
                    return null;

                if (retType.IsPrimitive || retType.IsEnum)
                    return NeoBoxReturnValue(retType, retDst, method.CompiledFrame.ReturnPrimitiveSize);

                if (retType.IsValueType)
                    throw new NotImplementedException("Neo InvocationFrame: value-type return is not yet implemented (Step 13).");

                int refIdx = *(int*)retDst;
                object refObj = refIdx >= 0 ? mStack[refIdx] : null;
                return retType.TypeForCLR.CheckCLRTypes(refObj);
            }

            public int ReadInt32()
            {
                if (!executed)
                    throw new InvalidOperationException("Execute must be called before reading the return value.");
                return *(int*)retDst;
            }

            public long ReadInt64()
            {
                if (!executed)
                    throw new InvalidOperationException("Execute must be called before reading the return value.");
                return *(long*)retDst;
            }

            public float ReadSingle()
            {
                if (!executed)
                    throw new InvalidOperationException("Execute must be called before reading the return value.");
                return *(float*)retDst;
            }

            public double ReadDouble()
            {
                if (!executed)
                    throw new InvalidOperationException("Execute must be called before reading the return value.");
                return *(double*)retDst;
            }

            public T ReadInt32<T>() { throw new NotImplementedException(GenericStubMsg); }
            public T ReadInt64<T>() { throw new NotImplementedException(GenericStubMsg); }
            public T ReadSingle<T>() { throw new NotImplementedException(GenericStubMsg); }
            public T ReadDouble<T>() { throw new NotImplementedException(GenericStubMsg); }

            public void Dispose()
            {
                if (mStack != null)
                {
                    mStack.RemoveRange(mStackBase, mStack.Count - mStackBase);
                    mStack = null;
                }
            }

            // ---------------------------------------------------------------
            // Internals
            // ---------------------------------------------------------------

            // Resolves the declared IType for a parameter slot. Slot 0 is `this`
            // (when HasThis) whose declared type is method.DeclearingType; subsequent
            // slots map to method.Parameters with an index shift.
            IType GetParamType(int index)
            {
                if (method.HasThis)
                {
                    if (index == 0)
                        return method.DeclearingType;
                    return method.Parameters[index - 1];
                }
                return method.Parameters[index];
            }

            void WriteObjectInternal(int index, object value)
            {
                // CrossBindingAdaptor unwrapping applies to any managed-side handle,
                // regardless of whether the slot is `this` or a regular argument.
                if (value is CrossBindingAdaptorType cbt)
                    value = cbt.ILInstance;

                var slot = method.CompiledFrame.ParamInfos[index];
                IType paramType = GetParamType(index);
                byte* pDst = frameBase + slot.Offset;
                int pRefBase = calleeRefBase + slot.RefOffset;

                if (value == null)
                {
                    for (int b = 0; b < slot.Size; b++)
                        pDst[b] = 0;
                    for (int r = 0; r < slot.RefCount; r++)
                        mStack[pRefBase + r] = null;
                    // Managed-object slot: sentinel -1 marks null, matching what the
                    // interpreter emits for ldnull / uninitialised reference locals.
                    if (slot.Size == 4 && slot.RefCount == 1 && !paramType.IsValueType && !paramType.IsPrimitive)
                        *(int*)pDst = -1;
                    return;
                }

                if (paramType.IsPrimitive || paramType.IsEnum)
                {
                    WriteNeoPrimitive(pDst, paramType, value);
                    return;
                }

                if (paramType.IsValueType)
                {
                    if (value is ILTypeInstance vt)
                    {
                        if (slot.Size > 0)
                        {
                            fixed (byte* src = vt.Primitives)
                                Buffer.MemoryCopy(src, pDst, slot.Size, slot.Size);
                        }
                        for (int r = 0; r < slot.RefCount; r++)
                            mStack[pRefBase + r] = vt.ManagedObjects[r];
                        return;
                    }
                    throw new NotSupportedException("Neo InvocationFrame: value-type argument must be an ILTypeInstance.");
                }

                // Reference-type argument: park the object in the callee ref slot and
                // encode its mStack index in the primitive region so Ldarg/Ldfld can
                // dereference it.
                int argIdx = pRefBase;
                mStack[argIdx] = value;
                *(int*)pDst = argIdx;
            }

            static object NeoBoxReturnValue(IType returnType, byte* retDst, int retSize)
            {
                Type clr = returnType.TypeForCLR;
                if (returnType.IsEnum && clr.IsEnum)
                    clr = Enum.GetUnderlyingType(clr);

                if (clr == typeof(int)) return *(int*)retDst;
                if (clr == typeof(uint)) return *(uint*)retDst;
                if (clr == typeof(long)) return *(long*)retDst;
                if (clr == typeof(ulong)) return *(ulong*)retDst;
                if (clr == typeof(short)) return (short)*(int*)retDst;
                if (clr == typeof(ushort)) return (ushort)*(int*)retDst;
                if (clr == typeof(byte)) return (byte)*(int*)retDst;
                if (clr == typeof(sbyte)) return (sbyte)*(int*)retDst;
                if (clr == typeof(bool)) return *(int*)retDst != 0;
                if (clr == typeof(char)) return (char)*(int*)retDst;
                if (clr == typeof(float)) return *(float*)retDst;
                if (clr == typeof(double)) return *(double*)retDst;

                return retSize >= 4 ? (object)*(int*)retDst : (object)(int)*retDst;
            }
        }

        // Unbox a boxed CLR primitive/enum value into a 4/8-byte Neo frame slot.
        // Sub-int primitives (bool/byte/sbyte/short/ushort/char) are widened to 4B
        // to match Neo's register-slot layout (see JITCompiler.AllocateSlotForType).
        // Used by both InvocationFrame argument marshalling and the CLR reflection
        // return path in InvokeNeoClrMethod.
        internal static unsafe void WriteNeoPrimitive(byte* dst, IType type, object value)
        {
            Type clr = type.TypeForCLR;
            if (type.IsEnum && clr.IsEnum)
                clr = Enum.GetUnderlyingType(clr);

            switch (Type.GetTypeCode(clr))
            {
                case TypeCode.Int32:   *(int*)dst    = Convert.ToInt32(value);   break;
                case TypeCode.UInt32:  *(uint*)dst   = Convert.ToUInt32(value);  break;
                case TypeCode.Int64:   *(long*)dst   = Convert.ToInt64(value);   break;
                case TypeCode.UInt64:  *(ulong*)dst  = Convert.ToUInt64(value);  break;
                case TypeCode.Int16:   *(int*)dst    = Convert.ToInt16(value);   break;
                case TypeCode.UInt16:  *(int*)dst    = Convert.ToUInt16(value);  break;
                case TypeCode.Byte:    *(int*)dst    = Convert.ToByte(value);    break;
                case TypeCode.SByte:   *(int*)dst    = Convert.ToSByte(value);   break;
                case TypeCode.Boolean: *(int*)dst    = Convert.ToBoolean(value) ? 1 : 0; break;
                case TypeCode.Char:    *(int*)dst    = Convert.ToChar(value);    break;
                case TypeCode.Single:  *(float*)dst  = Convert.ToSingle(value);  break;
                case TypeCode.Double:  *(double*)dst = Convert.ToDouble(value);  break;
                default:
                    throw new NotSupportedException("Neo: unsupported primitive type " + clr.FullName);
            }
        }
    }
}
#endif
