//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixTryCatch
    {
        public string HandleException(int input)
        {
#if PATCHED
            try
            {
                if (input < 0)
                    throw new ArgumentException("negative");
                if (input == 0)
                    throw new InvalidOperationException("zero");
                return string.Format("ok:{0}", input * 2);
            }
            catch (ArgumentException ex)
            {
                return string.Format("arg:{0}", ex.Message);
            }
            catch (InvalidOperationException)
            {
                return "invalid";
            }
#else
            try
            {
                if (input < 0)
                    throw new ArgumentException("negative");
                return string.Format("ok:{0}", input);
            }
            catch (ArgumentException ex)
            {
                return string.Format("caught:{0}", ex.Message);
            }
#endif
        }

        public int TryFinally(int input)
        {
#if PATCHED
            int result = 0;
            try
            {
                result = input * 3;
            }
            finally
            {
                result += 100;
            }
            return result;
#else
            int result = 0;
            try
            {
                result = input * 2;
            }
            finally
            {
                result += 10;
            }
            return result;
#endif
        }

        public string TryCatchFinally(int input, ref int finallyCount)
        {
#if PATCHED
            try
            {
                if (input == 0)
                    throw new Exception("patched_error");
                return string.Format("patched:{0}", input);
            }
            catch (Exception ex)
            {
                return string.Format("patched_catch:{0}", ex.Message);
            }
            finally
            {
                finallyCount += 100;
            }
#else
            try
            {
                if (input == 0)
                    throw new Exception("error");
                return string.Format("normal:{0}", input);
            }
            catch (Exception ex)
            {
                return string.Format("catch:{0}", ex.Message);
            }
            finally
            {
                finallyCount += 1;
            }
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixForeach
    {
        public int SumList(List<int> list)
        {
#if PATCHED
            int sum = 0;
            foreach (var item in list)
                sum += item * 2;
            return sum;
#else
            int sum = 0;
            foreach (var item in list)
                sum += item;
            return sum;
#endif
        }

        public string JoinArray(string[] arr)
        {
#if PATCHED
            string result = "";
            foreach (var s in arr)
                result += "[" + s + "]";
            return result;
#else
            string result = "";
            foreach (var s in arr)
                result += s + ",";
            return result;
#endif
        }

        public int CountMatches(int[] arr, int threshold)
        {
#if PATCHED
            int count = 0;
            foreach (var item in arr)
            {
                if (item >= threshold)
                    count++;
            }
            return count * 10;
#else
            int count = 0;
            foreach (var item in arr)
            {
                if (item > threshold)
                    count++;
            }
            return count;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixUsing : IDisposable
    {
        public int DisposeCount;
        private int value;

        public void Dispose()
        {
#if PATCHED
            DisposeCount += 10;
#else
            DisposeCount += 1;
#endif
        }

        public int ProcessWithUsing()
        {
#if PATCHED
            value = 200;
            using (var helper = new DisposableHelper())
            {
                value += helper.GetValue() * 2;
            }
            return value;
#else
            value = 100;
            using (var helper = new DisposableHelper())
            {
                value += helper.GetValue();
            }
            return value;
#endif
        }
    }

    public class DisposableHelper : IDisposable
    {
        public int GetValue() => 50;
        public void Dispose() { }
    }

    [ILRuntimePatch]
    public class HotfixLock
    {
        private readonly object syncRoot = new object();
        private int counter;

        public int IncrementWithLock(int times)
        {
#if PATCHED
            for (int i = 0; i < times; i++)
            {
                lock (syncRoot)
                {
                    counter += 2;
                }
            }
            return counter;
#else
            for (int i = 0; i < times; i++)
            {
                lock (syncRoot)
                {
                    counter += 1;
                }
            }
            return counter;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixTypeCast
    {
        public string CastTest(object input)
        {
#if PATCHED
            if (input is int intVal)
                return string.Format("int:{0}", intVal * 2);
            if (input is string strVal)
                return string.Format("str:{0}", strVal.ToUpper());
            return "patched_unknown";
#else
            if (input is int intVal)
                return string.Format("int:{0}", intVal);
            if (input is string strVal)
                return string.Format("str:{0}", strVal);
            return "unknown";
#endif
        }

        public int SafeCast(object input)
        {
#if PATCHED
            var list = input as List<int>;
            if (list != null)
                return list.Count * 100;
            return -1;
#else
            var list = input as List<int>;
            if (list != null)
                return list.Count;
            return 0;
#endif
        }
    }

    public class HotfixControlFlowTestCases
    {
        static bool TestTryCatch(bool patched)
        {
            var obj = new HotfixTryCatch();
            string r1 = obj.HandleException(5);
            string r2 = obj.HandleException(-1);
            string r3 = obj.HandleException(0);

            if (patched)
                return r1 == "ok:10" && r2 == "arg:negative" && r3 == "invalid";
            else
                return r1 == "ok:5" && r2 == "caught:negative" && r3 == "ok:0";
        }

        static bool TestTryFinally(bool patched)
        {
            var obj = new HotfixTryCatch();
            int result = obj.TryFinally(5);

            return patched ? result == 115 : result == 20;
        }

        static bool TestTryCatchFinally(bool patched)
        {
            var obj = new HotfixTryCatch();
            int finallyCount = 0;
            string r1 = obj.TryCatchFinally(5, ref finallyCount);
            string r2 = obj.TryCatchFinally(0, ref finallyCount);

            if (patched)
                return r1 == "patched:5" && r2 == "patched_catch:patched_error" && finallyCount == 200;
            else
                return r1 == "normal:5" && r2 == "catch:error" && finallyCount == 2;
        }

        static bool TestForeachList(bool patched)
        {
            var obj = new HotfixForeach();
            var list = new List<int> { 1, 2, 3, 4, 5 };
            int result = obj.SumList(list);

            return patched ? result == 30 : result == 15;
        }

        static bool TestForeachArray(bool patched)
        {
            var obj = new HotfixForeach();
            string result = obj.JoinArray(new[] { "a", "b", "c" });

            return patched ? result == "[a][b][c]" : result == "a,b,c,";
        }

        static bool TestForeachWithCondition(bool patched)
        {
            var obj = new HotfixForeach();
            int result = obj.CountMatches(new[] { 1, 5, 3, 7, 2 }, 3);

            return patched ? result == 30 : result == 2;
        }

        static bool TestUsing(bool patched)
        {
            var obj = new HotfixUsing();
            int result = obj.ProcessWithUsing();

            return patched ? result == 300 : result == 150;
        }

        static bool TestDispose(bool patched)
        {
            var obj = new HotfixUsing();
            obj.Dispose();
            obj.Dispose();

            return patched ? obj.DisposeCount == 20 : obj.DisposeCount == 2;
        }

        static bool TestLock(bool patched)
        {
            var obj = new HotfixLock();
            int result = obj.IncrementWithLock(5);

            return patched ? result == 10 : result == 5;
        }

        static bool TestTypeCastIs(bool patched)
        {
            var obj = new HotfixTypeCast();
            string r1 = obj.CastTest(42);
            string r2 = obj.CastTest("hello");
            string r3 = obj.CastTest(3.14);

            if (patched)
                return r1 == "int:84" && r2 == "str:HELLO" && r3 == "patched_unknown";
            else
                return r1 == "int:42" && r2 == "str:hello" && r3 == "unknown";
        }

        static bool TestTypeCastAs(bool patched)
        {
            var obj = new HotfixTypeCast();
            var list = new List<int> { 1, 2, 3 };
            int r1 = obj.SafeCast(list);
            int r2 = obj.SafeCast("not a list");

            if (patched)
                return r1 == 300 && r2 == -1;
            else
                return r1 == 3 && r2 == 0;
        }

        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestTryCatch", TestTryCatch);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestTryFinally", TestTryFinally);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestTryCatchFinally", TestTryCatchFinally);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestForeachList", TestForeachList);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestForeachArray", TestForeachArray);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestForeachWithCondition", TestForeachWithCondition);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestUsing", TestUsing);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestDispose", TestDispose);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestLock", TestLock);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestTypeCastIs", TestTypeCastIs);
            yield return new DelegateTestCase("HotfixControlFlowTestCases.TestTypeCastAs", TestTypeCastAs);
        }
    }
}
