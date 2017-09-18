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

        public static TestVector3 operator +(TestVector3 a, TestVector3 b)
        {
            TestVector3 res = new TestVector3();
            res.X = a.X + b.Y;
            res.Y = a.Y + b.Y;
            res.Z = a.Z + b.Z;

            return res;
        }
    }
}
