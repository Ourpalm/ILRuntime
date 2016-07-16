
//using CSEvilTestor;
using System;
using System.Collections.Generic;
using System.Text;
namespace TestCases
{
    class Test05
    {
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
                throw new NotImplementedException();
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