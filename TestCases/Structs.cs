using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public struct Fixed64Vector3
    {
        public Fixed64 x;
        public Fixed64 y;
        public Fixed64 z;

        public static readonly Fixed64Vector3 Zero3;

        public Fixed64Vector3(int x, int y, int z)
        {
            this.x = new Fixed64(x);
            this.y = new Fixed64(y);
            this.z = new Fixed64(z);
        }

        static Fixed64Vector3()
        {
            Zero3 = new Fixed64Vector3(0, 0, 0);
        }
    }


    public struct Fixed64
    {
        private long m_rawValue;

        public static readonly Fixed64 Zero;

        public Fixed64(long value)
        {
            m_rawValue = value;
        }

        static Fixed64()
        {
            Zero = new Fixed64(0);
        }
    }
    public class Color32
    {
        public Color32(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public void GetA(out byte _byte)
        {
            _byte = a;
        }
        public void GetB(ref byte _byte)
        {
            _byte = b;
        }

        public override string ToString()
        {
            return r + "," + g + "," + b + "," + a;
        }
        public void TestType(Type t)
        {
            Console.WriteLine("type=" + t);
        }
    }
    public struct TT
    {
        public float abc;
    }
    public struct TestStruct
    {
        public int ID;
    }
    class TestStaticClass
    {
        public static TestStruct sss = new TestStruct();
    }
    public struct Vector3
    {

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
        }
        public static Vector3 operator *(Vector3 left, float right)
        {
            return new Vector3(left.x * right, left.y * right, left.z * right);
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
        }
        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return (left.x == right.x && left.y == right.y && left.z == right.z);
        }
        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left.x == right.x && left.y == right.y && left.z == right.z);
        }
        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }
        public float x;
        public float y;
        public float z;

        public float len
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }

        }

        public Vector3 Normalized()
        {
            float _len = len;
            return new Vector3(x / _len, y / _len, z / _len);
        }

        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            return new Vector3(left.y * right.z - left.z * right.y, left.z * right.x - left.x * right.z, left.x * right.y - left.y * right.x);
        }
        public static Vector3 Cross(Vector3 left, float v = 0)
        {
            return left;
        }
        public static float Dot(Vector3 left, Vector3 right)
        {
            return left.x * right.x + left.y * right.y + left.z * right.z;
        }
        static Vector3 one = new Vector3(1, 1, 1);
        public static Vector3 One
        {
            get
            {
                return one;
            }
        }
        public static Vector3 Zero
        {
            get
            {
                return new Vector3(0, 0, 0);
            }
        }
        public static string typetag;
        //测试支持性
        public static explicit operator int(Vector3 b) { return 0; } //这是一个显式转换
        public static implicit operator float(Vector3 a) { return a.x; } //这是一个隐式转换

        public Vector3 TestReturnThis(float x)
        {
            this.x += x;
            return this;
        }
    }

    public class StructTests
    {
        class MyClass
        {
            MyStruct stru = new MyStruct();
        }
        struct MyStruct
        {
            public int i;
            public EnumTest.TestEnum e;
        }

        public static void StructTest1()
        {
            var m = new MyStruct[10];
            m[1] = new MyStruct();   // `Throw exception here.`
        }

        public static void StructTest2()
        {
            MyClass c = new MyClass();
            Console.WriteLine(c.ToString());
        }

        public static void StructTest3()
        {
            var s = new MyStruct();
            s.i = 123;
            s.e = EnumTest.TestEnum.Enum2;
            var b = s;

            Console.WriteLine(s.e + " " + s.i);
            Console.WriteLine(b.e + " " + b.i);
        }

        public struct Number
        {
            public static readonly Number maxValue = new Number(Double.MaxValue);
            public static readonly Number minValue = new Number(Double.MinValue);
            public double val;
            public Number(double val)
            {
                this.val = val;
            }
        }

        public static void StructTest4()
        {
            Console.WriteLine(Number.minValue.val);
        }

        struct EnumTestStruct
        {
            public ILRuntimeTest.TestFramework.TestCLREnum value;
        }

        public static void StructTest5()
        {
            EnumTestStruct val = new EnumTestStruct();
            Console.WriteLine(val.value);
        }

        private struct StructTest
        {
            public Object objAsset;
            public string type;
        }
        public static void StructTest6()
        {
            Dictionary<string, StructTest> m_dictAsset = new Dictionary<string, StructTest>();
            StructTest cube = new StructTest();
            cube.type = "111";
            m_dictAsset["123"] = cube;
            cube.type = "123";
            string strId = "123";
            if (!m_dictAsset.TryGetValue(strId, out cube)) //注释：这句代码报错，提示错误InvalidCastException: Specified cast is not valid
            {
                throw new Exception();
            }
            if (cube.type != "111")
                throw new Exception();
        }

        class TestClass
        {
            public ILRuntimeTest.TestFramework.TestVector3 v21 = new ILRuntimeTest.TestFramework.TestVector3(111, 222, 333);
            public ILRuntimeTest.TestFramework.TestVector3 v22 = new ILRuntimeTest.TestFramework.TestVector3();

            public void Test()
            {
                v21.X = 123; //没问题
                v22.X = 222; //有问题， 报空 NullRef
            }
        }

        public static void StructTest7()
        {
            var cl = new TestClass();
            cl.Test();
        }

        public static void StructTest8()
        {
            Vector3 vec = new Vector3(1, 1, 1);
            Vector3 vec2 = vec.TestReturnThis(2);

            Console.WriteLine($"vec2.x ={vec2.x}");
            if (vec2.x != 3)
                throw new Exception();
        }

        class ClassA
        {
            public string str;
            public ClassA(string s)
            {
                str = s;
            }
            public static implicit operator string(ClassA a) => a.str;

        }

        struct StructA
        {
            public ClassA A;
        }
        public static void StructTest9()
        {
            StructTest9Sub(new ClassA("aaa"));
        }

        static void StructTest9Sub(ClassA a)
        {
            StructTest9Sub2(a);
            string str2 = a;
            if (str2 != "aaa")
                throw new Exception();
        }

        static StructA StructTest9Sub2(ClassA a)
        {
            StructA s = new StructA()
            {
                A = a,
            };
            return s;
        }

        public static void StructTest10()
        {
            ILRuntimeTest.TestFramework.TestVector3 vec = StructTest10Sub();
            if (vec.X != 1)
                throw new Exception();
        }

        static ILRuntimeTest.TestFramework.TestVector3 StructTest10Sub()
        {
            ILRuntimeTest.TestFramework.TestVector3 res = ILRuntimeTest.TestFramework.TestVector3.One2;
            switch(res.X)
            {
                case 2:
                    Console.WriteLine("113");
                    break;
                case 6:
                    Console.WriteLine("1136");
                    break;
                case 100:
                    Console.WriteLine("11399");
                    break;
                case 133:
                    Console.WriteLine("1131");
                    break;
            }
            object[] arr = new object[] { res };
            Console.WriteLine("vec=" + arr[0]);
            return res;
        }

        public static void StructTest11()
        {
            List<Anim> lst = new List<Anim>();
            for(int i = 0; i < 50; i++)
            {
                lst.Add(new Anim(i.ToString(), i + 10f));
            }
        }

        struct Anim
        {
            public string name;
            public float duration;

            public Anim(string name, float duration)
            {
                this.name = name;
                this.duration = duration;
            }
        }
    }
}
