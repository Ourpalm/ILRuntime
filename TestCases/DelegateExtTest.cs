using System;

namespace TestCases
{
    public class DelegateExtObj
    {
        public int Value;
        public void AddValue(int value)
        {
            this.Value += value;
        }
    }
    public static class DelegateExtObjMethod
    {

        public static void IntTest(this DelegateExtObj obj, int a)
        {
            obj.AddValue(1);
            Console.WriteLine(obj + " dele a=" + a);
        }

        public static void IntTest2(this DelegateExtObj obj, int a)
        {
            obj.AddValue(123);
            Console.WriteLine(obj + " dele2 a=" + obj.Value);
        }

        public static int IntTest3(this DelegateExtObj obj, int a)
        {
            Console.WriteLine(obj + " dele3 a=" + a);
            return a + 100;
        }
        public static string Void(this DelegateExtObj obj,string str)
        {
            Console.WriteLine(obj + " Void");
            return "Void"+str+"x";
        }
        public static string Extend(this DelegateExtObj obj,string str)
        {
            if (obj == null) 
                throw new Exception();
            return "Extend" + str;
        }
    }
    public class DelegateExtTest
    {


        static TestDelegate testDele;

        public static void DelegateExtTest01()
        {
            var obj = new DelegateExtObj();
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += obj.IntTest;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest += obj.IntTest2;
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest(123);

            Func<DelegateExtObj,string,string> func = DelegateExtObjMethod.Extend;
            var ret = func.Invoke(obj,"1");
            if(!ret.Equals("Extend1"))
               throw new Exception();
        }

        public static void DelegateExtTest02()
        {
            var obj = new DelegateExtObj();
            Action<int> a = null;
            a += obj.IntTest;
            a += obj.IntTest2;


            DelegateTestCls cls = new DelegateTestCls(1000);
            a += cls.IntTest;
            a += cls.IntTest2;
            a += (i) =>
            {
                Console.WriteLine("lambda a=" + i);
            };
            a.Invoke(124);
            Console.WriteLine("obj Value=" + obj.Value);

        }
        public static void DelegateExtTest03()
        {
            var obj = new DelegateExtObj();
            Func<string,string> a = null;

            a += obj.Void;
            a += obj.Extend;
            Console.WriteLine("a=" + a("xxzz"));
        }
        public static void DelegateExtTest04()
        {
            var obj = new DelegateExtObj();
            Func<string> a = null;
            var o1 = new DelegateTestCls(11);
            a += o1.GetString;

            Console.WriteLine("no extend method lambda a=" + a());
        }
        class DelegateTestCls : DelegateTestClsBase
        {
        
            public DelegateTestCls(int b)
            {
                this.b = b;
            }
            public string GetString()
            {
                return "x";
            }
           
        }

        class DelegateTestClsBase
        {
            protected int b;
            public virtual void IntTest(int a)
            {
                Console.WriteLine("dele3base a=" + (a + b));
            }
            public virtual void IntTest2(int a)
            {
                Console.WriteLine("dele4 a=" + (a + b));
            }
        }

        delegate int TestDelegate(int b);



    }
}
