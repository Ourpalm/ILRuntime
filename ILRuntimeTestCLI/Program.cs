using ILRuntimeTest.TestBase;
using System;
using System.Collections.Generic;

namespace ILRuntimeTestCLI
{
    class Program
    {
        static int Main(string[] args)
        {
            if(args.Length <2)
            {
                Console.WriteLine("Usage: ILRuntimeTestCLI path useRegister[true|false]");
                return -1;
            }

            string path = args[0];
            bool useRegister = args[1].ToLower() == "true";
            TestSession session = new TestSession();
            session.Load(path, useRegister);
            int failCnt = 0;
            foreach(var i in session.TestList)
            {
                
                i.Run();
                var res = i.CheckResult();
                if (!res.Result)
                    failCnt++;

                Console.WriteLine(res.Message);
            }

            Console.WriteLine($"Ran {session.TestList.Count} tests, {failCnt} failded");
            return failCnt <= 0 ? 0 : -1;
        }
    }
}
