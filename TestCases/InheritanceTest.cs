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
        interface ITest
        {
            void TestMethod();
        }

        public class TestA : ITest
        {
            void ITest.TestMethod()//显式实现接口方法
            {
                Console.WriteLine("方法A");
            }
        }

        public class TestB : ITest
        {
            void ITest.TestMethod()//显式实现接口方法
            {
                Console.WriteLine("方法A");
            }

            public void TestMethod()
            {
                Console.WriteLine("方法B");
            }
        }
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

        public static void InheritanceTest_Interface2()
        {
            ITest b = new TestB();
            b.TestMethod();//此处输入结果为 方法B
            ITest a = new TestA();
            a.TestMethod();//发生异常
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
            Console.WriteLine(cls);
            cls.VMethod1();
            int val = 0;
            cls.VMethod3(ref val);
            if (val != 11)
                throw new Exception($"{val} != 11");
            float val2 = cls.AbMethod2(val);
            if (val2 != 12.2f)
                throw new Exception($"{val2} != 12.1f");

            cls = new TestCls6();
            val = 0;
            cls.VMethod3(ref val);
            if (val != 21)
                throw new Exception($"{val} != 21");
            val2 = cls.AbMethod2(val);
            if (val2 != 24.2f)
                throw new Exception($"{val2} != 23.2f");
        }

        public static void InheritanceTest08()
        {
            using (AABase obj = new AABase())
            {
                obj.R = 1;
                obj.R2 = 2;

                var m = typeof(InheritanceTest).GetMethod("InheritanceTest08_Sub");

                m.Invoke(null, new object[] { obj });

            }
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

        public static void InheritanceTest11()
        {
            SubParent sub = new SubParent();
            sub.Test();
        }

        static void InheritanceTest12Sub()
        {
            Console.WriteLine("OK");
        }

        public static void InheritanceTest12()
        {
            for(int i = 0; i < 5; i++)
            {
                System.Type t = typeof(InheritanceTest);
                t.GetMethod("InheritanceTest12Sub", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
            }
        }

        public static void InheritanceTest13()
        {
            new TestClass().TestMethod();
        }

        public static void InheritanceTest14()
        {
            Data2 data = new Data2();

            GenericInheritanceTestCls<Data2> instance = new GenericInheritanceTestCls<Data2>(data);
            instance.TestVirtual();
        }

        public static void InheritanceTest15()
        {
            var cls = new TestCls7();
            int val = 0;
            cls.VMethod3(ref val);

            if (val != 1)
                throw new Exception();
        }

        public static void InheritanceTest16()
        {
            InheritanceTest16SubCls2 cls = new InheritanceTest16SubCls2();
            cls.KKK();
        }
        public static void InheritanceTest17()
        {
            ClassB b = new ClassB();
            CrossClass crossClass = new CrossClass();
            if (!b.OutMethod(out crossClass.classA)) return;
        }

        public static void InheritanceTest18()
        {
            TestCls5 obj = new TestCls5();
            TestClass2.Register(obj);

            TestCls5 res;
            Alloc<TestCls5>(out res);
            res.bbbb = 5000;
            if (res.bbbb != 5000)
                throw new Exception();
        }

        static void Alloc<T>(out T value)where T : TestClass2
        {
            value = TestClass2.Alloc() as T;
        }

        class CrossClass : TestClass3
        {
            public ClassA classA;
        }

        class ClassA
        {

        }
        class ClassC
        {
            public void Init()
            {
                ClassB b = new ClassB();
                CrossClass crossClass = new CrossClass();
                if (!b.OutMethod(out crossClass.classA)) return;
            }
        }
        class ClassB
        {
            public bool OutMethod(out ClassA a)
            {
                a = new ClassA();
                if (a == null)
                {
                    throw new Exception("classA is null");
                }
                return true;
            }
        }

        class InheritanceTest16SubCls : TestClass2
        {
            public int constVal = 11111;
            protected override void AbMethod1()
            {
                
            }

            public override float AbMethod2(int arg1)
            {
                return arg1 * 12333f;
            }

            public int Test(int arg)
            {
                return arg + constVal;
            }
        }

        class InheritanceTest16SubCls2 : TestClass4
        {
            public override void KKK()
            {
                cls2 = new InheritanceTest16SubCls();

                float res = cls2.AbMethod2(1);
                if (Math.Abs(12333 - res) > 0.00001f)
                    throw new Exception();

                int res2 = ((InheritanceTest16SubCls)cls2).Test(5);
                if (res2 != 11116)
                    throw new Exception();
            }
        }

        public interface IData { }

        public class Data : IData { }

        public abstract class Test<T> where T : class, IData
        {
            public T Data { get; protected set; }

            public void TestMethod()
            {
                Data = null;//此处赋值为null会出现异常, 非null不会发生异常
                Console.WriteLine(Data);
            }
        }

        public class TestClass : Test<Data>
        {
        }

        class TestCls5 : TestClass2
        {
            public int bbbb;
            public override void VMethod3(ref int arg)
            {
                base.VMethod3(ref arg);
                arg += 10;
            }
            public override bool VMethod2()
            {
                return base.VMethod2();
            }
            public override float AbMethod2(int arg1)
            {
                return arg1 + 1.2f;
            }

            protected override void AbMethod1()
            {
                
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }

        class TestCls6 : TestClass2
        {
            public override void VMethod3(ref int arg)
            {
                base.VMethod3(ref arg);
                arg += 20;
            }
            public override bool VMethod2()
            {
                return base.VMethod2();
            }
            public override float AbMethod2(int arg1)
            {
                return arg1 + 3.2f;
            }

            protected override void AbMethod1()
            {

            }
        }

        class TestCls7 : TestClass2
        {
            public override float AbMethod2(int arg1)
            {
                return arg1 + 1.2f;
            }

            protected override void AbMethod1()
            {

            }
        }
        class BaseData
        {
            public string m_Message;
        }

        class Data2: BaseData
        {
            public Data2()
            {
                m_Message = "hello";
            }
        }
        class GenericInheritanceTestCls<T> : ClassInheritanceTest where T : BaseData
        {
            protected T m_Data;     //泛型字段

            public GenericInheritanceTestCls(T data)
            {
                m_Data = data;
            }

            public override void TestVirtual()
            {
                m_Data = null;          //这行可能把stack写坏了
                TestAbstract();  //运行到这行会报错，解决方法有2个：1、将"m_Data = null;"这行注释掉。2、将"protected T m_Data;" 改为 "protected BaseData m_Data;"，去掉泛型字段

            }
            public override void TestAbstract()
            {
                Console.WriteLine("OK");
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
        public new void AA1()
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
        public virtual void Test()
        {
            Console.WriteLine("Parent.test");
        }
    }

    public class SubParent: Parent
    {
        public override void Test()
        {
            base.Test();
            Console.WriteLine("SubParent.test");
        }
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
            ClassInheritanceTest.staticField.Dispose();
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
