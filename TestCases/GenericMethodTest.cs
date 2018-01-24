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

            public T SeekUI<T>(T rootTrans, string path) where T : ClassInheritanceTest
            {
                return rootTrans;
            }

            public T SeekUI<T>(string path) where T : ClassInheritanceTest, new()
            {
                return SeekUI<T>(new T(), path);
            }

            public void SeekUI<T>(string path, out T control) where T : ClassInheritanceTest, new()
            {
                control = SeekUI<T>(path);
            }

        }

        delegate void ACallback<T>(T t);

        class AClass
        {
            public string MSG;
        }

        class GenericClass<T>
        {
            public T t;
        }

        class TClass
        {
            public void Output<T>(T d, ACallback<T> callback)
            {
                callback(d);
            }

            public void Test<T>(T d, ACallback<T> callback)
            {
                Output(d, callback);
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

        public static void GenericMethodTest2()
        {
            GTest test = new GTest();
            var t2 = test.SeekUI<TestCls>("abse");

            Console.WriteLine(t2.TestVal2.ToString());
        }

        public static void GenericMethodTest3()
        {
            var tclass = new TClass();

            var a = new AClass() { MSG = "AClass" };
            Console.WriteLine("Simple Type");
            tclass.Test(a, param =>
            {
                Console.WriteLine($"param: {LitJson.JsonMapper.ToJson(param)}");
            });

            var ga = new GenericClass<AClass>() { t = a };
            Console.WriteLine("Complex Type");
            tclass.Test(ga, param =>
            {
                Console.WriteLine($"param: {LitJson.JsonMapper.ToJson(param)}");
            });
        }
    }
}
