using System;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public class TestValueTypeBinding
    {
        public static void Test00()
        {
            var a = TestVector3.One;
            a.X += 100;

            Console.WriteLine(a.ToString());
        }

        public static void Test01()
        {
            TestVector3 vec = new TestVector3(100, 1, 0);
            TestVector3.One.X += vec.X;

            Console.WriteLine(TestVector3.One.ToString());
        }
        public static void UnitTest_10022()
        {
            TestVector3 pos = TestVector3.One;

            pos.X += 1;
            pos.Y += 2;

            if (pos.X > 10)
                pos.X = 10;
            if (pos.X < -10)
                pos.X = -10;
            if (pos.Y > 10)
                pos.Y = 10;
            if (pos.Y < -10)
                pos.Y = -10;

            var pos2 = tttt(pos);
            Console.WriteLine("pos.x = " + pos.X);
            Console.WriteLine("pos2.x = " + pos2.X);

            if (pos.X == pos2.X)
                throw new Exception("Value Type Violation");
        }

        static TestVector3 tttt(TestVector3 a)
        {
            a.X = 12345;
            return a;
        }

        /*public static void UnitTest_10023()
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
        }*/
    }
}