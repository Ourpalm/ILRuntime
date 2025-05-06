//#define PATCHED
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixInheritance : HotfixInheritanceBaseClass
    {
        int privField;
        public int pubField;
#if PATCHED
        int privFieldAdded;
#endif
        public HotfixInheritance(int val)
            :base(val)
        {
#if PATCHED
            privField = middleProtected + 400;
            pubField = middleProtected + baseProtected + 400;
            privFieldAdded = privField + 100;
#else
            privField = middleProtected + 300;
            pubField = middleProtected + baseProtected + 300;
#endif
        }

        public override int Value
        {
            get
            {
                Console.WriteLine("Val=" + privField);
                return privField;
            }
        }

        public int GetTestValue(int val)
        {
#if PATCHED
            return CalculateValue(val) + privField + 100;
#else
            return CalculateValue(val) + privField;
#endif
        }

        public int ModifyProtected()
        {
#if PATCHED
            middleProtectedAdded += privFieldAdded + privField;
            return middleProtectedAdded;
#else
            return 0;
#endif
        }
    }

    [ILRuntimePatch]
    public class HotfixInheritanceBaseClass : HotfixInheritanceBaseClassNoAttribute
    {
        int middlePrivate;
        protected int middleProtected;
        public int middlePublic;
#if PATCHED
        protected int middleProtectedAdded;
#endif
        public HotfixInheritanceBaseClass(int val)
            :base(val)
        {
            middlePrivate = val + 300;
            middleProtected = baseProtected + 100;
            middlePublic = basePublic + 100;
#if PATCHED
            middleProtectedAdded = middleProtected + 1000;
#endif
        }

        public override int Value => middlePrivate;

        protected override int CalculateValue(int val)
        {
#if PATCHED
            return base.CalculateValue(val) + middlePrivate + 100;
#else
            return base.CalculateValue(val) + middlePrivate;
#endif
        }

        public override bool TestProp
        {
            get
            {
                return true;
            }
            set
            {

            }
        }
    }

    public class HotfixInheritanceBaseClassNoAttribute
    {
        int basePrivate;
        protected int baseProtected;
        public int basePublic;

        public virtual bool TestProp { get; set; }

        public HotfixInheritanceBaseClassNoAttribute(int val)
        {
            basePrivate = val;
            baseProtected = val + 100;
            basePublic = val + 200;
        }

        public virtual int Value
        {
            get
            {
                return basePrivate;
            }
        }

        protected virtual int CalculateValue(int val)
        {
            return basePrivate + val;
        }
    }

    public class HotfixTestInheritanceTestCases
    {
        static bool Test01(bool patched)
        {
            HotfixInheritance cls = new HotfixInheritance(100);
            Console.WriteLine("pubField=" + cls.pubField);

            return patched ? cls.Value == 700 && cls.pubField == 900: cls.Value == 600;
        }
        static bool Test02(bool patched)
        {
            HotfixInheritance cls = new HotfixInheritance(0);
            int result = cls.GetTestValue(123);
            Console.WriteLine("result=" + result);
            return patched ? result == 1223 : result == 1023;
        }
        static bool Test03(bool patched)
        {
            HotfixInheritance cls = new HotfixInheritance(0);
            int result = cls.ModifyProtected();
            Console.WriteLine("value = " + cls.Value + " result=" + result);
            return patched ? result == 2500 : result == 0;
        }
        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase($"{nameof(HotfixTestInheritanceTestCases)}.{nameof(Test01)}", Test01);
            yield return new DelegateTestCase($"{nameof(HotfixTestInheritanceTestCases)}.{nameof(Test02)}", Test02);
            yield return new DelegateTestCase($"{nameof(HotfixTestInheritanceTestCases)}.{nameof(Test03)}", Test03);
        }
    }
}
