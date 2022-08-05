using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.Other;

namespace ILRuntimeTest.TestFramework
{
    public struct TestVector3NoBinding
    {
        public float x, y, z;

        static TestVector3NoBinding mZero = new TestVector3NoBinding(0, 0, 0);
        static TestVector3NoBinding mOne = new TestVector3NoBinding(1, 1, 1);

        public static TestVector3NoBinding zero => mZero;
        public static TestVector3NoBinding one => mOne;

        public TestVector3NoBinding(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public static TestVector3NoBinding operator +(TestVector3NoBinding a, TestVector3NoBinding b)
        {
            TestVector3NoBinding res = new TestVector3NoBinding();
            res.x = a.x + b.x;
            res.y = a.y + b.y;
            res.z = a.z + b.z;

            return res;
        }

        public static TestVector3NoBinding operator *(TestVector3NoBinding a, float b)
        {
            TestVector3NoBinding res = new TestVector3NoBinding();
            res.x = a.x * b;
            res.y = a.y * b;
            res.z = a.z * b;

            return res;
        }
        public override string ToString()
        {
            return $"({x},{y},{z})";
        }
    }

    public delegate void TestValueTypeDelegate(TestVector3 vec);
    public struct TestVector3
    {
        public float X, Y, Z;

        public static TestVector3 One = new TestVector3(1, 1, 1);

        public static Func<TestVector3, float> TestDelegate;

        public static TestValueTypeDelegate TestDelegate2;

        public static void DoTest2()
        {
            TestDelegate2(TestVector3.One);
        }

        public TestVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static TestVector3 One2 { get { return new TestVector3(1, 1, 1); } }

        public void Normalize()
        {
            float d = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            X = X / d;
            Y = Y / d;
            Z = Z / d;
        }

        public static TestVector3 operator +(TestVector3 a, TestVector3 b)
        {
            TestVector3 res = new TestVector3();
            res.X = a.X + b.X;
            res.Y = a.Y + b.Y;
            res.Z = a.Z + b.Z;

            return res;
        }

        public static TestVector3 operator *(TestVector3 a, float b)
        {
            TestVector3 res = new TestVector3();
            res.X = a.X * b;
            res.Y = a.Y * b;
            res.Z = a.Z * b;

            return res;
        }

        public void Test(out TestVector3 a, out float b)
        {
            b = 123f;
            a = this * 123f;
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", X, Y, Z);
        }
    }

    public class TestVectorClass
    {
        public TestVector3 vector = new TestVector3(20f, 10f, 1f);
        public TestVector3 Vector2 { get; set; } = TestVector3.One * 0.33f;
        public TestVectorClass Obj { get; set; }

        public static void ValueTypePerfTest(int a, string b, TestVector3 c, TestVectorClass d)
        {

        }

        public static void ValueTypePerfTest2(int a, string b, TestVectorClass c, TestVectorClass d)
        {

        }
    }

    public struct TestVectorStruct
    {
        public int A;
        public TestVectorStruct2 B;
        public TestVector3 C;
    }

    public struct TestVectorStruct2
    {
        public TestVectorStruct3 A;
        public TestVector3 Vector;
    }

    public struct TestVectorStruct3
    {
        public float A;
        public float B;
    }

    public struct JInt
    {
        private int _obscuredInt;
        private int _obscuredKey;
        private int _originalValue;

        private int Value
        {
            get
            {
                var result = _obscuredInt - _obscuredKey;
                if (!_originalValue.Equals(result))
                {
                    //AntiCheatHelper.OnDetected();
                }
                return result;
            }

            set
            {
                _originalValue = value;
                unchecked
                {
                    //_obscuredKey = JRandom.RandomNum(int.MaxValue - value);
                    _obscuredInt = value + _obscuredKey;
                }
            }
        }

        public JInt(int val = 0)
        {
            _obscuredInt = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }

