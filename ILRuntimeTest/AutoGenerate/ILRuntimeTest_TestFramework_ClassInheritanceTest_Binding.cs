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
    unsafe class ILRuntimeTest_TestFramework_ClassInheritanceTest_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest);
            args = new Type[]{};
            method = type.GetMethod("TestAbstract", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestAbstract_0);
            args = new Type[]{};
            method = type.GetMethod("TestVirtual", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestVirtual_1);
            args = new Type[]{};
            method = type.GetMethod("TestField", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestField_2);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.InterfaceTest)};
            method = type.GetMethod("Test3", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Test3_3);

            field = type.GetField("TestVal2", flag);
            app.RegisterCLRFieldGetter(field, get_TestVal2_0);
            app.RegisterCLRFieldSetter(field, set_TestVal2_0);
            field = type.GetField("staticField", flag);
            app.RegisterCLRFieldGetter(field, get_staticField_1);
            app.RegisterCLRFieldSetter(field, set_staticField_1);


        }


        static StackObject* TestAbstract_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestAbstract();

            return __ret;
        }

        static StackObject* TestVirtual_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestVirtual();

            return __ret;
        }

        static StackObject* TestField_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestField();

            return __ret;
        }

        static StackObject* Test3_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.InterfaceTest @ins = (ILRuntimeTest.TestFramework.InterfaceTest)typeof(ILRuntimeTest.TestFramework.InterfaceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestFramework.ClassInheritanceTest.Test3(@ins);

            return __ret;
        }


        static object get_TestVal2_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.ClassInheritanceTest)o).TestVal2;
        }
        static void set_TestVal2_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.ClassInheritanceTest)o).TestVal2 = (System.Int32)v;
        }
        static object get_staticField_1(ref object o)
        {
            return ILRuntimeTest.TestFramework.ClassInheritanceTest.staticField;
        }
        static void set_staticField_1(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.ClassInheritanceTest.staticField = (System.IDisposable)v;
        }


    }
}
