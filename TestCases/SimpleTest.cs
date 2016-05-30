using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            for( int i = 0; i < 50; i++)
            {
                b += foo(b);
            }
            return b;
        }

        public static int foo(int init)
        {
            int b = init;
            for( int i = 0; i < 10000; i++)
            {
                b += i;
            }
            SimpleTest.b = b;
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

        public static void InstanceTest()
        {
            SimpleTest t = new SimpleTest();
            int a = t.c;
            SimpleTest t2 = new SimpleTest(12);
            Console.Write(t2.a.ToString());
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
