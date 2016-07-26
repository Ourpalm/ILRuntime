using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Mono.Cecil;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Utils;
namespace ILRuntime.CLR.Method
{
    class CLRMethod : IMethod
    {
        MethodInfo def;
        ConstructorInfo cDef;
        List<IType> parameters;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        CLRType declaringType;
        ParameterInfo[] param;
        bool isConstructor;
        Func<object, object[], IType[], object> redirect;
        IType[] genericArguments;
        object[] invocationParam;

        public IType DeclearingType
        {
            get
            {
                return declaringType;
            }
        }
        public string Name
        {
            get
            {
                return def.Name;
            }
        }
        public bool HasThis
        {
            get
            {
                return isConstructor ? !cDef.IsStatic : !def.IsStatic;
            }
        }
        public int GenericParameterCount
        {
            get
            {
                if (def.ContainsGenericParameters && def.IsGenericMethodDefinition)
                {
                    return def.GetGenericArguments().Length;
                }
                return 0;
            }
        }
        public bool IsGenericInstance
        {
            get
            {
                return genericArguments != null;
            }
        }

        public IType[] GenericArguments { get { return genericArguments; } }

        public CLRMethod(MethodInfo def, CLRType type, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.def = def;
            declaringType = type;
            this.appdomain = domain;
            param = def.GetParameters();
            if (!def.IsGenericMethod && !def.ContainsGenericParameters)
            {
                ReturnType = domain.GetType(def.ReturnType.FullName);
                InitParameters();
            }
            isConstructor = false;
        }
        public CLRMethod(ConstructorInfo def, CLRType type, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.cDef = def;
            declaringType = type;
            this.appdomain = domain;
            param = def.GetParameters();
            if (!def.IsGenericMethod && !def.ContainsGenericParameters)
            {
                ReturnType = type;
                InitParameters();
            }
            isConstructor = true;
        }

        public int ParameterCount
        {
            get
            {
                return param != null ? param.Length : 0;
            }
        }


        public List<IType> Parameters
        {
            get
            {
                if (parameters == null)
                {
                    InitParameters();
                }
                return parameters;
            }
        }

        public IType ReturnType
        {
            get;
            private set;
        }

        public bool IsConstructor
        {
            get
            {
                return cDef != null;
            }
        }

        void InitParameters()
        {
            parameters = new List<IType>();
            foreach (var i in param)
            {
                IType type = appdomain.GetType(i.ParameterType.FullName);
                parameters.Add(type);
            }
            if (def != null)
            {
                if (def.IsGenericMethod && !def.IsGenericMethodDefinition)
                {
                    appdomain.RedirectMap.TryGetValue(def.GetGenericMethodDefinition(), out redirect);
                }
                else
                    appdomain.RedirectMap.TryGetValue(def, out redirect);
            }
        }

        public unsafe object Invoke(StackObject* esp, List<object> mStack,bool isNewObj=false)
        {
            if (parameters == null)
            {
                InitParameters();
            }
            int paramCount = ParameterCount;
            if (invocationParam == null)
                invocationParam = new object[paramCount];
            object[] param = invocationParam;
            for (int i = 1; i <= paramCount; i++)
            {
                var p = esp - i;
                var obj = this.param[i - 1].ParameterType.CheckPrimitiveTypes(p->ToObject(appdomain, mStack));

                param[paramCount - i] = obj;
            }

            if (isConstructor)
            {
                if (!isNewObj)
                {
                    if (!cDef.IsStatic)
                    {
                        object instance = declaringType.TypeForCLR.CheckPrimitiveTypes((esp - paramCount - 1)->ToObject(appdomain, mStack));
                        if (instance == null)
                            throw new NullReferenceException();
                        cDef.Invoke(instance, param);
                        return null;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    var res = cDef.Invoke(param);
                    return res;
                }
               
            }
            else
            {
                object instance = null;

                if (!def.IsStatic)
                {
                    instance = declaringType.TypeForCLR.CheckPrimitiveTypes((esp - paramCount - 1)->ToObject(appdomain, mStack));
                    if (instance == null)
                        throw new NullReferenceException();
                }
                object res = null;
                if (redirect != null)
                    res = redirect(instance, param, genericArguments);
                else
                    res = def.Invoke(instance, param);
                return res;
            }
        }

        public IMethod MakeGenericMethod(IType[] genericArguments)
        {
            Type[] p = new Type[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                p[i] = genericArguments[i].TypeForCLR;
            }
            var t = def.MakeGenericMethod(p);
            var res = new CLRMethod(t, declaringType, appdomain);
            res.genericArguments = genericArguments;
            return res;
        }
    }
}
