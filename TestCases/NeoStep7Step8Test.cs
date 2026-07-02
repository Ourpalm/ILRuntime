using System;
using ILRuntimeTest;

namespace TestCases
{
    public class Step7TargetClass
    {
        public int IntField;
        public string StringField;
        public Step7TargetClass RefField;
    }

    public class NeoStep7Step8Test
    {
        // --- Step 7: Ldstr & primitive field
        public static void NeoTestFieldReadWrite()
        {
            var obj = new Step7TargetClass();
            obj.IntField = 42;
            obj.StringField = "HelloNeo";
            
            if (obj.IntField != 42)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
            if (obj.StringField != "HelloNeo")
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- Step 7: Ref field & Ldnull
        public static void NeoTestRefFieldAndNull()
        {
            var obj = new Step7TargetClass();
            obj.RefField = new Step7TargetClass();
            obj.RefField.IntField = 100;

            if (obj.RefField.IntField != 100)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
            
            obj.RefField = null;
            if (obj.RefField != null)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- Step 8: Call primitive params and Ret
        public static int Add3(int a, int b, int c)
        {
            return a + b + c;
        }

        public static void NeoTestCallPrimitive()
        {
            int r = Add3(10, 20, 30);
            if (r != 60)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        public static void NeoTestCLRBindingSmallPrimitiveArgs()
        {
            int r = ILRuntimeTest.TestFramework.TestCLRBinding.ValidateNeoSmallPrimitiveArgs(
                (byte)7,
                (sbyte)-3,
                (short)-200,
                (ushort)500,
                true,
                'A',
                90000);
            if (r != 91369)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- Step 8: Call reference params and Ret
        public static string ConcatRef(Step7TargetClass obj)
        {
            return obj.StringField + "_End";
        }

        public static void NeoTestCallReference()
        {
            var obj = new Step7TargetClass();
            obj.StringField = "Start";
            
            // Note: we can't test string concat yet because it's a CLR method call (Step 9).
            // Let's just return the object itself to test reference return.
            var r = GetRefField(obj);
            if (r != null)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }
        
        public static Step7TargetClass GetRefField(Step7TargetClass obj)
        {
            return obj.RefField; // returns null in the above test
        }

        // --- Step 8: Recursion
        public static int Fib(int n)
        {
            if (n <= 1) return n;
            return Fib(n - 1) + Fib(n - 2);
        }

        public static void NeoTestRecursion()
        {
            int r = Fib(6);
            if (r != 8)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- Step 8b: Constructor with params
        public class Point
        {
            public int X;
            public int Y;
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public static void NeoTestNewobjWithParams()
        {
            var p = new Point(5, 7);
            if (p.X != 5 || p.Y != 7)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }
    }
}
