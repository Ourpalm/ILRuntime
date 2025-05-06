using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntimeTest.Test;

namespace ILRuntimeTest.TestBase
{
    public class HotfixTestUnit : ITestable
    {
        HotfixAOT.ITestCase testCase;
        TestResults result;
        string message;
        bool isPatched;

        public string TestName => testCase.Name;

        public HotfixTestUnit(HotfixAOT.ITestCase testCase, bool isPatched)
        {
            this.testCase = testCase;
            this.isPatched = isPatched;
        }

        public void Run(bool skipPerformance = false)
        {
            try
            {
                Console.WriteLine($"Running {testCase.Name}(IsPatched={isPatched})");
                bool res = testCase.RunTest(isPatched);
                Console.WriteLine("Result:" + res);
                message = $"Running {testCase.Name}(IsPatched={isPatched})\r\nresult = {res}";
                result = res ? TestResults.Pass : TestResults.Failed;
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                result = TestResults.Failed;
            }
        }

        public TestResultInfo CheckResult()
        {
            return new TestResultInfo(testCase.Name, result, message, false);
        }
    }
}
