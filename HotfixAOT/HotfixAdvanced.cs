//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    public enum TestEnum { A, B, C, D }

    [ILRuntimePatch]
    public class HotfixReturnTypes
    {
        public string GetString()
        {
#if PATCHED
            return "patched";
#else
            return "original";
#endif
        }

        public float GetFloat()
        {
#if PATCHED
            return 3.14f;
#else
            return 1.0f;
#endif
        }

        public long GetLong()
        {
#if PATCHED
            return 9999999999L;
#else
            return 100L;
#endif
        }

        public double GetDouble()
        {
#if PATCHED
            return 2.718281828;
#else
            return 1.0;
#endif
        }

        public TestEnum GetEnum()
        {
#if PATCHED
            return TestEnum.D;
#else
            return TestEnum.A;
#endif
        }

        public object GetObject()
        {
#if PATCHED
            return "patched_object";
#else
            return 42;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixRefOutAdvanced
    {
        public void RefString(ref string s)
        {
#if PATCHED
            s = s + "_patched";
#else
            s = s + "_original";
#endif
        }

        public void OutObject(out object obj)
        {
#if PATCHED
            obj = "patched_out";
#else
            obj = "original_out";
#endif
        }

        public void RefFloat(ref float f)
        {
#if PATCHED
            f = f * 2.0f;
#else
            f = f + 1.0f;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixProperties
    {
        private int _value;

        public int Value
        {
            get
            {
#if PATCHED
                return _value * 2;
#else
                return _value;
#endif
            }
            set
            {
#if PATCHED
                _value = value * 3;
#else
                _value = value;
#endif
            }
        }

        private string _name;
        public string Name
        {
            get
            {
#if PATCHED
                return _name + "_patched";
#else
                return _name;
#endif
            }
            set
            {
#if PATCHED
                _name = value.ToUpper();
#else
                _name = value;
#endif
            }
        }
    }

    public interface ICalculator
    {
        int Calculate(int a, int b);
    }

    [ILRuntimePatch]
    public class HotfixCalculator : ICalculator
    {
        public int Calculate(int a, int b)
        {
#if PATCHED
            return a * b;
#else
            return a + b;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixException
    {
        public int SafeDivide(int a, int b)
        {
#if PATCHED
            if (b == 0) return -1;
            return a / b;
#else
            if (b == 0) return 0;
            return a / b;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixMultiGeneric
    {
        public TResult Convert<TInput, TResult>(TInput input)
        {
#if PATCHED
            return default(TResult);
#else
            return (TResult)(object)input;
#endif
        }

        public T Identity<T>(T val)
            where T : struct
        {
#if PATCHED
            return default(T);
#else
            return val;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixFieldTypes
    {
        private TestEnum enumField;
        private long longField;
        private double doubleField;

        public void Initialize()
        {
#if PATCHED
            enumField = TestEnum.C;
            longField = 888888888L;
            doubleField = 9.99;
#else
            enumField = TestEnum.A;
            longField = 100L;
            doubleField = 1.11;
#endif
        }

        public TestEnum GetEnum() { return enumField; }
        public long GetLong() { return longField; }
        public double GetDouble() { return doubleField; }
    }

    [ILRuntimePatch]
    public class HotfixNullable
    {
        public int? GetNullableInt(bool returnNull)
        {
#if PATCHED
            return returnNull ? (int?)42 : null;
#else
            return returnNull ? (int?)null : 42;
#endif
        }
    }

    public class HotfixAdvancedTestCases
    {
        static bool TestReturnTypes(bool patched)
        {
            var obj = new HotfixReturnTypes();
            if (patched)
            {
                return obj.GetString() == "patched"
                    && obj.GetFloat() == 3.14f
                    && obj.GetLong() == 9999999999L
                    && obj.GetDouble() == 2.718281828
                    && obj.GetEnum() == TestEnum.D
                    && (string)obj.GetObject() == "patched_object";
            }
            else
            {
                return obj.GetString() == "original"
                    && obj.GetFloat() == 1.0f
                    && obj.GetLong() == 100L
                    && obj.GetDouble() == 1.0
                    && obj.GetEnum() == TestEnum.A
                    && (int)obj.GetObject() == 42;
            }
        }

        static bool TestRefOutAdvanced(bool patched)
        {
            var obj = new HotfixRefOutAdvanced();
            string s = "hello";
            obj.RefString(ref s);
            bool refStringOk = patched ? s == "hello_patched" : s == "hello_original";

            object outObj;
            obj.OutObject(out outObj);
            bool outObjOk = patched ? (string)outObj == "patched_out" : (string)outObj == "original_out";

            float f = 5.0f;
            obj.RefFloat(ref f);
            bool refFloatOk = patched ? f == 10.0f : f == 6.0f;

            return refStringOk && outObjOk && refFloatOk;
        }

        static bool TestProperties(bool patched)
        {
            var obj = new HotfixProperties();
            obj.Value = 10;
            obj.Name = "test";

            if (patched)
            {
                // setter: _value = 10 * 3 = 30, getter: 30 * 2 = 60
                // setter: _name = "test".ToUpper() = "TEST", getter: "TEST" + "_patched" = "TEST_patched"
                return obj.Value == 60 && obj.Name == "TEST_patched";
            }
            else
            {
                return obj.Value == 10 && obj.Name == "test";
            }
        }

        static bool TestInterface(bool patched)
        {
            ICalculator calc = new HotfixCalculator();
            int result = calc.Calculate(3, 4);
            return patched ? result == 12 : result == 7;
        }

        static bool TestException(bool patched)
        {
            var obj = new HotfixException();
            int normalResult = obj.SafeDivide(10, 2);
            int zeroResult = obj.SafeDivide(10, 0);

            if (patched)
                return normalResult == 5 && zeroResult == -1;
            else
                return normalResult == 5 && zeroResult == 0;
        }

        static bool TestMultiGeneric(bool patched)
        {
            var obj = new HotfixMultiGeneric();
            int intResult = obj.Convert<int, int>(99);
            string strResult = obj.Convert<string, string>("test");
            int identityResult = obj.Identity<int>(42);

            if (patched)
                return intResult == 0 && strResult == null && identityResult == 0;
            else
                return intResult == 99 && strResult == "test" && identityResult == 42;
        }

        static bool TestFieldTypes(bool patched)
        {
            var obj = new HotfixFieldTypes();
            obj.Initialize();
            if (patched)
                return obj.GetEnum() == TestEnum.C && obj.GetLong() == 888888888L && obj.GetDouble() == 9.99;
            else
                return obj.GetEnum() == TestEnum.A && obj.GetLong() == 100L && obj.GetDouble() == 1.11;
        }

        static bool TestNullable(bool patched)
        {
            var obj = new HotfixNullable();
            int? withTrue = obj.GetNullableInt(true);
            int? withFalse = obj.GetNullableInt(false);

            if (patched)
                return withTrue == 42 && withFalse == null;
            else
                return withTrue == null && withFalse == 42;
        }

        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestReturnTypes)}", TestReturnTypes);
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestRefOutAdvanced)}", TestRefOutAdvanced);
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestProperties)}", TestProperties);
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestInterface)}", TestInterface);
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestException)}", TestException);
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestMultiGeneric)}", TestMultiGeneric);
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestFieldTypes)}", TestFieldTypes);
            yield return new DelegateTestCase($"{nameof(HotfixAdvancedTestCases)}.{nameof(TestNullable)}", TestNullable);
        }
    }
}
