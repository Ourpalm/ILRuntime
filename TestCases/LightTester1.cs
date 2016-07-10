using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCases
{
    //原来C#LightTestor的部分
    public class ExpTest_10
    {
        //只要有一个静态函数包含UnitTest名称的，就作为单元测试
        public static object UnitTest_1001()
        {
            //1001 _1001_expression
            return 1 + 2 > 3 / 4;

        }
        public static object UnitTest_1002()
        {
            //1002 _1002_expression

            int a = (int)(Math.Pow(1.4f, 5) * 5f);

            Vector3 v3 = new Vector3((252.0f / 255.0f), (207.0f / 255.0f), (20.0f / 255.0f));
            Console.WriteLine("v3=" + v3);
            return (2 + 55) * 3434 + a;

        }
        public static object UnitTest_1003()
        {
            //1003 _1003_expression
            return 1 + 2 * 2 + 55 * 3434 + (2 - 3) - (3 - 3) + 5;

        }
        public static object UnitTest_1004()
        {
            //1004 _1004_expression
            string a = "abc";
            string b = "abd";

            return "str=" + a + b;

        }
        public static object UnitTest_1005()
        {
            float s = 0.2f;
            if (s <= 0f || s >= 2f)
            {
                Console.WriteLine("" + 1);
            }
            else
            {
                Console.WriteLine("" + 2);
            }
            return 1;
        }

        public static string UnitTest_1006()
        {
            byte b = 11;
            return b.ToString();
        }

        public static byte UnitTest_1007()
        {
            byte b = 12;
            byte c = 255;
            return (byte)(b + c);
        }
        public static sbyte UnitTest_1007_2()
        {
            sbyte b = 12;
            sbyte c = 127;
            return (sbyte)(b + c);
        }

        struct TestStruc
        {
            public int a;
            public float b;

            public TestStruc(int a, float b)
            {
                this.a = a;
                this.b = b;
            }
        }       

        public static int UnitTest_1008()
        {
            TestStruc a, b;
            a.a = 1;
            a.b = 2.3f;
            b = new TestStruc();
            b = new TestStruc(9, 10.2f);
            Console.WriteLine("Value of b:");
            Console.WriteLine(b.a);
            Console.WriteLine(b.b);
            b = a;
            b.a = 2;
            b.b = 3.2f;
            ValueTest(a);
            a.b = b.b;
            return a.a;
        }

        static void ValueTest(TestStruc a)
        {
            a.a = 3;
        }
        class ClsA
        {
            public int A { get; set; }
            public bool B { get; set; }

            public virtual void TestVirtual()
            {
                A = 1;
                B = false;
            }

            public override string ToString()
            {
                return string.Format("A={0},B={1}", A, B);
            }
        }

        class B : ClsA
        {
            public override void TestVirtual()
            {
                base.TestVirtual();
                B = true;
            }
        }

        class C : B
        {

        }

        public static bool UnitTest_1009()
        {
            B test = new B();
            test.TestVirtual();
            Console.WriteLine(string.Format("A={0},B={1}", test.A, test.B));
            ClsA test2 = (ClsA)test;
            test2.TestVirtual();
            Console.WriteLine(string.Format("A={0},B={1}", test2.A, test2.B));
            test2 = new C();
            test2.TestVirtual();
            Console.WriteLine(string.Format("A={0},B={1}", test2.A, test2.B));            
            ClsA test3 = new ClsA();
            test3.TestVirtual();
            Console.WriteLine(string.Format("A={0},B={1}", test3.A, test3.B));      
            test = test3 as B;
            if (test == null)
            {
                Console.WriteLine(string.Format("A={0},B={1}", test.A, test.B));            
            }
            return test3.B;
        }
    }
}
