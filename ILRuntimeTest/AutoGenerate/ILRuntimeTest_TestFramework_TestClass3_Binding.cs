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
    unsafe class ILRuntimeTest_TestFramework_TestClass3_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestClass3);

            field = type.GetField("Struct", flag);
            app.RegisterCLRFieldGetter(field, get_Struct_0);
            app.RegisterCLRFieldSetter(field, set_Struct_0);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_Struct_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestClass3)o).Struct;
        }
        static void set_Struct_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.TestClass3)o).Struct = (ILRuntimeTest.TestFramework.TestStruct)v;
        }

        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new ILRuntimeTest.TestFramework.TestClass3();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
