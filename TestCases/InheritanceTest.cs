using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public class InheritanceTest
    {
        public static void InheritanceTest01()
        {
            TestCls cls = new TestCases.InheritanceTest.TestCls();
            TestCls2 cls2 = new TestCases.InheritanceTest.TestCls2();

            Console.WriteLine("Test invoking from sub type...");
            Console.WriteLine(cls.ToString());
            cls.TestAbstract();
            cls.TestVirtual();
            cls.TestField();

            Console.WriteLine(cls2.ToString());
            cls2.TestAbstract();
            cls2.TestVirtual();
            cls2.TestField();

            Test01Sub(cls);
            Test01Sub(cls2);

            Console.WriteLine("TestCls.TestVal2 = " + cls.TestVal2);

            ClassInheritanceTest.Test3(cls);
            ClassInheritanceTest.Test3(cls2);
        }

        static void Test01Sub(ClassInheritanceTest cls)
        {
            Console.WriteLine("Test invoking from base type...");
            cls.TestAbstract();
            cls.TestVirtual();
            cls.TestField();
        }

        class TestCls : ClassInheritanceTest
        {
            public TestCls()
            {
                testVal = 1;
            }
            public override void TestAbstract()
            {
                Console.WriteLine("This is TestCls.TestAbstract");
            }

            public override void TestVirtual()
            {
                base.TestVirtual();
                Console.WriteLine("This is TestCls.TestVirtual");
            }

            public override string ToString()
            {
                return "This is TestCls";
            }
        }

        class TestCls2 : ClassInheritanceTest
        {
            public TestCls2()
            {
                testVal = 2;
            }
            public override void TestAbstract()
            {
                Console.WriteLine("This is TestCls2.TestAbstract");
            }

            public override void TestVirtual()
            {
                Console.WriteLine("This is TestCls2.TestVirtual");
            }
        }
    }
}
