using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Stack;
using ILRuntime.Other;
using ILRuntime.Runtime.Enviorment;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Intepreter
{
    #region Functions
    class FunctionDelegateAdapter<TResult> : DelegateAdapter
    {
        Func<TResult> action;

        static InvocationTypes[] pTypes;
        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Func<TResult>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod()
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
                if (method.HasThis && !method.IsExtend)
                {
                    // Delegate `this` needs to be prepended for instance methods.
                    // But callers have already pushed only visible parameters, so we
                    // begin a fresh context that includes `this`.
                    // See NeoInvoke for multicast handling.
                }
                return NeoInvokeAndRead<TResult>(ref ctx, pTypes[0]);
#else
                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp); 
                return ctx.ReadResult<TResult>(pTypes[0]);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, TResult> : DelegateAdapter
    {
        Func<T1, TResult> action;

        static InvocationTypes[] pTypes;
        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Func<T1, TResult>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);

#if ENABLE_NEO_MODE
                if (method.HasThis && !method.IsExtend)
                {
                    // Delegate `this` needs to be prepended for instance methods.
                    // But callers have already pushed only visible parameters, so we
                    // begin a fresh context that includes `this`.
                    // See NeoInvoke for multicast handling.
                }
                return NeoInvokeAndRead<TResult>(ref ctx, pTypes[1]);
#else
                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[1]);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<T1, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, T2, TResult> : DelegateAdapter
    {
        Func<T1, T2, TResult> action;

        static InvocationTypes[] pTypes;
        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Func<T1, T2, TResult>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1, T2 p2)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);

#if ENABLE_NEO_MODE
                if (method.HasThis && !method.IsExtend)
                {
                    // Delegate `this` needs to be prepended for instance methods.
                    // But callers have already pushed only visible parameters, so we
                    // begin a fresh context that includes `this`.
                    // See NeoInvoke for multicast handling.
                }
                return NeoInvokeAndRead<TResult>(ref ctx, pTypes[2]);
#else
                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[2]);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, T2, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, T2, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<T1, T2, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, T2, TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, T2, T3, TResult> : DelegateAdapter
    {
        Func<T1, T2, T3, TResult> action;

        static InvocationTypes[] pTypes;

        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Func<T1, T2, T3, TResult>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1, T2 p2, T3 p3)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);

#if ENABLE_NEO_MODE
                if (method.HasThis && !method.IsExtend)
                {
                    // Delegate `this` needs to be prepended for instance methods.
                    // But callers have already pushed only visible parameters, so we
                    // begin a fresh context that includes `this`.
                    // See NeoInvoke for multicast handling.
                }
                return NeoInvokeAndRead<TResult>(ref ctx, pTypes[3]);
#else
                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[3]);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, T2, T3, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, T2, T3, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }
        public override void Combine(Delegate dele)
        {
            action += (Func<T1, T2, T3, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, T2, T3, TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, T2, T3, T4, TResult> : DelegateAdapter
    {
        Func<T1, T2, T3, T4, TResult> action;

        static InvocationTypes[] pTypes;

        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<T4>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Func<T1, T2, T3, T4, TResult>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1, T2 p2, T3 p3, T4 p4)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
                ctx.PushParameter(pTypes[3], p4);

#if ENABLE_NEO_MODE
                if (method.HasThis && !method.IsExtend)
                {
                    // Delegate `this` needs to be prepended for instance methods.
                    // But callers have already pushed only visible parameters, so we
                    // begin a fresh context that includes `this`.
                    // See NeoInvoke for multicast handling.
                }
                return NeoInvokeAndRead<TResult>(ref ctx, pTypes[4]);
#else
                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[4]);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, T2, T3, T4, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, T2, T3, T4, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<T1, T2, T3, T4, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, T2, T3, T4, TResult>)dele;
        }
    }
    #endregion

    #region Methods
    class MethodDelegateAdapter<T1> : DelegateAdapter
    {
        Action<T1> action;
        static InvocationTypes pType;

        static MethodDelegateAdapter()
        {
            pType = InvocationContext.GetInvocationType<T1>();
        }

        public MethodDelegateAdapter()
        {
            
        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Action<T1>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pType, p1);
#if ENABLE_NEO_MODE
                NeoInvoke(ref ctx);
#else
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1>)dele;
        }
    }

    class MethodDelegateAdapter<T1, T2> : DelegateAdapter
    {
        Action<T1, T2> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Action<T1, T2>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
#if ENABLE_NEO_MODE
                NeoInvoke(ref ctx);
#else
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2>)dele;
        }
    }

    class MethodDelegateAdapter<T1, T2, T3> : DelegateAdapter
    {
        Action<T1, T2, T3> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Action<T1, T2, T3>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2, T3 p3)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
