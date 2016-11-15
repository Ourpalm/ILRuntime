using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{

    class Test03
    {
        public class TestGenrRefBase
        {
            public int v;

            public TestGenrRefBase()
            {
                v = 1;
            }

            public TestGenrRefBase(int val)
            {
                v = val;
            }
        }

        public class TestGenrRef : TestGenrRefBase
        {
            public TestGenrRef()
            {
                v = 5;
            }
        }

        public static void UnitTest_GenericsRefOut()
        {
            TestGenrRef t1 = new TestGenrRef();
            t1.v = 2;
            TestGenrRef t2 = ReadData<TestGenrRef>(ref t1);

            Console.WriteLine("new val:" + t2.v);
        }


        static T ReadData<T>(ref T obj) where T : TestGenrRefBase, new()
        {
            obj = new T();
            obj.v = 6;
            return obj;
        }

        public static void UnitTest_GenericsRefOut2()
        {

            TestGenrRef t = ReadData2<TestGenrRef>(null, new TestGenrRef());

            Console.WriteLine("new val:" + t.v);
        }


        static T ReadData2<T>(byte[] data, T obj) where T : TestGenrRefBase, new()
        {   
            return Read<T>(obj);
        }

        static T Read<T>(T dest) where T : TestGenrRefBase, new()
        {
            dest.v = 111;
            return dest;
        }

        public static void UnitTest_RefTest()
        {
            TestCls r = new TestCases.TestCls();
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


        public static void UnitTest_OutTest()
        {
            Dictionary<int, TestCls> dic = new Dictionary<int, TestCls>();
            dic[1] = new TestCls();
            TestCls abc;
            if(dic.TryGetValue(1, out abc))
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

        public static object Run()
        {
            List<int> list1 = new List<int>();//c#Light 不支持模板，所以这里要注意一下
                                              //List<int> 可以 List < int > 有空格不可以
            list1.Add(1);
            list1.Add(2);
            list1.Add(3);
            List<List<int>> list2 = new List<List<int>>();

            list2.Add(list1);
            List<List<List<int>>> list3 = new List<List<List<int>>>();
            list3.Add(list2);

            return list3;
        }

        public static object Run2()
        {
            Dictionary<string, int> test1 = new Dictionary<string, int>();
            test1["aa"] = 100;
            test1["bb"] = 200;

            Console.WriteLine("Test dic start...");
            Console.WriteLine("aa=" + test1["aa"]);
            Console.WriteLine("bb=" + test1["bb"]);

            Dictionary<string, DicTest> test2 = new Dictionary<string, DicTest>();
            DicTest a = new DicTest();
            a.Value = 100;
            test2["aa"] = a;
            a = new DicTest();
            a.Value = 200;
            test2["bb"] = a;

            Console.WriteLine("Test dic2 start...");
            Console.WriteLine("aa=" + test2["aa"].Value);
            Console.WriteLine("bb=" + test2["bb"].Value);

            Dictionary<DicTest, int> test3 = new Dictionary<DicTest, int>();
            a = new DicTest();
            a.Value = 100;
            DicTest b = new DicTest();
            b.Value = 200;

            test3[a] = 100;
            test3[b] = 200;

            Console.WriteLine("Test dic3 start...");
            Console.WriteLine("aa=" + test3[a]);
            Console.WriteLine("bb=" + test3[b]);

            return test3;
        }
        
        public static object Run3()
        {
            int[] arr = new int[] { 1, 2, 3, 4, 5 };
            int index = Array.IndexOf(arr, 3);
            List<int[]>[] arr2 = new List<int[]>[10];
            List<int[]> e = new List<int[]>();
            e.Add(arr);
            Console.WriteLine("e.Contains:" + e.Contains<int[]>(arr));
            arr2[3] = e;
            index = Array.IndexOf(arr2, e);
            return index;
        }

        class DicTest
        {
            public int Value { get; set; }
        }
    }
}
