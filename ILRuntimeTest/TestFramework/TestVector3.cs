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

        public static TestVector3 operator +(TestVector3 a, TestVector3 b)
        {
            TestVector3 res = new TestVector3();
            res.X = a.X + b.Y;
            res.Y = a.Y + b.Y;
            res.Z = a.Z + b.Z;

            return res;
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", X, Y, Z);
        }
    }
}
