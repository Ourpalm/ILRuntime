
//using CSEvilTestor;
using System;
using System.Collections.Generic;
using System.Text;
namespace TestCases
{
    class Student<T>
    {
        public T name;
        public Student(T name)
        {
            this.name = name;
            Console.WriteLine(name);
        }
    }
    class TestExplicit
    {
        bool boolVal;
        double doubleVal;

        public TestExplicit()
        {

        }

        public TestExplicit(bool boolVal)
        {
            this.boolVal = boolVal;
        }

        public TestExplicit(double doubleVal)
        {
            this.doubleVal = doubleVal;
        }

        public static explicit operator bool(TestExplicit data)
        {
            return data.boolVal;
        } 

        public static explicit operator double(TestExplicit data)
        {
            return data.doubleVal;
        }
    }
    public class B
    {
        public int aa = 2;
        public B()
        {

        }
        public B(int a)
        {
            aa = a;
        }
        public virtual void Print()
        {
            Console.WriteLine("I am B"); ;
        }
    }

    class A
    {
        public int a = 1;
        public A(int a)
        {
            this.a = a;
        }

        public void Check(A e)
        {
            Console.WriteLine("other != this = " + (e != this && e.Geta() == this.Geta()));
        }

        public int Geta()
        {
            return a;
        }
    }

    class NRow<A, B>
    {
        public A K { get; set; }
        public B V { get; set; }
    }

    class Test05
    {
        class MGenericTest
        {
            public TestStruct TestGeneric<T>(string a, string b)
            {
                return TestGenericSub<T>(a, b);
            }

            TestStruct TestGenericSub<T>(string a, string b)
            {
                TestStruct res = new TestStruct();
                res.address = a + b;
                return res;
            }
        } 
        public struct TestStruct
        {
            public int id;
            public string address;

            public override string ToString()
            {
                return id.ToString();
            }
        }

        public static void TestStudent()
        {
            Student<string> a = new TestCases.Student<string>("aaaaaa");
            Console.WriteLine(a.name);
        }
        public static void UnitTest_Out2()
        {
            Dictionary<string, TestClassC[]> data = new Dictionary<string, TestClassC[]>();
            data["key"] = new TestClassC[1] { new TestClassC() };
            TestClassC[] item;
            data.TryGetValue("key", out item);
            Console.WriteLine("value : " + item[0]);
        }

        class TestClassC
        {
        }
        public static void TestArrayValueType()
        {
            int[] arr = new int[4] { 1, 2, 3, 4 };
            for(int i = 0; i < arr.Length; i++)
            {
                string str = arr[i].ToString();
                Console.WriteLine("!!! new str = " + str);
            }
        }

        public static void TestGenericMethod()
        {
            var test = TestGenericMethodSub();
            var r = test.TestGeneric<ILRuntimeTest.TestFramework.TestClass3>("123", "456");
            Console.WriteLine("address " + r.address);
        }

        public static void TestGenericMethod2()
        {
            var res = GetRows<int, int>("123", "345");
            Console.WriteLine(string.Format("res[0], K={0} V={1}", res[0].K, res[0].V));
            var res2 = GetRows<int, double>("789", "345.678");
            Console.WriteLine(string.Format("res2[0], K={0} V={1}", res2[0].K, res2[0].V));

        }

        static List<NRow<A, B>> GetRows<A, B>(string a, string b)
        {
            List<NRow<A, B>> res = new List<TestCases.NRow<A, B>>();
            NRow<A, B> row;
            for (int i = 0; i < 1; i++)
            {
                row = new TestCases.NRow<A, B>();
                row.K = (A)Convert.ChangeType(a, typeof(A));
                row.V = (B)Convert.ChangeType(b, typeof(B));
                res.Add(row);
            }
            return res;
        }

        static MGenericTest TestGenericMethodSub()
        {
            return new MGenericTest();
        }

        public static void TestStructDictionary()
        {
            Dictionary<int, TestStruct> dicts = new Dictionary<int, TestStruct>();
            for(int i = 0; i < 2; i++)
            {
                var def = new TestStruct();
                def.id = i;
                def.address = "address " + i;

                dicts.Add(i, def);
            }

            List<TestStruct> lists = new List<TestStruct>();
            foreach(var i in dicts.Values)
            {
                lists.Add(i);
            }

            for(int i = 0; i < lists.Count; i++)
            {
                var item = lists[i];
                Console.WriteLine(string.Format("id:{0} address:{1}", item.id, item.address));
            }
        }

        public static void TestMethodDefaultStructValue()
        {
            PrintValue(100);
        }

        public static void PrintValue(int a, TestStruct b = default(TestStruct))
        {
            Console.WriteLine("a = " + a + ", b = " + b.ToString());
        }

        public static void TestForEach()
        {
            List<string> a = new List<string>() { "1", "2", "3" };
            foreach (var i in a)
            {
                ParseOne(i);
            }
        }


