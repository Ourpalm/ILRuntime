using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.Intepreter
{
    class MethodDelegateAdapter<T1> : MethodDelegateAdapter, IDelegateAdapter
    {
        Action<T1> action;
        
        public MethodDelegateAdapter()
        {
            
        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        void InvokeILMethod(T1 p1)
        {
            appdomain.Invoke(method, p1);
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1>(appdomain, instance, method);
        }
    }
    class MethodDelegateAdapter : ILTypeInstance, IDelegateAdapter
    {
        protected ILMethod method;
        protected ILTypeInstance instance;
        protected Enviorment.AppDomain appdomain;
        Action action;

        public MethodDelegateAdapter()
        {

        }

        protected MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            this.appdomain = appdomain;
            this.instance = instance;
            this.method = method;
            action = InvokeILMethod;
        }

        public virtual Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        void InvokeILMethod()
        {
            appdomain.Invoke(method);
        }

        public unsafe StackObject* ILInvoke(ILIntepreter intp, StackObject* esp)
        {
            bool unhandled;
            var ret = intp.Execute(method, esp, out unhandled);
            return ret;
        }

        public unsafe StackObject* ILInvokeVirtual(ILIntepreter intp, StackObject* esp)
        {
            throw new NotImplementedException();
        }

        public virtual IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter(appdomain, instance, method);
        }
    }

    unsafe interface IDelegateAdapter
    {
        Delegate Delegate { get; }
        StackObject* ILInvoke(ILIntepreter intp, StackObject* esp);
        StackObject* ILInvokeVirtual(ILIntepreter intp, StackObject* esp);
        IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);
    }
}
