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
    }
}
