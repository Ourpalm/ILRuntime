using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public class ArrayTest
    {
        public int val = 123;
        public static void ArrayTest01()
        {
            var a = new int[0][];  //Assets/ILRuntime/Generated/System_Int32_Binding_Array.cs(26,78): error CS0178: Invalid rank specifier, expecting `,' or `]'
            var b = new int[0][][]; //Assets/ILRuntime/Generated/System_Int32[]_Binding_Array.cs

        }

        public static void ArrayTest02()
        {
            int[] aa = new int[] { 1, 2, 3, 4, 5 };

            uint index = 2;
            int i = 10;
            aa[index] = 20;
            i = aa[index];
            Console.WriteLine("i {0}", i);
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


        public static void ArrayTest04()
        {
            var t = typeof(ArrayTest);
            var arr = Array.CreateInstance(t, 10);
            for (int i = 0; i < 10; i++)
            {
                arr.SetValue(Activator.CreateInstance(t), i);
            }

            Console.WriteLine(arr.Length);

            ArrayTest[] arr2 = arr as ArrayTest[];
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(arr2[i].val);
            }
        }

        public static void ArrayTest05()
        {
            ILRuntimeTest.TestFramework.TestVector3[] arr = new ILRuntimeTest.TestFramework.TestVector3[] { ILRuntimeTest.TestFramework.TestVector3.One, ILRuntimeTest.TestFramework.TestVector3.One2 };
            Console.WriteLine(arr[0].ToString());
        }

        public static void ArrayTest06()
        {
            int[,,] pos = new int[,,] { { { 1, 2, 3 }, { 3, 4, 5 } }, { { 1, 3, 3 }, { 2, 3, 4 } } };

            Console.WriteLine(pos[0, 1, 1]);
        }

        public static void ArrayTest07()
        {
            int[,] a = new int[2, 3];
            a[0, 0] = 1; //ios il2cpp ipad test: invocation exception
            a[0, 0].ToString(); //unityeditor crash on windows pc test
        }

        public static void ArrayBindTest()
        {
            ILRuntimeTest.TestFramework.TestClass4 cls = new ILRuntimeTest.TestFramework.TestClass4();
            cls.TestArrayOut(out var arr);
            Console.WriteLine(arr.Length);
        }

        static int[] sa = new int[] { 1, 2, 3, 4, 5 };
        public static void ArrayTest08()
        {
            uint index = 2;
            int i = sa[index]; 
            Console.WriteLine(i);
        }

        public static void ArrayTest09()
        {
            int[] aa = new int[] { 1, 2, 3, 4, 5 };
            int index = 2;
            int i = aa[index];
            Console.WriteLine(i);
        }
        public static void ArrayTest10()
        {
            ushort[] aa = new ushort[] { 1, 2, 65535, 4, 5 };
            uint index = 2;
            ushort i = aa[index]; 
            Console.WriteLine(i);
        }
        public static void ArrayTest11()
        {
            long[] aa = new long[3] { -1, 54 , 43742424878678 };
            uint index = 2;
            long i = aa[index]; 
            Console.WriteLine(i);
        }
        public static void ArrayTest12()
        {
            byte[] aa = new byte[] { 7, 16, 32 };
            uint index = 2;
            byte i = aa[index]; 
            Console.WriteLine(i);
        }
        public static void ArrayTest13()
        {
            char[] aa = new char[] {'a', 'b', 'c', 'd'}; //data is error ; ldtoken
            uint index = 2;
            char i = aa[index]; //元数据
            Console.WriteLine(i);
        }
        public static void ArrayTest14()
        {
            float[] aa = new float[] { 1.5f, float.MaxValue, 10.0f, 0.54587122115f };
            uint index = 3;
            float i = aa[index];
            Console.WriteLine(i);
        }
        public static void ArrayTest15()
        {
            double[] aa = new double[] { 1.5f, float.MaxValue, 10.0f, 0.54587122115f };
            uint index = 3;
            double i = aa[index];
            Console.WriteLine(i);
        }
    }
}
