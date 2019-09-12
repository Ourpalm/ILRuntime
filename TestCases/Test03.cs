using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{

    class Test03
    {
        public static void Test_01()
        {
            Console.WriteLine(Equals2(new int[] { 1 }, 1)); //NotSupportedException: Not supported opcode Readonly
            Console.WriteLine(Equals(1, new object())); //InvalidCastException: Cannot cast from source type to destination type.
            Console.WriteLine(IndexOf(new byte[] { 0, 1, 2, 3 }, (byte)1)); //NullReferenceException: Object reference not set to an instance of an object
        }

        static bool Equals2<T>(T[] a, T b)
        {
            return a[0].Equals(b);
        }

        static bool Equals<T>(T a, object b)
        {
            return a.Equals(b);
        }

        static int IndexOf<T>(T[] array, T value)
        {
            return System.Array.IndexOf(array, value);
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

        public static void Test04()
        {

            Test04Sub((byte)1); //wrong
            Test04Sub2(new object[] { (byte)1 }); //wrong
            Test04Sub3((byte)1); //right
        }

        static void Test04Sub(params object[] o)
        {
            Test04Sub3(o[0]);
        }
        static void Test04Sub2(object[] o)
        {
            Test04Sub3(o[0]);
        }

        static void Test04Sub3(object o)
        {
            List<object> l = new List<object>() { o };
            Console.WriteLine("contains int:" + l.Contains(1));
            Console.WriteLine("contains byte:" + l.Contains((byte)1));
        }
        
        public static void Test05()
        {
            IsBombOrHulu(new byte[] { 1, 1, 3, 3, 5, 6, 7, 8, 9 }, false, false);

        }
        const int StarCard = 0x41;
        public static bool IsBombOrHulu(byte[] sortedCards, bool crazy, bool hasStarCard)
        {
            var groupList =
                (from p in sortedCards
                 where p != StarCard
                 group p by p
                    into g
                 select new { Card = GetCardNum(g.Key), Count = g.Count() }).OrderBy(p => p.Count).ToArray();
            foreach(var i in groupList)
            {
                Console.WriteLine($"{i.Card},{i.Count}");
            }
            return true;
        }
        public static int GetCardNum(byte card)
        {
            return card & 0x0F;
        }
    }
}
