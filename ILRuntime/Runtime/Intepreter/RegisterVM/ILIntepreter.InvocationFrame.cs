#if ENABLE_NEO_MODE
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

            // Generic-form writers. For primitive/enum T, route through PrimitiveConverter<T>
            // so the compile-time-known converter delegate returns a boxed-free primitive value
            // that the non-generic WriteXxx path writes straight into the frame.
            public void WriteInt32<T>(int index, T value)
            {
                WriteInt32(index, ILRuntime.Runtime.Enviorment.PrimitiveConverter<T>.CheckAndInvokeToInteger(value));
            }

            public void WriteInt64<T>(int index, T value)
            {
                WriteInt64(index, ILRuntime.Runtime.Enviorment.PrimitiveConverter<T>.CheckAndInvokeToLong(value));
            }

            public void WriteSingle<T>(int index, T value)
            {
                WriteSingle(index, ILRuntime.Runtime.Enviorment.PrimitiveConverter<T>.CheckAndInvokeToFloat(value));
            }

            public void WriteDouble<T>(int index, T value)
            {
                WriteDouble(index, ILRuntime.Runtime.Enviorment.PrimitiveConverter<T>.CheckAndInvokeToDouble(value));
            }

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

            public void PushInt32<T>(T value) { WriteInt32<T>(nextParamIdx++, value); }
            public void PushInt64<T>(T value) { WriteInt64<T>(nextParamIdx++, value); }
            public void PushSingle<T>(T value) { WriteSingle<T>(nextParamIdx++, value); }
            public void PushDouble<T>(T value) { WriteDouble<T>(nextParamIdx++, value); }

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
                    return ReadNeoPrimitive(retDst, retType);

                if (retType.IsValueType)
                {
                    // Dispatch by StructStorage: Inline reads Inline layout back into a boxed CLR object;
                    // Boxed reads the mStack index and returns the boxed reference directly.
                    var retClrType = retType as ILRuntime.CLR.TypeSystem.CLRType;
                    if (retClrType == null)
                        throw new NotSupportedException($"Neo InvocationFrame: unresolved CLR value type '{retType.FullName}'.");
                    if (retClrType.StructStorage == ILRuntime.CLR.TypeSystem.StructStorage.Inline)
                    {
                        var boxed = retClrType.CreateDefaultInstance();
                        // Return-value ref base: retRefBase points into mStack for the caller-owned ref segment.
                        ILIntepreter.CopyFrameToBoxedClrObjectStatic(boxed, retClrType, retDst, mStack, retRefBase);
                        return boxed;
                    }
                    int boxedIdx = *(int*)retDst;
                    return boxedIdx >= 0 ? mStack[boxedIdx] : null;
                }

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

            public T ReadInt32<T>() { return ReadGenericPrimitive<T>(); }
            public T ReadInt64<T>() { return ReadGenericPrimitive<T>(); }
            public T ReadSingle<T>() { return ReadGenericPrimitive<T>(); }
            public T ReadDouble<T>() { return ReadGenericPrimitive<T>(); }

            T ReadGenericPrimitive<T>()
            {
                if (!executed)
                    throw new InvalidOperationException("Execute must be called before reading the return value.");
                var tt = typeof(T);
                if (!(tt.IsPrimitive || tt.IsEnum || tt == typeof(IntPtr) || tt == typeof(UIntPtr)))
                    throw new NotSupportedException("Reference-type generic stub reserved for Step 13b.");
                var it = intp.AppDomain.GetType(tt);
                return (T)ILIntepreter.ReadNeoPrimitive(retDst, it);
            }

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
                    if (paramType is CLRType clrType)
                    {
                        if (clrType.StructStorage == StructStorage.Inline)
                        {
                            ILIntepreter.CopyBoxedClrObjectToFrameStatic(
                                value, clrType, pDst, mStack, pRefBase);
                        }
                        else
                        {
                            mStack[pRefBase] = value;
                            *(int*)pDst = pRefBase;
                        }
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
        }

        // Unbox a boxed CLR primitive/enum value into a 4/8-byte Neo frame slot.
        // Sub-int primitives (bool/byte/sbyte/short/ushort/char) are widened to 4B
        // to match Neo's register-slot layout (see JITCompiler.AllocateSlotForType).
        // Used by both InvocationFrame argument marshalling and the CLR reflection
        // return path in InvokeNeoClrMethod.
        internal static unsafe void WriteNeoPrimitive(byte* dst, IType type, object value)
        {
            // IL-defined enums box as ILEnumTypeInstance (holds the underlying primitive bytes),
            // not as CLR enum values. Unpack them by copying the underlying primitive bytes
            // straight into the Neo slot (low-order bytes for sub-int widening).
            if (type is ILType ilType && ilType.IsEnum)
            {
                var underlying = ilType.FieldTypes[0];
                int usz = ilType.AppDomain.GetPrimitiveSize(underlying);
                if (usz < 4) *(int*)dst = 0; // zero the 4-byte widened slot before writing sub-int bytes
                if (value is ILEnumTypeInstance ins)
                {
                    ref byte srcP = ref MemoryMarshal.GetReference(ins.Primitives.AsSpan());
                    Unsafe.CopyBlock(ref *dst, ref srcP, (uint)usz);
                }
                else
                {
                    // Caller passed a CLR-boxed primitive matching the underlying type.
                    WriteNeoPrimitive(dst, underlying, value);
                }
                return;
            }

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
                    if (clr == typeof(IntPtr)) { *(long*)dst = ((IntPtr)value).ToInt64(); break; }
                    if (clr == typeof(UIntPtr)) { *(ulong*)dst = ((UIntPtr)value).ToUInt64(); break; }
                    throw new NotSupportedException("Neo: unsupported primitive type " + clr.FullName);
            }
        }

        // Read a Neo-widened primitive slot into a boxed CLR value. Mirror of WriteNeoPrimitive:
        // sub-int slots occupy 4 bytes; long/double/IntPtr occupy their native width. CLR enums box
        // back to their declared enum type. IL-defined enums box to their boxed underlying primitive
        // (matching Legacy StackObject.ToObject); ILEnumTypeInstance is only produced by explicit
        // `box` opcodes, not by boundary marshalling.
        internal static unsafe object ReadNeoPrimitive(byte* src, IType type)
        {
            Type declared = type.TypeForCLR;
            Type clr = declared;
            if (type.IsEnum && clr.IsEnum)
                clr = Enum.GetUnderlyingType(clr);

            switch (Type.GetTypeCode(clr))
            {
                case TypeCode.Int32:   return declared.IsEnum ? Enum.ToObject(declared, *(int*)src)    : (object)*(int*)src;
                case TypeCode.UInt32:  return declared.IsEnum ? Enum.ToObject(declared, *(uint*)src)   : (object)*(uint*)src;
                case TypeCode.Int64:   return declared.IsEnum ? Enum.ToObject(declared, *(long*)src)   : (object)*(long*)src;
                case TypeCode.UInt64:  return declared.IsEnum ? Enum.ToObject(declared, *(ulong*)src)  : (object)*(ulong*)src;
                case TypeCode.Int16:   return declared.IsEnum ? Enum.ToObject(declared, (short)*(int*)src)  : (object)(short)*(int*)src;
                case TypeCode.UInt16:  return declared.IsEnum ? Enum.ToObject(declared, (ushort)*(int*)src) : (object)(ushort)*(int*)src;
                case TypeCode.Byte:    return declared.IsEnum ? Enum.ToObject(declared, (byte)*(int*)src)   : (object)(byte)*(int*)src;
                case TypeCode.SByte:   return declared.IsEnum ? Enum.ToObject(declared, (sbyte)*(int*)src)  : (object)(sbyte)*(int*)src;
                case TypeCode.Boolean: return *(int*)src != 0;
                case TypeCode.Char:    return (char)*(int*)src;
                case TypeCode.Single:  return *(float*)src;
                case TypeCode.Double:  return *(double*)src;
                default:
                    if (clr == typeof(IntPtr)) return new IntPtr(*(long*)src);
                    if (clr == typeof(UIntPtr)) return new UIntPtr(*(ulong*)src);
                    throw new NotSupportedException("Neo: unsupported primitive type " + clr.FullName);
            }
        }
    }
}
#endif
