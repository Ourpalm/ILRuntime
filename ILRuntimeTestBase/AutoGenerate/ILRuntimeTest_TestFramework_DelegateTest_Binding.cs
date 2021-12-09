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
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.IntDelegate)};
            method = type.GetMethod("add_IntDelegateEventTest", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_IntDelegateEventTest_0);
            args = new Type[]{};
            method = type.GetMethod("TestEvent", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestEvent_1);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.IntDelegate)};
            method = type.GetMethod("remove_IntDelegateEventTest", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, remove_IntDelegateEventTest_2);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.IntDelegate)};
            method = type.GetMethod("add_IntDelegateEventTest2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_IntDelegateEventTest2_3);
            args = new Type[]{};
            method = type.GetMethod("TestEvent2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestEvent2_4);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.IntDelegate)};
            method = type.GetMethod("remove_IntDelegateEventTest2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, remove_IntDelegateEventTest2_5);
            args = new Type[]{};
            method = type.GetMethod("TestEnumDelegate", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestEnumDelegate_6);
            args = new Type[]{};
            method = type.GetMethod("TestEnumDelegate2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestEnumDelegate2_7);

            field = type.GetField("IntDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_IntDelegateTest_0);
            app.RegisterCLRFieldSetter(field, set_IntDelegateTest_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_IntDelegateTest_0, AssignFromStack_IntDelegateTest_0);
            field = type.GetField("IntDelegateTest2", flag);
            app.RegisterCLRFieldGetter(field, get_IntDelegateTest2_1);
            app.RegisterCLRFieldSetter(field, set_IntDelegateTest2_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_IntDelegateTest2_1, AssignFromStack_IntDelegateTest2_1);
            field = type.GetField("GenericDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_GenericDelegateTest_2);
            app.RegisterCLRFieldSetter(field, set_GenericDelegateTest_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_GenericDelegateTest_2, AssignFromStack_GenericDelegateTest_2);
            field = type.GetField("EnumDelegateTest", flag);
            app.RegisterCLRFieldGetter(field, get_EnumDelegateTest_3);
            app.RegisterCLRFieldSetter(field, set_EnumDelegateTest_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_EnumDelegateTest_3, AssignFromStack_EnumDelegateTest_3);
            field = type.GetField("EnumDelegateTest2", flag);
            app.RegisterCLRFieldGetter(field, get_EnumDelegateTest2_4);
            app.RegisterCLRFieldSetter(field, set_EnumDelegateTest2_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_EnumDelegateTest2_4, AssignFromStack_EnumDelegateTest2_4);
            field = type.GetField("DelegatePerformanceTest", flag);
            app.RegisterCLRFieldGetter(field, get_DelegatePerformanceTest_5);
            app.RegisterCLRFieldSetter(field, set_DelegatePerformanceTest_5);
            app.RegisterCLRFieldBinding(field, CopyToStack_DelegatePerformanceTest_5, AssignFromStack_DelegatePerformanceTest_5);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* add_IntDelegateEventTest_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.IntDelegate @value = (ILRuntimeTest.TestFramework.IntDelegate)typeof(ILRuntimeTest.TestFramework.IntDelegate).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateEventTest += value;

            return __ret;
        }

        static StackObject* TestEvent_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            ILRuntimeTest.TestFramework.DelegateTest.TestEvent();

            return __ret;
        }

        static StackObject* remove_IntDelegateEventTest_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.IntDelegate @value = (ILRuntimeTest.TestFramework.IntDelegate)typeof(ILRuntimeTest.TestFramework.IntDelegate).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateEventTest -= value;

            return __ret;
        }

        static StackObject* add_IntDelegateEventTest2_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.IntDelegate @value = (ILRuntimeTest.TestFramework.IntDelegate)typeof(ILRuntimeTest.TestFramework.IntDelegate).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.DelegateTest instance_of_this_method = (ILRuntimeTest.TestFramework.DelegateTest)typeof(ILRuntimeTest.TestFramework.DelegateTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.IntDelegateEventTest2 += value;

            return __ret;
        }

        static StackObject* TestEvent2_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.DelegateTest instance_of_this_method = (ILRuntimeTest.TestFramework.DelegateTest)typeof(ILRuntimeTest.TestFramework.DelegateTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestEvent2();

            return __ret;
        }

        static StackObject* remove_IntDelegateEventTest2_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.IntDelegate @value = (ILRuntimeTest.TestFramework.IntDelegate)typeof(ILRuntimeTest.TestFramework.IntDelegate).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.DelegateTest instance_of_this_method = (ILRuntimeTest.TestFramework.DelegateTest)typeof(ILRuntimeTest.TestFramework.DelegateTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.IntDelegateEventTest2 -= value;

            return __ret;
        }

        static StackObject* TestEnumDelegate_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            ILRuntimeTest.TestFramework.DelegateTest.TestEnumDelegate();

            return __ret;
        }

        static StackObject* TestEnumDelegate2_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
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

        static StackObject* CopyToStack_IntDelegateTest_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_IntDelegateTest_0(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest = (ILRuntimeTest.TestFramework.IntDelegate)v;
        }

        static StackObject* AssignFromStack_IntDelegateTest_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.IntDelegate @IntDelegateTest = (ILRuntimeTest.TestFramework.IntDelegate)typeof(ILRuntimeTest.TestFramework.IntDelegate).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest = @IntDelegateTest;
            return ptr_of_this_method;
        }

        static object get_IntDelegateTest2_1(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2;
        }

        static StackObject* CopyToStack_IntDelegateTest2_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_IntDelegateTest2_1(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2 = (ILRuntimeTest.TestFramework.IntDelegate2)v;
        }

        static StackObject* AssignFromStack_IntDelegateTest2_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.IntDelegate2 @IntDelegateTest2 = (ILRuntimeTest.TestFramework.IntDelegate2)typeof(ILRuntimeTest.TestFramework.IntDelegate2).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ILRuntimeTest.TestFramework.DelegateTest.IntDelegateTest2 = @IntDelegateTest2;
            return ptr_of_this_method;
        }

        static object get_GenericDelegateTest_2(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.GenericDelegateTest;
        }

        static StackObject* CopyToStack_GenericDelegateTest_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.DelegateTest.GenericDelegateTest;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_GenericDelegateTest_2(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.GenericDelegateTest = (System.Action<ILRuntimeTest.TestFramework.BaseClassTest>)v;
        }

        static StackObject* AssignFromStack_GenericDelegateTest_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<ILRuntimeTest.TestFramework.BaseClassTest> @GenericDelegateTest = (System.Action<ILRuntimeTest.TestFramework.BaseClassTest>)typeof(System.Action<ILRuntimeTest.TestFramework.BaseClassTest>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ILRuntimeTest.TestFramework.DelegateTest.GenericDelegateTest = @GenericDelegateTest;
            return ptr_of_this_method;
        }

        static object get_EnumDelegateTest_3(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest;
        }

        static StackObject* CopyToStack_EnumDelegateTest_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_EnumDelegateTest_3(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest = (System.Action<ILRuntimeTest.TestFramework.TestCLREnum>)v;
        }

        static StackObject* AssignFromStack_EnumDelegateTest_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<ILRuntimeTest.TestFramework.TestCLREnum> @EnumDelegateTest = (System.Action<ILRuntimeTest.TestFramework.TestCLREnum>)typeof(System.Action<ILRuntimeTest.TestFramework.TestCLREnum>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest = @EnumDelegateTest;
            return ptr_of_this_method;
        }

        static object get_EnumDelegateTest2_4(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest2;
        }

        static StackObject* CopyToStack_EnumDelegateTest2_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest2;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_EnumDelegateTest2_4(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest2 = (System.Func<ILRuntimeTest.TestFramework.TestCLREnum>)v;
        }

        static StackObject* AssignFromStack_EnumDelegateTest2_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Func<ILRuntimeTest.TestFramework.TestCLREnum> @EnumDelegateTest2 = (System.Func<ILRuntimeTest.TestFramework.TestCLREnum>)typeof(System.Func<ILRuntimeTest.TestFramework.TestCLREnum>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ILRuntimeTest.TestFramework.DelegateTest.EnumDelegateTest2 = @EnumDelegateTest2;
            return ptr_of_this_method;
        }

        static object get_DelegatePerformanceTest_5(ref object o)
        {
            return ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest;
        }

        static StackObject* CopyToStack_DelegatePerformanceTest_5(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_DelegatePerformanceTest_5(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest = (System.Func<System.Int32, System.Single, System.Int16, System.Double>)v;
        }

        static StackObject* AssignFromStack_DelegatePerformanceTest_5(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Func<System.Int32, System.Single, System.Int16, System.Double> @DelegatePerformanceTest = (System.Func<System.Int32, System.Single, System.Int16, System.Double>)typeof(System.Func<System.Int32, System.Single, System.Int16, System.Double>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ILRuntimeTest.TestFramework.DelegateTest.DelegatePerformanceTest = @DelegatePerformanceTest;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new ILRuntimeTest.TestFramework.DelegateTest();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
