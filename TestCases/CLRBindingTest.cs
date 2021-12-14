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
    }
}
