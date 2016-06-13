using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ILRuntimeTest.Test;

namespace ILRuntimeTest.TestBase
{
    class StaticTestUnit : BaseTestUnit
    {
        //protected object _returnType;

        public override void Run()
        {
            Invoke(_typeName, _methodName);
        }

        public override bool Check()
        {
            return _pass;
        }

        public override TestResultInfo CheckResult()
        {
            return new TestResultInfo(_typeName + "." + _methodName, _pass, message.ToString());
        }
    }
}
