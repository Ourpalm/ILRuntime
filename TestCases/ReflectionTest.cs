using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public class ReflectionTest
    {
        public static void ReflectionTest01()
        {
            Console.Write("Test typeof...");
            Console.WriteLine(typeof(TestCls) == typeof(TestCls));
            Console.WriteLine(typeof(TestCls) == typeof(TestCls2));
            Console.WriteLine(typeof(TestCls) != typeof(TestCls2));
        }

        public static void ReflectionTest02()
        {
            var type = typeof(TestCls);
            var method = type.GetMethod("foo");
            var method2 = type.GetMethod("bar");

            method.Invoke(new TestCls(), new object[] { 100 });
            method2.Invoke(null, null);
        }

        public static void ReflectionTest03()
        {
            var ins = new TestCls();
            var type = ins.GetType();
            var method = type.GetMethod("foo");
            var method2 = type.GetMethod("bar");

            method.Invoke(ins, new object[] { 100 });
            method2.Invoke(null, null);
        }

        public static void ReflectionTest04()
        {
            var arr = typeof(TestCls2).GetCustomAttributes(typeof(TestAttribute), false);
            foreach(var i in arr)
            {
                TestAttribute a = (TestAttribute)i;
                Console.WriteLine(a.TestProp);
            }

            arr = typeof(TestCls).GetCustomAttributes(false);
            foreach (var i in arr)
            {
                Console.WriteLine(i);
            }
        }


        public static void ReflectionTest05()
        {
            var fi = typeof(TestCls).GetField("aa");
            var fi2 = typeof(TestCls).GetField("bb");

            var a = new TestCls();

            Console.WriteLine("aa=" + fi.GetValue(a));
            Console.WriteLine("bb=" + fi2.GetValue(null));

            fi.SetValue(a, 123);
            fi2.SetValue(null, 233);
            Console.WriteLine("aa=" + fi.GetValue(a));
            Console.WriteLine("bb=" + fi2.GetValue(null));

            a.foo(110);

            var arr = fi.GetCustomAttributes(false);
            foreach (var i in arr)
            {
                Console.WriteLine(i);
            }
        }

        [Obsolete("gasdgas")]
        class TestCls
        {
            [ContextStatic]
            int aa = 203;

            [ContextStatic]
            static int bb = 333;
            [Microsoft.SqlServer.Server.SqlFunction(DataAccess = Microsoft.SqlServer.Server.DataAccessKind.Read)]
            public TestCls foo(int b)
            {
                Console.WriteLine("foo" + (aa + b));

                return this;
            }

            public static void bar()
            {
                Console.WriteLine("bar");
            }
        }

        [Test(true, TestProp = "1234")]
        [Test]
        [Serializable]
        class TestCls2
        {

        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        class TestAttribute : Attribute
        {
            bool testField;
            public TestAttribute()
            {

            }
            public TestAttribute(bool test)
            {
                testField = test;
            }

            public string TestProp { get; set; }
        }
    }
}
