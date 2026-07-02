using System;
using ILRuntimeTest;

namespace TestCases
{
    /// <summary>
    /// Step 6 smoke tests for the Neo register VM. These exercise the typed
    /// arithmetic / compare / branch / Conv / immediate variants emitted under
    /// ENABLE_NEO_MODE.
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
                Console.WriteLine("NeoAddI4 failed: r != 42");
                throw new Exception("NeoAddI4 failed: r != 42");
            }
        }

        // --- I4 immediate arithmetic (Addi)
        public static void NeoAddiI4()
        {
            int a = 37;
            int r = a + 5;
            if (r != 42)
            {
                Console.WriteLine("NeoAddiI4 failed: r != 42");
                throw new Exception("NeoAddiI4 failed: r != 42");
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
                Console.WriteLine("NeoMaxI4 failed: r1 != 13");
                throw new Exception("NeoMaxI4 failed: r1 != 13");
            }
            int c = 99;
            int d2 = 1;
            int r2 = c > d2 ? c : d2;
            if (r2 != 99)
            {
                Console.WriteLine("NeoMaxI4 failed: r2 != 99");
                throw new Exception("NeoMaxI4 failed: r2 != 99");
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
                Console.WriteLine("NeoSumI4 failed: s != 45");
                throw new Exception("NeoSumI4 failed: s != 45");
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
                Console.WriteLine("NeoAddI8 failed: r != 40000000002L");
                throw new Exception("NeoAddI8 failed: r != 40000000002L");
            }
        }

        // --- I8 immediate arithmetic (Addi_I8)
        public static void NeoAddiI8()
        {
            long a = 200L;
            long r = a + 100L;
            if (r != 300L)
            {
                Console.WriteLine("NeoAddiI8 failed: r != 300L");
                throw new Exception("NeoAddiI8 failed: r != 300L");
            }
        }

        // --- I8 immediate compare (Ceqi_I8)
        public static void NeoEqI8()
        {
            long x = 42L;
            if (!(x == 42L))
            {
                Console.WriteLine("NeoEqI8 failed: !(x == 42L)");
                throw new Exception("NeoEqI8 failed: !(x == 42L)");
            }
            long y = 43L;
            if (y == 42L)
            {
                Console.WriteLine("NeoEqI8 failed: y == 42L");
                throw new Exception("NeoEqI8 failed: y == 42L");
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
                Console.WriteLine("NeoSumI8 failed: s != 10L");
                throw new Exception("NeoSumI8 failed: s != 10L");
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
                Console.WriteLine("NeoDivR8 failed: r != 2.5");
                throw new Exception("NeoDivR8 failed: r != 2.5");
            }
            double inf = 1.0 / 0.0;
            if (!(inf > 1e300))
            {
                Console.WriteLine("NeoDivR8 failed: !(inf > 1e300)");
                throw new Exception("NeoDivR8 failed: !(inf > 1e300)");
            }
            double ninf = -1.0 / 0.0;
            if (!(ninf < -1e300))
            {
                Console.WriteLine("NeoDivR8 failed: !(ninf < -1e300)");
                throw new Exception("NeoDivR8 failed: !(ninf < -1e300)");
            }
        }

        // --- R8 NaN compare must not branch
        public static void NeoNaNR8()
        {
            double nan = 0.0 / 0.0;
            if (nan == nan)
            {
                Console.WriteLine("NeoNaNR8 failed: nan == nan");
                throw new Exception("NeoNaNR8 failed: nan == nan");
            }
            if (!(nan != nan))
            {
                Console.WriteLine("NeoNaNR8 failed: !(nan != nan)");
                throw new Exception("NeoNaNR8 failed: !(nan != nan)");
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
                Console.WriteLine("NeoShlI8 failed: r != (3L << 32)");
                throw new Exception("NeoShlI8 failed: r != (3L << 32)");
            }
        }

        // --- Conv R8 -> I4
        public static void NeoConvR8ToI4()
        {
            double d = 3.7;
            int r = (int)d;
            if (r != 3)
            {
                Console.WriteLine("NeoConvR8ToI4 failed: r != 3");
                throw new Exception("NeoConvR8ToI4 failed: r != 3");
            }
            double e = -3.9;
            int r2 = (int)e;
            if (r2 != -3)
            {
                Console.WriteLine("NeoConvR8ToI4 failed: r2 != -3");
                throw new Exception("NeoConvR8ToI4 failed: r2 != -3");
            }
        }

        // --- Conv I4 -> I8 sign-extend
        public static void NeoConvI4ToI8()
        {
            int i = -1;
            long r = (long)i;
            if (r != -1L)
            {
                Console.WriteLine("NeoConvI4ToI8 failed: r != -1L");
                throw new Exception("NeoConvI4ToI8 failed: r != -1L");
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
                Console.WriteLine("NeoDivByZeroI4 failed: unreachable");
                throw new Exception("NeoDivByZeroI4 failed: unreachable");
            }
        }
    }
}
