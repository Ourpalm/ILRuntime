using System;
using System.Collections.Generic;
using System.Linq;
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
    unsafe class ILRuntimeTest_TestMainForm_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestMainForm);

            field = type.GetField("_app", flag);
            app.RegisterCLRFieldGetter(field, get__app_0);
            app.RegisterCLRFieldSetter(field, set__app_0);


        }



        static object get__app_0(ref object o)
        {
            return ILRuntimeTest.TestMainForm._app;
        }
        static void set__app_0(ref object o, object v)
        {
            ILRuntimeTest.TestMainForm._app = (ILRuntime.Runtime.Enviorment.AppDomain)v;
        }


    }
}
