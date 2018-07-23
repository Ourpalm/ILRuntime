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
    unsafe class ILRuntimeTest_TestFramework_DelegateTest_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.DelegateTest);
            args = new Type[]{};
            method = type.GetMethod("TestEnumDelegate", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestEnumDelegate_0);
            args = new Type[]{};
            method = type.GetMethod("TestEnumDelegate2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestEnumDelegate2_1);

            field = type.GetField("IntDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_IntDelegateTest_0);
            app.RegisterCLRFieldSetter(field, set_IntDelegateTest_0);
            field = type.GetField("IntDelegateTest2", flag);
            app.RegisterCLRFieldGetter(field, get_IntDelegateTest2_1);
            app.RegisterCLRFieldSetter(field, set_IntDelegateTest2_1);
            field = type.GetField("GenericDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_GenericDelegateTest_2);
            app.RegisterCLRFieldSetter(field, set_GenericDelegateTest_2);
            field = type.GetField("EnumDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_EnumDelegateTest_3);
            app.RegisterCLRFieldSetter(field, set_EnumDelegateTest_3);
            field = type.GetField("EnumDelegateTest2", flag);
            app.RegisterCLRFieldGetter(field, get_EnumDelegateTest2_4);
            app.RegisterCLRFieldSetter(field, set_EnumDelegateTest2_4);
            field = type.GetField("DelegatePerformanceTest", flag);
            app.RegisterCLRFieldGetter(field, get_DelegatePerformanceTest_5);
            app.RegisterCLRFieldSetter(field, set_DelegatePerformanceTest_5);


        }


        static StackObject* TestEnumDelegate_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            ILRuntimeTest.TestFramework.DelegateTest.TestEnumDelegate();

            return __ret;
        }

        static StackObject* TestEnumDelegate2_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            ILRuntimeTest.TestFramework.DelegateTest.TestEnumDelegate2();

            return __ret;
        }


        static object get_IntDelegateTest_0(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest;
        }
        static void set_IntDelegateTest_0(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest = (ILRuntimeTest.TestFramework.IntDelegate)v;
        }
        static object get_IntDelegateTest2_1(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2;
        }
        static void set_IntDelegateTest2_1(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2 = (ILRuntimeTest.TestFramework.IntDelegate2)v;
        }
        static object get_GenericDelegateTest_2(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.GenericDelegateTest;
        }
        static void set_GenericDelegateTest_2(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.GenericDelegateTest = (System.Action<ILRuntimeTest.TestFramework.BaseClassTest>)v;
        }
        static object get_EnumDelegateTest_3(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest;
        }
        static void set_EnumDelegateTest_3(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest = (System.Action<ILRuntimeTest.TestFramework.TestCLREnum>)v;
        }
        static object get_EnumDelegateTest2_4(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest2;
        }
        static void set_EnumDelegateTest2_4(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest2 = (System.Func<ILRuntimeTest.TestFramework.TestCLREnum>)v;
        }
        static object get_DelegatePerformanceTest_5(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest;
        }
        static void set_DelegatePerformanceTest_5(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest = (System.Func<System.Int32, System.Single, System.Int16, System.Double>)v;
        }


    }
}
