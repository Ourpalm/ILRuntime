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
            TestCls cls = new TestCls();
            TestCls2 cls2 = new TestCls2();
            TestCls3 cls3 = new TestCls3();

            Console.WriteLine("Test invoking from sub type...");
            Console.WriteLine(cls.ToString());
            cls.TestAbstract();
            cls.TestVirtual();
            cls.TestField();

            Console.WriteLine(cls2.ToString());
            cls2.TestAbstract();
            cls2.TestVirtual();
            cls2.TestField();

            Console.WriteLine(cls3.ToString());
            cls3.TestAbstract();
            cls3.TestVirtual();
            cls3.TestField();

            Test01Sub(cls);
            Test01Sub(cls2);

            Console.WriteLine("TestCls.TestVal2 = " + cls.TestVal2);

            
            ClassInheritanceTest.Test3(cls);
            ClassInheritanceTest.Test3(cls2);
            ClassInheritanceTest.Test3(cls3);

        }

        static void Test01Sub(ClassInheritanceTest cls)
        {
            Console.WriteLine("Test invoking from base type...");
            cls.TestAbstract();
            cls.TestVirtual();
            cls.TestField();
        }

        public static void InheritanceTest02()
        {
            GenericTestCls<TestCls> cls = new GenericTestCls<TestCls>();
            GenericTestCls<TestCls2> cls2 = new GenericTestCls<TestCls2>();

            cls.DoTest();
            cls2.DoTest();
        }

        public static void InheritanceTest03()
        {
            List<TestCls> list = new List<TestCls>();
            TestCls a = new TestCls();
            a.TestVal2 = 243;
            list.Add(a);

            Console.WriteLine(list[0].TestVal2.ToString());
        }

        public static void InheritanceTest04()
        {
            List<GenericTestCls<TestCls>> list = new List<GenericTestCls<TestCls>>();
            list.Add(new GenericTestCls<TestCls>());
            list[0].DoTest();
        }

        public static void InheritanceTest05()
        {
            TestCls4 cls = new TestCases.TestCls4();
            cls.TestVirtual();
        }

        class GenericTestCls<T>
            where T : ClassInheritanceTest, new()
        {
            T instance = new T();

            public void DoTest()
            {
                instance.TestAbstract();
                instance.TestVirtual();
                instance.TestField();
            }
        }

        
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
            : base(4, 5)
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

    class TestCls3 : InterfaceTest
    {
        int testVal;

        public TestCls3()
        {
            testVal = 3;
        }
        public void TestAbstract()
        {
            Console.WriteLine("This is TestCls3.TestAbstract");
        }

        public void TestField()
        {
            Console.WriteLine("testValChild = " + testVal);
        }

        public void TestVirtual()
        {
            Console.WriteLine("This is TestCls3.TestVirtual");
        }
    }

    class TestCls4 : ClassInheritanceTest2<TestCls4>
    {
        public override void TestVirtual()
        {
            base.TestVirtual();
        }
    }
}
