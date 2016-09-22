using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Enviorment
{
    public class DelegateManager
    {
        List<DelegateMapNode> methods = new List<DelegateMapNode>();
        IDelegateAdapter zeroParamMethodAdapter = new MethodDelegateAdapter();
        Dictionary<Type, Func<Delegate, Delegate>> clrDelegates = new Dictionary<Type, Func<Delegate, Delegate>>();
        Enviorment.AppDomain appdomain;
        public DelegateManager(Enviorment.AppDomain appdomain)
        {
            this.appdomain = appdomain;
        }

        public void RegisterDelegateConvertor<T>(Func<Delegate, Delegate> action)
        {
            var type = typeof(T);
            if (type.IsSubclassOf(typeof(Delegate)))
            {
                clrDelegates[type] = action;
            }
            else
                throw new NotSupportedException();
        }

        public void RegisterMethodDelegate<T1>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1>();
            node.ParameterTypes = new Type[] { typeof(T1) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1>>((dele) => dele);
        }

        internal Delegate ConvertToDelegate(Type clrDelegateType, IDelegateAdapter adapter)
        {
            Func<Delegate, Delegate> func;
            if (clrDelegates.TryGetValue(clrDelegateType, out func))
            {
                return func(adapter.Delegate);
            }
            else
                throw new KeyNotFoundException("Cannot find convertor for " + clrDelegateType);
        }

        internal IDelegateAdapter FindDelegateAdapter(ILTypeInstance instance, ILMethod method)
        {
            if(method.ReturnType == appdomain.VoidType)
            {
                if (method.ParameterCount == 0)
                    return zeroParamMethodAdapter;
                foreach(var i in methods)
                {
                    if(i.ParameterTypes.Length == method.ParameterCount)
                    {
                        bool match = true;
                        for(int j = 0; j < method.ParameterCount; j++)
                        {
                            if (i.ParameterTypes[j] != method.Parameters[j].TypeForCLR)
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                            return i.Adapter.Instantiate(appdomain, instance, method);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            throw new KeyNotFoundException("Cannot find Delegate Adapter for:" + method);
        }

        public void RegisterFunctionDelegate<T1, TReturn>()
        {

        }

        class DelegateMapNode
        {
            public IDelegateAdapter Adapter { get; set; }
            public Type[] ParameterTypes { get; set; }
        }
    }
}
