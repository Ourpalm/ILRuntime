using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    class Test01
    {
        public static Dictionary<EnumTest.TestEnum, EnumTest.TestEnum[]> dicAttrConver5 = new Dictionary<EnumTest.TestEnum, EnumTest.TestEnum[]>()
        {
            {
                EnumTest.TestEnum.Enum1,new EnumTest.TestEnum[]{ EnumTest.TestEnum.Enum2}
            },
        };
        public static int foo(int init)
        {
            int b = init;
            for (int i = 0; i < 10000; i++)
            {
                b += i;
            }

            return b;
        }
        public static int foo()
        {
            int b = 0;
            for (int i = 0; i < 50; i++)
            {
                b += foo(b);
            }

            return b;
        }
        public static void UnitTest_ValueType()
        {
            string a = 11.ToString();

        }

        static int Add(int a, int b)
        {
            return a + b;
        }

        public static void UnitTest_Performance2()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                Add(1, 2);
            }
            sw.Stop();
            Console.WriteLine(string.Format("cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }
        /// <summary>
        /// 性能测试
        /// </summary>
        /// <returns></returns>
        public static void UnitTest_Performance()
        {
            Console.WriteLine("UnitTest_Performance");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int cnt = 0;
            for (int i = 0; i < 500000; i++)
            {
                cnt += i;
            }
            sw.Stop();

            Console.WriteLine(string.Format("Elapsed time:{0:0}ms, result = {1}", sw.ElapsedMilliseconds, cnt));

            sw.Reset();
            sw.Start();
            cnt = foo();
            sw.Stop();

            Console.WriteLine(string.Format("Elapsed time:{0:0}ms, result = {1}", sw.ElapsedMilliseconds, cnt));
            sw.Reset();
            sw.Start();
            cnt = 0;
            for (int i = 0; i < 1000000; i++)
            {
                FuncCallResult(ref cnt, i);
            }
            sw.Stop();

            Console.WriteLine(string.Format("Elapsed time:{0:0}ms, result = {1}", sw.ElapsedMilliseconds, cnt));

            PerfTest test = new PerfTest();
            sw.Reset();
            sw.Start();
            cnt = 0;
            for (int i = 0; i < 1000000; i++)
            {
                test.FuncCallResult(ref cnt, i);
            }
            sw.Stop();
            Console.WriteLine(string.Format("Elapsed time:{0:0}ms, result = {1}", sw.ElapsedMilliseconds, cnt));

        }

        public static void UnitTest_Performance3()
        {
            Console.WriteLine("UnitTest_Performance3");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int[] arr = new int[5000000];
            for (int i = 0; i < 5000000; i++)
            {
                arr[i] = i;
            }
            sw.Stop();

            Console.WriteLine(string.Format("Elapsed time:{0:0}ms, result = {1}", sw.ElapsedMilliseconds, arr.Length));
        }
        public static void UnitTest_Performance4()
        {
            Func<int, float, short, double> func = (a, b, c) =>
               {
                   return a + b + c;
               };
            ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest = (a, b, c) =>
             {
                 return a + b + c;
             };
            var func2 = ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            for(int i = 0; i < 100000; i++)
            {
                func(1, 3.6f, 4);
            }
            sw.Stop();

            Console.WriteLine(string.Format("Elapsed time:{0:0}ms", sw.ElapsedMilliseconds));

            sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                func2(1, 3.6f, 4);
            }
            sw.Stop();
            Console.WriteLine(string.Format("Elapsed time2:{0:0}ms", sw.ElapsedMilliseconds));
        }

        public static void UnitTest_Cls()
        {
            object obj = new object();
            Console.WriteLine("UnitTest_Cls"); 
            Test1098Cls cls = new Test1098Cls();
            Test1098Sub(cls);

            Console.WriteLine(string.Format("A={0} B={1}", cls.A, cls.B));
        }

        public static void UnitTest_Generics()
        {
            Console.WriteLine("UnitTest_Generixs");
            //如果一个类继承一个泛型参数为这个类本身的泛型类，就没法正确找到该类型了
            SingletonTest.Inst.Test = "bar";
            Console.WriteLine(SingletonTest.Inst.foo());
            SingletonTest2.Inst.Test = 2;
            Console.WriteLine(SingletonTest2.Inst.foo());

            Console.WriteLine(SingletonTest2.Inst.GetString<SingletonTest>(SingletonTest.Inst));

            Console.WriteLine(SingletonTest2.IsSingletonInstance(SingletonTest2.Inst).ToString());
        }

        public static void UnitTest_Generics2()
        {
            Console.WriteLine("UnitTest_Generics2"); 
            SingletonTest.Inst.Test = "bar";
            Console.WriteLine(SingletonTest.Inst.foo());
            SingletonTest2.Inst.Test = 2;
            Console.WriteLine(SingletonTest2.Inst.foo());
            Console.WriteLine(SingletonTest2.Inst.GetString<SingletonTest>(SingletonTest.Inst));
            Console.WriteLine(SingletonTest2.IsSingletonInstance(new SingletonTest2()).ToString());
            Console.WriteLine(SingletonTest2.IsSingletonInstance(SingletonTest2.Inst).ToString());
        }

        public static void UnitTest_Generics3()
        {
            Console.WriteLine("UnitTest_Generics3"); 
            Console.WriteLine(new List<NestedTest>().ToString());
        }

        public static void UnitTest_Generics4()
        {
            object r = TestGeneric<object>();
            Console.WriteLine("Result = " + r);
        }

        static T TestGeneric<T>() where T : new()
        {
            T obj = new T();
            return obj;
        }


        public static void UnitTest_NestedGenerics()
        {
            Console.WriteLine("UnitTest_NestedGenerics");
            //如果一个嵌套的类是泛型类参数，则这个类无法被找到
            Console.WriteLine(new NestedTestBase<NestedTest>().ToString());
        }

        class Test1098Cls
        {
            public int A { get; set; }
            public string B { get; set; }
        }

        static void Test1098Sub(Test1098Cls cls)
        {
            cls.A = 2;
            cls.B = "ok";
        }

        class PerfTest
        {
            public void FuncCallResult(ref int cnt, int i)
            {
                cnt++;
            }
        }
        static void FuncCallResult(ref int cnt, int i)
        {
            cnt++;
        }

        class NestedTest
        {

        }

        class NestedTestBase<T>
        {

        }
    }

    class SingletonTest : Singleton<SingletonTest>
    {
        public string Test { get; set; }
        public float testFloat;
        public static int TestStaticField;
        public string foo()
        {
            return Inst.Test;
        }
    }

    class SingletonTest2 : Singleton<SingletonTest2>
    {
        public int Test { get; set; }
        public string foo()
        {
            return Inst.Test.ToString();
        }
    }
    class Singleton<T> where T : class,new()
    {
        private static T _inst;
        public int testField;

        public Singleton()
        {
        }

        public static T Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new T();
                }
                return _inst;
            }
        }

        public string GetString<K>(K obj)
        {
            return obj.ToString();
        }

        public static bool IsSingletonInstance(T inst)
        {
            return _inst == inst;
        }
    }

    public class DictionaryEnumeratorTest<TKey, TValue>
    {
        Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
        public void Add(TKey key, TValue value)
        {
            dic.Add(key, value);
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dic.GetEnumerator();    //报错
        }

        public void GetEnumeratorTest()
        {
            var e = dic.GetEnumerator();   //正常
            while (e.MoveNext())
            {
                Console.WriteLine(e.Current.Key + "  " + e.Current.Value);
            }
        }

        public void GetEnumeratorTest2()
        {
            IEnumerator<KeyValuePair<TKey, TValue>> e = dic.GetEnumerator();   //报错
            while (e.MoveNext())
            {
                Console.WriteLine(e.Current.Key + "  " + e.Current.Value);
            }
        }
    }

    public class MyTest
    {
        public static Dictionary<string, MyTest[]> data = new Dictionary<string, MyTest[]>();

        public static void UnitTest_Test1()
        {
            var arr = new MyTest[] { new MyTest(), null, null };
            data["test"] = arr;
            Console.WriteLine(data["test"][0]);
        }
        public static void Test()
        {
            DictionaryEnumeratorTest<int, int> t = new DictionaryEnumeratorTest<int, int>();
            t.Add(1, 1);
            t.GetEnumeratorTest();
            t.GetEnumeratorTest2();
            var e = t.GetEnumerator();
            while (e.MoveNext())
            {
                Console.WriteLine(e.Current.Key + "  " + e.Current.Value);
            }
        }
    }
}
