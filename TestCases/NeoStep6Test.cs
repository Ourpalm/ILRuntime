using System;
using ILRuntimeTest;

namespace TestCases
{
    /// <summary>
    /// Step 6 smoke tests for the Neo register VM. These exercise the typed
    /// arithmetic / compare / branch / Conv / immediate variants emitted under
    /// ENABLE_NEO_MODE.
    ///
    /// Constraints (Step 6 only): no Call, no Ldstr, no Newobj, no Console,
    /// no string.Format, no static-field references (Ldsfld). Each method's
    /// body must compile to nothing more than the opcodes Step 6 implements.
    ///
    /// Failure signaling: every check that fails performs an inline
    /// `int x = 1 / 0;` to raise a DivideByZeroException. The harness
    /// (BaseTestUnit.Invoke) treats any unexpected exception as Failed, so a
    /// non-zero bail-out is reported even though we never call into anything.
    /// </summary>
    public class NeoStep6Test
    {
        // --- I4 three-slot arithmetic
        public static void NeoAddI4()
        {
            int a = 40;
            int b = 2;
            int r = a + b;
            if (r != 42)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- I4 immediate arithmetic (Addi)
        public static void NeoAddiI4()
        {
            int a = 37;
            int r = a + 5;
            if (r != 42)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- I4 compare + branch (Cgt + Brtrue)
        public static void NeoMaxI4()
        {
            int a = 7;
            int b = 13;
            int r1 = a > b ? a : b;
            if (r1 != 13)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
            int c = 99;
            int d2 = 1;
            int r2 = c > d2 ? c : d2;
            if (r2 != 99)
            {
                int z = 1; int dd = 0; int _ = z / dd;
            }
        }

        // --- I4 immediate branch in a loop (Blti)
        public static void NeoSumI4()
        {
            int s = 0;
            for (int i = 0; i < 10; i++)
                s += i;
            if (s != 45)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- I8 three-slot arithmetic
        public static void NeoAddI8()
        {
            long a = 40000000000L;
            long b = 2L;
            long r = a + b;
            if (r != 40000000002L)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- I8 immediate arithmetic (Addi_I8)
        public static void NeoAddiI8()
        {
            long a = 200L;
            long r = a + 100L;
            if (r != 300L)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- I8 immediate compare (Ceqi_I8)
        public static void NeoEqI8()
        {
            long x = 42L;
            if (!(x == 42L))
            {
                int z = 1; int d = 0; int _ = z / d;
            }
            long y = 43L;
            if (y == 42L)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- I8 immediate branch loop (Blti_I8)
        public static void NeoSumI8()
        {
            long s = 0L;
            for (long i = 0L; i < 5L; i++)
                s += i;
            if (s != 10L)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- R8 arithmetic (Div_R8 + R8 compare)
        public static void NeoDivR8()
        {
            double a = 10.0;
            double b = 4.0;
            double r = a / b;
            if (r != 2.5)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
            double inf = 1.0 / 0.0;
            if (!(inf > 1e300))
            {
                int z = 1; int d = 0; int _ = z / d;
            }
            double ninf = -1.0 / 0.0;
            if (!(ninf < -1e300))
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- R8 NaN compare must not branch
        public static void NeoNaNR8()
        {
            double nan = 0.0 / 0.0;
            if (nan == nan)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
            if (!(nan != nan))
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- I8 shift (Shl_I8)
        public static void NeoShlI8()
        {
            long v = 3L;
            int n = 32;
            long r = v << n;
            if (r != (3L << 32))
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- Conv R8 -> I4
        public static void NeoConvR8ToI4()
        {
            double d = 3.7;
            int r = (int)d;
            if (r != 3)
            {
                int z = 1; int dd = 0; int _ = z / dd;
            }
            double e = -3.9;
            int r2 = (int)e;
            if (r2 != -3)
            {
                int z = 1; int dd = 0; int _ = z / dd;
            }
        }

        // --- Conv I4 -> I8 sign-extend
        public static void NeoConvI4ToI8()
        {
            int i = -1;
            long r = (long)i;
            if (r != -1L)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }

        // --- Integer divide-by-zero must throw (Div_I4 path).
        [ILRuntimeTest(ExpectException = typeof(DivideByZeroException))]
        public static void NeoDivByZeroI4()
        {
            int x = 10;
            int y = 0;
            int r = x / y; // expected to throw
            // sink so the compiler keeps the divide:
            if (r == 0)
            {
                int z = 1; int d = 0; int _ = z / d;
            }
        }
    }
}
