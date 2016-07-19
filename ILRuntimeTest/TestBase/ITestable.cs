using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntimeTest.TestBase;

namespace ILRuntimeTest
{
    /// <summary>
    /// Test Interface
    /// </summary>
    interface ITestable
    {
        bool Init(string fileName);
        bool Init(ILRuntime.Runtime.Enviorment.AppDomain app);
        bool Init(ILRuntime.Runtime.Enviorment.AppDomain app, string type, string method);
        void Run();
        bool Check();
        TestResultInfo CheckResult();
    }
}
