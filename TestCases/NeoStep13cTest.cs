using System;
using ILRuntimeTest.TestBase;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    // Step 13c tests. Every test method here is a thin IL-side wrapper that grabs the
    // AppDomain through TestSession.LastSession.Appdomain and hands control to a CLR-side
    // host in `NeoStep13cCLRHost` that actually exercises the three migrated entry points:
    //   * app.BeginInvoke → InvocationContext.Invoke  → InvocationFrame primitive/enum/ref
    //   * MethodInfo.Invoke → CLRRedirections.MethodInfoInvoke Neo branch
    //   * (Delegate coverage is deferred until Step 19 supplies IL-side ldftn/ldvirtftn)
    //
    // The IL wrapper does not carry the test payload itself — the CLR-side host performs
    // the actual assertions and throws a descriptive Exception on failure. Test framework
    // reports the Exception message as the failure reason.

    public class NeoStep13cTest
    {
        public static void NeoStep13cBeginInvokeAddInts()
            => NeoStep13cCLRHost.TestBeginInvokeAddInts(TestSession.LastSession.Appdomain);

        public static void NeoStep13cBeginInvokeAddLongs()
            => NeoStep13cCLRHost.TestBeginInvokeAddLongs(TestSession.LastSession.Appdomain);

        public static void NeoStep13cBeginInvokeMixedFloats()
            => NeoStep13cCLRHost.TestBeginInvokeMixedFloats(TestSession.LastSession.Appdomain);

        public static void NeoStep13cBeginInvokeStringParam()
            => NeoStep13cCLRHost.TestBeginInvokeStringParam(TestSession.LastSession.Appdomain);

        public static void NeoStep13cBeginInvokeVoid()
            => NeoStep13cCLRHost.TestBeginInvokeVoid(TestSession.LastSession.Appdomain);

        public static void NeoStep13cPushReferenceIncrement()
            => NeoStep13cCLRHost.TestPushReferenceIncrement(TestSession.LastSession.Appdomain);

        public static void NeoStep13cEnumReadResult()
            => NeoStep13cCLRHost.TestEnumReadResult(TestSession.LastSession.Appdomain);

        public static void NeoStep13cReflectionInvokeStatic()
            => NeoStep13cCLRHost.TestReflectionInvokeStatic(TestSession.LastSession.Appdomain);

        public static void NeoStep13cReflectionInvokeInstance()
            => NeoStep13cCLRHost.TestReflectionInvokeInstance(TestSession.LastSession.Appdomain);
    }

    // Target IL methods that the CLR-side host calls into.
    public class NeoStep13cTargets
    {
        static int _counter;

        public static int AddInts(int a, int b) { return a + b; }
        public static long AddLongs(long a, long b) { return a + b; }
        public static double AddFloatDouble(float a, double b) { return a + b; }
        public static int StringLength(string s) { return s.Length; }
        public static void ResetCounter() { _counter = 0; }
        public static void IncrementCounter() { _counter++; }
        public static int ReadCounter() { return _counter; }
        public static void Increment(ref int x) { x = x + 1; }
        public static NeoStep13cColor GetGreen() { return NeoStep13cColor.Green; }
    }

    public class NeoStep13cInstanceTarget
    {
        int _base;
        public void SetBase(int v) { _base = v; }
        public int Add(int x) { return _base + x; }
    }
}
