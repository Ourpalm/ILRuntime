using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;
namespace ILRuntime.CLR.TypeSystem
{
    class CLRType : IType
    {
        Type clrType;
        Dictionary<string, List<CLRMethod>> methods;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        List<CLRMethod> constructors;
        KeyValuePair<string,IType>[] genericArguments;
        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
        {
            get
            {
                return appdomain;
            }
        }

        public CLRType(Type clrType, Runtime.Enviorment.AppDomain appdomain)
        {
            this.clrType = clrType;
            this.appdomain = appdomain;
        }
        
        public bool IsGenericInstance
        {
            get
            {
                return genericArguments != null;
            }
        }

        public KeyValuePair<string, IType>[] GenericArguments
        {
            get
            {
                return genericArguments;
            }
        }

        public Type TypeForCLR
        {
            get
            {
                return clrType;
            }
        }

        public string FullName
        {
            get
            {
                return clrType.FullName;
            }
        }

        void InitializeMethods()
        {
            methods = new Dictionary<string, List<CLRMethod>>();
            constructors = new List<CLRMethod>();
            foreach (var i in clrType.GetMethods())
            {
                List<CLRMethod> lst;
                if (!methods.TryGetValue(i.Name, out lst))
                {
                    lst = new List<CLRMethod>();
                    methods[i.Name] = lst;
                }
                lst.Add(new CLRMethod(i, this, appdomain));
            }
            foreach(var i in clrType.GetConstructors())
            {
                constructors.Add(new CLRMethod(i, this, appdomain));
            }
        }
        public List<IMethod> GetMethods()
        {
            if (methods == null)
                InitializeMethods();
            List<IMethod> res = new List<IMethod>();
            foreach (var i in methods)
            {
                foreach (var j in i.Value)
                    res.Add(j);
            }

            return res;
        }
        public IMethod GetMethod(string name, int paramCount)
        {
            if (methods == null)
                InitializeMethods();
            List<CLRMethod> lst;
            if (methods.TryGetValue(name, out lst))
            {
                foreach (var i in lst)
                {
                    if (i.ParameterCount == paramCount)
                        return i;
                }
            }
            return null;
        }

        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments)
        {
            if (methods == null)
                InitializeMethods();
            List<CLRMethod> lst;
            IMethod genericMethod = null;
            if (methods.TryGetValue(name, out lst))
            {
                foreach (var i in lst)
                {
                    if (i.ParameterCount == param.Count)
                    {
                        bool match = true;
                        if (genericArguments != null && i.GenericParameterCount == genericArguments.Length)
                        {
                            genericMethod = i;
                        }
                        else
                        {
                            for (int j = 0; j < param.Count; j++)
                            {
                                if (param[j].TypeForCLR != i.Parameters[j].TypeForCLR)
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                                return i;
                        }
                    }
                }
            }
            if (genericArguments != null && genericMethod != null)
            {
                var m = genericMethod.MakeGenericMethod(genericArguments);
                lst.Add((CLRMethod)m);
                return m;
            }
            return null;
        }
        public bool CanAssignTo(IType type)
        {
            if (this == type)
            {
                return true;
            }
            else
                return false;
        }

        public IMethod GetConstructor(List<IType> param)
        {
            if (constructors == null)
                InitializeMethods();
            foreach (var i in constructors)
            {
                if (i.ParameterCount == param.Count)
                    return i;
            }
            return null;
        }

        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            Type[] args = new Type[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                args[i] = genericArguments[i].Value.TypeForCLR;
            }
            Type newType = clrType.MakeGenericType(args);
            var res = new CLRType(newType, appdomain);
            res.genericArguments = genericArguments;
            return res;
        }
    }
}
