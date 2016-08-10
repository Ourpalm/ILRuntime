using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
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
            c = 2;
        }
        public SimpleTest(int a)
        {
            this.a = a;
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
    }
}
