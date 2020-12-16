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
    unsafe class ILRuntimeTest_TestFramework_TestCLREnumClass_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestCLREnumClass);
            args = new Type[]{};
            method = type.GetMethod("get_Test2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Test2_0);

            field = type.GetField("Test", flag);
            app.RegisterCLRFieldGetter(field, get_Test_0);
            app.RegisterCLRFieldSetter(field, set_Test_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Test_0, AssignFromStack_Test_0);


        }


        static StackObject* get_Test2_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = ILRuntimeTest.TestFramework.TestCLREnumClass.Test2;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_Test_0(ref object o)
        {
            return ILRuntimeTest.TestFramework.TestCLREnumClass.Test;
        }

        static StackObject* CopyToStack_Test_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.TestCLREnumClass.Test;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Test_0(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.TestCLREnumClass.Test = (ILRuntimeTest.TestFramework.TestCLREnum)v;
        }

        static StackObject* AssignFromStack_Test_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.TestCLREnum @Test = (ILRuntimeTest.TestFramework.TestCLREnum)typeof(ILRuntimeTest.TestFramework.TestCLREnum).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ILRuntimeTest.TestFramework.TestCLREnumClass.Test = @Test;
            return ptr_of_this_method;
        }



    }
}
