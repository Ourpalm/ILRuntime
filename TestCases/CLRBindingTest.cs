using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TestCases
{
    class CLRBindingTest
    {
        public static void CLRBindingTest01()
        {
            byte[] mAllMissionData = new byte[10];
            int missionID = 2;
            ILRuntimeTest.TestFramework.TestClass3.setBit(ref mAllMissionData[(missionID - 1) >> 2], (missionID - 1) & 3, 1);
        }

        public static void CLRBindingTest02()
        {
            int[][] val = new int[10][];
            ILRuntimeTest.TestBase.StaticGenericMethods.ArrayMethod(val);
        }
        public static void CLRBindingTest03()
        {
            int[][][] val = new int[10][][];
            ILRuntimeTest.TestBase.StaticGenericMethods.ArrayMethod(val);
        }
        public static void CLRBindingTest04()
        {
            ILRuntimeTest.TestBase.TestSession.LastSession.Appdomain.AllowUnboundCLRMethod = false;
        }
        public static void CLRBindingTest05()
        {            
            ILRuntimeTest.TestFramework.TestCLRBinding binding = new ILRuntimeTest.TestFramework.TestCLRBinding();
            binding.Emit(binding);
        }

        public static void CLRBindingTest06()
        {
            ILRuntimeTest.TestFramework.TestCLRBinding binding = new ILRuntimeTest.TestFramework.TestCLRBinding();
            CLRBindingTest06Sub(binding, binding);
        }
        static void CLRBindingTest06Sub<T>(ILRuntimeTest.TestFramework.TestCLRBinding binding, T obj)
        {
            binding.LoadAsset("222", obj);
        }

        public static void CLRBindingTest07()
        {
            ILRuntimeTest.TestFramework.TestCLRBinding binding = new ILRuntimeTest.TestFramework.TestCLRBinding();
            CLRBindingTest07Sub(binding, "");
        }

        public static void CLRBindingTest08()
        {
            ILRuntimeTest.TestFramework.TestCLRBinding binding = new ILRuntimeTest.TestFramework.TestCLRBinding();
            CLRBindingTest07Sub2(binding, 12334);
        }
        static void CLRBindingTest07Sub<T>(ILRuntimeTest.TestFramework.TestCLRBinding binding, T obj)
        {
            CLRBindingTest06Sub(binding, obj);
        }

        static void CLRBindingTest07Sub2<K,V>(K binding, V obj) where K: ILRuntimeTest.TestFramework.TestCLRBinding
        {
            CLRBindingTest07Sub(binding, obj);
        }

        public static void CLRBindingTestEnd()
        {
            ILRuntimeTest.TestBase.TestSession.LastSession.Appdomain.AllowUnboundCLRMethod = true;
        }
    }
}
