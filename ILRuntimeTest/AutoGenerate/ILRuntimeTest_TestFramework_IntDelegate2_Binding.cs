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
    unsafe class ILRuntimeTest_TestFramework_IntDelegate2_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            app.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Int32> ();

            app.DelegateManager.RegisterDelegateConvertor<ILRuntimeTest.TestFramework.IntDelegate2>((act) =>
            {
                return new ILRuntimeTest.TestFramework.IntDelegate2((a) =>
                {
                    return ((Func<System.Int32, System.Int32>)act)(a);
                });
            });
        }
    }
}
