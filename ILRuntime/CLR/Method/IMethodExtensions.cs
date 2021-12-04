namespace ILRuntime.CLR.Method
{
    public static class IMethodExtensions
    {
        public static bool IsExtendMethod(this IMethod iLMethod)
        {
            return iLMethod.ParameterCount > 0 && iLMethod.DeclearingType.ReflectionType.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
        }

    }
}
