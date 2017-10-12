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
            var t = Type.GetType("TestCases.ReflectionTest/TestCls2");
            var obj = Activator.CreateInstance(t);
            Console.WriteLine(obj);

            var arr = t.GetCustomAttributes(typeof(TestAttribute), false);
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

        public static void ReflectionTest06()
        {
            var arr = typeof(SingletonTest).GetFields();
            foreach (var i in arr)
            {
                Console.WriteLine(i);
            }

            var fi = typeof(SingletonTest).GetField("testFloat");
            fi.SetValue(SingletonTest.Inst, 1.2f);
            Console.WriteLine(SingletonTest.Inst.testFloat);
        }

        public static void ReflectionTest07()
        {
            //-----------------------------CLR-----------------------------//
            var isDefined = typeof(TestCls2).IsDefined(typeof(ObsoleteAttribute), true);

            if (isDefined == false)
            {
                throw new Exception("isDefeinded == false 1");
            }

            isDefined = typeof(TestCls2).GetProperty("Attribute_prop").IsDefined(typeof(TestCLRAttribute), true);

            if (isDefined == false)
            {
                throw new Exception("isDefeinded == false 2");
            }

            isDefined = typeof(TestCls2).GetField("Attribute_field").IsDefined(typeof(TestCLRAttribute), true);

            if (isDefined == false)
            {
                throw new Exception("isDefeinded == false 3");
            }
        }

        public static void ReflectionTest08()
        {
            //-----------------------------CLR-----------------------------//
            var isDefined = typeof(TestCls2).IsDefined(typeof(TestAttribute), true);

            if (isDefined == false)
            {
                throw new Exception("isDefeinded == false 1");
            }

            isDefined = typeof(TestCls2).GetProperty("ILAttribute_prop").IsDefined(typeof(TestAttribute), true);

            if (isDefined == false)
            {
                throw new Exception("isDefeinded == false 2");
            }

            isDefined = typeof(TestCls2).GetField("ILAttribute_field").IsDefined(typeof(TestAttribute), true);

            if (isDefined == false)
            {
                throw new Exception("isDefeinded == false 3");
            }
        }




        [Obsolete("gasdgas")]
        class TestCls
        {
            [ContextStatic]
            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Collapsed)]
            int aa = 203;

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

        
        [Serializable]
        [Obsolete]
        [Test(true, TestProp = "1234")]
        [Test]
        public class TestCls2
        {
            [TestCLR]
            public int Attribute_field;

            [Test]
            public int ILAttribute_field;

            [TestCLR]
            public int Attribute_prop { get; set; }

            [Test]
            public int ILAttribute_prop { get; set; }

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
