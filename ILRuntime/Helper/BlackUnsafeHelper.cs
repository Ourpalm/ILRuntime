using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Helper
{
    public static class BlackUnsafeHelper
    {
        static bool HasAttribute(this Type type, string attributeName)
        {
            var attrs = type.GetCustomAttributes();
            foreach (var attr in attrs)
            {
                if (attr.GetType().FullName.EndsWith(attributeName))
                {
                    return true;
                }
            }
            if (type.IsNested)
            {
                return HasAttribute(type.DeclaringType, attributeName);
            }
            return false;
        }

        public static bool IsBlack(this Type type)
        {
            return HasAttribute(type, "BlackAttribute");
        }
        public static bool IsUnsafe(this MethodInfo methodInfo)
        {
            if (HasUnsafeParameters(methodInfo))
            {
                return true;
            }

            return methodInfo.ReturnType.IsPointer || methodInfo.ReturnType.FullName == "System.IntPtr";
        }

        static bool HasUnsafeParameters(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            bool hasUnsafe = parameters.Any(p => p.ParameterType.IsPointer);
            hasUnsafe = hasUnsafe || parameters.Any(p => p.ParameterType.FullName == "System.IntPtr");
            return hasUnsafe;
        }

        public static bool IsBlack(this MethodBase methodBase)
        {
            var attrs = methodBase.GetCustomAttributes();
            foreach (var attr in attrs)
            {
                if (attr.GetType().FullName.EndsWith("BlackAttribute"))
                {
                    return true;
                }
            }
            if (methodBase.IsSpecialName && (methodBase.Name.StartsWith("get_") || methodBase.Name.StartsWith("set_")))
            {
                var flag = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var propertys = methodBase.ReflectedType.GetProperties(flag);
                string name = methodBase.Name.Substring(4);
                foreach (var ator in propertys)
                {
                    if (ator.Name == name)
                    {
                        var atorAttrs = ator.GetCustomAttributes();
                        foreach (var atorAttr in atorAttrs)
                        {
                            if (atorAttr.GetType().FullName.EndsWith("BlackAttribute"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
