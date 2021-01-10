using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    class JsonTest
    {
        enum JsonTestEnum
        {
            Test1,
            Test2,
            Test3,
        }
        class JsonTestClass
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
            public JsonTestEnum EnumProp { get; set; }
            public JsonTestSubClass SubClassProp { get; set; }
            public Dictionary<string, JsonTestSubClass> DicTest { get; set; }
            public Dictionary<string, int> DicTest2 { get; set; }
        }

        class JsonTestSubClass
        {
            public long LongProp { get; set; }
            public List<JsonTestSubClass> SubClassList { get; set; }
            public List<JsonTestEnum> SubClassEnumList { get; set; }
            public JsonTestSubClass[] ArrayProp { get; set; }
        }

        class MyTestDataItem : TestClass3
        {
            public string Type;
        }

        class MyTestDataItem1 // : TestClass3
        {
            public int Type;
        }

        class MyTestDataItem2 // : TestClass3
        {
            public string Type;
        }

        class MyTestDataItem3 // : TestClass3
        {
            public bool B = true;
        }

        class MyTestDataMap //: TestClass3
        {
            public Dictionary<string, MyTestDataItem1> Items1;
            public Dictionary<string, MyTestDataItem2> Items2;
        }

        public static void JsonTest1()
        {
            JsonTestClass cls = new TestCases.JsonTest.JsonTestClass();
            cls.IntProp = 1;
            cls.StringProp = "2";
            cls.EnumProp = JsonTestEnum.Test3;
            var sub = new TestCases.JsonTest.JsonTestSubClass();
            sub.LongProp = 4;
            var sub2 = new TestCases.JsonTest.JsonTestSubClass();
            sub2.LongProp = 5;
            var sub3 = new TestCases.JsonTest.JsonTestSubClass();
            sub3.LongProp = 6;

            cls.SubClassProp = sub;
            sub.ArrayProp = new JsonTestSubClass[2];
            sub.ArrayProp[0] = sub2;
            sub.ArrayProp[1] = sub3;
            sub.SubClassList = new List<TestCases.JsonTest.JsonTestSubClass>();
            sub.SubClassList.Add(sub2);
            sub.SubClassList.Add(sub3);
            sub.SubClassEnumList = new List<JsonTestEnum>();
            sub.SubClassEnumList.Add(JsonTestEnum.Test2);

            cls.DicTest = new Dictionary<string, TestCases.JsonTest.JsonTestSubClass>();
            cls.DicTest["11111"] = sub;
            cls.DicTest2 = new Dictionary<string, int>();
            cls.DicTest2["111222"] = 333444;

            var str = JsonMapper.ToJson(cls);
            Console.WriteLine(str);

            var cls2 = JsonMapper.ToObject<JsonTestClass>(str);
            Console.WriteLine(cls2.SubClassProp.ArrayProp[0].LongProp);
            Console.WriteLine(cls2.SubClassProp.ArrayProp[1].LongProp);
            Console.WriteLine(cls2.SubClassProp.SubClassList[0].LongProp);
            Console.WriteLine(cls2.SubClassProp.SubClassList[1].LongProp);
            Console.WriteLine(cls2.SubClassProp.SubClassEnumList[0]);
            Console.WriteLine(cls2.DicTest["11111"].LongProp);
            Console.WriteLine(cls2.DicTest2["111222"]);

        }

        public static void JsonTest2()
        {
            string json = "[{\"Type\":\"MyName\"}]";
            List<MyTestDataItem> data = JsonMapper.ToObject<List<MyTestDataItem>>(json);
            Console.WriteLine("Name : " + data[0].Type);
        }

        public static void JsonTest3()
        {
            string json = "{\"Items1\":{\"one\":{\"Type\":1},\"two\":{\"Type\":2}},\"Items2\":{\"one\":{\"Type\":\"OneType\"},\"two\":{\"Type\":\"TwoType\"}}}";
            MyTestDataMap data = JsonMapper.ToObject<MyTestDataMap>(json);
            Console.WriteLine("Type : " + data.Items1["one"].Type);
            Console.WriteLine("Type : " + data.Items2["one"].Type);

        }
        public static void JsonTest4()
        {
            var tj = new MyTestDataItem3();
            var json = JsonMapper.ToJson(tj);
            Console.WriteLine(json); //output: {"b":1} Error

            var outTj = JsonMapper.ToObject<MyTestDataItem3>(json); //Error Point!
            Console.WriteLine(outTj.B);
        }

        public static void JsonTest5()
        {
            if (typeof(MyTestDataItem1).Equals(typeof(MyTestDataItem2)))
            {
                Console.WriteLine("MyTestDataItem1 == MyTestDataItem2");
            }
            else
            {
                Console.WriteLine("MyTestDataItem1 != MyTestDataItem2");
            }
            if (typeof(MyTestDataItem1).Equals(typeof(MyTestDataItem1)))
            {
                Console.WriteLine("MyTestDataItem1 == MyTestDataItem1");
            }
            else
            {
                Console.WriteLine("MyTestDataItem1 != MyTestDataItem1");
            }
        }


        class MyClass
        {
            public ILRuntimeTest.TestFramework.TestCLREnum myEnum;
        }
        public static void JsonTest6()
        {
            MyClass myClass = new MyClass();
            myClass.myEnum = ILRuntimeTest.TestFramework.TestCLREnum.Test2;

            string json = LitJson.JsonMapper.ToJson(myClass);

            MyClass myNewClass = LitJson.JsonMapper.ToObject<MyClass>(json);
            if (myNewClass.myEnum == ILRuntimeTest.TestFramework.TestCLREnum.Test2)
            {
                Console.WriteLine("Ok");
            }
            else
            {
                Console.WriteLine("Fail");
            }
        }

        public static void JsonTest7()
        {
            JsonTestEnum[] arr = new JsonTestEnum[] { JsonTestEnum.Test2, JsonTestEnum.Test3 };
            string json = JsonMapper.ToJson(arr);

            JsonTestEnum[] arr2 = JsonMapper.ToObject<JsonTestEnum[]>(json);
            if (arr2[0] != JsonTestEnum.Test2)
                throw new Exception();
        }

        public class GenericTest<T>
        {
            public string name;
            public List<T> data;

            public override string ToString()
            {
                return $"name={name}, data=[{string.Join(", ", data)}]";
            }
        }

        public class DataClass
        {
            public int code;
            public string msg;
            public override string ToString()
            {
                return $"code:{code}, msg:{msg}";
            }
        }
        
        public static void JsonTest8()
        {
            //T如果非热更，是可以的
            Console.WriteLine("Generic Test where T in local");
            GenericTest<int> g = new GenericTest<int>();
            g.name = "intTest";
            g.data = new List<int>()
            {
                0,1,2,3,4,5
            };
            js = JsonMapper.ToJson(g);
            Console.WriteLine("js:\n" + js);
            var gObj = JsonMapper.ToObject<GenericTest<int>>(js);
            Console.WriteLine(gObj.ToString());
            Console.WriteLine("====================");

            //T如果热更，也是可以的
            Console.WriteLine("Generic Test where T in hotfix");
            GenericTest<DataClass> d = new GenericTest<DataClass>();
            d.name = "dataClassTest";
            d.data = new List<DataClass>()
            {
                new DataClass()
                {
                    code = 200,
                    msg="测试200"
                },
                new DataClass()
                {
                    code = 404,
                    msg="测试404"
                }
            };
            js = JsonMapper.ToJson(d);
            Console.WriteLine("js:\n" + js);
            var gObj2 = JsonMapper.ToObject<GenericTest<DataClass>>(js);
            Console.WriteLine(gObj2.ToString());
            Console.WriteLine("====================");
        }

        public class TestJson
        {
            public EnumTest enumTest = EnumTest.c;

            public List<EnumTest> enumTestList = new List<EnumTest>()
            {
                EnumTest.a,
                EnumTest.b
            };

            public List<string> stringTestList = new List<string>()
            {
                "aaa","bbb","ccc"
            };

            public List<int> intTestList = new List<int>()
            {
            1,2,3
            };

            public List<double> doubleTestList = new List<double>()
            {
                1,2,3
            };

            public List<TestJsonSub> TestJsonSubList = new List<TestJsonSub>()
            {
                new TestJsonSub(){name = "sub a"},
                new TestJsonSub(){name = "sub b"},
            };


            public Dictionary<EnumTest, double> enumDict = new Dictionary<EnumTest, double>()
            {
                {EnumTest.a,1.0},
                {EnumTest.b,2.0},
            };


            public Dictionary<string, double> stringDict = new Dictionary<string, double>()
            {
                {"stra",1.0},
                {"strb",2.0},
            };

            public override string ToString()
            {
                string s = $"{enumTest} \n";
                foreach (var test in enumTestList)
                {
                    s += test + ",";
                }
                s += "\n";
                foreach (var test in enumDict)
                {
                    s += test.Key + "=" + test.Value + " , ";
                }
                s += "\n";
                foreach (var test in stringDict)
                {
                    s += test.Key + "=" + test.Value + " , ";
                }
                s += "\n";
                foreach (var test in TestJsonSubList)
                {
                    s += test.name;
                }
                return s;
            }
        }

        public static void JsonTest9()
        {
            //原本的不可以，改了后可以
            Console.WriteLine("Enum/List/Dictionary Test");
            TestJson t = new TestJson();
            t.enumTestList.Add(EnumTest.c);
            t.enumDict.Add(EnumTest.c, 99999.999);
            var js = JsonMapper.ToJson(t);
            Console.WriteLine("js:\n" + js);
            var obj = JsonMapper.ToObject<TestJson>(js);
            Console.WriteLine(obj.ToString());
            Console.WriteLine("====================");
        }

    }
}
