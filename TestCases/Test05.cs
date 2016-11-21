
//using CSEvilTestor;
using System;
using System.Collections.Generic;
using System.Text;
namespace TestCases
{
    class TestExplicit
    {
        bool boolVal;
        double doubleVal;

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
    class Test05
    {
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