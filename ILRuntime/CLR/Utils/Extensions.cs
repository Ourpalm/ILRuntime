using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using Mono.Cecil;
namespace ILRuntime.CLR.Utils
{
    static class Extensions
    {
        static List<IType> emptyParamList = new List<IType>();
        public static List<IType> GetParamList(this MethodReference def, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IType contextType)
        {
            if (def.HasParameters)
            {
                List<IType> param = new List<IType>();
                var dt = appdomain.GetType(def.DeclaringType, contextType);
                foreach (var i in def.Parameters)
                {
                    IType t = null;
                    if (i.ParameterType.IsGenericParameter)
                    {
                        if (dt.GenericArguments != null)
                        {
                            foreach (var j in dt.GenericArguments)
                            {
                                if (j.Key == i.ParameterType.Name)
                                {
                                    t = j.Value;
                                    break;
                                }
                            }
                        }                        
                    }
                    if (t == null && def.IsGenericInstance)
                    {
                        GenericInstanceMethod gim = (GenericInstanceMethod)def;
                        for (int j = 0; j < gim.GenericArguments.Count; j++)
                        {
                            var gp = gim.ElementMethod.GenericParameters[j];
                            var ga = gim.GenericArguments[j];
                            if(i.ParameterType.Name == gp.Name)
                            {
                                t = appdomain.GetType(ga, contextType);
                                break;
                            }
                            else if (i.ParameterType.Name.Contains(gp.Name))
                            {
                                t = appdomain.GetType(i.ParameterType.FullName.Replace(gp.Name, ga.FullName));
                                break;
                            }
                        }
                    }
                    if (t == null)
                    {
                        string typeName = i.ParameterType.FullName;

                        t = appdomain.GetType(typeName);
                    }
                    if (t == null)
                        throw new KeyNotFoundException("Cannot find type:" + i.ParameterType);
                    param.Add(t);
                }
                return param;
            }
            else
                return emptyParamList;
        }
    }
}
