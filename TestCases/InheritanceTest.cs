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

        public static void InheritanceTest06()
        {
            InheritanceTest06_Sub<MyClass>();
        }

        static void InheritanceTest06_Sub<T>() where T:MyClass
        {
            T obj = Activator.CreateInstance(typeof(T)) as T; //这样写错误
                                                              //MyClass obj = Activator.CreateInstance(typeof(T)) as  MyClass; //这样写正确
            obj.Reg();
        }

        interface IMy
        {
        }


        class MyClass
        {
            public void Reg()
            {
                if (this is IMy)
                {
                    Console.WriteLine("is IMy"); //正确结果不应该打印这句；但实际上会打印这句，还会抛一个异常
                }
                else
                {
                    Console.WriteLine("not is IMy");
                }
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

    interface IAs1
    {
        void AA1();
    }

    class AABase : IDisposable
    {
        public void AA1()
        {
            Console.WriteLine("AABase");
        }

        public void Dispose()
        {
        }
    }

    class AA : AABase
    {
        public void AA1()
        {
            Console.WriteLine("AA1");
        }
    }

    class BB : ILRuntimeTest.TestFramework.ClassInheritanceTest, IAs1
    {
        public void AA1()
        {
            Console.WriteLine("AA1");
        }

        public override void TestAbstract()
        {
            Console.WriteLine("BB");
        }
    }

    public class Parent
    {

    }

    public interface IAwake
    {
        void Awake();
    }

    public class Child : Parent, IAwake
    {
        public void Awake()
        {
        }
    }

    class TestAs
    {
        public static bool TestAs01()
        {
            AA aa = new AA();
            if (aa is IAs1)
            {
                throw new Exception("error");
            }
            return true;
        }

        public static bool TestAs02()
        {
            AA aa = new AA();
            IAs1 ias = aa as IAs1;
            if (ias != null)
            {
                throw new Exception("error");
            }
            return true;
        }

        public static bool TestAs03()
        {
            BB aa = new BB();
            Dictionary<int, BB> dic = new Dictionary<int, BB>();
            dic[0] = aa;
            IAs1 ias = dic[0] as IAs1;
            if (ias == null)
            {
                throw new Exception("error");
            }
            else
                ias.AA1();
            ClassInheritanceTest id = dic[0] as ClassInheritanceTest;
            if (id == null)
            {
                throw new Exception("error2");
            }
            else
                id.TestAbstract();
            return true;
        }
        public static bool TestAs04()
        {
            object child = new Child();
            IAwake awake = child as IAwake;
            if (awake == null)
            {
                Console.WriteLine("as fail!");
                throw new Exception("error");
            }
            return true;
        }
    }



    class TestIs
    {
        public class Base
        {
            public virtual int Value()
            {
                return 0;
            }
        }

        public class Base2 : Base
        {

        }

        public class Base3 : Base2
        {

        }

        public static bool TestIs01()
        {
            AA aa = new AA();
            if (aa is IAs1)
            {
                throw new Exception("error");
            }
            return true;
        }

        public static bool TestIs02()
        {
            AA aa = new AA();
            if (aa is AABase == false)
            {
                throw new Exception("error");
            }
            return true;
        }

        public static bool TestIs03()
        {
            AA aa = new AA();
            if (aa is IDisposable == false)
            {
                throw new Exception("error");
            }
            return true;
        }

        public static bool TestIs04()
        {
            IDisposable aa = new AA();
            if (aa is AABase == false)
            {
                throw new Exception("error");
            }
            return true;
        }

        public static bool TestIs05()
        {
            Base aa = new Base3();
            if (aa is Base2 == false)
            {
                throw new Exception("error");
            }
            return true;
        }


    }

    public class TestVirtual
    {
        public class Base
        {
            public virtual int Value()
            {
                return 0;
            }
        }

        public class Base2 : Base
        {
            public override int Value()
            {
                return 2;
            }
        }

        public class Base3 : Base2
        {

        }

        public static bool TestVirtualMethod01()
        {
            Base aa = new Base3();

            if (aa.Value() == 2)
            {
                return true;
            }

            throw new Exception("error");

        }

        public static bool TestVirtualMethod02()
        {
            Base aa = new Base2();

            if (aa.Value() == 2)
            {
                return true;
            }

            throw new Exception("error");

        }

        public static bool TestVirtualMethod03()
        {
            Base2 aa = new Base2();

            if (aa.Value() == 2)
            {
                return true;
            }

            throw new Exception("error");

        }
    }

}
