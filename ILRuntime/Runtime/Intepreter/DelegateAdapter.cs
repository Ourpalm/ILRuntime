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

        public override void Combine(Delegate dele)
        {
            action += (Action<T1>)dele;
        }
    }
    class MethodDelegateAdapter : DelegateAdapter
    {
        Action action;
        
        public MethodDelegateAdapter()
        {

        }

        protected MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            this.appdomain = appdomain;
            this.instance = instance;
            this.method = method;
        }

        public override Delegate Delegate
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

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            var res = new MethodDelegateAdapter(appdomain, instance, method);
            res.action = res.InvokeILMethod;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action)dele;
        }
    }

    abstract class DelegateAdapter : ILTypeInstance, IDelegateAdapter
    {
        protected ILMethod method;
        protected ILTypeInstance instance;
        protected Enviorment.AppDomain appdomain;
        IDelegateAdapter next;

        public abstract Delegate Delegate { get; }

        public unsafe StackObject* ILInvoke(ILIntepreter intp, StackObject* esp)
        {
            bool unhandled;
            var ret = intp.Execute(method, esp, out unhandled);
            if (next != null)
                ret = next.ILInvoke(intp, esp);
            return ret;
        }

        public unsafe StackObject* ILInvokeVirtual(ILIntepreter intp, StackObject* esp)
        {
            throw new NotImplementedException();
        }

        public abstract IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);

        public virtual void Combine(IDelegateAdapter adapter)
        {
            if (next != null)
                next.Combine(adapter);
            else
                next = adapter;
        }

        public abstract void Combine(Delegate dele);

        public override string ToString()
        {
            return method.ToString();
        }
    }

    unsafe interface IDelegateAdapter
    {
        Delegate Delegate { get; }
        StackObject* ILInvoke(ILIntepreter intp, StackObject* esp);
        StackObject* ILInvokeVirtual(ILIntepreter intp, StackObject* esp);
        IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);
        void Combine(IDelegateAdapter adapter);
        void Combine(Delegate dele);
    }
}
