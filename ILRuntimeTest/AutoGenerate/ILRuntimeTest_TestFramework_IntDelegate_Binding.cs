using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ILRuntimeTest_TestFramework_IntDelegate_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            app.DelegateManager.RegisterMethodDelegate<System.Int32> ();

            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.IntDelegate>((act) =>
            {
                return new ILRuntimeTest.TestFramework.IntDelegate((a) =>
                {
                    ((Action<System.Int32>)act)(a);
                });
            });
        }
    }
}
