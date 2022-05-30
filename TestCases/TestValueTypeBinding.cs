using System;
using System.Collections.Generic;
using ILRuntimeTest;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public class TestValueTypeBinding
    {
        class TestValueTypeCls
        {
            public TestVectorStruct A;
            public Vector3 B;
        }
        public static void Test00()
        {
            var a = TestVector3.One;
            a.X += 100;

            Console.WriteLine(a.ToString());
        }

        [ILRuntimeTest(IsToDo = true)]
        public static void Test01()
        {
            TestVector3 vec = new TestVector3(100, 1, 0);
            TestVector3.One.X += vec.X;

            Console.WriteLine(TestVector3.One.ToString());
            if (TestVector3.One.X == 1)
                throw new Exception();
        }

        public static void Test02()
        {
            TestVector3 vec = new TestVector3(100, 1, 0);
            vec += TestVector3.One;

            Console.WriteLine(vec.ToString());
        }

        public static object Test03()
        {
            TestVector3[] a = new TestVector3[10000];
            Vector3[] b = new Vector3[100];
            Console.WriteLine(b[0]);
            for (int i = 0; i < 10000; i++)
            {
                a[i] = TestVector3.One;
            }

            return a;
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

        public static void UnitTest_10023()
        {
            TestVectorStruct a;
            a = Sub10023();
            TestVector3 pos = a.C;
            pos.X += 123;

            TestVector3 pos2 = a.B.Vector;
            pos2.X -= 120;

            Console.WriteLine("pos.x=" + pos.X);
            Console.WriteLine("a.C.x=" + a.C.X);
            if (pos.X == a.C.X)
                throw new Exception("Value Type Violation");
            Console.WriteLine("pos2.x=" + pos2.X);
            Console.WriteLine("a.B.Vector.x=" + a.B.Vector.X);
            if (pos2.X == a.B.Vector.X)
                throw new Exception("Value Type Violation");

        }

        static TestVectorStruct Sub10023()
        {
            TestVectorStruct a;
            a.A = 123;
            a.C = TestVector3.One;
            a.B = new TestVectorStruct2();
            a.B.Vector = TestVector3.One * 123;
            return a;
        }

        public static void UnitTest_10024()
        {
            TestVectorStruct a;
            a = Sub10023();

            Sub10024(a);
        }

        static void Sub10024(object obj)
        {
            TestVectorStruct a = (TestVectorStruct)obj;
            Console.WriteLine("a.B.Vector.x=" + a.B.Vector.X);
        }

        public static void UnitTest_10025()
        {
            TestValueTypeCls cls = new TestValueTypeCls();
            cls.A = Sub10023();
            cls.B = Vector3.One;

            Console.WriteLine("a.B.Vector.x=" + cls.A.B.Vector.X);
            Console.WriteLine("cls.B.x=" + cls.B.x);
        }

        public static void UnitTest_10026()
        {
            Console.WriteLine(DateTime.UtcNow.ToString());
        }

        public static void UnitTest_10027()
        {
            TestVectorClass cls = new TestVectorClass();
            cls.vector = new TestVector3(123, 123, 123);

            Console.WriteLine("x:" + cls.vector.X + " y:" + cls.vector.Y + " z:" + cls.vector.Z);
        }

        public static void UnitTest_10028()
        {
            TestVector3 a = TestVector3.One;
            float b = 1f;

            TestVector3 c = new TestVector3();
            a.Test(out c, out b);
        }

        public static void UnitTest_10029()
        {
            TestVector3 c = new TestVector3();
            UnitTest_10029Sub2(out c);
            Console.WriteLine(c.ToString());
            c = new TestVector3();
            UnitTest_10029Sub(out c);
            Console.WriteLine(c.ToString());
        }

        static void UnitTest_10029Sub(out TestVector3 v3)
        {
            v3 = TestVector3.One;//
        }

        static void UnitTest_10029Sub2(out TestVector3 v3)
        {
            v3 = TestVector3.One2;//
        }

        static TestVectorClass vecCls = new TestVectorClass();

        public static void UnitTest_10030()
        {
            vecCls.Vector2 += TestVector3.One2 * (0.456f - vecCls.Vector2.Y * 2);//��������

            Console.WriteLine(vecCls.Vector2);
        }

        public static void UnitTest_10031()
        {
            TestVector3 pos = TestVector3.One2;
            float offsetX = pos.X - 0.1f;
            float offsetY = pos.Y - 0.1f;//��������������
            if (offsetX > 1)
                Console.WriteLine("1");
            else if (offsetX < -1)
                Console.WriteLine("2");
            //ע������Ĵ���Ͳ��������
            else if (offsetY > 1)
                Console.WriteLine("3");
            else if (offsetY < -1)
                Console.WriteLine("4");

        }

        public static void UnitTest_10032()
        {
            TestVectorClass VTest = new TestVectorClass();

            TestVector3 value = VTest.vector;
            value.Normalize();
            VTest.vector.Normalize();
            Console.WriteLine("Vector3BindingTest local Vector3 normalized = " + value);
            Console.WriteLine("Vector3BindingTest Vector3 normalized = " + VTest.vector);

        }

        public static void UnitTest_10033()
        {
            TestVector3[] arr2 = new TestVector3[10];
            arr2[0].X = 1243;

            Vector3[] arr3 = new Vector3[10];
            arr3[0].x = 3143;

            Console.WriteLine(arr2[0].X);
            Console.WriteLine(arr3[0].x);
        }

        public static void UnitTest_10034()
        {
            Dictionary<UInt32, TestValueTypeBinding> testDic = new Dictionary<uint, TestValueTypeBinding>();
            testDic.Add(1, new TestValueTypeBinding());
            testDic.Add(2, new TestValueTypeBinding());
            foreach (var item in testDic)
            {
                Console.WriteLine(string.Format("dic item {0}", item.Key));
            }
        }

        public static void UnitTest_10035()
        {
            TestVector3[] FuncBtnsPos1 = new TestVector3[3] { TestVector3.One, TestVector3.One, TestVector3.One };
            foreach (var i in FuncBtnsPos1)
            {
                Console.WriteLine(i.ToString());
            }
        }

        static TestVector3NoBinding m_curPos;

        public static void UnitTest_10036()
        {
            TestVector3NoBinding rawPos = new TestVector3NoBinding(1, 2, 3);
            TestVector3NoBinding tmpPos = rawPos;
            m_curPos = rawPos;
            Console.WriteLine("before raw:" + rawPos + " tmp:" + tmpPos + " cur:" + m_curPos);
            ChangePosY(tmpPos);
            ChangePosY(m_curPos);
            Console.WriteLine("after raw:" + rawPos + " tmp:" + tmpPos + " cur:" + m_curPos);
            if (tmpPos.y == 0 || m_curPos.y == 0)
                throw new AccessViolationException();
        }

        static TestVector3 m_curPos2;
        public static void UnitTest_10037()
        {
            TestVector3 rawPos = new TestVector3(1, 2, 3);
            TestVector3 tmpPos = rawPos;
            m_curPos2 = rawPos;
            Console.WriteLine("before raw:" + rawPos + " tmp:" + tmpPos + " cur:" + m_curPos2);
            ChangePosY2(tmpPos);
            ChangePosY2(m_curPos2);
            Console.WriteLine("after raw:" + rawPos + " tmp:" + tmpPos + " cur:" + m_curPos2);
            if (tmpPos.Y == 0 || m_curPos2.Y == 0)
                throw new AccessViolationException();
        }

        static void ChangePosY(TestVector3NoBinding pos)
        {
            pos.y = 0;
        }

        static void ChangePosY2(TestVector3 pos)
        {
            pos.Y = 0;
        }

        public static void UnitTest_10038()
        {
            TestVector3NoBinding rawPos = TestVector3NoBinding.zero;
            rawPos.y = 1122333;

            Console.WriteLine(rawPos.y);
            if (rawPos.y != 1122333)
                throw new AccessViolationException();
        }

        public static void UnitTest_10039()
        {
            Test10039 obj = new Test10039();
            obj.Test();
        }

        class Test10039
        {
            TestVector3 m;
            TestVector3NoBinding m2;
            public void Test()
            {
                m = new TestVector3();
                m2 = TestVector3NoBinding.zero;
                UnitTest_10039Sub(m);
                UnitTest_10039Sub2(m2);
            }
            void UnitTest_10039Sub(TestVector3 arg)
            {
                arg = TestVector3.One2;

                if (arg.X != 1)
                    throw new Exception();
            }

            void UnitTest_10039Sub2(TestVector3NoBinding arg)
            {
                arg = TestVector3NoBinding.one;

                if (arg.x != 1)
                    throw new Exception();
            }
        }

        public static void UnitTest_10040()
        {
            TestVector3 dot = new TestVector3();
            dot.X = 10;
            dot.Y = 10;

            for (int i = 0; i < 50; i++)
            {
                dot.X++;
            }

            Console.WriteLine("dot.X == " + dot.X);
            if (dot.X != 60)
                throw new AccessViolationException();

        }

        public static void UnitTest_10041()
        {
            TestVector3NoBinding dot = new TestVector3NoBinding();
            dot.x = 10;
            dot.x = 10;

            for (int i = 0; i < 50; i++)
            {
                dot.x++;
            }

            Console.WriteLine("dot.X == " + dot.x);
            if (dot.x != 60)
                throw new AccessViolationException();

        }

        static TestVector3 GetVector3()
        {
            return new TestVector3(123, 123, 123);
        }

        static Func<TestVector3> GetVector3Func = GetVector3;
        public static void UnitTest_10042()
        {
            TestVector3 pos = GetVector3Func?.Invoke() ?? TestVector3.One2;
            if (pos.X != 123)
                throw new Exception();
        }

        public static void UnitTest_10043()
        {
            SubTestClass test = new SubTestClass();
        }

        public class TestBase
        {
            public int a;
            public TestVector3 vec = new TestVector3();//������

            public TestBase()
            {
            }
        }

        public class TestClass : TestBase
        {
            private int b;

            public TestClass()
            {
            }
        }

        public class SubTestClass : TestClass
        {
            int c;
            public SubTestClass()
            {
            }
        }

        class A<T>
        {
            T value = default;
            public A(T value)
            {
                this.value = value;
            }
        }

        class IntA : A<int>
        {
            public IntA(int value)
                : base(value)
            {

            }
        }

        public static void UnitTest_10044()
        {

            IntA a = new IntA(100);
        }

        public static void UnitTest_10045()
        {
            JInt i = new JInt();
            if (i++ != 0)
                throw new Exception();
            if (++i != 2)
                throw new Exception();
            if (i-- != 2)
                throw new Exception();
            i -= 10;
            if (i != -9)
                throw new Exception();
            --i;
            Console.WriteLine(i);

            for (JInt x = 20; x > 0; x--)
            {
                Console.WriteLine($"for��: x = {x}");
            }
        }

        public static void UnitTest_10046()
        {
            TestVector3.TestDelegate2 = UnitTest_10046Sub;
            TestVector3.DoTest2();
        }

        static void UnitTest_10046Sub(TestVector3 a)
        {
            a = a + TestVector3.One2;
            Console.WriteLine(a);
            if (a.X != 2)
                throw new Exception();
        }

        public static void UnitTest_10047()
        {
            TestVector3[] arr2 = new TestVector3[10];
            arr2[0].X = 1243;
            arr2[0] += TestVector3.One;

            if(Math.Abs(arr2[0].X - 1244)>0.001f)
                throw new Exception();
        }

        public static void UnitTest_10048()
        {
            TestVectorClass cls = new TestVectorClass();
            TestVectorClass cls2 = new TestVectorClass();
            for (int i = 0; i < 3; i++)
            {
                cls.Vector2 = cls2.Vector2 = new TestVector3(3, 4, i);
                Console.WriteLine(cls.Vector2);
            }

            if (cls.Vector2.Z != 2)
                throw new Exception($"cls.Vector2.Z == {cls.Vector2.Z}");
        }
        static TestStructB mAttr;
        public static void UnitTest_10049()
        {
            MinCirclePath path = new MinCirclePath();
            path.Test();
        }

        public class MinCirclePath
        {
            private TestStructB mAttr;

            public void Test()
            {
                mAttr = GetAttr();

                Console.WriteLine(mAttr);
            }

            private TestStructB GetAttr()
            {
                var a = new TestStructA() { value = 5 };

                //����д�����
                //var ret = TestStructB.GetOne(a);
                //return ret;

                //����ֱ�ӷ��ؾͻ��
                return TestStructB.GetOne(a);
            }
        }

        public static void UnitTest_10050()
        {
            // �ȸ�����ʹ��
            List<TestVector3NoBinding> structs = new List<TestVector3NoBinding>();
            for (int i = 0; i < 5; i++)
            {
                TestVector3NoBinding t = new TestVector3NoBinding();
                if (i % 2 != 0)
                    t.x = i;
                structs.Add(t);
            }
            for (int i = 0; i < 5; i++)
            {
                var item = structs[i];
                Console.WriteLine(item.x);
                if (i % 2 != 0)
                {
                    if (item.x != i)
                        throw new Exception();
                }
                else
                {
                    if (item.x != 0)
                        throw new Exception();
                }
            }
        }
    }
}