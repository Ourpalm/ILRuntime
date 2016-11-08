using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTools
{
    public static class ReflectionExt
    {
        public static readonly List<AccessModifier> AccessModifiers = new List<AccessModifier>
        {
            AccessModifier.Private,
            AccessModifier.Protected,
            AccessModifier.Internal,
            AccessModifier.Public
        };

        public static AccessModifier Accessmodifier(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.SetMethod == null)
                return propertyInfo.GetMethod.Accessmodifier();
            if (propertyInfo.GetMethod == null)
                return propertyInfo.SetMethod.Accessmodifier();
            var max = Math.Max(AccessModifiers.IndexOf(propertyInfo.GetMethod.Accessmodifier()),
                AccessModifiers.IndexOf(propertyInfo.SetMethod.Accessmodifier()));
            return AccessModifiers[max];
        }

        public static AccessModifier Accessmodifier(this MethodInfo methodInfo)
        {
            if (methodInfo.IsPrivate)
                return AccessModifier.Private;
            if (methodInfo.IsFamily)
                return AccessModifier.Protected;
            if (methodInfo.IsAssembly)
                return AccessModifier.Internal;
            if (methodInfo.IsPublic)
                return AccessModifier.Public;
            throw new ArgumentException("Did not find access modifier", "methodInfo");
        }
    }
    public enum AccessModifier
    {
        Private,
        Protected,
        Internal,
        Public
    }
}
