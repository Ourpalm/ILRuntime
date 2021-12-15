namespace ILRuntime.CLR.Method
{
    public static class IMethodExtensions
    {
        public static bool IsExtendMethod(this IMethod iMethod)
        {
            if (!iMethod.IsStatic)
            {
                return false;
            }
            if(iMethod is ILMethod)
            {
                var im = iMethod as ILMethod;
                return im.ReflectionMethodInfo.GetCustomAttributes(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false).Length > 0;
            }
            if(iMethod is CLRMethod)
            {
                var cm = iMethod as CLRMethod;
                return cm.MethodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
            }
            return false;
        }

    }
}
