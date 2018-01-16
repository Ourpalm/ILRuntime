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
        public async static Task TestRun()
        {
            Console.WriteLine("Start: TestRunSub1..");
            await TestRunSub();
            Console.WriteLine("Result: TestRunSub1 complete..");
        }


        async static Task TestRunSub()
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
