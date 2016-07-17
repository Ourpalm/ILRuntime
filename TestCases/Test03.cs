using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCases
{
    class Test03
    {
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
            
            return index;
        }

        class DicTest
        {
            public int Value { get; set; }
        }
    }
}
