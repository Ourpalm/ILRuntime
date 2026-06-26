//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixCrossPatch
    {
        public int MethodA(int x)
        {
#if PATCHED
            return x * 3;
#else
            return x * 2;
#endif
        }

        public int MethodB(int x)
        {
#if PATCHED
            return MethodA(x) + 100;
#else
            return MethodA(x) + 10;
#endif
        }

        public int MethodC(int x)
        {
#if PATCHED
            return MethodB(MethodA(x)) + 1000;
#else
            return MethodB(MethodA(x));
#endif
        }
    }

    public interface IEventSource
    {
        event EventHandler<int> ValueChanged;
        void RaiseEvent(int value);
    }

    [ILRuntimePatch]
    public class HotfixEvent : IEventSource
    {
        public event EventHandler<int> ValueChanged;
        private int lastValue;

        public void RaiseEvent(int value)
        {
#if PATCHED
            lastValue = value * 2;
            ValueChanged?.Invoke(this, lastValue);
#else
            lastValue = value;
            ValueChanged?.Invoke(this, lastValue);
#endif
        }

        public int GetLastValue() => lastValue;
    }

    public interface IExplicit
    {
        int Calculate(int a, int b);
        string Format(int value);
    }

    [ILRuntimePatch]
    public class HotfixExplicitInterface : IExplicit
    {
        int IExplicit.Calculate(int a, int b)
        {
#if PATCHED
            return (a + b) * 2;
#else
            return a + b;
#endif
        }

        string IExplicit.Format(int value)
        {
#if PATCHED
            return "[" + value.ToString() + "]";
#else
            return value.ToString();
#endif
        }
    }

    [ILRuntimePatch]
    public struct HotfixStructMutation
    {
        public int X;
        public int Y;

        public void Translate(int dx, int dy)
        {
#if PATCHED
            X += dx * 2;
            Y += dy * 2;
#else
            X += dx;
            Y += dy;
#endif
        }

        public void Scale(int factor)
        {
#if PATCHED
            X *= factor + 1;
            Y *= factor + 1;
#else
            X *= factor;
            Y *= factor;
#endif
        }

        public void Reset()
        {
#if PATCHED
            X = -1;
            Y = -1;
#else
            X = 0;
            Y = 0;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixOuter
    {
        [ILRuntimePatch]
        public class HotfixNested
        {
            private int value;

            public HotfixNested(int v)
            {
#if PATCHED
                value = v + 100;
#else
                value = v;
#endif
            }

            public int GetValue()
            {
#if PATCHED
                return value * 2;
#else
                return value;
#endif
            }
        }

        public int UseNested(int input)
        {
#if PATCHED
            var nested = new HotfixNested(input);
            return nested.GetValue() + 50;
#else
            var nested = new HotfixNested(input);
            return nested.GetValue();
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixStringOps
    {
        public string Interpolation(int a, string b)
        {
#if PATCHED
            return string.Format("Result: [{0}] - [{1}]", a * 2, b.ToUpper());
#else
            return string.Format("Result: {0} - {1}", a, b);
#endif
        }

        public string BuildString(int count)
        {
#if PATCHED
            var sb = new System.Text.StringBuilder();
            for (int i = count - 1; i >= 0; i--)
                sb.Append(i).Append('|');
            return sb.ToString();
#else
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < count; i++)
                sb.Append(i).Append(',');
            return sb.ToString();
#endif
        }
    }

    public class HotfixInteractionTestCases
    {
        static bool TestCrossPatchChain(bool patched)
        {
            var obj = new HotfixCrossPatch();
            int rA = obj.MethodA(5);
            int rB = obj.MethodB(5);
            int rC = obj.MethodC(5);

            if (patched)
                // A(5)=15, B(5)=A(5)+100=115, C(5)=B(A(5))+1000=B(15)+1000=A(15)+100+1000=45+100+1000=1145
                return rA == 15 && rB == 115 && rC == 1145;
            else
                // A(5)=10, B(5)=A(5)+10=20, C(5)=B(A(5))=B(10)=A(10)+10=20+10=30
                return rA == 10 && rB == 20 && rC == 30;
        }

        static bool TestEvent(bool patched)
        {
            var obj = new HotfixEvent();
            int received = 0;
            obj.ValueChanged += (sender, value) => { received = value; };
            obj.RaiseEvent(42);

            if (patched)
                return received == 84 && obj.GetLastValue() == 84;
            else
                return received == 42 && obj.GetLastValue() == 42;
        }

        static bool TestExplicitInterface(bool patched)
        {
            IExplicit obj = new HotfixExplicitInterface();
            int calc = obj.Calculate(3, 4);
            string fmt = obj.Format(99);

            if (patched)
                return calc == 14 && fmt == "[99]";
            else
                return calc == 7 && fmt == "99";
        }

        static bool TestStructMutationTranslate(bool patched)
        {
            var s = new HotfixStructMutation { X = 10, Y = 20 };
            s.Translate(5, 3);

            return patched ? s.X == 20 && s.Y == 26 : s.X == 15 && s.Y == 23;
        }

        static bool TestStructMutationScale(bool patched)
        {
            var s = new HotfixStructMutation { X = 4, Y = 6 };
            s.Scale(3);

            return patched ? s.X == 16 && s.Y == 24 : s.X == 12 && s.Y == 18;
        }

        static bool TestStructMutationReset(bool patched)
        {
            var s = new HotfixStructMutation { X = 99, Y = 88 };
            s.Reset();

            return patched ? s.X == -1 && s.Y == -1 : s.X == 0 && s.Y == 0;
        }

        static bool TestNestedClass(bool patched)
        {
            var outer = new HotfixOuter();
            int result = outer.UseNested(10);

            // patched: nested(10) → value=110, GetValue()=220, +50=270
            return patched ? result == 270 : result == 10;
        }

        static bool TestNestedClassDirect(bool patched)
        {
            var nested = new HotfixOuter.HotfixNested(50);
            int result = nested.GetValue();

            // patched: value=150, GetValue()=300
            return patched ? result == 300 : result == 50;
        }

        static bool TestStringInterpolation(bool patched)
        {
            var obj = new HotfixStringOps();
            string result = obj.Interpolation(5, "hello");

            return patched ? result == "Result: [10] - [HELLO]" : result == "Result: 5 - hello";
        }

        static bool TestStringBuilder(bool patched)
        {
            var obj = new HotfixStringOps();
            string result = obj.BuildString(3);

            return patched ? result == "2|1|0|" : result == "0,1,2,";
        }

        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestCrossPatchChain)), TestCrossPatchChain);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestEvent)), TestEvent);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestExplicitInterface)), TestExplicitInterface);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestStructMutationTranslate)), TestStructMutationTranslate);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestStructMutationScale)), TestStructMutationScale);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestStructMutationReset)), TestStructMutationReset);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestNestedClass)), TestNestedClass);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestNestedClassDirect)), TestNestedClassDirect);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestStringInterpolation)), TestStringInterpolation);
            yield return new DelegateTestCase(string.Format("{0}.{1}", nameof(HotfixInteractionTestCases), nameof(TestStringBuilder)), TestStringBuilder);
        }
    }
}
