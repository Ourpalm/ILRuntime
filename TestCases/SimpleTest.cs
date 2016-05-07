using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCases
{
    public class SimpleTest
    {
        int a;
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
            for( int i = 0; i < 10000; i++)
            {
                b += i;
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

            return b;
        }

        public int foo()
        {
            int b = a;
            for (int i = 0; i < 10000; i++)
            {
                b += i;
            }

            return b;
        }
    }
}
