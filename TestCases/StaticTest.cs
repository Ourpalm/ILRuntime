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
    }
}
