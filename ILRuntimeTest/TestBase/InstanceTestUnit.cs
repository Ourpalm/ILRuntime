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
            //Invoke(_typeName, _methodName);
            var obj = _app.Instantiate(_typeName);
            if (obj != null)
                Invoke(obj, _typeName, _methodName);
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
