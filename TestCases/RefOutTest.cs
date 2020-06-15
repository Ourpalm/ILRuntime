using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{

    public class RefOutTest
    {
        static class Check
        {
            public static void Test<T>(T value)
            {
                if (value == null)
                    Console.WriteLine("null");
            }

            public static void Test2<T>(T obj)
            {
                Action<T> action = (value) => { Console.WriteLine(value); };
                action(obj);
            }
        }


        public static void UnitTest_NullCheck()
        {
            Check.Test(new byte[] { 1, 2 });
        }

        public static void UnitTest_GenericDelegate()
        {
            Check.Test2(1);
        }

        static void Swap<T>(T[] arr, int idx1, int idx2)
        {
            if (idx1 >= arr.Length || idx2 >= arr.Length) return;
            T temp = arr[idx1];
            arr[idx1] = arr[idx2];
            arr[idx2] = temp;
        }

        public static void UnitTest_Swap()
        {
            int[] arr = new int[] { 1, 2 };
            Swap(arr, 0, 1);

            Console.WriteLine(arr[0]);
        }
        static bool TryGet(out A a)
        {
            a = null;
            return true;
        }

        public static void UnitTest_RefOutNull()
        {
            A aaa;
            TryGet(out aaa);
        }
        public static void UnitTest_Typeof()
        {
            object obj = 1;
            if (obj.GetType() == typeof(int))
            {
                Console.WriteLine("ok");
            }
            else
                Console.WriteLine("error");

            if (obj is int)
            {
                Console.WriteLine("ok");
            }
            else
                Console.WriteLine("error");

        }
        public class TestGenrRefBase
        {
            public int v;
            TestGenrRefBase t;
            ILRuntimeTest.TestFramework.TestStruct str;
            static ILRuntimeTest.TestFramework.TestStruct str2;
            public TestGenrRefBase()
            {
                v = 1;
            }

            public TestGenrRefBase(int val)
            {
                v = val;
            }

            void TestRef(ref int a)
            {
                a = 123;
            }

            public void DoTestRef()
            {
                TestRef(ref v);
            }

            public void DoTestRef2()
            {
                t = new TestGenrRef();
                t.v = 2;
                TestGenrRefBase t2 = ReadData(ref t);

                Console.WriteLine("new val:" + t2.v);
                Console.WriteLine("new val:" + t.v);
            }

            public void DoTestRef3()
            {
                ILRuntimeTest.TestFramework.TestStruct.DoTest(ref str);

                Console.WriteLine("new val:" + str.value);

                ILRuntimeTest.TestFramework.TestStruct.DoTest(ref str2);

                Console.WriteLine("new val:" + str2.value);

                ILRuntimeTest.TestFramework.TestStruct.DoTest(ref v);

                Console.WriteLine("new val:" + v);

                var b = str;
                b.value = 1233333;
                ILRuntimeTest.TestFramework.TestStruct.DoTest2(str);
                Console.WriteLine("new val:" + str.value);
            }

            public string GetString()
            {
                return "1";
            }

            public T ReadData<T>(ref T obj) where T : TestGenrRefBase, new()
            {
                obj = new T();
                obj.v = 8;
                return obj;
            }
        }

        public class TestGenrRef : TestGenrRefBase
        {
            public TestGenrRef()
            {
                v = 5;
            }
        }

        static TestGenrRef ttt;
        public static void UnitTest_GenericsRefOut()
        {
            ttt = new TestGenrRef();
            ttt.v = 2;
            TestGenrRef t2 = ReadData<TestGenrRef>(ref ttt);

            Console.WriteLine("new val:" + t2.v);
            Console.WriteLine("new val:" + ttt.v);

            t2.DoTestRef();
            Console.WriteLine("new val:" + t2.v);
            Console.WriteLine("new val:" + ttt.v);

            t2.DoTestRef2();
            t2.DoTestRef3();
        }


        static T ReadData<T>(ref T obj) where T : TestGenrRefBase, new()
        {
            obj = new T();
            obj.v = 6;
            return obj;
        }

        public static void UnitTest_GenericsRefOut2()
        {

            TestGenrRef t = ReadData2<TestGenrRef>(null);

            Console.WriteLine("new val:" + t.v);
        }


        static T ReadData2<T>(byte[] data) where T : TestGenrRefBase, new()
        {
            T obj = null;
            Read<T>(ref obj);
            return obj;
        }

        static T Read<T>(ref T dest) where T : TestGenrRefBase, new()
        {
            dest = new T();
            dest.v = 111;
            dest.GetString();
            return dest;
        }

        public static void UnitTest_GenericsRefOutList()
        {
            List<TestGenrRef> list = null;
            ReadList<TestGenrRef>(ref list);
            Console.WriteLine(list[0]);
        }

        static int ReadList<T>(ref List<T> dest) where T : TestGenrRefBase, new()
        {
            if (dest == null)
                dest = new List<T>();

            T obj = new T();

            dest.Add(obj);
            return 0;
        }


        public static void UnitTest_RefTest()
        {
            TestCls r = null;
            TestRef(ref r);
            Console.WriteLine("Result = " + r.TestVal2);
            float a = 1.0f;
            float b = a / 2;
            var str = string.Format("{0:0.000}", b);
            Console.WriteLine(str);
        }

        public static void UnitTest_RefTest2()
        {
            SingletonTest r = new TestCases.SingletonTest();
            SingletonTest r2 = TestRef<SingletonTest>(ref r);

            Console.WriteLine("Result = " + r.testFloat);

        }

        public static void UnitTest_RefTest3()
        {
           //下面这里跑不过
            var arr = new int[3];
            Func(ref arr);
            Console.WriteLine(arr[1]);
        }

        static void Func(ref int[] arr)
        {
            arr[1] = 2;
        }


        public static void UnitTest_OutTest()
        {
            Dictionary<int, TestCls> dic = new Dictionary<int, TestCls>();
            dic[1] = new TestCls();
            TestCls abc;
            if (dic.TryGetValue(1, out abc))
            {
                Console.WriteLine(abc.TestVal2);
            }
        }
        static int TestRef(ref TestCls dest)
        {
            dest = new TestCases.TestCls();
            dest.TestVal2 = 2;
            return 0;
        }

        static T TestRef<T>(ref T obj) where T : SingletonTest, new()
        {
            obj.testFloat = 3;

            return obj;
        }

        public static void UnitTest_ArrayReferenceTest()
        {
            Dictionary<int, TestGenrRef> dic = new Dictionary<int, TestGenrRef>();
            dic[123] = new TestGenrRef();
            TestGenrRef[] arr = new TestGenrRef[10];
            if (dic.TryGetValue(123, out arr[1]))
            {
                arr[1].DoTestRef();
            }
            else
                throw new Exception();
        }

        class LongHolder
        {
            public long lv;
        }

        public static void UnitTest_LongRefTest()
        {
            long stackof = 4200000000L;
            long stackl = 24;
            var heapLong = new LongHolder();
            ILRuntimeTest.TestFramework.ClassInheritanceTest.TestLongRef(ref stackof);//无论代码是否binding，输出TestLongRef:-94967296
            ILRuntimeTest.TestFramework.ClassInheritanceTest.TestLongRef(ref stackl);//无论代码是否binding，输出TestLongRef:24
            ILRuntimeTest.TestFramework.ClassInheritanceTest.TestLongRef(ref heapLong.lv);//若代码binding，输出TestLongRef:4（或者其他莫名的值），否则输出TestLongRef:0，换成ref int也一样，class对象的话没测试过。
        }

        public static void UnitTest_LongRefTest2()
        {
            long test = 1;
            UnitTest_LongRefTest2Sub(ref test);

            Console.WriteLine(string.Format("{0:X8}", test));
        }
        private static void UnitTest_LongRefTest2Sub(ref long mask)
        {
            for (int i = 0; i < 50; i++)
            {
                mask |= (long)1 << i;
            }
        }
    }
}
