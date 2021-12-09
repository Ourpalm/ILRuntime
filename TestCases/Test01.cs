using ILRuntime.Runtime;
using ILRuntimeTest;
using ILRuntimeTest.TestFramework;
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

        [ILRuntimeJIT(ILRuntimeJITFlags.JITOnDemand)]
        static int Add(int a, int b)
        {
            if (b < 0)
                return a;
            return a + b;
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance2()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int cnt = 0;
            for (int i = 0; i < 1000000; i++)
            {
                cnt += Add(i, 2);
            }
            sw.Stop();
            Console.WriteLine(string.Format("time: {0:0}ms cps:{1:0}, result={2}", sw.ElapsedMilliseconds, (1000000 * 1000 / sw.ElapsedMilliseconds), cnt));

            sw.Restart();
            cnt = 0;
            for (int i = 0; i < 1000000; i++)
            {
                cnt += ILRuntimeTest.TestFramework.TestClass2.Add(i, 2);
            }
            sw.Stop();
            Console.WriteLine(string.Format("time: {0:0}ms cps:{1:0}, result={2}", sw.ElapsedMilliseconds, (1000000 * 1000 / sw.ElapsedMilliseconds), cnt));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static int UnitTest_PerformanceSimple()
        {
            int cnt = 0;
            for (int i = 0; i < 5000000; i++)
            {
                cnt += i;
            }
            return cnt;
        }
        /// <summary>
        /// 性能测试
        /// </summary>
        /// <returns></returns>
        [ILRuntimeTest(IsPerformanceTest = true)]
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

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance3()
        {
            Console.WriteLine("UnitTest_Performance3");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int[] arr = new int[500000];
            for (int i = 0; i < 500000; i++)
            {
                arr[i] = i;
            }
            sw.Stop();

            Console.WriteLine(string.Format("Elapsed time:{0:0}ms, result = {1}", sw.ElapsedMilliseconds, arr.Length));
        }
        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance5()
        {
            PerformanceTestCls obj = new PerformanceTestCls();
            int cnt = 0;
            int a = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                a = obj.A;
            }
            sw.Stop();
            Console.WriteLine(string.Format("res=" + a + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance9()
        {
            int[] array = new int[1024];
            for (int i = 0; i < 1024; ++i)
            {
                array[i] = i;
            }

            int total = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int j = 0; j < 1000; ++j)
            {
                for (int i = 0; i < 1024; ++i)
                {
                    total = total + array[i];
                }
            }

            sw.Stop();
            Console.WriteLine(string.Format("res=" + total + ", time:{0:0}", sw.ElapsedMilliseconds));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance6()
        {
            PerformanceTestCls obj = new PerformanceTestCls();
            int cnt = 0;
            int a = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                obj.A = a;
            }
            sw.Stop();
            Console.WriteLine(string.Format("res=" + a + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance7()
        {
            PerformanceTestCls obj = new PerformanceTestCls();
            int cnt = 0;
            int a = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                a = obj.B;
            }
            sw.Stop();
            Console.WriteLine(string.Format("res=" + a + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance8()
        {
            PerformanceTestCls obj = new PerformanceTestCls();
            int cnt = 0;
            int a = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                obj.B = a;
            }
            sw.Stop();
            Console.WriteLine(string.Format("res=" + a + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance10()
        {
            TestVectorClass a = new TestVectorClass();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                TestVectorClass.ValueTypePerfTest(i, "b", a.Vector2, a);
            }
            sw.Stop();
            Console.WriteLine(string.Format("time=" + sw.ElapsedMilliseconds + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance11()
        {
            TestVectorClass a = new TestVectorClass();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                TestVectorClass.ValueTypePerfTest2(i, "b", a.Obj, a);
            }
            sw.Stop();
            Console.WriteLine(string.Format("time=" + sw.ElapsedMilliseconds + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }
        const int T12_NOBJS = 100;
        const int T12_NLOOP = 1000000;
        int A;
        string B;
        float C;
        // 构造10W次脚本对象
        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance12()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Test01[] arr = new Test01[T12_NOBJS];
            for (int i = 0; i < T12_NLOOP; i++)
            {
                arr[i % T12_NOBJS] = new Test01();
            }

            sw.Stop();
            Console.WriteLine("time:" + sw.ElapsedMilliseconds);
        }

        // 循环修改脚本短字段10W次
        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance12_1()
        {
            Test01[] arr = new Test01[T12_NOBJS];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new Test01();
            }

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < T12_NLOOP; i++)
            {
                var obj = arr[i % T12_NOBJS];
                obj.A = i;
            }

            sw.Stop();
            Console.WriteLine("time:" + sw.ElapsedMilliseconds);
        }

        // 循环修改脚本对象多个字段10W次
        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance12_4()
        {
            Test01[] arr = new Test01[T12_NOBJS];
            for (int i = 0; i < arr.Length; i++)
            {
                var obj = new Test01();
                arr[i] = obj;
                obj.A = i;
                obj.B = (i + 1).ToString();
                obj.C = 2 * i;
            }

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < T12_NLOOP; ++i)
            {
                var obj = arr[i % T12_NOBJS];
                var obj2 = arr[(i + 1) % T12_NOBJS];
                obj.A = obj2.A;
                obj.B = obj2.B;
                obj.C = obj2.C;
            }
            sw.Stop();
            Console.WriteLine("time:" + sw.ElapsedMilliseconds);
        }
        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance13()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            TestVector3 vec = default;
            TestVector3 vec2 = TestVector3.One2;
            for (int i = 0; i < 1000000; i++)
            {
                vec += vec2 * 2 + TestVector3.One2;
            }
            sw.Stop();
            Console.WriteLine(string.Format("res="+ vec + ",time = " + sw.ElapsedMilliseconds + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }

        [ILRuntimeTest(IsPerformanceTest = true)]
        public static void UnitTest_Performance14()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            TestVector3NoBinding vec = default;
            TestVector3NoBinding vec2 = TestVector3NoBinding.one;
            for (int i = 0; i < 1000000; i++)
            {
                vec += vec2 * 2 + TestVector3NoBinding.one;
            }
            sw.Stop();
            Console.WriteLine(string.Format("res=" + vec + ",time = " + sw.ElapsedMilliseconds + ", cps:{0:0}", (1000000 * 1000 / sw.ElapsedMilliseconds)));
        }
        class PerformanceTestCls
        {
            public int A = 1;
            public int B { get; set; } = 2;
        }
        [ILRuntimeTest(IsPerformanceTest = true)]
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
            for (int i = 0; i < 100000; i++)
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
    class Singleton<T> where T : class, new()
    {
        private static T _inst;
        public int testField;

        public Singleton()
        {
        }

        public static T Inst
        {
            [ILRuntimeTest(Ignored = true)]
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
