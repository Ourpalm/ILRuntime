using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestCases
{
    class AsyncAwaitTest
    {
        public static void TestRun()
        {
            TestRunSub();
        }

        async static void TestRunSub()
        {
            int cnt = 0;
            var res = await Task.Run<int>(() =>
            {
                for (int i = 0; i < 100000; i++)
                    cnt++;

                return cnt;
            });

            Console.WriteLine($"Result:{res}");
        }
    }
}
