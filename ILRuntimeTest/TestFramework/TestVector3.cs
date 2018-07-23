using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.Other;

namespace ILRuntimeTest.TestFramework
{   
    public struct TestVector3
    {
        public float X, Y, Z;

        public static TestVector3 One = new TestVector3(1, 1, 1);

        public TestVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static TestVector3 One2 { get { return new TestVector3(1, 1, 1); } }

        public static TestVector3 operator +(TestVector3 a, TestVector3 b)
        {
            TestVector3 res = new TestVector3();
            res.X = a.X + b.Y;
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
        public TestVector3 vector;
        public TestVector3 Vector2 { get; set; } = TestVector3.One * 0.33f;
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
}
