using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ILRuntime.Other;

namespace ILRuntimeTest.TestBase
{
    public class ExtensionClass { }
    public class ExtensionClass<TValue> : ExtensionClass { public TValue Value { get; } }
    public class SubExtensionClass : ExtensionClass { }
    public class SubExtensionClass<TValue> : ExtensionClass<TValue> { }
    public static class GenericExtensions
    {
        public static void Method1<T>(this T i, Action<object> a) where T : ExtensionClass { }
        public static void Method1<T>(this T i, Action<T, object> a) where T : ExtensionClass { }

        public static void Method2(this ExtensionClass i, Action<Exception> a) { }
        public static void Method2(this ExtensionClass i, Action<ExtensionClass, Exception> a) { }
        public static void Method2<TException>(this ExtensionClass i, Action<TException> a) where TException : Exception { }
        public static void Method2<TException>(this ExtensionClass i, Action<ExtensionClass, TException> a) where TException : Exception { }
        public static void Method2<TValue>(this ExtensionClass<TValue> i, Action<Exception> a) { }
        public static void Method2<TValue>(this ExtensionClass<TValue> i, Action<ExtensionClass<TValue>, Exception> a) { }
        public static void Method2<TValue, TException>(this ExtensionClass<TValue> i, Action<TException> a) where TException : Exception { }
        public static void Method2<TValue, TException>(this ExtensionClass<TValue> i, Action<ExtensionClass<TValue>, TException> a) where TException : Exception { }

        public static void Method3<T>(this T i, Exception ex) where T : ExtensionClass { }
        public static void Method3<TException>(this ExtensionClass i, TException ex) where TException : Exception { }
        public static void Method3<T, TException>(this T i, TException ex) where T : ExtensionClass where TException : Exception { }
    }

    public static class StaticGenericMethods
    {
        public static void StaticMethod(Action action) { }
        public static void StaticMethod(Action<ExtensionClass> action) { }
        public static void StaticMethod<TValue>(Func<TValue> func) { }
        public static void StaticMethod<TValue>(Func<ExtensionClass, TValue> action) { }

        public static void StaticMethod(Func<Task> func) { }
        public static void StaticMethod(Func<ExtensionClass, Task> func) { }
        public static void StaticMethod<TValue>(Func<Task<TValue>> func) { }
        public static void StaticMethod<TValue>(Func<ExtensionClass, Task<TValue>> func) { }
        public static void Method(string name, params KeyValuePair<string, string[]>[] panels)
        {
            Console.WriteLine("进来了");
        }
    }
}
