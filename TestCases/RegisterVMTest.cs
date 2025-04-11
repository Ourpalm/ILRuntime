using ILRuntime.Runtime;
using ILRuntimeTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TestCases
{
    public class RegisterVMTest
    {
        public static void RegisterVMTest01()
        {
            InlineTest test = new InlineTest();
            test.DoTest(true);
        }

        [ILRuntimeJIT(ILRuntimeJITFlags.NoJIT)]
        public static void RegisterVMTest02()
        {
            LoginData1 data1 = new LoginData1();
            data1.Id = 1;

            for (int i = 0; i < 20; i++)
            {
                var val = data1.GetAttrValue("id");
                if (val == null)
                    throw new Exception();
                Console.WriteLine(val + ":::" + i);
            }
        }

        class InlineTest
        {
            int a;

            int A
            {
                [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
                get
                {
                    if (a > 10)
                        return 10;

                    if (a < 2)
                        return 2;

                    return a;
                }
            }
            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public void DoTest(bool arg)
            {
                if (arg)
                {
                    a = 8;
                    var tmp = A;
                    if (tmp != 8)
                        throw new Exception();
                    Console.WriteLine(tmp);
                }
            }
        }

        public class ILSqlite3
        {
            public static ILSqlite3 instance = new ILSqlite3();

            public static object singleLock = new object();

            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public static object GetILSqlData<T>(ILSqlData1<T> data, bool onlyEmpty = false)
            {

                if (data == null) return null;

                //如果是仅判断是否为 null 直接返回一个固定object 这样可以避免很多强转消耗
                if (onlyEmpty == true) return singleLock;
                return data.value;

            }
        }
        public class ILSqlData1<T>
        {
            public T value;
            public ILSqlData1(T value)
            {
                this.value = value;
            }
            public ILSqlData1()
            {
            }
        }
        public class LoginData1
        {

            public ILSqlData1<int> id;
            public int Id
            {
                get
                {
                    if (id == null) return 0;
                    return id.value;
                }
                set
                {
                    if (id == null) id = new ILSqlData1<int>();
                    id.value = value;
                }
            }

            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public object GetAttrValue(string fieldName)
            {

                switch (fieldName)
                {
                    case "id": return ILSqlite3.GetILSqlData(id);
                }

                return null;

            }
        }
    }
}
