//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    public interface ITestCase
    { 
        string Name { get; }
        bool RunTest(bool patched);
    }

    class DelegateTestCase : ITestCase
    {
        string name;
        Func<bool, bool> cb;
        public DelegateTestCase(string name, Func<bool, bool> cb)
        {
            this.name = name;
            this.cb = cb;
        }

        public string Name => name;

        public bool RunTest(bool patched)
        {
            return cb(patched);
        }
    }

    public class AllTestCases
    {
        public static IEnumerable<ITestCase> GetAllTestCases()
        {
            foreach (var i in HotfixBasicTestCases.GetTestCases())
                yield return i;
            foreach (var i in HotfixTestGenericTestCases.GetTestCases())
                yield return i;
            foreach (var i in HotfixTestInheritanceTestCases.GetTestCases())
                yield return i;
        }
    }
}
