using ILRuntimeTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TestCases
{
    public class RegisterVMTest
    {
        public static void RegisterVMTest01()
        {
            InlineTest test = new InlineTest();
            test.DoTest(true);
        }

        class InlineTest
        {
            int a;

            int A
            {
                get
                {
                    if (a > 10)
                        return 10;

                    if (a < 2)
                        return 2;

                    return a;
                }
            }

            public void DoTest(bool arg)
            {
                if (arg)
                {
                    a = 8;
                    var tmp = A;
                    if (tmp != 8)
                        throw new Exception();
                    Console.WriteLine(tmp);
                }
            }
        }
    }
}
