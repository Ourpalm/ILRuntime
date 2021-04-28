using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class StaticTest
    {

        public class Test_A
        {
            public Test_A(Test_B b)
            {

            }
        }

        public class Test_B
        {

        }
        public sealed class NormalClass
        {
            //public static int result;
            public static void Create()
            {
                //result++;
                Console.WriteLine("2");
            }
            static NormalClass()
            {
                //result++;
                Console.WriteLine("1");
            }
        }
        

        static Test_A Create_With_Reference()
        {
            return new Test_A(new Test_B());
        }

        public static void UnitTest_StaticTest01()
        {
            Test_A a = Create_With_Reference();
            if (a.GetType() != typeof(Test_A))
                throw new Exception($"{a.GetType()} != Test_A");
        }

        public static void UnitTest_StaticTest02()
        {
            NormalClass.Create();
            //if (NormalClass.result != 2)
            //    throw new Exception("result != 2");
            //NormalClass.result = 0;
        }

        public static void UnitTest_StaticTest03()
        {
            dict[1] = new List<string>() { "helloworld", "hi" };
            UnitTest_StaticTest03Sub();
        }
        public static Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();
        static void UnitTest_StaticTest03Sub()
        {
            if (dict.TryGetValue(1, out var ls))
            {
                ls.Add("ggg");
                foreach (var item in ls)
                {
                    Console.WriteLine(item);
                }
            }
        }

        private static int testVal;
        private static Vector3 testVal2;
        private static object testVal3;

        private static void UnitTest_StaticTest04Sub(ref int i)
        {
            Console.WriteLine(i);
            i = 0;
        }

        public static void UnitTest_StaticTest04()
        {
            testVal = 111;
            UnitTest_StaticTest04Sub(ref testVal);
            if (testVal != 0)
                throw new Exception();
        }

        private static void UnitTest_StaticTest05Sub(ref Vector3 i)
        {
            Console.WriteLine(i);
            i = Vector3.Zero;
        }

        public static void UnitTest_StaticTest05()
        {
            testVal2 = Vector3.One;
            UnitTest_StaticTest05Sub(ref testVal2);
            if (testVal2.x != 0)
                throw new Exception();
        }

        private static void UnitTest_StaticTest06Sub(ref object i)
        {
            Console.WriteLine(i);
            i = 0;
        }

        public static void UnitTest_StaticTest06()
        {
            testVal3 = 111;
            UnitTest_StaticTest06Sub(ref testVal3);
            if ((int)testVal3 != 0)
                throw new Exception();
        }
    }
}
