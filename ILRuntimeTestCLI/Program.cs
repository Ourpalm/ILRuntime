using ILRuntimeTest.TestBase;
using System;
using System.Collections.Generic;

namespace ILRuntimeTestCLI
{
    class Program
    {
        static int Main(string[] args)
        {
            if(args.Length <3)
            {
                Console.WriteLine("Usage: ILRuntimeTestCLI path patchPath useRegister[true|false]");
                return -1;
            }

            string path = args[0];
            string patchPath = args[1];
            bool useRegister = args[2].ToLower() == "true";
            TestSession session = new TestSession();
            session.Load(path, patchPath, useRegister);
            int ignoreCnt = 0;
            int todoCnt = 0;
            List<TestResultInfo> failedTests = new List<TestResultInfo>();
            foreach(var i in session.TestList)
            {                
                i.Run(true);
                var res = i.CheckResult();
                if (res.Result == ILRuntimeTest.Test.TestResults.Failed)
                {
                    if (res.HasTodo)
                        todoCnt++;
                    else
                        failedTests.Add(res);
                }
                else if (res.Result == ILRuntimeTest.Test.TestResults.Ignored)
                    ignoreCnt++;

                Console.WriteLine(res.Message);
                Console.WriteLine("===============================");
            }
            Console.WriteLine("===============================");
            Console.WriteLine($"{failedTests.Count} tests failed");
            foreach(var i in failedTests)
            {
                Console.WriteLine($"Test name:{i.TestName}, Message:{i.Message}");
                Console.WriteLine("===============================");
            }
            Console.WriteLine($"Ran {session.TestList.Count} tests, {failedTests.Count} failded, {ignoreCnt} ignored, {todoCnt} todos");
            session.Dispose();
            return failedTests.Count <= 0 ? 0 : -1;
        }
    }
}
