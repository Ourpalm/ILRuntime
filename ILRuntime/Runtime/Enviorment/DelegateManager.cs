using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Enviorment
{
    class DelegateManager
    {
        List<DelegateMapNode> methods = new List<DelegateMapNode>();
        Dictionary<Type, Func<IDelegateAdapter, Delegate>> clrDelegates = new Dictionary<Type, Func<IDelegateAdapter, Delegate>>();
        Enviorment.AppDomain appdomain;
        public DelegateManager(Enviorment.AppDomain appdomain)
        {
            this.appdomain = appdomain;
        }

        public void RegisterDelegateConvertor<T>(Func<IDelegateAdapter, Delegate> action)
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
        }

        public Delegate ConvertToDelegate(Type clrDelegateType, IDelegateAdapter adapter)
        {
            Func<IDelegateAdapter, Delegate> func;
            if (clrDelegates.TryGetValue(clrDelegateType, out func))
            {
                return func(adapter);
            }
            else
                return null;
        }

        public IDelegateAdapter FindDelegateAdapter(ILTypeInstance instance, ILMethod method)
        {
            if(method.ReturnType == appdomain.VoidType)
            {
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
                            return i.Adapter;
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
