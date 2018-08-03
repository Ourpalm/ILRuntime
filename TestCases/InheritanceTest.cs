using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntimeTest.TestFramework;

namespace TestCases
{   public interface InterfaceTest2
    {
        void TestVirtual();
        void TestAbstract();
        void TestField();
    }

    public class InheritanceTest
    {
        public static void InheritanceTest01()
        {
            TestCls cls = new TestCls();
            TestCls2 cls2 = new TestCls2();

            Console.WriteLine("Test invoking from sub type...");
            Console.WriteLine(cls.ToString());
            cls.TestAbstract();
            cls.TestVirtual();
            cls.TestField();

            Console.WriteLine(cls2.ToString());
            cls2.TestAbstract();
            cls2.TestVirtual();
            cls2.TestField();

            Console.WriteLine("----------------------------------");

           
            Console.WriteLine("----------------------------------");

            Test01Sub(cls);
            Test01Sub(cls2);

            Console.WriteLine("TestCls.TestVal2 = " + cls.TestVal2);


            ClassInheritanceTest.Test3(cls);
            ClassInheritanceTest.Test3(cls2);
        }

        public static void InheritanceTest_Interface()
        {
            TestCls3 cls3 = new TestCls3();
            Console.WriteLine(cls3.ToString());
            cls3.TestAbstract();
            ((InterfaceTest2)cls3).TestVirtual();
            cls3.TestField();
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

        public static void InheritanceTest07()
        {
            TestClass2 cls = new TestCls5();
            Console.WriteLine(cls.AbMethod2(122));
        }

        public static void InheritanceTest08()
        {
            AABase obj = new AABase();
            obj.R = 1;
            obj.R2 = 2;

            var m = typeof(InheritanceTest).GetMethod("InheritanceTest08_Sub");

            m.Invoke(null, new object[] { obj });
        }

        static void InheritanceTest08_Sub(AABase obj)
        {
            var k = obj.R;
            var l = obj.R2;

            Console.WriteLine(string.Format("{0},{1}", k, l));
        }

        public static void InheritanceTest09()
        {
            ClassInheritanceTest a = new TestCls();
            ClassInheritanceTest b = new TestCls2();

            if(a is TestCls2)
            {
                throw new Exception("Error");
            }
        }

        public static void InheritanceTest10()
        {
            List<Parent> list = new List<Parent>();
            Child2 c1 = new Child2();
            SubParent c2 = new Child3();
            list.Add(new Child2());
            list.Add(new Child3());

            list.RemoveAll((c) => c is Child2);

            if (list.Count != 1)
                throw new Exception("Error");
            c1 = (Child2)c2;
            Console.WriteLine(c1.ToString());
            c1 = c2 as Child2;
            Console.WriteLine(c1 == null);
        }

        class TestCls5 : TestClass2
        {
            public override float AbMethod2(int arg1)
            {
                return arg1 + 1.2f;
            }

            protected override void AbMethod1()
            {
                
            }
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
            InheritanceTest it = new InheritanceTest();
            it.InheritanceTest06_Sub<MyClass>();
        }

        void InheritanceTest06_Sub<T>() where T:MyClass
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

    class TestCls3 : InterfaceTest2
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

        void InterfaceTest2.TestVirtual()
        {
            Console.WriteLine("TestVirtual = " + testVal);
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
        public int R;
        public int R2 { get; set; }
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

    public class SubParent: Parent
    {

    }

    public class Child2 : SubParent
    {

    }

    public class Child3 : SubParent
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

        public static void TestInterface()
        {
            var obj = new AA();
            ClassInheritanceTest.staticField = obj;
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