        public static void TestForEachTry()
        {
            List<string> a = new List<string>() { "1", "2", "3" };
            foreach(var i in a)
            {
                try
                {
                    ParseOne(i);   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        static string ParseOne(string line)
        {
            Console.WriteLine(line.ToString());            
            throw new NotSupportedException("error");            
        }

        static bool CheckValue(List<B> lst)
        {
            foreach (B a in lst)
            {
                if (a.aa >= 1)
                {
                    Console.WriteLine("CheckValue 111111");
                    return false;
                }
            }
            Console.WriteLine("CheckValue 2222222");
            return true;
        }

        public static void TestReturn()
        {
            CheckValue(new List<B>() { new B(1), new B(2) });
        }

        public static void TestThis()
        {
            A aa = new A(5);
            aa.Check(aa);
        }

        class t
        {
            public long a = 0;
        }

        class a
        {
            public long b = 1111;
        }
        public static void TestEqual()
        {
            Console.WriteLine("TestEqual");

            var t = new t();
            var a = new a();
            
            if (t.a == 0 || t.a == a.b)
            {
                Console.WriteLine("TestEqual true");//这行不执行
            }

            Console.WriteLine("TestEqual end ");
        }
        public static void TestStringFormat()
        {
            Console.WriteLine("TestStringFormat");
            var ts = string.Format("{0}{1}{2}{3}", 1, "gfgf", -3, 4);
            Console.WriteLine(ts);
            Console.WriteLine("TestStringFormat End");
        }
        public static void ParseDB<T>(System.Action<int, T> cbk) where T : new()
        {

        }
        public static string Test()
        {
            ParseDB<B>((c, d) =>
            {

            });
            ParseDB<TestExplicit>((c, d) =>
            {

            });
            return "xxxx1";
        }
        public static void TestArrayNull()
        {
            Console.WriteLine("TestArrayNull");
            byte[] b = null;
            if (b != null)
            {
                Console.WriteLine("Error");
            }
            Console.WriteLine("TestArrayNullEnd ");
        }

        public static void TestStringNull()
        {
            string str = null;
            if (!string.IsNullOrEmpty(str))
                Console.WriteLine("Error");
        }

        public static void TestTypeof()
        {
            Dictionary<Type, int> dic = new Dictionary<Type, int>();
            dic[typeof(TestCls2)] = 2;
            int a;
            if (dic.TryGetValue(typeof(TestCls), out a))
            {
                Console.WriteLine("Error!!!");
            }
        }

        public static void TestExplicit()
        {
            TestExplicit data = new TestCases.TestExplicit(true);
            bool boolVal = (bool)data;
            Console.WriteLine("bool=" + boolVal);

            data = new TestCases.TestExplicit(1.2);
            double doubleVal = (double)data;
            Console.WriteLine("double=" + doubleVal);
        }

        interface IInterface
        {
            void Check();
        }


        struct MM : IInterface
        {
            public int i;
            public string s;

            public void Check()
            {
                Console.WriteLine("checkkk:" + i);
            }

        }

        public static void TestGenericArray()
        {
            var d = new MM[]
            {
                new MM() {i = 1, s = "01"},
                new MM() {i = 2, s = "02"},
            };

            //d = new MM(); //对于MM没问题

            TestGenericArraySub(d);

        }
        private static void TestGenericArraySub<T>(T d)
        {
            object obj = d; //此处抛异常, 如果T不是MM[]，而是MM，就没有这个问题
        }

        public static void TestGenericStruct()
        {
            var m2 = new MM() { i = 1, s = "01" };
            Ttt(m2); //抛异常
            Ttt2(m2);//正确

        }

        static void Ttt<T>(T obj) where T : IInterface
        {
            obj.Check();
        }

        static void Ttt2(IInterface obj)
        {
            obj.Check();
        }

        public static void Run()
        {
            TestTryCatch();
            if (c5 == null)
            {
                c5 = new C5();
                Console.WriteLine(C6.A.ToString());
                Console.WriteLine(C6.B);
                C6.A = 1;
                Console.WriteLine(C5.A);
                C6.foo();
                Console.WriteLine(C6.B);
                TestTryCatch();            
            }
        }
        static void TestTryCatch()
        {
            try
            {
                try
                {
                    c5.bar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("{0}\n{1}\n{2}", ex.Message, ex.Data["StackTrace"], ex.StackTrace));
                }
                c5.bar();
                throw new NotImplementedException("new exception");
            }
            catch (NotSupportedException err)
            {
                Console.WriteLine("not here.");
                Console.WriteLine(err.ToString());

                return;
            }
            catch (NotImplementedException err)
            {
                Console.WriteLine("Got.");

                Console.WriteLine(err.ToString());
            }
            catch (Exception err)
            {
                Console.WriteLine("Got 2.");
                Console.WriteLine(err.ToString());
            }
            finally
            {
                Console.WriteLine("Finally");
            }
            Console.WriteLine("Finished");
        }

        static C5 c5 = null;
    }

    
    class C6 : C5
    {
        public static string B = "tt";

        public static void foo()
        {
            B = "aa";
        }
    }

    class C5
    {
        public static int A = 4;

        public void bar()
        {
            Console.WriteLine("bar = " + A);
        }
    }
}