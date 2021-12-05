namespace ILRuntime.CLR.Method
{
    public static class IMethodExtensions
    {
        public static bool IsExtendMethod(this IMethod iLMethod)
        {
            if (!iLMethod.IsStatic)
            {
                return false;
            }
            return iLMethod.ParameterCount > 0 && iLMethod.DeclearingType.ReflectionType.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
        }

    }
}
