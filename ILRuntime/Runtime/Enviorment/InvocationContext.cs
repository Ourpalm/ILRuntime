using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Enviorment
{
    public static class PrimitiveConverter<T>
    {
        public static Func<T, int> ToInteger;
        public static Func<int, T> FromInteger;
        public static Func<T, long> ToLong;
        public static Func<long, T> FromLong;
        public static Func<T, float> ToFloat;
        public static Func<float, T> FromFloat;
        public static Func<T, double> ToDouble;
        public static Func<double, T> FromDouble;

        public static int CheckAndInvokeToInteger(T val)
        {
            if (ToInteger != null)
                return ToInteger(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Int32", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromInteger(int val)
        {
            if (FromInteger != null)
                return FromInteger(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Int32 to {0}", typeof(T).FullName));
        }

        public static long CheckAndInvokeToLong(T val)
        {
            if (ToLong != null)
                return ToLong(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Int64", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromLong(long val)
        {
            if (FromLong != null)
                return FromLong(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Int64 to {0}", typeof(T).FullName));
        }

        public static float CheckAndInvokeToFloat(T val)
        {
            if (ToFloat != null)
                return ToFloat(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Single", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromFloat(float val)
        {
            if (FromFloat != null)
                return FromFloat(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Single to {0}", typeof(T).FullName));
        }

        public static double CheckAndInvokeToDouble(T val)
        {
            if (ToDouble != null)
                return ToDouble(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Double", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromDouble(double val)
        {
            if (FromDouble != null)
                return FromDouble(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Double to {0}", typeof(T).FullName));
        }
    }

    // Shared across both Legacy struct and Neo ref-struct variants of InvocationContext.
    internal enum InvocationTypes
    {
        Integer,
        Long,
        Float,
        Double,
        Enum,
        ValueType,
        Object,
    }

    internal static class InvocationContextShared
    {
        static bool defaultConverterIntialized = false;
        internal static void InitializeDefaultConverters()
        {
            if (!defaultConverterIntialized)
            {
                PrimitiveConverter<int>.ToInteger = (a) => a;
                PrimitiveConverter<int>.FromInteger = (a) => a;
                PrimitiveConverter<short>.ToInteger = (a) => a;
                PrimitiveConverter<short>.FromInteger = (a) => (short)a;
                PrimitiveConverter<byte>.ToInteger = (a) => a;
                PrimitiveConverter<byte>.FromInteger = (a) => (byte)a;
                PrimitiveConverter<sbyte>.ToInteger = (a) => a;
                PrimitiveConverter<sbyte>.FromInteger = (a) => (sbyte)a;
                PrimitiveConverter<ushort>.ToInteger = (a) => a;
                PrimitiveConverter<ushort>.FromInteger = (a) => (ushort)a;
                PrimitiveConverter<char>.ToInteger = (a) => a;
                PrimitiveConverter<char>.FromInteger = (a) => (char)a;
                PrimitiveConverter<uint>.ToInteger = (a) => (int)a;
                PrimitiveConverter<uint>.FromInteger = (a) => (uint)a;
                PrimitiveConverter<bool>.ToInteger = (a) => a ? 1 : 0;
                PrimitiveConverter<bool>.FromInteger = (a) => a == 1;
                PrimitiveConverter<long>.ToLong = (a) => a;
                PrimitiveConverter<long>.FromLong = (a) => a;
                PrimitiveConverter<ulong>.ToLong = (a) => (long)a;
                PrimitiveConverter<ulong>.FromLong = (a) => (ulong)a;
                PrimitiveConverter<float>.ToFloat = (a) => a;
                PrimitiveConverter<float>.FromFloat = (a) => a;
                PrimitiveConverter<double>.ToDouble = (a) => a;
                PrimitiveConverter<double>.FromDouble = (a) => a;

                defaultConverterIntialized = true;
            }
        }

        internal static InvocationTypes GetInvocationType<T>()
        {
            var type = typeof(T);
            if (type.IsPrimitive)
            {
                if (type == typeof(int)) return InvocationTypes.Integer;
                if (type == typeof(short)) return InvocationTypes.Integer;
                if (type == typeof(bool)) return InvocationTypes.Integer;
                if (type == typeof(long)) return InvocationTypes.Long;
                if (type == typeof(float)) return InvocationTypes.Float;
                if (type == typeof(double)) return InvocationTypes.Double;
                if (type == typeof(char)) return InvocationTypes.Integer;
                if (type == typeof(ushort)) return InvocationTypes.Integer;
                if (type == typeof(uint)) return InvocationTypes.Integer;
                if (type == typeof(ulong)) return InvocationTypes.Long;
                if (type == typeof(byte)) return InvocationTypes.Integer;
                if (type == typeof(sbyte)) return InvocationTypes.Integer;
                throw new NotImplementedException(string.Format("Not supported type:{0}", type.FullName));
            }
            else if (type.IsEnum)
            {
                if (PrimitiveConverter<T>.ToInteger != null && PrimitiveConverter<T>.FromInteger != null)
                    return InvocationTypes.Integer;
                if (PrimitiveConverter<T>.ToLong != null && PrimitiveConverter<T>.FromLong != null)
                    return InvocationTypes.Long;
                return InvocationTypes.Enum;
            }
            else if (type.IsValueType)
                return InvocationTypes.ValueType;
            else
                return InvocationTypes.Object;
        }
    }

#if ENABLE_NEO_MODE
    // Neo mode: InvocationContext is a `ref struct` wrapping an `InvocationFrame`.
    //
    // Why ref struct: BeginInvoke pins the interpreter to a single ILMethod for the
    // lifetime of the context; the frame's flat-byte region lives on the interpreter's
    // stack. Making the context a ref struct enforces short-lived stack-only ownership
    // (no field storage, no async/await capture, no lambda capture), which matches the
    // interpreter's rented lifetime and prevents accidental leaks of frameBase.
    //
    // `using (var ctx = app.BeginInvoke(m)) { ... }` works via pattern-based Dispose
    // (C# 8+ does not require IDisposable for ref structs).
    public unsafe ref struct InvocationContext
    {
        ILIntepreter.InvocationFrame frame;
        AppDomain domain;
        ILIntepreter intp;
        ILMethod method;
        bool invocated;
        int paramCnt;
        bool hasReturn;

        // Ref-argument backing storage: cells live at `stack.StackBase + [0, refArgCursor)`.
        // The InvocationFrame is only started *after* all ref-arg cells have been allocated,
        // and its frameBase is placed at `stack.StackBase + refArgCursor` (aligned up). This
        // way nested Call opcodes inside the callee, which extend the frame forward, never
        // clobber the ref-arg region — it sits behind the callee's frameBase.
        //
        // Consequently, once PushInt32/PushObject/PushReference/Invoke has been called (i.e.
        // frameStarted == true), further PutRefIntXX are refused: reallocating the region
        // would move frameBase and invalidate any offsets already handed out.
        byte* stackBase;
        int refArgCursor;
        bool frameStarted;

        // Non-generic converter initialization keeps parity with legacy AppDomain setup.
        internal static void InitializeDefaultConverters() => InvocationContextShared.InitializeDefaultConverters();
        internal static InvocationTypes GetInvocationType<T>() => InvocationContextShared.GetInvocationType<T>();

        internal InvocationContext(AppDomain domain, ILIntepreter intp, ILMethod method)
        {
            this.domain = domain;
            this.intp = intp;
            this.method = method;
            this.frame = default;              // Lazy: constructed on first Push/Invoke via EnsureFrameStarted.
            this.invocated = false;
            this.paramCnt = 0;
            this.hasReturn = method.ReturnType != null && method.ReturnType != domain.VoidType;
            this.stackBase = (byte*)intp.Stack.StackBase;
            this.refArgCursor = 0;
            this.frameStarted = false;
        }

        internal ILIntepreter Intepreter => intp;
        internal AppDomain Domain => domain;
        internal ILMethod TargetMethod => method;

        // Materializes the callee InvocationFrame. Idempotent. Called by every Push*/Invoke path.
        // Once invoked, the frameBase byte offset is frozen and no further ref-arg cells may
        // be allocated (attempted PutRefIntXX will throw).
        void EnsureFrameStarted()
        {
            if (frameStarted) return;
            // Align frame start to 8 so long/double param slots in the callee frame stay
            // naturally aligned. refArgCursor is the byte extent already claimed by ref-arg
            // cells from stack.StackBase.
            int frameStart = (refArgCursor + 7) & ~7;
            frame = ILIntepreter.InvocationFrame.Begin(intp, method, frameStart);
            frameStarted = true;
        }

        public void PushBool(bool val) { EnsureFrameStarted(); frame.PushInt32(val ? 1 : 0); paramCnt++; }
        public void PushInteger(int val) { EnsureFrameStarted(); frame.PushInt32(val); paramCnt++; }
        public void PushInteger(long val) { EnsureFrameStarted(); frame.PushInt64(val); paramCnt++; }
        public void PushInteger<T>(T val) { EnsureFrameStarted(); frame.PushInt32<T>(val); paramCnt++; }
        public void PushLong<T>(T val) { EnsureFrameStarted(); frame.PushInt64<T>(val); paramCnt++; }
        public void PushFloat(float val) { EnsureFrameStarted(); frame.PushSingle(val); paramCnt++; }
        public void PushFloat<T>(T val) { EnsureFrameStarted(); frame.PushSingle<T>(val); paramCnt++; }
        public void PushDouble(double val) { EnsureFrameStarted(); frame.PushDouble(val); paramCnt++; }
        public void PushDouble<T>(T val) { EnsureFrameStarted(); frame.PushDouble<T>(val); paramCnt++; }

        public void PushObject(object obj, bool isBox = true)
        {
            EnsureFrameStarted();
            frame.PushObject(obj);
            paramCnt++;
        }

        public void PushValueType<T>(ref T obj)
        {
            EnsureFrameStarted();
            frame.PushObject(obj);
            paramCnt++;
        }

        // Allocate a caller-owned 4-byte / 8-byte cell in the ref-arg region and initialize it
        // with `value`. Returns a handle (an absolute byte offset from stack.StackBase) that
        // must later be paired with:
        //   * PushReference(handle) to emit the `(objectIndex=-1, offset=handle)` Ref Slot
        //     into the next callee parameter position.
        //   * ReadRefInt32/64(handle) to observe any writeback after Invoke.
        //
        // Must be called BEFORE any Push*/Invoke on this context, otherwise the callee frame
        // has already been positioned and moving it would invalidate previously handed-out
        // handles.
        public int PutRefInt32(int value)
        {
            if (frameStarted)
                throw new InvalidOperationException(
                    "Neo InvocationContext: PutRefInt32 must be called before any Push*/Invoke; the callee frame has already started.");
            int handle = (refArgCursor + 3) & ~3;
            *(int*)(stackBase + handle) = value;
            refArgCursor = handle + 4;
            return handle;
        }

        public int PutRefInt64(long value)
        {
            if (frameStarted)
                throw new InvalidOperationException(
                    "Neo InvocationContext: PutRefInt64 must be called before any Push*/Invoke; the callee frame has already started.");
            int handle = (refArgCursor + 7) & ~7;
            *(long*)(stackBase + handle) = value;
            refArgCursor = handle + 8;
            return handle;
        }

        public int ReadRefInt32(int handle) => *(int*)(stackBase + handle);
        public long ReadRefInt64(int handle) => *(long*)(stackBase + handle);

        // Emit an 8-byte Ref Slot `(objectIndex=-1, offset=handle)` into the next callee
        // parameter slot. `handle` must come from a prior PutRefInt32/PutRefInt64 on this
        // context. FRAME_REF encoding means the callee's Stind/Ldind resolves via
        // `stackBase + handle`, reading/writing the cell that PutRefIntXX allocated.
        public void PushReference(int handle)
        {
            EnsureFrameStarted();
            frame.PushByRefSlot(-1, handle);
            paramCnt++;
        }

        public void PushParameter<T>(T val) => PushParameter(GetInvocationType<T>(), val);

        internal void PushParameter<T>(InvocationTypes type, T val)
        {
            switch (type)
            {
                case InvocationTypes.Integer: PushInteger<T>(val); break;
                case InvocationTypes.Long: PushLong<T>(val); break;
                case InvocationTypes.Float: PushFloat<T>(val); break;
                case InvocationTypes.Double: PushDouble<T>(val); break;
                case InvocationTypes.Enum: PushObject(val, false); break;
                case InvocationTypes.ValueType: PushValueType<T>(ref val); break;
                default: PushObject(val); break;
            }
        }

        public void Invoke()
        {
            if (invocated)
                throw new NotSupportedException("A invocation context can only be used once");
            invocated = true;
            var cnt = method.HasThis ? method.ParameterCount + 1 : method.ParameterCount;
            if (cnt != paramCnt)
                throw new ArgumentException("Argument count mismatch");
            EnsureFrameStarted();
            frame.Execute(out bool unhandledException);
        }

        void CheckReturnValue()
        {
            if (!invocated)
                throw new NotSupportedException("You have to invocate first before you try to read the return value");
            if (!hasReturn)
                throw new NotSupportedException("The target method does not have a return value");
        }

        void CheckInvoked()
        {
            if (!invocated)
                throw new NotSupportedException("You have to invocate first before you try to read parameter values");
        }

        public T ReadResult<T>()
        {
            CheckReturnValue();
            return ReadResultInternal<T>(GetInvocationType<T>());
        }

        public T ReadResult<T>(int index)
        {
            CheckInvoked();
            var type = GetInvocationType<T>();
            switch (type)
            {
                case InvocationTypes.Integer: return frame.ReadInt32<T>(index);
                case InvocationTypes.Long: return frame.ReadInt64<T>(index);
                case InvocationTypes.Float: return frame.ReadSingle<T>(index);
                case InvocationTypes.Double: return frame.ReadDouble<T>(index);
                case InvocationTypes.ValueType: return (T)frame.ReadObject(index);
                default: return (T)frame.ReadObject(index);
            }
        }

        internal T ReadResultInternal<T>(InvocationTypes type)
        {
            switch (type)
            {
                case InvocationTypes.Integer: return frame.ReadInt32<T>();
                case InvocationTypes.Long: return frame.ReadInt64<T>();
                case InvocationTypes.Float: return frame.ReadSingle<T>();
                case InvocationTypes.Double: return frame.ReadDouble<T>();
                case InvocationTypes.ValueType: return (T)frame.ReadObject();
                default: return (T)frame.ReadObject();
            }
        }

        // Convenience overload kept for cross-config symmetry with legacy caller code.
        internal T ReadResult<T>(InvocationTypes type) => ReadResultInternal<T>(type);

        public int ReadInteger() { CheckReturnValue(); return frame.ReadInt32(); }
        public int ReadInteger(int index) { CheckInvoked(); return frame.ReadInt32(index); }
        public T ReadInteger<T>() { CheckReturnValue(); return frame.ReadInt32<T>(); }

        public long ReadLong() { CheckReturnValue(); return frame.ReadInt64(); }
        public long ReadLong(int index) { CheckInvoked(); return frame.ReadInt64(index); }
        public T ReadLong<T>() { CheckReturnValue(); return frame.ReadInt64<T>(); }

        public float ReadFloat() { CheckReturnValue(); return frame.ReadSingle(); }
        public float ReadFloat(int index) { CheckInvoked(); return frame.ReadSingle(index); }
        public T ReadFloat<T>() { CheckReturnValue(); return frame.ReadSingle<T>(); }

        public double ReadDouble() { CheckReturnValue(); return frame.ReadDouble(); }
        public double ReadDouble(int index) { CheckInvoked(); return frame.ReadDouble(index); }
        public T ReadDouble<T>() { CheckReturnValue(); return frame.ReadDouble<T>(); }

        public bool ReadBool() { CheckReturnValue(); return frame.ReadInt32() != 0; }
        public bool ReadBool(int index) { CheckInvoked(); return frame.ReadInt32(index) != 0; }

        public T ReadValueType<T>() { CheckReturnValue(); return (T)frame.ReadObject(); }
        public T ReadValueType<T>(int index) { CheckInvoked(); return (T)frame.ReadObject(index); }

        public T ReadObject<T>() { CheckReturnValue(); return (T)typeof(T).CheckCLRTypes(frame.ReadObject()); }
        public object ReadObject(Type type) { CheckReturnValue(); return type.CheckCLRTypes(frame.ReadObject()); }
        public T ReadObject<T>(int index) { CheckInvoked(); return (T)typeof(T).CheckCLRTypes(frame.ReadObject(index)); }

        public void Dispose()
        {
            if (frameStarted)
                frame.Dispose();
            if (intp != null)
                domain.FreeILIntepreter(intp);
            intp = null;
            domain = null;
            method = null;
        }

        // Neo does not use StackObject-based marshalling; the following helpers are kept
        // as compile-time stubs so ILIntepreter.PushObject<T>/RetrieveObject<T> (which live
        // outside #if guards) resolve. They must never be reached at runtime under Neo.
        internal static unsafe StackObject* PushValueTypeSub<T>(ref T obj, StackObject* esp, AppDomain domain, ILIntepreter intp, AutoList mStack, bool useRegister)
        {
            throw new NotSupportedException("Neo mode: PushValueTypeSub is a StackObject legacy path and must not be reached.");
        }

        internal static unsafe T ReadValueTypeSub<T>(StackObject* val, AppDomain domain, ILIntepreter intp, AutoList mStack)
        {
            throw new NotSupportedException("Neo mode: ReadValueTypeSub is a StackObject legacy path and must not be reached.");
        }
    }
#else
    public unsafe struct InvocationContext : IDisposable
    {
        StackObject* ebp;
        StackObject* esp;
        AppDomain domain;
        ILIntepreter intp;
        ILMethod method;
        AutoList mStack;
        bool invocated;
        int paramCnt;
        bool hasReturn;
        bool useRegister;

        internal static void InitializeDefaultConverters() => InvocationContextShared.InitializeDefaultConverters();
        internal static InvocationTypes GetInvocationType<T>() => InvocationContextShared.GetInvocationType<T>();

        internal InvocationContext(ILIntepreter intp, ILMethod method)
        {
            var stack = intp.Stack;
            mStack = stack.ManagedStack;
            esp = stack.StackBase;
            ebp = esp;
            stack.ResetValueTypePointer();

            this.domain = intp.AppDomain;
            this.intp = intp;
            this.method = method;

            invocated = false;
            paramCnt = 0;
            hasReturn = method.ReturnType != domain.VoidType;
            useRegister = method.ShouldUseRegisterVM;
        }

        internal void SetInvoked(StackObject* esp)
        {
            this.esp = esp - 1;
            invocated = true;
        }

        public StackObject* ESP
        {
            get { return esp; }
            set { esp = value; }
        }

        public ILIntepreter Intepreter => intp;
        public AutoList ManagedStack => mStack;

        public void PushBool(bool val) { PushInteger(val ? 1 : 0); }
        public void PushInteger<T>(T val) { PushInteger(PrimitiveConverter<T>.CheckAndInvokeToInteger(val)); }
        public void PushLong<T>(T val) { PushInteger(PrimitiveConverter<T>.CheckAndInvokeToLong(val)); }

        public void PushInteger(int val)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = val;
            esp->ValueLow = 0;
            if (useRegister) mStack.Add(null);
            esp++;
            paramCnt++;
        }

        public void PushInteger(long val)
        {
            esp->ObjectType = ObjectTypes.Long;
            *(long*)&esp->Value = val;
            if (useRegister) mStack.Add(null);
            esp++;
            paramCnt++;
        }

        public void PushFloat<T>(T val) { PushFloat(PrimitiveConverter<T>.CheckAndInvokeToFloat(val)); }

        public void PushFloat(float val)
        {
            esp->ObjectType = ObjectTypes.Float;
            *(float*)&esp->Value = val;
            if (useRegister) mStack.Add(null);
            esp++;
            paramCnt++;
        }

        public void PushDouble<T>(T val) { PushDouble(PrimitiveConverter<T>.CheckAndInvokeToDouble(val)); }

        public void PushDouble(double val)
        {
            esp->ObjectType = ObjectTypes.Double;
            *(double*)&esp->Value = val;
            if (useRegister) mStack.Add(null);
            esp++;
            paramCnt++;
        }

        internal static StackObject* PushValueTypeSub<T>(ref T obj, StackObject* esp, Runtime.Enviorment.AppDomain domain, ILIntepreter intp, AutoList mStack, bool useRegister)
        {
            Type t = typeof(T);
            bool needPush = false;
            StackObject* res = default(StackObject*);
            ValueTypeBinder binder;
            if (domain.ValueTypeBinders.TryGetValue(t, out binder))
            {
                var binderT = binder as ValueTypeBinder<T>;
                if (binderT != null)
                {
                    binderT.PushValue(ref obj, intp, esp, mStack);
                    if (useRegister) mStack.Add(null);
                    res = esp + 1;
                }
                else
                    needPush = true;
            }
            else
                needPush = true;
            if (needPush)
            {
                res = ILIntepreter.PushObject(esp, mStack, obj, true);
            }
            return res;
        }

        public void PushValueType<T>(ref T obj)
        {
            esp = PushValueTypeSub(ref obj, esp, domain, intp, mStack, useRegister);
            paramCnt++;
        }

        public void PushObject(object obj, bool isBox = true)
        {
            if (obj is CrossBindingAdaptorType)
                obj = ((CrossBindingAdaptorType)obj).ILInstance;
            var res = ILIntepreter.PushObject(esp, mStack, obj, isBox);
            if (esp->ObjectType < ObjectTypes.Object && useRegister)
                mStack.Add(null);
            esp = res;
            paramCnt++;
        }

        public void PushReference(int index)
        {
            var dst = ebp + index;
            esp->ObjectType = ObjectTypes.StackObjectReference;
            *(long*)&esp->Value = (long)dst;
            if (useRegister) mStack.Add(null);
            esp++;
        }

        public void PushParameter<T>(T val) => PushParameter(GetInvocationType<T>(), val);

        internal void PushParameter<T>(InvocationTypes type, T val)
        {
            switch (type)
            {
                case InvocationTypes.Integer: PushInteger(val); break;
                case InvocationTypes.Long: PushLong(val); break;
                case InvocationTypes.Float: PushFloat(val); break;
                case InvocationTypes.Double: PushDouble(val); break;
                case InvocationTypes.Enum: PushObject(val, false); break;
                case InvocationTypes.ValueType: PushValueType(ref val); break;
                default: PushObject(val); break;
            }
        }

        public T ReadResult<T>() => ReadResult<T>(GetInvocationType<T>());

        public T ReadResult<T>(int index)
        {
            var type = GetInvocationType<T>();
            switch (type)
            {
                case InvocationTypes.Integer: return PrimitiveConverter<T>.CheckAndInvokeFromInteger(ReadInteger(index));
                case InvocationTypes.Long: return PrimitiveConverter<T>.CheckAndInvokeFromLong(ReadLong(index));
                case InvocationTypes.Float: return PrimitiveConverter<T>.CheckAndInvokeFromFloat(ReadFloat(index));
                case InvocationTypes.Double: return PrimitiveConverter<T>.CheckAndInvokeFromDouble(ReadDouble(index));
                case InvocationTypes.ValueType: return ReadValueType<T>(index);
                default: return ReadObject<T>(index);
            }
        }

        internal T ReadResult<T>(InvocationTypes type)
        {
            switch (type)
            {
                case InvocationTypes.Integer: return ReadInteger<T>();
                case InvocationTypes.Long: return ReadLong<T>();
                case InvocationTypes.Float: return ReadFloat<T>();
                case InvocationTypes.Double: return ReadDouble<T>();
                case InvocationTypes.ValueType: return ReadValueType<T>();
                default: return ReadObject<T>();
            }
        }

        public void Invoke()
        {
            if (invocated)
                throw new NotSupportedException("A invocation context can only be used once");
            invocated = true;
            var cnt = method.HasThis ? method.ParameterCount + 1 : method.ParameterCount;
            if (cnt != paramCnt)
                throw new ArgumentException("Argument count mismatch");
            bool unhandledException;
            if (useRegister)
                esp = intp.ExecuteR(method, esp, out unhandledException);
            else
                esp = intp.Execute(method, esp, out unhandledException);
            esp--;
        }

        void CheckReturnValue()
        {
            if (!invocated)
                throw new NotSupportedException("You have to invocate first before you try to read the return value");
            if (!hasReturn)
                throw new NotSupportedException("The target method does not have a return value");
        }

        public int ReadInteger() { CheckReturnValue(); return esp->Value; }
        public int ReadInteger(int index) { var e = ebp + index; return e->Value; }
        public T ReadInteger<T>() => PrimitiveConverter<T>.CheckAndInvokeFromInteger(ReadInteger());

        public long ReadLong() { CheckReturnValue(); return *(long*)&esp->Value; }
        public long ReadLong(int index) { var e = ebp + index; return *(long*)&e->Value; }
        public T ReadLong<T>() => PrimitiveConverter<T>.CheckAndInvokeFromLong(ReadLong());

        public float ReadFloat() { CheckReturnValue(); return *(float*)&esp->Value; }
        public float ReadFloat(int index) { var e = ebp + index; return *(float*)&e->Value; }
        public T ReadFloat<T>() => PrimitiveConverter<T>.CheckAndInvokeFromFloat(ReadFloat());

        public double ReadDouble() { CheckReturnValue(); return *(double*)&esp->Value; }
        public double ReadDouble(int index) { var e = ebp + index; return *(double*)&e->Value; }
        public T ReadDouble<T>() => PrimitiveConverter<T>.CheckAndInvokeFromDouble(ReadDouble());

        public bool ReadBool() { CheckReturnValue(); return esp->Value == 1; }
        public bool ReadBool(int index) { var e = ebp + index; return e->Value == 1; }

        public T ReadValueType<T>(int index)
        {
            var e = ebp + index;
            return ReadValueTypeSub<T>(e, domain, intp, mStack);
        }

        internal static T ReadValueTypeSub<T>(StackObject* val, Runtime.Enviorment.AppDomain domain, ILIntepreter intp, AutoList mStack)
        {
            Type t = typeof(T);
            T res = default(T);
            ValueTypeBinder binder;
            if (domain.ValueTypeBinders.TryGetValue(t, out binder))
            {
                var binderT = binder as ValueTypeBinder<T>;
                if (binderT != null)
                    binderT.ParseValue(ref res, intp, val, mStack);
                else
                    res = (T)t.CheckCLRTypes(StackObject.ToObject(val, domain, mStack));
            }
            else
                res = (T)t.CheckCLRTypes(StackObject.ToObject(val, domain, mStack));
            return res;
        }

        public T ReadValueType<T>()
        {
            CheckReturnValue();
            return ReadValueTypeSub<T>(esp, domain, intp, mStack);
        }

        public T ReadObject<T>()
        {
            CheckReturnValue();
            return (T)typeof(T).CheckCLRTypes(StackObject.ToObject(esp, domain, mStack));
        }

        public object ReadObject(Type type)
        {
            CheckReturnValue();
            return type.CheckCLRTypes(StackObject.ToObject(esp, domain, mStack));
        }

        public T ReadObject<T>(int index)
        {
            var e = ebp + index;
            return (T)typeof(T).CheckCLRTypes(StackObject.ToObject(e, domain, mStack));
        }

        public void Dispose()
        {
            domain.FreeILIntepreter(intp);
            esp = null;
            intp = null;
            domain = null;
            method = null;
            mStack = null;
        }
    }
#endif
}
