using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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
            foreach (var i in arr)
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

            var attr = (TestCLRAttribute)typeof(TestCls2).GetField("Attribute_field").GetCustomAttributes(typeof(TestCLRAttribute), false)[0];
            if (attr.Name != "Example")
            {
                throw new Exception("attr.Name != Example");
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

            var attr = (TestAttribute)typeof(TestCls2).GetField("ILAttribute_field").GetCustomAttributes(typeof(TestAttribute), false)[0];
            if (attr.Name != "Example")
            {
                throw new Exception("attr.Name != Example");
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
            public TestCls2()
            {

            }

            public TestCls2(int a1, int a2)
            {
                Attribute_field = a1;
                ILAttribute_field = a2;
            }
            [TestCLR(Name = "Example")]
            public int Attribute_field;

            [Test(Name = "Example")]
            public int ILAttribute_field;

            [TestCLR]
            public int Attribute_prop { get; set; }

            [Test]
            public int ILAttribute_prop { get; set; }

            public override string ToString()
            {
                return $"Attribute_field={Attribute_field},ILAttribute_field={ILAttribute_field}";
            }

        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        class TestAttribute : Attribute
        {
            bool testField;
            public string Name;
            public TestAttribute()
            {

            }
            public TestAttribute(bool test)
            {
                testField = test;
            }

            public string TestProp { get; set; }
        }

        public class MyClass
        {
            public uint B { get; set; }
        }

        public static void ReflectionTest09()
        {
            var r = new MyClass()
            {
                B = 14,
            };

            PropertyInfo p = typeof(MyClass).GetProperty("B");
            object obj = p.GetGetMethod().Invoke(r, null);
            Console.WriteLine("p type: " + obj.GetType().FullName); //显示System.Int32，正确应该是System.UInt32

            if (obj is ValueType)
            {
                Console.WriteLine("value type");
            }
            else
            {
                throw new Exception("not value type"); //走进了这里
            }

            obj = p.GetValue(r, null);
            Console.WriteLine("p type: " + obj.GetType().FullName); //显示System.Int32，正确应该是System.UInt32

            if (obj is ValueType)
            {
                Console.WriteLine("value type");
            }
            else
            {
                throw new Exception("not value type"); //走进了这里
            }
        }

        public class Tx
        {
            public float FloatField { get; set; }
            public int IntField { get; set; }
            public EnumTest.TestEnum EnumField { get; set; }
            public static int StaticField { get; set; }
        }

        public static void ReflectionTest10()
        {
            Tx obj = new Tx { FloatField = 21, IntField = 21 };
            Type t = obj.GetType();
            var fields = t.GetProperties(BindingFlags.Public);
            var info = fields[0]; //FloatField

            object value = info.GetGetMethod().Invoke(obj, null);
            Console.WriteLine(string.Format("{0} = {1}", info.Name, value));

            if (value == null)
            {
                throw new Exception("null obj - FloatField"); // 不应该走到这里来，但走到了这里
            }

            info = fields[1];

            value = info.GetGetMethod().Invoke(obj, null);
            Console.WriteLine(string.Format("{0} = {1}", info.Name, value));

            if (value == null)
            {
                throw new Exception("null obj - IntField");
            }
            else
            {
                Console.WriteLine("not null obj - IntField"); // 对于int是正确的，走到了这里
            }

            info = fields[2];

            value = info.GetGetMethod().Invoke(obj, null);
            Console.WriteLine(string.Format("{0} = {1}", info.Name, value));

            info = fields[3];

            value = info.GetValue(null, null);
            Console.WriteLine(string.Format("{0} = {1}", info.Name, value));
        }



        class test24Class
        {
            public int this[int index, long index2]
            {
                get
                {
                    return (int)(index + index2);
                }
                set
                {
                    Console.WriteLine($"{index},{index2}={value}");
                }
            }
        }
        public static void ReflectionTest11()
        {
            ReflectionTest11Sub(new test24Class());
        }

        static void ReflectionTest11Sub(object o)
        {
            var p = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);//error
            foreach (var i in p)
            {
                Console.WriteLine(i.GetValue(o, new object[] { 1, 2L, 3333 }));
                i.SetValue(o, 333, new object[] { 123, 345L, 678 });
            }
        }

        public static void ReflectionTest12()
        {
            var types = ILRuntimeTest.TestMainForm._app.LoadedTypes.ToArray();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i].Value.ReflectionType;

                //if (type.BaseType != null && (type.BaseType == typeof(Attribute) || type.BaseType.Name == "Attribute"))
                //    continue;

                //if (type.BaseType != null && type.BaseType.Name == "Void")
                //    continue;

                object[] attrs = type.GetCustomAttributes(typeof(TestAttribute), false);
            }

        }

        public static void ReflectionTest13()
        {
            object[] attrs = typeof(TestController).GetCustomAttributes(typeof(ObjectEventAttribute), false);
            //结果attrs的Length > 0 , 这是错误的结果吧
            Console.WriteLine(attrs.Length);

        }

        public static void ReflectionTest14()
        {
            TestTypeAssignableFrom(typeof(PlayerInfo));
        }

        class Ron<T>
        {
            public T Value { get; set; }
        }
        public static void ReflectionTest15()
        {
            Type t = typeof(Ron<>);
            Console.WriteLine("t Type ==> " + t);
            Type constructed = t.MakeGenericType(new Type[] { typeof(string) });
            Ron<string> ins = Activator.CreateInstance(constructed, new object[] { "Test Success" }) as Ron<string>;
            ins.Value = "OK";
            Console.WriteLine(ins.Value);
        }
        private static void TestTypeAssignableFrom(Type targetType)
        {
            foreach (System.Reflection.PropertyInfo property in targetType.GetProperties())
            {
                if (!property.CanWrite)
                {
                    continue;
                }
                Console.WriteLine(property.Name + "|" + typeof(System.Collections.ICollection).IsAssignableFrom(property.PropertyType));
            }

            foreach (System.Reflection.FieldInfo field in targetType.GetFields())
            {
                Console.WriteLine(field.Name + "|" + typeof(System.Collections.ICollection).IsAssignableFrom(field.FieldType));
            }
        }


        class PlayerInfo
        {
            public string[] tags_F;

            public Detail[] Details_F;

            public string[] tags_P { get; set; }

            public Detail[] Details_P { get; set; }
        }

        class Detail
        {
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class ObjectEventAttribute : Attribute
        {
        }

        [Test]
        public sealed class TestController
        {
            public static TestController instance = new TestController();
        }

        public static void ReflectionTest16()
        {
            MethodInfo info = typeof(ReflectionTest).GetMethod(nameof(ReflectionTest16Sub), BindingFlags.NonPublic | BindingFlags.Static);
            info.Invoke(null, new object[] { null });
            info.Invoke(null, new object[] { null });
        }

        public static void ReflectionTest17()
        {
            MethodInfo info = typeof(ReflectionTest).GetMethod(nameof(ReflectionTest17Sub), BindingFlags.NonPublic | BindingFlags.Static);
            int cnt = 0;
            cnt += (int)info.Invoke(null, new object[] { null });
            cnt += (int)info.Invoke(null, new object[] { null });

            Console.WriteLine(cnt);
            if (cnt != 40)
                throw new Exception("Wrong result");
        }

        static void ReflectionTest16Sub(TestCases.TestCls t)
        {
            Console.WriteLine("OK");
        }

        static int ReflectionTest17Sub()
        {
            return 20;
        }

        public static void ReflectionTest18()
        {
            var t = Type.GetType("TestCases.ReflectionTest/TestCls2");
            var obj = Activator.CreateInstance(t, 2, 3);

            Console.WriteLine(obj);
        }

        public static void ReflectionTest19()
        {
            var t = Type.GetType("TestCases.ReflectionTest");
            var mi = t.GetMethod(nameof(ReflectionTest19));
            if (!mi.IsStatic)
                throw new Exception();
        }

        public static void ReflectionTest20()
        {
            var t = Type.GetType("System.Action`1<int[]>");

            Console.WriteLine(t);
        }
    }
}
