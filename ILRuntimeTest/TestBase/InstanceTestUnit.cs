using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntimeTest.Test;

namespace ILRuntimeTest.TestBase
{
    class InstanceTestUnit : BaseTestUnit
    {
        public override void Run()
        {
            //Invoke(TypeName, MethodName);
            var obj = App.Instantiate(TypeName);
            if (obj != null)
                Invoke(obj, TypeName, MethodName);
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
