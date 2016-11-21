using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ILRuntimeTest.Test;

namespace ILRuntimeTest.TestBase
{
    class StaticTestUnit : BaseTestUnit
    {
        //protected object _returnType;

        public override void Run()
        {
            Invoke(TypeName, MethodName);
        }

        public override bool Check()
        {
            return Pass;
        }

        public override TestResultInfo CheckResult()
        {
            return new TestResultInfo(TypeName + "." + MethodName, Pass, Message.ToString());
        }
    }
}
