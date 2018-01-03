using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class ArrayTest
    {
        public static void ArrayTest01()
        {
            var a = new int[0][];  //Assets/ILRuntime/Generated/System_Int32_Binding_Array.cs(26,78): error CS0178: Invalid rank specifier, expecting `,' or `]'
            var b = new int[0][][]; //Assets/ILRuntime/Generated/System_Int32[]_Binding_Array.cs

        }

        public static void ArrayTest02()
        {
            int[] aa = new int[] { 1, 2, 3, 4, 5 };

            uint index = 2;
            int i = aa[index]; //throw exception here

        }

        public static void ArrayTest03()
        {
            double[] propValueArr = new double[] { 0.5, 0.5 };
            ArrayTest03Sub(propValueArr, 1, new ArrayTest03Cls(), 14);
        }

        class ArrayTest03Cls
        {
            public float[] levelParam = new float[] { 0.5f, 0.5f };
        }

        static void ArrayTest03Sub(double[] propValurArr, int i, ArrayTest03Cls conf, int skillLevel)
        {
            propValurArr[i] += conf.levelParam[i] * (skillLevel - 1);
            Console.WriteLine("{0} buff0 {1} {2} {3} {4} {5} {6}", 61901, skillLevel, 0, 0, propValurArr[i], conf.levelParam[i], conf.levelParam[i] * (skillLevel - 1));
        }

    }
}
