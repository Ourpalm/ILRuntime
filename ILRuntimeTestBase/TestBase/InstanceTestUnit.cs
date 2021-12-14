using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntimeTest.Test;

namespace ILRuntimeTest.TestBase
{
    public class InstanceTestUnit : BaseTestUnit
    {
        public override void Run(bool skipPerformance = false)
        {
            //Invoke(TypeName, MethodName);
            var obj = App.Instantiate(TypeName);
            if (obj != null)
                Invoke(obj, TypeName, MethodName);
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
