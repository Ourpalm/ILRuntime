using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Cecil = ILRuntime.Mono.Cecil;
using Cil = ILRuntime.Mono.Cecil.Cil;

namespace ILRuntimeTest.TestFramework
{
    public static class NeoStep14CLRHost
    {
        public static Exception CreateException(bool argument)
        {
            if (argument)
                return new ArgumentException("Step14 nested");

            return new InvalidOperationException("Step14 original");
        }

        public static void AssertSame(object expected, object actual)
        {
            if (!ReferenceEquals(expected, actual))
                throw new Exception("Step14 exception identity changed");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowFromCLR(Exception exception)
        {
            throw exception;
        }

        private static ILMethod Method(AppDomain app, string name, int count)
        {
            return (ILMethod)app.LoadedTypes["TestCases.NeoStep14Targets"].GetMethod(name, count);
        }

        private static object Invoke(AppDomain app, string name, params object[] args)
        {
            return app.Invoke(Method(app, name, args.Length), null, args);
        }

        private static void Equal(object expected, object actual)
        {
            if (!Equals(expected, actual))
                throw new Exception($"Step14 expected {expected}, got {actual}");
        }

        private static void Same(object expected, object actual)
        {
            if (!ReferenceEquals(expected, actual))
                throw new Exception($"Step14 exception identity lost: {actual}");
        }

        private static Exception Capture(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                return ex;
            }

            throw new Exception("Step14 expected an exception");
        }

        public static void Test(AppDomain app, string name)
        {
#if ENABLE_NEO_MODE
            var original = CreateException(false);
            var nested = CreateException(true);
            switch (name)
            {
                case "Rethrow":
                case "NestedCatchRethrow":
                    var caught = Capture(() => Invoke(app, name, name == "Rethrow"
                        ? new object[] { original }
                        : new object[] { original, nested }));
                    Same(original, caught);
                    if (!caught.StackTrace.Contains(nameof(ThrowFromCLR)))
                        throw new Exception("Step14 rethrow lost CLR origin");
                    if (caught.Data["StackTrace"] == null)
                        throw new Exception("Step14 missing ILRuntime diagnostic");
                    break;

                case "NoMatchingCatch":
                    Same(original, Capture(() => Invoke(app, name, original)));
                    break;

                case "FinallyNestedRethrow":
                    Same(nested, Capture(() => Invoke(app, name, original, nested)));
                    break;

                case "CatchCallsHandledMethod":
                    Same(original, Capture(() => Invoke(app, name, original, nested)));
                    break;

                case "ReuseInterpreter":
                    TestReuse(app, original);
                    break;

                case "RejectClauses":
                    TestRejectedClauses();
                    break;

                case "HandlerAtMethodEnd":
                    TestHandlerAtEnd(original);
                    break;

                default:
                    throw new Exception("Unknown Step14 CLR boundary test: " + name);
            }
#endif
        }

#if ENABLE_NEO_MODE
        private static object StackOf(ILIntepreter intp)
        {
            return typeof(ILIntepreter)
                .GetProperty("Stack", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(intp);
        }

        private static int Count(object stack, string property)
        {
            object collection = stack.GetType().GetProperty(property).GetValue(stack);
            return (int)collection.GetType().GetProperty("Count").GetValue(collection);
        }

