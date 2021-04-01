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
    }
}
