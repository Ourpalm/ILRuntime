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

        public async static Task TestRun1()
        {
            Console.WriteLine("Start: TestRunSub1..");
            await TestRunSub1();
            Console.WriteLine("Result: TestRunSub1 complete..");
        }
        async static Task TestRunSub1()
        {
            AsyncAwaitTest1 test1 = null;
            test1.a = "xxxxxx";

            await Task.Run<int>(() =>
            {
                Console.WriteLine($"run 000");
                return 0;
            });
            Console.WriteLine($"TestRunSub1 complete");
        }

        public static async Task<int> TestRun2()
        {
            try
            {
                return await Task.Run(async () =>
                {
                    Console.WriteLine("Start");
                    await Task.Delay(0);
                    Console.WriteLine("End");
                    return 0;
                });
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.Data["StackTrace"]}\n{e.ToString()}");
                throw e;
            }
        }

        public static async Task<int> TestRun3()
        {
            try
            {
                return await Task.Run(async () =>
                {
                    Console.WriteLine("Start");
                    await Task.Delay(0);
                    Console.WriteLine("End");
                    return 0;
                });
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.Data["StackTrace"]}\n{e.ToString()}");
                throw e;
            }
        }

        class AsyncAwaitTest1
        {
            public string a;
        }
    }
}
