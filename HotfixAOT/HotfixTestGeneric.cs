//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixTestGeneric<T>
    {
        private T obj;

        public HotfixTestGeneric(T val)
        {
            TestVirtual();
            obj = val;
            TestVal(val);

        }

        public void Append(List<T> list)
        {
#if PATCHED
            list.Insert(0, obj);
#else
            list.Add(obj);
#endif
        }

        public T GetValue()
        {
#if PATCHED
            return Transform(obj);
#else
            return obj;
#endif
        }

        protected virtual void TestVirtual()
        {

        }

        public void SetTransformer(object t)
        {
#if PATCHED
            Console.WriteLine("T:" + typeof(T).FullName);
            transformer = (Func<T, T>)t;
#endif
        }

        public void TestVal(object val)
        {
        }

#if PATCHED
        private T result;
        Func<T, T> transformer;
        
        T Transform(T obj)
        {
            return transformer(obj);
        }
#endif
    }

    public class HotfixTestGenericTestCases
    {
        static bool Test01(bool patched)
        {
            HotfixTestGeneric<int> cls = new HotfixTestGeneric<int>(100);
            List<int> list = new List<int>();
            list.Add(0);
            cls.Append(list);
            return patched ? list[0] == 100 : list[0] == 0;
        }
        static bool Test02(bool patched)
        {
            HotfixTestGeneric<int> cls = new HotfixTestGeneric<int>(100);
            cls.SetTransformer(new Func<int, int>((a) => a + 200));
            return patched ? cls.GetValue() == 300 : cls.GetValue() == 100;
        }
        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase($"{nameof(HotfixTestGenericTestCases)}.{nameof(Test01)}", Test01);
            yield return new DelegateTestCase($"{nameof(HotfixTestGenericTestCases)}.{nameof(Test02)}", Test02);
        }
    }
}
