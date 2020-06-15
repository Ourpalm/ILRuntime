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
        public async static Task TestRun4()
        {
            Console.WriteLine("Load 1");
            await TestLoad("Load 1 complete");
            Console.WriteLine("Load 2");
            await TestLoad("Load 2 complete");
            Console.WriteLine("Load 3");
            await TestLoad("Load 3 complete");

        }
        static async Task TestLoad(string msg)
        {
            await Task.Delay(1000);
            Console.WriteLine(msg);
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

        /*public static async void TestRun4()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 1; i <= 3; ++i)
            {
                string path = $"{i}.txt";
                Console.WriteLine(path);
                if (!System.IO.File.Exists(path))
                    System.IO.File.CreateText(path);
                Task task = Task.Run(async () =>
                {
                    return await readText(path);
                });
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        private static async Task<List<string>> readText(string path)
        {
            Console.WriteLine(path);
            List<string> lines = new List<string>();
            using (System.IO.StreamReader reader = System.IO.File.OpenText(path))
            {
                string line;
                while (null != (line = await reader.ReadLineAsync()))
                {
                    lines.Add(line);
                }
            }

            return lines;
        }*/

        class AsyncAwaitTest1
        {
            public string a;
        }
    }
}
