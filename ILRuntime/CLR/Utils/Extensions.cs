using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.CLR.TypeSystem;
using Mono.Cecil;
namespace ILRuntime.CLR.Utils
{
    static class Extensions
    {
        static List<IType> emptyParamList = new List<IType>();
        public static List<IType> GetParamList(this MethodReference def, ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            if (def.HasParameters)
            {
                List<IType> param = new List<IType>();
                foreach (var i in def.Parameters)
                {
                    string typeName = i.ParameterType.FullName;

                    var t = appdomain.GetType(typeName);
                    if (t == null)
                        throw new KeyNotFoundException("Cannot find type:" + typeName);
                    param.Add(t);
                }
                return param;
            }
            else
                return emptyParamList;
        }
    }
}
