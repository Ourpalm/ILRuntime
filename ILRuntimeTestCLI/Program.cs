using ILRuntimeTest.TestBase;
using System;
using System.Collections.Generic;

namespace ILRuntimeTestCLI
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("=== UNHANDLED EXCEPTION ===");
                Console.Error.WriteLine(ex.ToString());
                var inner = ex.InnerException;
                while (inner != null)
                {
                    Console.Error.WriteLine("--- Inner ---");
                    Console.Error.WriteLine(inner.ToString());
                    inner = inner.InnerException;
                }
                return -2;
            }
        }

        static int Run(string[] args)
        {
            if(args.Length <3)
            {
                Console.WriteLine("Usage: ILRuntimeTestCLI path patchPath useRegister[true|false] [nameFilter]");
                return -1;
            }

            string path = args[0];
            string patchPath = args[1];
            bool useRegister = args[2].ToLower() == "true";
            string nameFilter = args.Length >= 4 ? args[3] : null;
            TestSession session = new TestSession();
            session.Load(path, patchPath, useRegister);
            int ignoreCnt = 0;
            int todoCnt = 0;
            List<TestResultInfo> failedTests = new List<TestResultInfo>();
            int ranCnt = 0;
            foreach(var i in session.TestList)
            {
                if (!string.IsNullOrEmpty(nameFilter) && !i.TestName.Contains(nameFilter))
                    continue;
                ranCnt++;
                try
                {
                    i.Run(true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"=== Test {i.TestName} threw during Run ===");
                    Console.Error.WriteLine(ex.ToString());
                    var inner = ex.InnerException;
                    while (inner != null)
                    {
                        Console.Error.WriteLine("--- Inner ---");
                        Console.Error.WriteLine(inner.ToString());
                        inner = inner.InnerException;
                    }
                }
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
            Console.WriteLine($"Ran {ranCnt} tests, {failedTests.Count} failded, {ignoreCnt} ignored, {todoCnt} todos");
            session.Dispose();
            return failedTests.Count <= 0 ? 0 : -1;
        }
    }
}
