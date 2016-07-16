
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
            if (c5 == null)
            {
                c5 = new C5();
                Console.WriteLine(C6.A.ToString());
                Console.WriteLine(C6.B);
                C6.A = 1;
                Console.WriteLine(C5.A);
                C6.foo();
                Console.WriteLine(C6.B);                
            }
            try
            {


                throw new NotImplementedException("E2");
            }
            catch (NotSupportedException err)
            {
                Console.WriteLine("not here.");
            }
            catch (NotImplementedException err)
            {
                Console.WriteLine("Got.");
            }
            catch (Exception err)
            {
                Console.WriteLine("Got 2.");
            }
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
    }
}