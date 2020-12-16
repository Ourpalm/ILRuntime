using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    class TestStatic
    {
        public static Dictionary<string, int> staticTest = new Dictionary<string, int>();
    }
    public class SimpleTest
    {
        
        static int b;
        int a;
        int c = 2;
        static SimpleTest()
        {
            b = 1;
        }

        public SimpleTest()
            : this(1)
        {
            Action a = () =>
            {
                c = 2;
            };
            a();
        }
        public SimpleTest(int a)
        {
            Action act = () =>
            {
                this.a = a;
            };
            act();
        }

        public void Test(int a, int c)
        {
            this.a = a;
            this.c = c;
        }
        public static long bar()
        {
            long b = 0;
            for (int i = 0; i < 10000; i++)
            {
                b += i;
            }

            return b;
        }
        public static int foo()
        {
            int b = 0;
            for (int i = 0; i < 50; i++)
            {
                b += foo(b);
            }
            return b;
        }

        public static int foo(object init)
        {
            int b = (int)init;
            for (int i = 0; i < 10000; i++)
            {
                b += i;
            }
            //SimpleTest.b = b;
            return b;
        }

        public static int foo2()
        {
            int b = 0;
            for (int i = 0; i < 500000; i++)
            {
                b += i;
            }

            return b;
        }

        public static int foo3()
        {
            int b = 3;
            int a = 4;
            int c = 5;
            return b + a - c;
        }

        public static void StaticTest()
        {
            TestStatic.staticTest.Add("1", 1);
            Console.WriteLine("Value for 1:" + TestStatic.staticTest["1"]);
        }

        public static void InstanceTest()
        {
            SimpleTest t = new SimpleTest();
            int a = t.c;
            SimpleTest t2 = new SimpleTest(12);            
            Console.WriteLine(t2.a.ToString());
            t2.Test(5, 4);
            Console.WriteLine(t2.a.ToString());
            Console.WriteLine(t2.c.ToString());
            int b = t2.c;
            if (a == b)
            {
                Console.WriteLine("true");
            }
            if (t != t2)
            {
                Console.Write("true 2");
            }
        }

        public class TestA
        {
            private static TestA _instance;
            public static TestA Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new TestA("testerror");
                    }
                    return _instance;
                }
            }

            private string name = "";
            public TestA(string name)
            {
                this.name = name;
            }

            public void TestCall(string name)
            {
                Console.WriteLine($"{this.name},{name}");
            }
        }

        public static void TestStaticFieldInstance()
        {
            TestA.Instance.TestCall("error test");
        }

        public static void MultiDimensionalArrayTest()
        {
            int[,] arr = new int[3, 4];
            arr[1, 2] = 3;

            MultiDimensionalArrayTestSub(arr);
        }

        static void MultiDimensionalArrayTestSub(int[,] arr)
        {
            Console.WriteLine("arr = " + arr[1, 2]);
        }

        public class FileCode<T>
        {
            public static readonly bool TypeSupportsPacking = default(T) != null;
        }

        public static void GenericDefaultTest()
        {
            var ins = new FileCode<uint>();
            Console.WriteLine(FileCode<uint>.TypeSupportsPacking);
            Console.WriteLine(FileCode<string>.TypeSupportsPacking);
        }

        public static void EqualsTest()
        {
            act1 = foo4;
            Action act2 = foo4;
            Console.WriteLine(new Test4().Equals2(act2));//true
            Console.WriteLine(new Test4().Equals(act2));//false
        }

        public static void NullableTest()
        {
            int? val = 123;
            if (val.HasValue)
                Console.WriteLine(val.Value.ToString());
        }

        static void foo4() { }
        static Action act1 = null;
        class Test4
        {
            public bool Equals2(object obj)
            {
                return act1.Equals(obj);
            }

            public override bool Equals(object obj)
            {
                return act1.Equals(obj);
            }
        }
    }
}
