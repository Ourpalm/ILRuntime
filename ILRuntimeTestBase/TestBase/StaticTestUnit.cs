using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ILRuntimeTest.Test;

namespace ILRuntimeTest.TestBase
{
    public class StaticTestUnit : BaseTestUnit
    {
        //protected object _returnType;

        public override void Run(bool skipPerformance = false)
        {
            Invoke(TypeName, MethodName, skipPerformance);
        }

        public override bool Check()
        {
            return Pass == TestResults.Pass || Pass == TestResults.Ignored;
        }

        public override TestResultInfo CheckResult()
        {
            return new TestResultInfo(TypeName + "." + MethodName, Pass, Message.ToString(), IsToDo);
        }
    }
}
