using ILRuntimeTest.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntimeTest.TestBase
{
    public class TestResultInfo
    {


        public TestResultInfo(string testName, TestResults result, string message, bool isTodo)
        {
            TestName = testName;
            Result = result;
            Message = message;
            HasTodo = isTodo;
        }


        public string TestName { get; private set; }

        public TestResults Result { get; private set; }

        public string Message { get; private set; }

        public bool HasTodo { get; private set; }
    }
}
