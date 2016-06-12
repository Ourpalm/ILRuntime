using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntimeTest.TestBase
{
    class TestResultInfo
    {


        public TestResultInfo(string testName, bool result)
        {
            TestName = testName;
            Result = result;
        }


        public string TestName { get; private set; }

        public bool Result { get; private set; }
    }
}
