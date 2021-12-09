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
        bool Init(string fileName);
        bool Init(ILRuntime.Runtime.Enviorment.AppDomain app);
        bool Init(ILRuntime.Runtime.Enviorment.AppDomain app, string type, string method);
        void Run(bool skipPerformance = false);
        bool Check();
        TestResultInfo CheckResult();
    }
}
