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
    unsafe class ILRuntimeTest_TestFramework_DelegateTest_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.DelegateTest);

            field = type.GetField("IntDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_IntDelegateTest_0);
            app.RegisterCLRFieldSetter(field, set_IntDelegateTest_0);
            field = type.GetField("IntDelegateTest2", flag);
            app.RegisterCLRFieldGetter(field, get_IntDelegateTest2_1);
            app.RegisterCLRFieldSetter(field, set_IntDelegateTest2_1);
            field = type.GetField("GenericDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_GenericDelegateTest_2);
            app.RegisterCLRFieldSetter(field, set_GenericDelegateTest_2);


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


    }
}
