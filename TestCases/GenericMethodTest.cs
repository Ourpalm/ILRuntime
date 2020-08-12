using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using ILRuntimeTest.TestFramework;
using ILRuntimeTest.TestBase;

namespace TestCases
{

    public static class TestGenericExtensionExtension
    {
        public class Test1<T>
        {

        }

        public class Test2<T>
        {

        }
        public static void CreateCollector<T>(this Test1<T> arg1, Test2<T>[] arg2)
        {

        }
    }
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

        public delegate void CallbackDelegate<T>(T response);

        public class Command<T>
        {
            public T Decode()
            {
                var json = "{}";
                return LitJson.JsonMapper.ToObject<T>(json);
            }
        }

        public class OneCallbackData
        {
            public string Message = $"This is {nameof(OneCallbackData)}";
        }

        public class AnotherCallbackData
        {
            public string Message = $"This is {nameof(AnotherCallbackData)}";
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
            public void DoSomething<T>(Command<T> command, CallbackDelegate<T> callback)
            {
                callback(command.Decode());
            }

            public void Test<T>(CallbackDelegate<T> callback)
            {
                DoSomething(new Command<T>(), callback);
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

        public static void GenericMethodTest4()
        {
            var tclass = new TClass();

            tclass.Test<OneCallbackData>(param =>
            {
                Console.WriteLine($"输出: {LitJson.JsonMapper.ToJson(param)}");
            });

            tclass.Test<AnotherCallbackData>(param =>
            {
                Console.WriteLine($"输出: {LitJson.JsonMapper.ToJson(param)}");
            });
        }

        public static void GenericMethodTest5()
        {
            BugTest test = new BugTest();
            test.Test();
        }

        public static void GenericMethodTest6()
        {
            int i = 0;
            Foo(2, out i);
            Console.WriteLine(i);
        }

        static void Foo<T>(T arr, out int i)
        {
            i = 1;
            Console.WriteLine(arr.ToString());
        }

        public static void GenericMethodTest7()
        {
            Foo(new List<int> { }, Bar); //NullReferenceException: Object reference not set to an instance of an object (at Assets/ILRuntime/CLR/TypeSystem/ILType.cs:762)
        }

        public static int Bar(List<int> o)
        {
            return 0;
        }
        public static void Foo<T1, T2>(T1 t, Func<T1, T2> func)
        {

        }

        public static void GenericMethodTest8()
        {
            Foo2(new object[][] { }, Bar2);  //NullReferenceException: Object reference not set to an instance of an object (at Assets/ILRuntime/CLR/TypeSystem/ILType.cs:762)
        }

        public static object Bar2(object o)
        {
            return o;
        }
        public static void Foo2<T1, T2>(T1[] t, Func<T1, T2> func)
        {

        }
        class BugTest
        {
            public class TestClass<T>
            {

            }
            public void Test()
            {
                TestClass<int> r;
                dict.TryGetValue(0, out r);
            }
            Dictionary<int, TestClass<int>> dict = new Dictionary<int, TestClass<int>>();
        }

        //====================分割线，复现流程============================
        // 接口Disposer定义
        public interface IDisposer
        {
            int Id { get; set; }
            void Foo(object bar, int bar2, object bar3);
            void Dispose();
        }
        // 实现IDiposer的类
        public class TestInterface : IDisposer
        {
            public int Id { get; set; }
            public void Foo(object bar, int bar2, object bar3)
            {
                Console.WriteLine(bar.ToString() + bar2 + bar3);
            }
            public void Dispose()
            {
            }
        }

        static T CreateInterface<T>() where T : IDisposer
        {
            var type = typeof(TestInterface);
            var result = Activator.CreateInstance(type);
            return (T)result;
        }


        static T Fetch<T>() where T : IDisposer
        {
            var t = CreateInterface<T>();
            //          ((IDisposer)t).Id = 111; // ??
            t.Id = 111; // ??
            t.Foo(123, 456, 789);
            return t;
        }

        public static void GenericMethodTest9()
        {
            var obj = Fetch<TestInterface>();
            Console.WriteLine(obj.ToString());
        }

        interface IBinderCollection<T>
        {
            
        }
        class BinderCollection<T> : IBinderCollection<T>
        {
            public void BindToGList<TListItem>(object _list, Action<T, TListItem> _bindItemAction)
            {
                Console.WriteLine(_list.ToString());
            }
        }

        public static void GenericMethodTest10()
        {
            BinderCollection<string> binder = new BinderCollection<string>();
            binder.BindToGList<object>(123, (a, b) => { });
        }

        public class TestBind<T> where T : IComparable<T>
        {
            public T data;


            public T Data
            {
                get { return data; }
                set
                {
                    if (null != data && data.CompareTo(value) == 0)
                    {
                        return;
                    }

                    data = value;
                }
            }
        }

        public class SubBind : TestBind<int>
        {

        }

        public static void GenericMethodTest11()
        {
            SubBind bind = new SubBind();
            bind.Data = 100;

            Console.WriteLine("bind.data = " + bind.data);
        }

        public static void GenericMethodTest12()
        {
            GenericMethodTest12Sub(new int[] { 3 }, k => {
                k = k + 2;//error;
                k = k << 2;//error;
            });
        }

        static void GenericMethodTest12Sub<T>(T[] a, Action<T> func)
        {
            func(a[0]);
        }

        public static void GenericMethodTest13()
        {
            GenericMethodTest13Sub<int>();
        }

        static void GenericMethodTest13Sub<T>()
        {
            var t = typeof(T[]);
            Console.WriteLine(t.ToString());
        }

        public static void GenericMethodTest14()
        {
            Dictionary<int, List<int>> _notSelDict = new Dictionary<int, List<int>>();
            var notSelGuids = _notSelDict.SelectMany(dict => dict.Value).ToList();
            Console.WriteLine(notSelGuids.Count.ToString());
        }

        public static void GenericExtensionMethod1Test1()
        {
            new ExtensionClass().Method1(v => { });
        }

        public static void GenericExtensionMethod1Test2()
        {
            new ExtensionClass().Method1((i, v) => { });
        }

        public static void GenericExtensionMethod2Test1()
        {
            new ExtensionClass().Method2(e => { });
        }

        public static void GenericExtensionMethod2Test2()
        {
            new ExtensionClass().Method2((i, e) => { });
        }

        public static void GenericExtensionMethod2Test3()
        {
            new ExtensionClass().Method2<ArgumentException>(e => { });
        }

        public static void GenericExtensionMethod2Test4()
        {
            new ExtensionClass().Method2<ArgumentException>((i, e) => { });
        }

        public static void GenericExtensionMethod2Test5()
        {
            new ExtensionClass<int>().Method2(e => { });
        }

        public static void GenericExtensionMethod2Test6()
        {
            new ExtensionClass<int>().Method2((i, e) => { });
        }

        public static void GenericExtensionMethod2Test7()
        {
            new ExtensionClass<int>().Method2<int, ArgumentException>(e => { });
        }

        public static void GenericExtensionMethod2Test8()
        {
            new ExtensionClass<int>().Method2<int, ArgumentException>((i, e) => { });
        }

        public static void GenericExtensionMethod3Test1()
        {
            new ExtensionClass().Method3(null);
        }

        public static void GenericExtensionMethod3Test2()
        {
            new ExtensionClass<int>().Method3(null);
        }

        public static void GenericExtensionMethod3Test3()
        {
            new SubExtensionClass().Method3(null);
        }

        public static void GenericExtensionMethod3Test4()
        {
            new SubExtensionClass<int>().Method3(null);
        }

        public static void GenericExtensionMethod3Test5()
        {
            new ExtensionClass().Method3(new ArgumentException());
        }

        public static void GenericExtensionMethod3Test6()
        {
            new ExtensionClass<int>().Method3(new ArgumentException());
        }

        public static void GenericExtensionMethod3Test7()
        {
            new SubExtensionClass<int>().Method3(new ArgumentException());
        }

        public static void GenericExtensionMethod3Test8()
        {
            var t1 = new TestGenericExtensionExtension.Test1<int>();
            var t2 = new TestGenericExtensionExtension.Test2<int>[] { new TestGenericExtensionExtension.Test2<int>() };
            t1.CreateCollector(t2);
        }        

        public static void GenericStaticMethodTest1()
        {
            StaticGenericMethods.StaticMethod(() => { });
        }

        public static void GenericStaticMethodTest2()
        {
            StaticGenericMethods.StaticMethod(i => { });
        }

        public static void GenericStaticMethodTest3()
        {
            StaticGenericMethods.StaticMethod(() => 1);
        }

        public static void GenericStaticMethodTest4()
        {
            StaticGenericMethods.StaticMethod(i => 1);
        }
        
        public static void GenericStaticMethodTest5()
        {
            StaticGenericMethods.StaticMethod(async () => await Task.Delay(1));
        }

        public static void GenericStaticMethodTest6()
        {
            StaticGenericMethods.StaticMethod(async i => await Task.Delay(1));
        }

        public static void GenericStaticMethodTest7()
        {
            StaticGenericMethods.StaticMethod(async () => await Task.FromResult(1));
        }

        public static void GenericStaticMethodTest8()
        {
            StaticGenericMethods.StaticMethod(async i => await Task.FromResult(1));
        }
        public static void GenericStaticMethodTest9()
        {           
            GenericStaticMethodTest_9("","",(MethodTest9_A[] arr) => {

            });
        }

        class MethodTest9_A { };
        public static void GenericStaticMethodTest_9<T>(string bundle, string assetname, Action<T[]> callback)
        {
            
        }
        static void TestMethod<T>(Action<GenericClass<T>> enter)
        {
            GenericClass<T> ins = new GenericClass<T>();
            enter(ins);

        }
        public static void GenericStaticMethodTest10()
        {
            TestMethod<int>((ins) =>
            {
                Console.WriteLine(ins.t);
            });
        }


        class ClassA<T>
        {
        }
        class ClassB<TT>
        {
            public void TestMethod(out ClassA<TT> enter)
            {
                enter = null;
            }
        }

        public static void GenericStaticMethodTest11()
        {
            ClassB<int> a = new ClassB<int>();
            ClassB<string> b = new ClassB<string>();

            ClassA<int> aa = new ClassA<int>();
            ClassA<string> bb = new ClassA<string>();
            a.TestMethod(out aa);
            b.TestMethod(out bb);
        }

        public static void GenericStaticMethodTest12()
        {
            ILRuntimeTest.TestBase.StaticGenericMethods.Method("");
        }

        public static void GenericStaticMethodTest13()
        {
            GenericStaticMethodTest13Sub<int[]>();
        }
        static void GenericStaticMethodTest13Sub<T>()
        {
            var t = typeof(T);//出错
            Console.WriteLine(t);
        }
    }
}