        private static void TestReuse(AppDomain app, Exception original)
        {
            var intp = new ILIntepreter(app);
            object stack = StackOf(intp);
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    Same(original, Capture(() => intp.Run(
                        Method(app, "ThrowHelper", 1),
                        null,
                        new object[] { original })));
                    Equal(0, Count(stack, "Frames"));
                    Equal(0, Count(stack, "ManagedStack"));

                    var failure = Capture(() => intp.Run(Method(app, "Unsupported", 1), null, new object[] { 0 }));
                    if (!(failure is NotImplementedException))
                        throw new Exception("Step14 fail-fast changed type", failure);
                    Equal(0, Count(stack, "Frames"));
                    Equal(0, Count(stack, "ManagedStack"));

                    Same(original, intp.Run(Method(app, "CrossFrame", 1), null, new object[] { original }));
                    Equal(0, Count(stack, "Frames"));
                    Equal(0, Count(stack, "ManagedStack"));

                    Equal(42, intp.Run(Method(app, "Identity", 1), null, new object[] { 42 }));
                    Equal(0, Count(stack, "Frames"));
                    Equal(0, Count(stack, "ManagedStack"));
                }
            }
            finally
            {
                ((IDisposable)stack).Dispose();
            }
        }
        private static void TestHandlerAtEnd(Exception original)
        {
            using (var module = Cecil.ModuleDefinition.CreateModule("NeoStep14End", Cecil.ModuleKind.Dll))
            using (var bytes = new MemoryStream())
            {
                var type = new Cecil.TypeDefinition(
                    "Tests",
                    "End",
                    Cecil.TypeAttributes.Public | Cecil.TypeAttributes.Class,
                    module.TypeSystem.Object);
                module.Types.Add(type);

                var method = new Cecil.MethodDefinition(
                    "RethrowAtEnd",
                    Cecil.MethodAttributes.Public | Cecil.MethodAttributes.Static,
                    module.TypeSystem.Void);
                method.Parameters.Add(new Cecil.ParameterDefinition(module.ImportReference(typeof(Exception))));
                type.Methods.Add(method);

                var il = method.Body.GetILProcessor();
                var start = il.Create(Cil.OpCodes.Ldarg_0);
                var handler = il.Create(Cil.OpCodes.Pop);

                il.Append(start);
                il.Append(il.Create(Cil.OpCodes.Throw));
                il.Append(handler);
                il.Append(il.Create(Cil.OpCodes.Rethrow));

                method.Body.ExceptionHandlers.Add(new Cil.ExceptionHandler(Cil.ExceptionHandlerType.Catch)
                {
                    TryStart = start,
                    TryEnd = handler,
                    HandlerStart = handler,
                    HandlerEnd = null,
                    CatchType = module.ImportReference(typeof(Exception))
                });

                module.Write(bytes);
                bytes.Position = 0;

                var app = new AppDomain(ILRuntime.Runtime.ILRuntimeJITFlags.JITNeo);
                app.LoadAssembly(bytes);
                var target = (ILMethod)app.LoadedTypes["Tests.End"].GetMethod(method.Name, 1);
                Same(original, Capture(() => app.Invoke(target, null, original)));
            }
        }

        private static void TestRejectedClauses()
        {
            foreach (var kind in new[] { Cil.ExceptionHandlerType.Filter, Cil.ExceptionHandlerType.Fault })
            {
                using (var module = Cecil.ModuleDefinition.CreateModule(
                    "NeoStep14Unsupported" + kind,
                    Cecil.ModuleKind.Dll))
                using (var bytes = new MemoryStream())
                {
                    var type = new Cecil.TypeDefinition(
                        "Tests",
                        "Rejected",
                        Cecil.TypeAttributes.Public | Cecil.TypeAttributes.Class,
                        module.TypeSystem.Object);
                    module.Types.Add(type);

                    var method = new Cecil.MethodDefinition(
                        "Reject" + kind,
                        Cecil.MethodAttributes.Public | Cecil.MethodAttributes.Static,
                        module.TypeSystem.Void);
                    type.Methods.Add(method);

                    var il = method.Body.GetILProcessor();
                    var start = il.Create(Cil.OpCodes.Nop);
                    var ret = il.Create(Cil.OpCodes.Ret);

                    il.Append(start);
                    il.Append(il.Create(Cil.OpCodes.Leave, ret));

                    Cil.Instruction filter = null;
                    if (kind == Cil.ExceptionHandlerType.Filter)
                    {
                        filter = il.Create(Cil.OpCodes.Pop);
                        il.Append(filter);
                        il.Append(il.Create(Cil.OpCodes.Ldc_I4_1));
                        il.Append(il.Create(Cil.OpCodes.Endfilter));
                    }

                    var handler = il.Create(kind == Cil.ExceptionHandlerType.Filter
                        ? Cil.OpCodes.Pop
                        : Cil.OpCodes.Nop);
                    il.Append(handler);
                    il.Append(kind == Cil.ExceptionHandlerType.Filter
                        ? il.Create(Cil.OpCodes.Leave, ret)
                        : il.Create(Cil.OpCodes.Endfinally));
                    il.Append(ret);

                    method.Body.ExceptionHandlers.Add(new Cil.ExceptionHandler(kind)
                    {
                        TryStart = start,
                        TryEnd = filter ?? handler,
                        FilterStart = filter,
                        HandlerStart = handler,
                        HandlerEnd = ret
                    });

                    module.Write(bytes);
                    bytes.Position = 0;

                    var app = new AppDomain(ILRuntime.Runtime.ILRuntimeJITFlags.JITNeo);
                    app.LoadAssembly(bytes);
                    var target = (ILMethod)app.LoadedTypes["Tests.Rejected"].GetMethod(method.Name, 0);
                    var intp = new ILIntepreter(app);
                    object stack = StackOf(intp);
                    try
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            var failure = Capture(() => intp.Run(target, null, null));
                            if (!(failure is NotSupportedException)
                                || !failure.Message.Contains(kind.ToString())
                                || !failure.Message.Contains(method.Name))
                                throw new Exception("Step14 missing unsupported clause diagnostic", failure);
                            Equal(0, Count(stack, "Frames"));
                            Equal(0, Count(stack, "ManagedStack"));
                        }
                    }
                    finally
                    {
                        ((IDisposable)stack).Dispose();
                    }
                }
            }
        }
#endif
    }
}
