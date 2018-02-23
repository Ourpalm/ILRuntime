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
    unsafe class System_Action_3_Int32_String_String_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            app.DelegateManager.RegisterMethodDelegate<System.Int32, System.String, System.String> ();

            app.DelegateManager.RegisterDelegateConvertor<System.Action<System.Int32, System.String, System.String>>((act) =>
            {
                return new System.Action<System.Int32, System.String, System.String>((arg1, arg2, arg3) =>
                {
                    ((Action<System.Int32, System.String, System.String>)act)(arg1, arg2, arg3);
                });
            });
        }
    }
}
