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
    unsafe class System_Func_2_Int32_Int32_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            app.DelegateManager.RegisterFunctionDelegate<System.Int32, System.Int32> ();

            app.DelegateManager.RegisterDelegateConvertor<System.Func<System.Int32, System.Int32>>((act) =>
            {
                return new System.Func<System.Int32, System.Int32>((arg) =>
                {
                    return ((Func<System.Int32, System.Int32>)act)(arg);
                });
            });
        }
    }
}