        public JInt(string val = "0")
        {
            _obscuredInt = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            var result = int.TryParse(val, out var _value);
            if (!result)
            {
                //Log.PrintError($"无法将{val}变为{Value.GetType()},已改为0");
                Value = 0;
            }
            else
            {
                Value = _value;
            }
        }

        public static implicit operator JInt(int val) => new JInt(val);
        public static implicit operator int(JInt val) => val.Value;
        public static bool operator ==(JInt a, JInt b) => a.Value == b.Value;
        public static bool operator !=(JInt a, JInt b) => a.Value != b.Value;

        public static JInt operator ++(JInt a)
        {
            a.Value++;
            return a;
        }

        public static JInt operator --(JInt a)
        {
            a.Value--;
            return a;
        }

        public static JInt operator +(JInt a, JInt b) => new JInt(a.Value + b.Value);
        public static JInt operator +(JInt a, int b) => new JInt(a.Value + b);

        public static JInt operator -(JInt a, JInt b) => new JInt(a.Value - b.Value);
        public static JInt operator -(JInt a, int b) => new JInt(a.Value - b);

        public static JInt operator *(JInt a, JInt b) => new JInt(a.Value * b.Value);
        public static JInt operator *(JInt a, int b) => new JInt(a.Value * b);

        public static JInt operator /(JInt a, JInt b) => new JInt(a.Value / b.Value);
        public static JInt operator /(JInt a, int b) => new JInt(a.Value / b);

        public static JInt operator %(JInt a, JInt b) => new JInt(a.Value % b.Value);
        public static JInt operator %(JInt a, int b) => new JInt(a.Value % b);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => Value.Equals((obj is JInt ? (JInt)obj : default).Value);
    }

    public struct TestStructA
    {
        public long value;
    }
    public struct Fixed64Vector2
    {
        public Fixed64 x;
        public Fixed64 y;

        public static readonly Fixed64Vector2 Zero2;

        public Fixed64Vector2(int x, int y)
        {
            this.x = new Fixed64(x);
            this.y = new Fixed64(y);
        }

        static Fixed64Vector2()
        {
            Zero2 = new Fixed64Vector2(0, 0);
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

        public long RawValue
        {
            get
            {
                return m_rawValue;
            }
        }

        static Fixed64()
        {
            Zero = new Fixed64(0);
        }

        public static bool operator <(Fixed64 x, Fixed64 y)
        {
            return x.m_rawValue < y.m_rawValue;
        }

        public static bool operator >(Fixed64 x, Fixed64 y)
        {
            return x.m_rawValue > y.m_rawValue;
        }

        public static bool operator ==(Fixed64 x, Fixed64 y)
        {
            return x.m_rawValue == y.m_rawValue;
        }

        public static bool operator !=(Fixed64 x, Fixed64 y)
        {
            return x.m_rawValue != y.m_rawValue;
        }
    }

    public struct TestStructB
    {
        public TestStructA m1;
        public TestStructA m2;
        public TestStructA m3;
        public TestStructA m4;
        public TestStructA m5;
        public TestStructA m6;
        public TestStructA m7;
        public TestStructA m8;
        public TestStructA m9;

        public static TestStructB GetOne(TestStructA a)
        {
            TestStructB ret = default;
            ret.m1 = new TestStructA { value = 1 };
            ret.m2 = new TestStructA { value = 2 };
            ret.m3 = new TestStructA { value = 3 };
            ret.m4 = new TestStructA { value = 4 };
            ret.m5 = new TestStructA { value = 5 };
            ret.m6 = new TestStructA { value = 6 };
            ret.m7 = new TestStructA { value = 7 };
            ret.m8 = new TestStructA { value = 8 };
            ret.m9 = new TestStructA { value = 9 };

            return ret;
        }

        public override string ToString()
        {
            return $"{m1.value}|{m2.value}|{ m3.value}|{ m4.value}|{ m5.value}|{ m6.value}|{ m7.value}|{ m8.value}|{ m9.value}";
        }
    }
}
