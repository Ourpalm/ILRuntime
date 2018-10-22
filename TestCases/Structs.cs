using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
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
    }
}
