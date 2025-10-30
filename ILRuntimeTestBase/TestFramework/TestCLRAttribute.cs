using System;
using System.Reflection;

namespace ILRuntimeTest.TestFramework
{
    [AttributeUsage(AttributeTargets.All)]
    public class TestCLRAttribute : Attribute
    {
        public string Name;
    }

    public class TestCLRAttribute2 : Attribute
    {
        public string Name { get; private set; }
        public string Description{ get; private set; }
        public string[] Parameters{ get; private set; }

        public TestCLRAttribute2(string name, string description, params string[] parameters)
        {
            Name = name;
            Description = description;
            Parameters = parameters;
        }

        public static bool TestIsDefineAttribute(MethodInfo info)
        {
            return info.IsDefined(typeof(TestCLRAttribute2), false);
        }

        public static TestCLRAttribute2 GetTestCLRAttribute2(MethodInfo info)
        {
            return (TestCLRAttribute2)info.GetCustomAttribute(typeof(TestCLRAttribute2), false);
        }
    }
}