#if ENABLE_NEO_MODE
                NeoInvoke(ref ctx);
#else
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2, T3>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2, T3>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2, T3>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2, T3>)dele;
        }
    }

    class MethodDelegateAdapter<T1, T2, T3, T4> : DelegateAdapter
    {
        Action<T1, T2, T3, T4> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<T4>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Action<T1, T2, T3, T4>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2, T3 p3, T4 p4)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
                ctx.PushParameter(pTypes[3], p4);
#if ENABLE_NEO_MODE
                NeoInvoke(ref ctx);
#else
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2, T3, T4>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2, T3, T4>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2, T3, T4>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2, T3, T4>)dele;
        }
    }

#if NET_4_6 || NET_STANDARD_2_0
    class MethodDelegateAdapter<T1, T2, T3, T4, T5> : DelegateAdapter
    {
        Action<T1, T2, T3, T4, T5> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<T4>(),
                InvocationContext.GetInvocationType<T5>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Action<T1, T2, T3, T4, T5>);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
#endif
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
                ctx.PushParameter(pTypes[3], p4);
                ctx.PushParameter(pTypes[4], p5);
#if ENABLE_NEO_MODE
                NeoInvoke(ref ctx);
#else
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2, T3, T4, T5>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2, T3, T4, T5>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2, T3, T4, T5>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2, T3, T4, T5>)dele;
        }
    }
#endif

    class MethodDelegateAdapter : DelegateAdapter
    {
        Action action;
        
        public MethodDelegateAdapter()
        {

        }

        protected MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }
        public override Type NativeDelegateType
        {
            get
            {
                return typeof(Action);
            }
        }
        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod()
        {
            var ctx = BeginInvoke();
            try
            {
#if ENABLE_NEO_MODE
                if (method.HasThis && instance != null)
                    ctx.PushObject(instance);
                NeoInvoke(ref ctx);
#else
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
#endif
            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action)dele;
        }
    }

    class DummyDelegateAdapter : DelegateAdapter
    {
        public DummyDelegateAdapter()
        {

        }

        protected DummyDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            
        }
        public override Type NativeDelegateType
        {
            get
            {
                ThrowAdapterNotFound(method);
                return null;
            }
        }
        public override Delegate Delegate
        {
            get
            {
                ThrowAdapterNotFound(method);
                return null;
            }
        }

        void InvokeILMethod()
        {
            if (method.HasThis)
                appdomain.Invoke(method, instance, null);
            else
                appdomain.Invoke(method, null, null);
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new DummyDelegateAdapter(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new DummyDelegateAdapter(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            ThrowAdapterNotFound(method);
        }

        public override void Remove(Delegate dele)
        {
            ThrowAdapterNotFound(method);
        }
    }
    #endregion

    abstract class DelegateAdapter : ILTypeInstance, IDelegateAdapter
    {
        protected ILMethod method;
        protected ILTypeInstance instance;
        protected Enviorment.AppDomain appdomain;
        Dictionary<Type, Delegate> converters;
        IDelegateAdapter next;
        protected bool isClone;

        public abstract Delegate Delegate { get; }

        public abstract Type NativeDelegateType { get; }

        public IDelegateAdapter Next { get { return next; } }

        public ILTypeInstance Instance { get { return instance; } }

        public ILMethod Method { get { return method; } }

        protected DelegateAdapter() { }

        protected DelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            this.appdomain = appdomain;
            this.instance = instance;
            this.method = method;
            CLRInstance = this;
        }

        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }

        public unsafe InvocationContext BeginInvoke()
        {
            var ctx = appdomain.BeginInvoke(method);
#if !ENABLE_NEO_MODE
            *ctx.ESP = default(StackObject);
            ctx.ESP++;//required to simulate delegate invocation
#endif
            return ctx;
        }

