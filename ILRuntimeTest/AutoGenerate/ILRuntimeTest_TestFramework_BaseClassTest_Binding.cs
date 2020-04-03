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
    unsafe class ILRuntimeTest_TestFramework_BaseClassTest_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.BaseClassTest);
            args = new Type[]{};
            method = type.GetMethod("DoTest", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DoTest_0);

            field = type.GetField("testField", flag);
            app.RegisterCLRFieldGetter(field, get_testField_0);
            app.RegisterCLRFieldSetter(field, set_testField_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_testField_0, AssignFromStack_testField_0);


        }


        static StackObject* DoTest_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            ILRuntimeTest.TestFramework.BaseClassTest.DoTest();

            return __ret;
        }


        static object get_testField_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.BaseClassTest)o).testField;
        }

        static StackObject* CopyToStack_testField_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.BaseClassTest)o).testField;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_testField_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.BaseClassTest)o).testField = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_testField_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @testField = ptr_of_this_method->Value == 1;
            ((ILRuntimeTest.TestFramework.BaseClassTest)o).testField = @testField;
            return ptr_of_this_method;
        }



    }
}
