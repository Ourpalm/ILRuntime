using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCases
{
     

    public class TestParam
    {
        private class A1
        {
            public int i = 0;
        }

        private class A2
        {
            public int i1 = 1;
            public int i2 = 2;
            public int i3 = 3;
            public int i4 = 4;
        }

        private class A3
        {
            public void DoSomething(A2 t2, bool b, A1 t1, List<int> list)
            {
                t1.i += 100;
                t2.i3 += 5;

            Console.WriteLine($"{t1.i}{t2.i3}");
            }
        }

        public static void Test()
        {
            A1 t1 = new A1();
            A2 t2 = new A2();
            A3 t3 = new A3();

            List<int> list = new List<int>();

            for (int i = 0; i < 100; i++)
            {
                t3.DoSomething(t2, (i & 1) == 1, t1, list);
            }

        }
    }
}
