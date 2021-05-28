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
            app.RegisterCLRFieldBinding(field, CopyToStack__app_0, AssignFromStack__app_0);


        }



        static object get__app_0(ref object o)
        {
            return ILRuntimeTest.TestMainForm._app;
        }

        static StackObject* CopyToStack__app_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestMainForm._app;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set__app_0(ref object o, object v)
        {
            ILRuntimeTest.TestMainForm._app = (ILRuntime.Runtime.Enviorment.AppDomain)v;
        }

        static StackObject* AssignFromStack__app_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntime.Runtime.Enviorment.AppDomain @_app = (ILRuntime.Runtime.Enviorment.AppDomain)typeof(ILRuntime.Runtime.Enviorment.AppDomain).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ILRuntimeTest.TestMainForm._app = @_app;
            return ptr_of_this_method;
        }



    }
}
