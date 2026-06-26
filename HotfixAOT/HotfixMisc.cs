//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixParams
    {
        public int Sum(params int[] values)
        {
#if PATCHED
            int sum = 0;
            foreach (var v in values) sum += v * 2;
            return sum;
#else
            int sum = 0;
            foreach (var v in values) sum += v;
            return sum;
#endif
        }

        public string Join(string sep, params string[] parts)
        {
#if PATCHED
            return "[" + string.Join(sep, parts) + "]";
#else
            return string.Join(sep, parts);
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixIndexer
    {
        private int[] data = new int[10];

        public int this[int index]
        {
            get
            {
#if PATCHED
                return data[index] + 100;
#else
                return data[index];
#endif
            }
            set
            {
#if PATCHED
                data[index] = value * 2;
#else
                data[index] = value;
#endif
            }
        }
    }

    [ILRuntimePatch]
    public struct HotfixOperator
    {
        public int X;
        public int Y;

        public HotfixOperator(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static HotfixOperator operator +(HotfixOperator a, HotfixOperator b)
        {
#if PATCHED
            return new HotfixOperator(a.X + b.X + 1, a.Y + b.Y + 1);
#else
            return new HotfixOperator(a.X + b.X, a.Y + b.Y);
#endif
        }

        public static HotfixOperator operator *(HotfixOperator a, int scalar)
        {
#if PATCHED
            return new HotfixOperator(a.X * scalar + 10, a.Y * scalar + 10);
#else
            return new HotfixOperator(a.X * scalar, a.Y * scalar);
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixRecursive
    {
        public int Factorial(int n)
        {
#if PATCHED
            if (n <= 1) return 2;
            return n * Factorial(n - 1);
#else
            if (n <= 1) return 1;
            return n * Factorial(n - 1);
#endif
        }

        public int Fibonacci(int n)
        {
#if PATCHED
            if (n <= 0) return 0;
            if (n == 1) return 2;
            return Fibonacci(n - 1) + Fibonacci(n - 2);
#else
            if (n <= 0) return 0;
            if (n == 1) return 1;
            return Fibonacci(n - 1) + Fibonacci(n - 2);
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixMultiReturn
    {
        public string Classify(int value)
        {
#if PATCHED
            if (value < 0) return "negative_patched";
            if (value == 0) return "zero_patched";
            if (value < 10) return "small_patched";
            if (value < 100) return "medium_patched";
            return "large_patched";
#else
            if (value < 0) return "negative";
            if (value == 0) return "zero";
            if (value < 10) return "small";
            if (value < 100) return "medium";
            return "large";
#endif
        }
    }

    public class HotfixMiscTestCases
    {
        static bool TestParams(bool patched)
        {
            var obj = new HotfixParams();
            int sum = obj.Sum(1, 2, 3, 4, 5);
            // non-patched: 1+2+3+4+5 = 15
            // patched: (1+2+3+4+5)*2 = 30
            return patched ? sum == 30 : sum == 15;
        }

        static bool TestParamsString(bool patched)
        {
            var obj = new HotfixParams();
            string result = obj.Join("-", "a", "b", "c");
            return patched ? result == "[a-b-c]" : result == "a-b-c";
        }

        static bool TestIndexer(bool patched)
        {
            var obj = new HotfixIndexer();
            obj[0] = 5;
            obj[1] = 10;
            int v0 = obj[0];
            int v1 = obj[1];

            if (patched)
            {
                // setter: data[0] = 5*2=10, getter: 10+100=110
                // setter: data[1] = 10*2=20, getter: 20+100=120
                return v0 == 110 && v1 == 120;
            }
            else
            {
                return v0 == 5 && v1 == 10;
            }
        }

        static bool TestOperatorAdd(bool patched)
        {
            var a = new HotfixOperator(1, 2);
            var b = new HotfixOperator(3, 4);
            var c = a + b;
            return patched ? c.X == 5 && c.Y == 7 : c.X == 4 && c.Y == 6;
        }

        static bool TestOperatorMul(bool patched)
        {
            var a = new HotfixOperator(2, 3);
            var c = a * 5;
            return patched ? c.X == 20 && c.Y == 25 : c.X == 10 && c.Y == 15;
        }

        static bool TestRecursiveFactorial(bool patched)
        {
            var obj = new HotfixRecursive();
            int result = obj.Factorial(5);
            // non-patched: 5! = 120
            // patched: 5*4*3*2*2 = 240 (base case returns 2)
            return patched ? result == 240 : result == 120;
        }

        static bool TestRecursiveFibonacci(bool patched)
        {
            var obj = new HotfixRecursive();
            int result = obj.Fibonacci(6);
            // non-patched: fib(6) = 8 (0,1,1,2,3,5,8)
            // patched: fib(6) with base=2: 0,2,2,4,6,10,16
            return patched ? result == 16 : result == 8;
        }

        static bool TestMultiReturn(bool patched)
        {
            var obj = new HotfixMultiReturn();
            bool ok = true;
            ok &= patched ? obj.Classify(-5) == "negative_patched" : obj.Classify(-5) == "negative";
            ok &= patched ? obj.Classify(0) == "zero_patched" : obj.Classify(0) == "zero";
            ok &= patched ? obj.Classify(5) == "small_patched" : obj.Classify(5) == "small";
            ok &= patched ? obj.Classify(50) == "medium_patched" : obj.Classify(50) == "medium";
            ok &= patched ? obj.Classify(200) == "large_patched" : obj.Classify(200) == "large";
            return ok;
        }

        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestParams)}", TestParams);
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestParamsString)}", TestParamsString);
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestIndexer)}", TestIndexer);
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestOperatorAdd)}", TestOperatorAdd);
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestOperatorMul)}", TestOperatorMul);
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestRecursiveFactorial)}", TestRecursiveFactorial);
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestRecursiveFibonacci)}", TestRecursiveFibonacci);
            yield return new DelegateTestCase($"{nameof(HotfixMiscTestCases)}.{nameof(TestMultiReturn)}", TestMultiReturn);
        }
    }
}
