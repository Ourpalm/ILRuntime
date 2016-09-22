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
            if (method.HasThis)
                appdomain.Invoke(method, instance, p1);
            else
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

        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }

        public unsafe StackObject* ILInvoke(ILIntepreter intp, StackObject* esp, List<object> mStack)
        {
            var ebp = esp;
            esp = ILInvokeSub(intp, esp, mStack);
            return ClearStack(intp, esp, ebp, mStack);
        }

        unsafe StackObject* ILInvokeSub(ILIntepreter intp, StackObject* esp, List<object> mStack)
        {
            var ebp = esp;
            bool unhandled;
            if (method.HasThis)
                esp = intp.PushObject(esp, mStack, instance);
            int paramCnt = method.ParameterCount;
            for(int i = paramCnt; i > 0; i--)
            {
                intp.CopyToStack(esp, ebp - paramCnt, mStack);
                esp++;
            }
            var ret = intp.Execute(method, esp, out unhandled);
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
        }

        unsafe StackObject* ClearStack(ILIntepreter intp, StackObject* esp, StackObject* ebp, List<object> mStack)
        {
            int paramCnt = method.ParameterCount;
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
                }
                intp.Free(ret);
            }
            for (int i = 1; i <= paramCnt; i++)
            {
                intp.Free(ebp - i);
            }
            var returnVal = ebp - paramCnt - 1;
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
        StackObject* ILInvoke(ILIntepreter intp, StackObject* esp, List<object> mStack);
        IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);
        void Combine(IDelegateAdapter adapter);
        void Combine(Delegate dele);
    }
}
