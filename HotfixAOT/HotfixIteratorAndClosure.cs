//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixIterator
    {
        public IEnumerable<int> GetNumbers(int count)
        {
#if PATCHED
            for (int i = count - 1; i >= 0; i--)
                yield return i * 10;
#else
            for (int i = 0; i < count; i++)
                yield return i;
#endif
        }

        public IEnumerable<string> GetStrings()
        {
#if PATCHED
            yield return "patched_a";
            yield return "patched_b";
#else
            yield return "a";
            yield return "b";
            yield return "c";
#endif
        }

        public IEnumerable<int> GetWithBreak(int max)
        {
#if PATCHED
            for (int i = 0; i < max; i++)
            {
                if (i >= 2) yield break;
                yield return i * 100;
            }
#else
            for (int i = 0; i < max; i++)
            {
                yield return i;
            }
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixClosureThis
    {
        private int factor;
        private string prefix;

        public HotfixClosureThis(int factor, string prefix)
        {
            this.factor = factor;
            this.prefix = prefix;
        }

        public Func<int, int> GetMultiplier()
        {
#if PATCHED
            return (x) => x * factor + 1;
#else
            return (x) => x * factor;
#endif
        }

        public Func<string, string> GetFormatter()
        {
#if PATCHED
            return (s) => $"[{prefix}]{s}";
#else
            return (s) => $"{prefix}:{s}";
#endif
        }

        public int ProcessWithLocalAndThis(int input)
        {
            int localOffset = 10;
#if PATCHED
            Func<int, int> fn = (x) => x * factor + localOffset + 5;
#else
            Func<int, int> fn = (x) => x * factor + localOffset;
#endif
            return fn(input);
        }
    }

    public class HotfixIteratorAndClosureTestCases
    {
        static bool TestIteratorNumbers(bool patched)
        {
            var obj = new HotfixIterator();
            var numbers = obj.GetNumbers(3).ToList();

            if (patched)
                return numbers.Count == 3 && numbers[0] == 20 && numbers[1] == 10 && numbers[2] == 0;
            else
                return numbers.Count == 3 && numbers[0] == 0 && numbers[1] == 1 && numbers[2] == 2;
        }

        static bool TestIteratorStrings(bool patched)
        {
            var obj = new HotfixIterator();
            var strings = obj.GetStrings().ToList();

            if (patched)
                return strings.Count == 2 && strings[0] == "patched_a" && strings[1] == "patched_b";
            else
                return strings.Count == 3 && strings[0] == "a" && strings[1] == "b" && strings[2] == "c";
        }

        static bool TestIteratorBreak(bool patched)
        {
            var obj = new HotfixIterator();
            var items = obj.GetWithBreak(5).ToList();

            if (patched)
                return items.Count == 2 && items[0] == 0 && items[1] == 100;
            else
                return items.Count == 5 && items[0] == 0 && items[4] == 4;
        }

        static bool TestClosureThis(bool patched)
        {
            var obj = new HotfixClosureThis(3, "tag");
            var multiplier = obj.GetMultiplier();
            int result = multiplier(10);
            return patched ? result == 31 : result == 30;
        }

        static bool TestClosureFormatter(bool patched)
        {
            var obj = new HotfixClosureThis(1, "info");
            var formatter = obj.GetFormatter();
            string result = formatter("hello");
            return patched ? result == "[info]hello" : result == "info:hello";
        }

        static bool TestClosureLocalAndThis(bool patched)
        {
            var obj = new HotfixClosureThis(2, "");
            int result = obj.ProcessWithLocalAndThis(5);
            // non-patched: 5 * 2 + 10 = 20
            // patched: 5 * 2 + 10 + 5 = 25
            return patched ? result == 25 : result == 20;
        }

        static bool TestLambdaNoCapture(bool patched)
        {
            // TestLambdaExp3: no capture lambda
            int result = HotfixClass.TestLambdaExp3(50);
            return patched ? result == 150 : result == 50;
        }

        static bool TestLambdaCaptureSingle(bool patched)
        {
            // TestLambdaExp5: captures only 'a' parameter
            int result = HotfixClass.TestLambdaExp5(50);
            return patched ? result == 150 : result == 50;
        }

        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestIteratorNumbers)}", TestIteratorNumbers);
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestIteratorStrings)}", TestIteratorStrings);
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestIteratorBreak)}", TestIteratorBreak);
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestClosureThis)}", TestClosureThis);
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestClosureFormatter)}", TestClosureFormatter);
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestClosureLocalAndThis)}", TestClosureLocalAndThis);
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestLambdaNoCapture)}", TestLambdaNoCapture);
            yield return new DelegateTestCase($"{nameof(HotfixIteratorAndClosureTestCases)}.{nameof(TestLambdaCaptureSingle)}", TestLambdaCaptureSingle);
        }
    }
}
