using System;

namespace ILRuntimeTest.TestBase
{
    public class ExtensionClass { }

    public class SubExtensionClass : ExtensionClass { }

    public static class GenericExtensions
    {
        public static void ExtensionMethod<TException>(this ExtensionClass instance, TException ex) where TException : Exception { }

        public static void ExtensionMethod<T>(this T instance, Exception ex) where T : ExtensionClass { }
    }
}
