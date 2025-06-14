using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCases
{
     

    public class TestLocalFunction
    {
        public void Test()
        {
            int i = 10;

            void ThisIsLocalFunction()
            {
                Console.WriteLine($"在这里设断点啊！！！{i--}");

                if (i > 0)
                    ThisIsLocalFunction();
            }

            ThisIsLocalFunction();
        }
    }
}