#if ENABLE_NEO_MODE
        // Neo-mode dispatch: parameters have already been pushed into ctx (the InvocationFrame).
        // For instance methods, prepend `this` before invocation because InvokeILMethod
        // callers push only the delegate-visible parameters. Multicast: each delegate
        // gets a fresh InvocationContext so their frames don't overlap; only the last
        // return value is preserved.
        internal unsafe void NeoInvoke(ref InvocationContext ctx)
        {
            InsertThisAndInvoke(ref ctx);

            // Multicast: for delegates chained via `next`, replay the same argument list
            // to each additional target. Since arguments were pushed into `ctx` by the
            // caller (typed as T1, T2, ...) and the InvocationContext for the head is
            // now consumed, we need the caller to re-push into a new context. To keep
            // the API stable, we instead walk the chain here by reading the args back
            // from the head frame and forwarding them. Simpler: rely on the fact that
            // delegate action variables in CLR already fan out via CLR multicast, and
            // ILRuntime's `next` chain is only used when a single IDelegateAdapter
            // wraps multiple IL targets. In that rare case we forward args one by one.
            DelegateAdapter n = (DelegateAdapter)next;
            while (n != null)
            {
                using (var ctxNext = appdomain.BeginInvoke(n.method))
                {
                    // Copy pushed arguments from the completed head frame into the new one.
                    // ParameterCount and slot indices are identical because the delegate
                    // signature is shared across the chain.
                    int firstArg = method.HasThis ? 1 : 0;
                    for (int i = 0; i < method.ParameterCount; i++)
                    {
                        // Reads back a boxed CLR representation of the head frame's arg,
                        // then pushes it into the new frame. This is a rare multicast path;
                        // per-call boxing here is acceptable.
                        var argObj = ctx.ReadResult<object>(firstArg + i);
                        ctxNext.PushObject(argObj);
                    }
                    var ctxRef = ctxNext;
                    n.InsertThisAndInvoke(ref ctxRef);
                }
                n = (DelegateAdapter)n.next;
            }
        }

        internal unsafe TResult NeoInvokeAndRead<TResult>(ref InvocationContext ctx, InvocationTypes retType)
        {
            NeoInvoke(ref ctx);
            return ctx.ReadResult<TResult>(retType);
        }

        void InsertThisAndInvoke(ref InvocationContext ctx)
        {
            // Delegate CLR-side push order is (p1, p2, ...); for instance methods the IL
            // frame expects (this, p1, p2, ...). We can't "insert" `this` retroactively into
            // the frame, so instance/HasThis invocations require callers to have pushed
            // `this` themselves. Extension methods behave the same: the extension `this`
            // is the first CLR-visible parameter and is already pushed by the caller.
            if (method.HasThis && !method.IsExtend)
            {
                // Should have been pushed by delegate wrapper — see MethodDelegateAdapter*
                // in Neo #if branches below. If not, that's a caller bug and Invoke() will
                // report argument count mismatch.
            }
            ctx.Invoke();
        }
