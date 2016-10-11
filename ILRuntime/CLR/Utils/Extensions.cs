using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using Mono.Cecil;
using ILRuntime.Runtime.Intepreter;
namespace ILRuntime.CLR.Utils
{
    public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);

    static class Extensions
    {
        public static List<IType> EmptyParamList = new List<IType>();
        public static List<IType> GetParamList(this MethodReference def, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IType contextType)
        {
            if (def.HasParameters)
            {
                List<IType> param = new List<IType>();
                var dt = appdomain.GetType(def.DeclaringType, contextType);
                foreach (var i in def.Parameters)
                {
                    IType t = null;
                    t = appdomain.GetType(i.ParameterType, dt);
                    if (t == null && def.IsGenericInstance)
                    {
                        GenericInstanceMethod gim = (GenericInstanceMethod)def;
                        string name = i.ParameterType.FullName;
                        for (int j = 0; j < gim.GenericArguments.Count; j++)
                        {
                            var gp = gim.ElementMethod.GenericParameters[j];
                            var ga = gim.GenericArguments[j];
                            if (name == gp.Name)
                            {
                                t = appdomain.GetType(ga, contextType);
                                break;
                            }
                            else if (name.Contains(gp.Name))
                            {
                                name = name.Replace(gp.Name, ga.FullName);
                            }
                        }
                        if (t == null)
                            t = appdomain.GetType(name);
                    }
                    if (t == null)
                        throw new KeyNotFoundException("Cannot find type:" + i.ParameterType);
                    param.Add(t);
                }
                return param;
            }
            else
                return EmptyParamList;
        }

        public static object CheckCLRTypes(this Type pt, Runtime.Enviorment.AppDomain domain, object obj)
        {
            if (obj == null)
                return null;
            if (pt.IsPrimitive && pt != typeof(int))
            {
                if (pt == typeof(bool) && !(obj is bool))
                {
                    obj = (int)obj == 1;
                }
                else if (pt == typeof(byte) && !(obj is byte))
                    obj = (byte)(int)obj;
                else if (pt == typeof(short) && !(obj is short))
                    obj = (short)(int)obj;
                else if (pt == typeof(char) && !(obj is char))
                    obj = (char)(int)obj;
                else if (pt == typeof(ushort) && !(obj is ushort))
                    obj = (ushort)(int)obj;
                else if (pt == typeof(uint) && !(obj is uint))
                    obj = (uint)(int)obj;
                else if (pt == typeof(sbyte) && !(obj is sbyte))
                    obj = (sbyte)(int)obj;
                else if (pt == typeof(ulong) && !(obj is ulong))
                {
                    obj = (ulong)(long)obj;
                }
            }
            else if (pt == typeof(Delegate) || pt.IsSubclassOf(typeof(Delegate)))
            {
                if (obj is Delegate)
                    return obj;
                if (pt == typeof(Delegate))
                    return ((IDelegateAdapter)obj).Delegate;
                return ((IDelegateAdapter)obj).GetConvertor(pt);
            }
            else if (pt.IsByRef)
            {
                return CheckCLRTypes(pt.GetElementType(), domain, obj);
            }
            else if (pt.IsEnum)
            {
                return Enum.ToObject(pt, obj);
            }
            else if(obj is ILTypeInstance)
            {
                return ((ILTypeInstance)obj).CLRInstance;
            }
            return obj;
        }
    }
}
