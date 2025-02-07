using ILRuntime.Runtime;
using System;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixClass
    {
        public int IntField;

#if PATCHED
        public int IntFieldAdded;
#endif

        public HotfixClass(int val)
        {
#if PATCHED
            IntField = val + 100;
            IntFieldAdded = val + 200;
#else
            IntField = val;
#endif
        }
        public void Method(int a, string b, out string result)
        {
#if PATCHED
            result = $"{b}{a + 100}";
#else
            result = $"{b}{a}";
#endif
        }

        public void MethodRef(ref int a, string b, out string result)
        {
#if PATCHED
            a += 100;
            result = $"{b}{a++}";
#else            
            result = $"{b}{a++}";
#endif
        }

        static Func<HotfixClass, int, int> ttt;
        public int Function(int a)
        {
#if PATCHED
            HotfixClass2 cls = new HotfixClass2(a);
            return cls.IntValue;
#else
            if(ttt !=null)
            {
                return ttt(this, a);
            }
            return a + 111;
#endif
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
}