#endif

        public unsafe StackObject* ILInvoke(ILIntepreter intp, StackObject* esp, AutoList mStack)
        {
            var ebp = esp;
            esp = ILInvokeSub(intp, esp, mStack);
            return ClearStack(intp, esp, ebp, mStack);
        }

        unsafe StackObject* ILInvokeSub(ILIntepreter intp, StackObject* esp, AutoList mStack)
        {
#if ENABLE_NEO_MODE
            // Neo delegates dispatch through NeoInvoke on InvokeILMethod call sites, so
            // this StackObject-based path is unreachable in Neo mode.
            throw new NotSupportedException("Neo mode: DelegateAdapter.ILInvokeSub is not used; delegate dispatch goes through InvocationFrame via NeoInvoke.");
#else
            var ebp = esp;
            bool unhandled;
            if (method.HasThis)
                esp = ILIntepreter.PushObject(esp, mStack, instance);
            int paramCnt = method.ParameterCount;
            if (method.IsExtend && instance != null)
            {
                esp = ILIntepreter.PushObject(esp, mStack, instance);
                paramCnt--;
            }
            bool useRegister = method.ShouldUseRegisterVM;
            for (int i = paramCnt; i > 0; i--)
            {
                intp.CopyToStack(esp, Minus(ebp, i), mStack);
                if (esp->ObjectType < ObjectTypes.Object && useRegister)
                    mStack.Add(null);
                esp++;
            }
            StackObject* ret;
            if (useRegister)
                ret = intp.ExecuteR(method, esp, out unhandled);
            else
                ret = intp.Execute(method, esp, out unhandled);
            if (next != null)
            {
                if (method.ReturnType != appdomain.VoidType)
                {
                    intp.Free(ret - 1);//Return value for multicast delegate doesn't make sense, only return the last one's value
                }
                DelegateAdapter n = (DelegateAdapter)next;
                ret = n.ILInvokeSub(intp, ebp, mStack);

            }
            return ret;
#endif
        }

        unsafe StackObject* ClearStack(ILIntepreter intp, StackObject* esp, StackObject* ebp, AutoList mStack)
        {
            int paramCnt = method.ParameterCount;
            if (method.IsExtend && instance != null)//如果是拓展方法，退一位
            {
                paramCnt--;
            }
            object retObj = null;
            StackObject retSObj = StackObject.Null;
            bool hasReturn = method.ReturnType != appdomain.VoidType;
            if (hasReturn)
            {
                var ret = esp - 1;
                retSObj = *ret;
                if(ret->ObjectType>= ObjectTypes.Object)
                {
                    retObj = mStack[ret->Value];
                    if(retObj == null)
                    {
                        retSObj.ObjectType = ObjectTypes.Null;
                        retSObj.Value = -1;
                        retSObj.ValueLow = 0;
                    }

                    intp.Free(ret);
                }
            }
            for (int i = 1; i <= paramCnt; i++)
            {
                intp.Free(ebp - i);
            }
            var returnVal = Minus(ebp, paramCnt + 1);
            intp.Free(returnVal);//Free delegateInstance
            if (hasReturn)
            {
                *returnVal = retSObj;
                if(retObj != null)
                {
                    returnVal->Value = mStack.Count;
                    mStack.Add(retObj);
                }
                returnVal++;
            }
            return returnVal;
        }

        public abstract IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);

        public new abstract IDelegateAdapter Clone();

        public bool IsClone
        {
            get
            {
                return isClone;
            }
        }

        public virtual void Combine(IDelegateAdapter adapter)
        {
            if (next != null)
                next.Combine(adapter);
            else
                next = adapter;
        }

        public abstract void Combine(Delegate dele);

        public virtual void Remove(IDelegateAdapter adapter)
        {
            if (next != null)
            {
                if (next.Equals(adapter))
                {
                    next = ((DelegateAdapter)next).next;
                }
                else
                    next.Remove(adapter);
            }
        }

        public abstract void Remove(Delegate dele);

        public virtual bool Equals(IDelegateAdapter adapter)
        {
            if (adapter is DelegateAdapter)
            {
                DelegateAdapter b = (DelegateAdapter)adapter;
                return instance == b.instance && method == b.method;
            }
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is DelegateAdapter)
            {
                DelegateAdapter b = (DelegateAdapter)obj;
                return instance == b.instance && method == b.method;
            }
            return false;
        }

        public virtual bool Equals(Delegate dele)
        {
            return Delegate == dele;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return method.ToString();
        }

        public override bool CanAssignTo(IType type)
        {
            if (type.IsDelegate)
            {
                var method_count = method.IsExtend ? method.ParameterCount - 1 : method.ParameterCount;
                var im = type.GetMethod("Invoke", method_count);
                if (im == null)
                {
                    return false;
                }
                var ret_type = im.ReturnType;
                if (im.ReturnType != appdomain.VoidType && type.IsGenericInstance)
                {
                    ret_type = type.GenericArguments[im.ParameterCount].Value;
                }
                if (im.IsDelegateInvoke)
                {
                    if (im.ParameterCount == method_count && method.ReturnType.CanAssignTo(ret_type))
                    {

                        for (int i = 0; i < im.ParameterCount; i++)
                        {
                            var index = method.IsExtend ? i + 1 : i;
                            if (im.Parameters[i] != method.Parameters[index] && (!(im is CLRMethod) || (im.Parameters[i].TypeForCLR != method.Parameters[index].TypeForCLR)))
                                return false;
                        }

                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public Delegate GetConvertor(Type type)
        {
            if (type.IsAssignableFrom(NativeDelegateType))
                return Delegate;
            if (converters == null)
                converters = new Dictionary<System.Type, Delegate>(new ByReferenceKeyComparer<Type>());
            Delegate res;
            if (converters.TryGetValue(type, out res))
                return res;
            else
            {
                res = appdomain.DelegateManager.ConvertToDelegate(type, this);
                converters[type] = res;
                return res;
            }
        }

        unsafe StackObject* Minus(StackObject* a, int b)
        {
            return (StackObject*)((long)a - sizeof(StackObject) * b);
        }

        public static void ThrowAdapterNotFound(IMethod method)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Cannot find Delegate Adapter for:");
            sb.Append(method.ToString());
            string clsName, rName;
            bool isByRef;
            if (method.ReturnType.Name != "Void" || method.ParameterCount > 0)
            {
                sb.AppendLine(", Please add following code:");
                if (method.ReturnType.Name == "Void")
                {
                    sb.Append("appdomain.DelegateManager.RegisterMethodDelegate<");
                    bool first = true;
                    foreach(var i in method.Parameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            sb.Append(", ");
                        }
                        i.TypeForCLR.GetClassName(out clsName, out rName, out isByRef);
                        sb.Append(rName);                        
                    }
                    sb.AppendLine(">();");
                }
                else
                {
                    sb.Append("appdomain.DelegateManager.RegisterFunctionDelegate<");
                    bool first = true;
                    foreach (var i in method.Parameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            sb.Append(", ");
                        }
                        i.TypeForCLR.GetClassName(out clsName, out rName, out isByRef);
                        sb.Append(rName);
                    }
                    if (!first)
                        sb.Append(", ");
                    method.ReturnType.TypeForCLR.GetClassName(out clsName, out rName, out isByRef);
                    sb.Append(rName);
                    sb.AppendLine(">();");
                }
            }
            throw new KeyNotFoundException(sb.ToString());
        }
    }

    public unsafe interface IDelegateAdapter
    {        
        Type NativeDelegateType { get; }
        Delegate Delegate { get; }
        IDelegateAdapter Next { get; }
        ILTypeInstance Instance { get; }
        ILMethod Method { get; }

        InvocationContext BeginInvoke();
        StackObject* ILInvoke(ILIntepreter intp, StackObject* esp, AutoList mStack);
        IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);
        bool IsClone { get; }
        IDelegateAdapter Clone();
        Delegate GetConvertor(Type type);
        void Combine(IDelegateAdapter adapter);
        void Combine(Delegate dele);
        void Remove(IDelegateAdapter adapter);
        void Remove(Delegate dele);
        bool Equals(IDelegateAdapter adapter);
        bool Equals(Delegate dele);
    }
}
