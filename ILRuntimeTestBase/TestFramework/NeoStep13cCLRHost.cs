using System;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace ILRuntimeTest.TestFramework
{
    // Step 13c CLR-side test host: exercises the three external entry points migrated to
    // InvocationFrame in Step 13c (InvocationContext.Invoke / MethodInfo.Invoke). IL-side
    // wrappers in TestCases.NeoStep13cTest grab AppDomain via TestSession.LastSession.Appdomain
    // and forward here so the real BeginInvoke / reflection calls happen on the CLR side
    // against IL targets — that is the only way to actually exercise the migrated code.
    //
    // Each method throws a descriptive Exception on failure (via AssertEqual helpers).
    // The IL-side wrapper does not need to inspect a return value.
    public static class NeoStep13cCLRHost
    {
        // BeginInvoke → PushInteger x2 → Invoke → ReadResult<int>
        public static void TestBeginInvokeAddInts(AppDomain app)
        {
            var m = ResolveIL(app, "TestCases.NeoStep13cTargets", "AddInts", 2);
            using (var ctx = app.BeginInvoke(m))
            {
                ctx.PushInteger(17);
                ctx.PushInteger(25);
                ctx.Invoke();
                int r = ctx.ReadResult<int>();
                AssertEqual("TestBeginInvokeAddInts result", 42, r);
            }
        }

        // BeginInvoke + PushLong / ReadResult<long>
        public static void TestBeginInvokeAddLongs(AppDomain app)
        {
            var m = ResolveIL(app, "TestCases.NeoStep13cTargets", "AddLongs", 2);
            using (var ctx = app.BeginInvoke(m))
            {
                ctx.PushInteger(0x100000000L);
                ctx.PushInteger(0x200000000L);
                ctx.Invoke();
                long r = ctx.ReadResult<long>();
                AssertEqual("TestBeginInvokeAddLongs result", 0x300000000L, r);
            }
        }

        // BeginInvoke + PushFloat / PushDouble mix
        public static void TestBeginInvokeMixedFloats(AppDomain app)
        {
            var m = ResolveIL(app, "TestCases.NeoStep13cTargets", "AddFloatDouble", 2);
            using (var ctx = app.BeginInvoke(m))
            {
                ctx.PushFloat(1.5f);
                ctx.PushDouble(2.25);
                ctx.Invoke();
                double r = ctx.ReadResult<double>();
                AssertEqual("TestBeginInvokeMixedFloats result", 3.75, r);
            }
        }

        // BeginInvoke + PushObject(string) → ReadResult<int> (StringLength)
        public static void TestBeginInvokeStringParam(AppDomain app)
        {
            var m = ResolveIL(app, "TestCases.NeoStep13cTargets", "StringLength", 1);
            using (var ctx = app.BeginInvoke(m))
            {
                ctx.PushObject("Hello, world!");
                ctx.Invoke();
                int r = ctx.ReadResult<int>();
                AssertEqual("TestBeginInvokeStringParam result", 13, r);
            }
        }

        // BeginInvoke against a void method: verify Invoke + Dispose don't throw and the
        // side-effect (IL static counter) is observable across multiple invocations.
        public static void TestBeginInvokeVoid(AppDomain app)
        {
            var reset = ResolveIL(app, "TestCases.NeoStep13cTargets", "ResetCounter", 0);
            var inc = ResolveIL(app, "TestCases.NeoStep13cTargets", "IncrementCounter", 0);
            var read = ResolveIL(app, "TestCases.NeoStep13cTargets", "ReadCounter", 0);

            using (var ctx = app.BeginInvoke(reset)) { ctx.Invoke(); }
            using (var ctx = app.BeginInvoke(inc)) { ctx.Invoke(); }
            using (var ctx = app.BeginInvoke(inc)) { ctx.Invoke(); }
            using (var ctx = app.BeginInvoke(read))
            {
                ctx.Invoke();
                AssertEqual("TestBeginInvokeVoid counter after 2 increments", 2, ctx.ReadResult<int>());
            }
        }

        // Ref argument: exercise Step 12b Ref Slot marshalling through the InvocationContext.
        // IL callee: `static void Increment(ref int x) { x = x + 1; }`. The caller first
        // allocates a ref-arg cell holding the initial value with PutRefInt32 (which returns
        // an offset handle relative to stack.StackBase), then PushReference(handle) emits the
        // accompanying 8-byte Ref Slot `(-1, handle)` into param slot 0. The callee's Stind_I4
        // writes back through the ref, which the caller observes via ReadRefInt32(handle).
        //
        // The ref-arg API is Neo-only (Legacy passes managed pointers directly on its eval
        // stack). Under Legacy this test is a no-op — the code path it validates doesn't exist.
        public static void TestPushReferenceIncrement(AppDomain app)
        {
#if ENABLE_NEO_MODE
            var m = ResolveIL(app, "TestCases.NeoStep13cTargets", "Increment", 1);
            using (var ctx = app.BeginInvoke(m))
            {
                int handle = ctx.PutRefInt32(41);
                ctx.PushReference(handle);
                ctx.Invoke();
                int updated = ctx.ReadRefInt32(handle);
                AssertEqual("TestPushReferenceIncrement writeback", 42, updated);
            }
#endif
        }

        // IL enum return via ReadResult<TEnum>. Legacy semantics: an IL enum is boxed as
        // its underlying primitive type; CheckCLRTypes then casts it into the declared
        // CLR enum type when the caller reads it back.
        public static void TestEnumReadResult(AppDomain app)
        {
            var m = ResolveIL(app, "TestCases.NeoStep13cTargets", "GetGreen", 0);
            using (var ctx = app.BeginInvoke(m))
            {
                ctx.Invoke();
                var raw = ctx.ReadObject(typeof(NeoStep13cColor));
                AssertNotNull("TestEnumReadResult raw", raw);
                AssertTrue("TestEnumReadResult raw is NeoStep13cColor",
                    raw is NeoStep13cColor, raw != null ? raw.GetType().FullName : "<null>");
                AssertEqual("TestEnumReadResult value",
                    (int)NeoStep13cColor.Green, (int)(NeoStep13cColor)raw);
            }
        }

        // MethodInfo.Invoke → CLRRedirections.MethodInfoInvoke Neo path (static IL target).
        public static void TestReflectionInvokeStatic(AppDomain app)
        {
            var t = app.LoadedTypes["TestCases.NeoStep13cTargets"].ReflectionType;
            var mi = t.GetMethod("AddInts");
            AssertNotNull("TestReflectionInvokeStatic MethodInfo", mi);
            var result = mi.Invoke(null, new object[] { 7, 35 });
            AssertTrue("TestReflectionInvokeStatic result is int",
                result is int, result != null ? result.GetType().FullName : "<null>");
            AssertEqual("TestReflectionInvokeStatic value", 42, (int)result);
        }

        // MethodInfo.Invoke → CLRRedirections.MethodInfoInvoke Neo path (instance IL target).
        public static void TestReflectionInvokeInstance(AppDomain app)
        {
            var t = app.LoadedTypes["TestCases.NeoStep13cInstanceTarget"];
            var instance = ((ILRuntime.CLR.TypeSystem.ILType)t).Instantiate();
            AssertNotNull("TestReflectionInvokeInstance instance", instance);

            var setBase = t.ReflectionType.GetMethod("SetBase");
            AssertNotNull("TestReflectionInvokeInstance SetBase MethodInfo", setBase);
            setBase.Invoke(instance, new object[] { 100 });

            var add = t.ReflectionType.GetMethod("Add");
            AssertNotNull("TestReflectionInvokeInstance Add MethodInfo", add);
            var result = add.Invoke(instance, new object[] { 42 });
            AssertTrue("TestReflectionInvokeInstance result is int",
                result is int, result != null ? result.GetType().FullName : "<null>");
            AssertEqual("TestReflectionInvokeInstance value", 142, (int)result);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        static ILRuntime.CLR.Method.ILMethod ResolveIL(AppDomain app, string typeName, string methodName, int paramCount)
        {
            var type = app.LoadedTypes[typeName];
            AssertNotNull("ResolveIL " + typeName, type);
            var m = type.GetMethod(methodName, paramCount) as ILRuntime.CLR.Method.ILMethod;
            if (m == null)
                throw new Exception("ResolveIL failed: " + typeName + "." + methodName + "/" + paramCount + " not found");
            return m;
        }

        static void AssertEqual(string label, int expected, int actual)
        {
            if (expected != actual)
                throw new Exception(label + " failed: expected=" + expected + " actual=" + actual);
        }
        static void AssertEqual(string label, long expected, long actual)
        {
            if (expected != actual)
                throw new Exception(label + " failed: expected=" + expected + " actual=" + actual);
        }
        static void AssertEqual(string label, double expected, double actual)
        {
            if (expected != actual)
                throw new Exception(label + " failed: expected=" + expected + " actual=" + actual);
        }
        static void AssertNotNull(string label, object obj)
        {
            if (obj == null)
                throw new Exception(label + " failed: expected non-null");
        }
        static void AssertTrue(string label, bool condition, string diagnostic)
        {
            if (!condition)
                throw new Exception(label + " failed: " + diagnostic);
        }
    }

    public enum NeoStep13cColor
    {
        Red = 1,
        Green = 2,
        Blue = 4,
    }
}
