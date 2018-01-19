using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    class GenericMethodTest
    {
        class GTest
        {
            public void SetValue<K, V>(Dictionary<K, V> value)
            {
                Console.WriteLine($"SetValue1,{value.Count}");
            }

            public void SetValue<K, V>(List<Dictionary<K, V>> value)
            {
                Console.WriteLine($"SetValue2,{value.Count}");
            }

            public void SetValue<K, V>(Dictionary<K, List<V>> value)
            {
                Console.WriteLine($"SetValue3,{value.Count}");
            }
        }

        public static void GenericMethodTest1()
        {
            GTest test = new GTest();
            List<Dictionary<string, object>> content = new List<Dictionary<string, object>>();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("key1", 1000);
            content.Add(dic);

            test.SetValue(content);
        }
    }
}
