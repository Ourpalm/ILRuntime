using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntimeTest.TestBase;

namespace ILRuntimeTest
{
    public class ILRuntimeTestAttribute : Attribute
    {
        public bool Ignored { get; set; }
        public bool IsToDo { get; set; }

        public bool IsPerformanceTest { get; set; }
        public Type ExpectException { get; set; }
    }
    /// <summary>
    /// Test Interface
    /// </summary>
    public interface ITestable
    {
        string TestName { get; }
        void Run(bool skipPerformance = false);
        TestResultInfo CheckResult();
    }
}
