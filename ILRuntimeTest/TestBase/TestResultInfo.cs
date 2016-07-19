using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntimeTest.TestBase
{
    class TestResultInfo
    {


        public TestResultInfo(string testName, bool result, string message)
        {
            TestName = testName;
            Result = result;
            Message = message;
        }


        public string TestName { get; private set; }

        public bool Result { get; private set; }

        public string Message { get; private set; }
    }
}
