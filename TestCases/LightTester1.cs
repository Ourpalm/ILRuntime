using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    //原来C#LightTestor的部分
    public class ExpTest_10
    {
        struct tttttt
        {
            public bool aol;
        }
        //只要有一个静态函数包含UnitTest名称的，就作为单元测试
        public static object UnitTest_1001()
        {
            int[] aaa = new int[10];
            for (uint i = 0; i < 10; i++)
            {
                aaa[i] = 1;
            }
            object[] arr = new object[3];

            for (uint i = 0; i < 2; i++)
            {
                arr[i + 1] = 1;
            }
            //1001 _1001_expression
            return TestIsInt(arr);

        }

        static bool TestIsInt(object[] obj)
        {
            if (obj[1] is StructTest2)
                return false;
            if (obj[1] is int)
                return true;
            return false;
        }
        public static object UnitTest_1002()
        {
            //1002 _1002_expression

            int a = (int)(Math.Pow(1.4f, 5) * 5f);

            Vector3 v3 = new Vector3((252.0f / 255.0f), (207.0f / 255.0f), (20.0f / 255.0f));
            Console.WriteLine("v3=" + v3);
            tttttt b = new tttttt();
            UnitTest_1002_Sub(ref b.aol);
            Console.WriteLine("bol=" + b.aol.ToString());
            return (2 + 55) * 3434 + a;

        }

        static void UnitTest_1002_Sub(ref bool aol)
        {
            aol = true;
            Console.WriteLine("bol=" + aol.ToString());
        }

        class StructTest
        {
            public Vector3 Vector;
            public ILRuntimeTest.TestFramework.TestStruct Struct;
        }

        struct StructTest3
        {
            public float A;
            public StructTest B;
            public Vector3 C;
        }

        public static void UnitTest_Struct()
        {
            StructTest a = new TestCases.ExpTest_10.StructTest();

            Vector3 vec = a.Vector;
            vec.x += 10;
            vec.y += 11;

            Console.WriteLine(vec.x);
            Console.WriteLine(a.Vector.x);

            ILRuntimeTest.TestFramework.TestStruct b = a.Struct;
            a.Struct.value += 100;
            Console.WriteLine(b.value);
            Console.WriteLine(a.Struct.value);            
        }

        class StructTest2
        {
            public ILRuntimeTest.TestFramework.TestStruct Struct;
        }

        public static void UnitTest_Struct2()
        {
            ILRuntimeTest.TestFramework.TestClass3 obj = new ILRuntimeTest.TestFramework.TestClass3();
            obj.Struct.value = 111;
            obj.Struct.value += 111;
            Console.WriteLine(obj.Struct.value);

            StructTest2 obj2 = new TestCases.ExpTest_10.StructTest2();
            obj2.Struct.value = 111;
            obj2.Struct.value += 111;
            Console.WriteLine(obj2.Struct.value);


            ILRuntimeTest.TestFramework.TestStruct.instance.value = 222;
            ILRuntimeTest.TestFramework.TestStruct.instance.value += 111;
            Console.WriteLine(ILRuntimeTest.TestFramework.TestStruct.instance.value);
        }

        public static void UnitTest_10022()
        {
            Vector3 pos = Vector3.One;

            pos.x += 1;
            pos.y += 2;

            if (pos.x > 10)
                pos.x = 10;
            if (pos.x < -10)
                pos.x = -10;
            if (pos.y > 10)
                pos.y = 10;
            if (pos.y < -10)
                pos.y = -10;

            var pos2 = tttt(pos);
            Console.WriteLine("pos.x = " + pos.x);
            Console.WriteLine("pos2.x = " + pos2.x);

            if (pos.x == pos2.x)
                throw new Exception("Value Type Violation");
        }

        public static void UnitTest_10023()
        {
            StructTest3 a;
            a = Sub10023();
            Vector3 pos = a.C;
            pos.x += 123;

            Vector3 pos2 = a.B.Vector;
            pos2.x -= 120;

            Console.WriteLine("pos.x=" + pos.x);
            Console.WriteLine("a.C.x=" + a.C.x);
            if (pos.x == a.C.x)
                throw new Exception("Value Type Violation");
            Console.WriteLine("pos2.x=" + pos2.x);
            Console.WriteLine("a.B.Vector.x=" + a.B.Vector.x);
            if (pos2.x == a.B.Vector.x)
                throw new Exception("Value Type Violation");

        }

        static StructTest3 Sub10023()
        {
            StructTest3 a;
            a.A = 123;
            a.C = Vector3.One;
            a.B = new StructTest();
            a.B.Vector = Vector3.One * 123;
            return a;
        }

        static Vector3 tttt(Vector3 a)
        {
            a.x = 12345;
            return a;
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

        public static void UnitTest_1010()
        {
            uint a = 100;
            TestUInt(a);
            TestUInt((ulong)a);
        }

        static void TestUInt(uint a)
        {
            float speedUp = (1 - (float)a / 1000f);
            uint speedUp2 = 1000 / a;
            Console.WriteLine(speedUp);
            Console.WriteLine(speedUp2);
        }

        static void TestUInt(ulong a)
        {
            float speedUp = (1 - (float)a / 1000f);
            ulong speedUp2 = 1000 / a;
            Console.WriteLine(speedUp);
            Console.WriteLine(speedUp2);
        }

        public static void UnitTest_1011()
        {
            long value = 0;
            value = value + 0xffffffff;
            Console.WriteLine(value);
        }

        public static void UnitTest_1012()
        {
            object och = 'd';
            char ch1 = (char)och;
            Console.WriteLine("test1:" + (int)ch1 + " " + ch1);
        }
    }
}
