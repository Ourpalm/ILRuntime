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
    }
}
