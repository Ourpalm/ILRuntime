//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixClass
    {
        public int IntField;
        static int IntFieldS;
        public bool BoolField;
        static bool BoolFieldS;
#if PATCHED
        public int IntFieldAdded;
        static float FloatFieldAdded;
        static bool BoolFieldAdded;
#endif

        public HotfixClass(int val)
        {   
#if PATCHED
            IntField = val + 100;
            IntFieldAdded = val + 200;
            BoolField = true;
            BoolFieldS = true;
#else
            IntField = val;
#endif
        }
        public void Method(int a, string b, out string result)
        {
#if PATCHED
            result = string.Format("{1}{0}", b, a + 100);
#else
            result = string.Format("{1}{0}", b, a);
#endif
        }

        public void MethodRef(ref int a, string b, out string result)
        {
#if PATCHED
            a += 100;
            result = string.Format("result:{1} ref:{0}", b, a++);
#else            
            result = string.Format("result:{1} ref:{0}", b, a++);
#endif
        }

        public void MethodRef2(ref uint a, string b, out string result)
        {
#if PATCHED
            a += 100;
            result = string.Format("result:{1} ref:{0}", b, a++);
#else            
            result = string.Format("result:{1} ref:{0}", b, a++);
#endif
        }

        public void MethodRef3(ref bool a, string b, out string result)
        {
#if PATCHED
            a = !a;
            result = string.Format("result:{1} ref:{0}", b, a);
#else            
            result = string.Format("result:{1} ref:{0}", b, a);
#endif
        }

        static Func<HotfixClass, int, int> ttt;
        public int Function(int a)
        {
#if PATCHED
            HotfixClass2 cls = new HotfixClass2(IntField + IntFieldAdded + a);
            return cls.IntValue;
#else
            if(ttt !=null)
            {
                return ttt(this, a);
            }
            return a + 111;
#endif
        }

        public static void InitStaticFields(int val)
        {

#if PATCHED
            IntFieldS = (val + 100) / 2;
            FloatFieldAdded = (float)val /3f;
            BoolFieldAdded = true;
#else
            IntFieldS = val / 2;
#endif
        }

        public static int GetIntField()
        {
            return IntFieldS;
        }

        public static float GetFloatField()
        {
#if PATCHED
            return FloatFieldAdded;
#else
            return 0;
#endif

        }

        public static bool GetBoolField()
        {

#if PATCHED
            return BoolFieldAdded;
#else
            return false;
#endif

        }

        public int FunctionAddParam(int a
#if PATCHED
, int b
#endif
            )
        {
            return a + 333;
        }

        public void TestArray(out int[] arr)
        {
#if PATCHED
            arr = new int[] { 333, 666, 555 };
#else
            arr = new int[] { 333, 444, 555 };
#endif
        }

        public void TestArray2()
        {
#if PATCHED
            TestVector3[] arr = new TestVector3[] { TestVector3.One, TestVector3.One2 };
            Console.WriteLine(arr[0].ToString());
#endif
        }

        public int FunctionNoChange(int a, string b)
        {
            return a + int.Parse(b);
        }

        public static int TestLambdaExp(int a)
        {
            int ttt = a + 100;
            Func<int, int> cb = (val) =>
            {
#if PATCHED
                return ttt + 100;
#else
                return ttt;
#endif
            };

            return cb(a);
        }
        public static int TestLambdaExp2(int a)
        {
            int ttt = a + 100;
            Func<int, int> cb = (val) =>
            {
                int bbb = val + a;
#if PATCHED
                int ddd = a / 2;
#endif
                Func<int, int> ccc = (v) =>
                {
#if PATCHED
                    return bbb + a + v + ddd;
#else
                    return bbb + a + v;
#endif
                };
                var res = ccc(ttt);
                Console.WriteLine("res=" + res);
                return res;
            };
            Func<int, int> cb2 = (val) =>
            {
                return ttt + a;
            };
            var res1 = cb(a);
            var res2 = cb2(a);
            int result = res1 + res2;
            Console.WriteLine(string.Format("res1={0} res2={1} result={2}", res1, res2, result));
            return result;
        }
        public static int TestLambdaExp3(int a)
        {
            Func<int, int> cb = (val) =>
            {
#if PATCHED
                return val + 100;
#else
                return val;
#endif
            };

            return cb(a);
        }
        public static int TestLambdaExp4(int a)
        {
            Func<int, int> cb = (val) =>
            {
#if PATCHED
                return val + 100;
#else
                return val;
#endif
            };

            return cb(a);
        }
        public static int TestLambdaExp5(int a)
        {
            Func<int, int> cb = (val) =>
            {
#if PATCHED
                return a + 100;
#else
                return a;
#endif
            };

            return cb(a);
        }
    }

    public class HotfixBasicTestCases
    {
        static bool Test01(bool patched)
        {
            HotfixClass cls = new HotfixClass(100);

            return patched ? cls.IntField == 200 && cls.BoolField : cls.IntField == 100;
        }

        static bool Test02(bool patched)
        {
            HotfixClass cls = new HotfixClass(100);
            cls.MethodRef(ref cls.IntField, "aaa", out var result);
            bool oldVal = cls.BoolField;
            cls.MethodRef3(ref cls.BoolField, "aaa", out var result2);

            if (patched)
            {
                Console.WriteLine(string.Format("result:{0},{1}", cls.IntField, cls.BoolField));
                return cls.IntField == 301 && !cls.BoolField && oldVal && result == $"result:300 ref:aaa";
            }
            else
            {
                return cls.IntField == 101 && result == $"result:{100} ref:aaa";
            }
        }
        static bool Test03(bool patched)
        {
            HotfixClass cls = new HotfixClass(0);
            int res = cls.Function(100);
            return patched ? res == 12733 : res == 211;
        }

        static bool Test04(bool patched)
        {
            HotfixClass.InitStaticFields(100);
            return patched ? HotfixClass.GetBoolField() && (HotfixClass.GetIntField() == 100) && Math.Abs(HotfixClass.GetFloatField() - (100f / 3f)) < 0.0001f:
                HotfixClass.GetIntField() == 50 && HotfixClass.GetFloatField() == 0;
        }

        static bool Test05(bool patched)
        {
            HotfixClass cls = new HotfixClass(0);
            cls.TestArray(out var result);
            return patched ? result[1] == 666 : result[1] == 444;
        }

        static bool Test06(bool patched)
        {
            int result = HotfixClass.TestLambdaExp(300);            
            return patched ? result == 500 : result == 400;
        }
        static bool Test07(bool patched)
        {
            int result = HotfixClass.TestLambdaExp2(300);
            return patched ? result == 2150 : result == 2000;
        }

        static bool Test08(bool patched)
        {
            HotfixClass cls = new HotfixClass(0);
            cls.TestArray2();
            return true;
        }
        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test01)}", Test01);
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test02)}", Test02);
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test03)}", Test03);
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test04)}", Test04);
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test05)}", Test05);
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test06)}", Test06);
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test07)}", Test07);
            yield return new DelegateTestCase($"{nameof(HotfixBasicTestCases)}.{nameof(Test08)}", Test08);
        }
    }

    public class HotfixClass2
    {
        int intVal;

        public int IntValue => intVal;
        public HotfixClass2(int val)
        {
            intVal = val + 12333;
        }
    }
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
}
