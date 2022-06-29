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
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Generated
{
    unsafe class ILRuntimeTest_TestFramework_BindableProperty_1_Int64_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.BindableProperty<System.Int64>);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("set_Value", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Value_0);

            field = type.GetField("OnChangeWithOldVal", flag);
            app.RegisterCLRFieldGetter(field, get_OnChangeWithOldVal_0);
            app.RegisterCLRFieldSetter(field, set_OnChangeWithOldVal_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnChangeWithOldVal_0, AssignFromStack_OnChangeWithOldVal_0);

            args = new Type[]{typeof(System.Int64)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* set_Value_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 @value = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.BindableProperty<System.Int64> instance_of_this_method = (ILRuntimeTest.TestFramework.BindableProperty<System.Int64>)typeof(ILRuntimeTest.TestFramework.BindableProperty<System.Int64>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Value = value;

            return __ret;
        }


        static object get_OnChangeWithOldVal_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.BindableProperty<System.Int64>)o).OnChangeWithOldVal;
        }

        static StackObject* CopyToStack_OnChangeWithOldVal_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.BindableProperty<System.Int64>)o).OnChangeWithOldVal;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnChangeWithOldVal_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.BindableProperty<System.Int64>)o).OnChangeWithOldVal = (ILRuntimeTest.TestFramework.BindableProperty<System.Int64>.onChangeWithOldVal)v;
        }

        static StackObject* AssignFromStack_OnChangeWithOldVal_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.BindableProperty<System.Int64>.onChangeWithOldVal @OnChangeWithOldVal = (ILRuntimeTest.TestFramework.BindableProperty<System.Int64>.onChangeWithOldVal)typeof(ILRuntimeTest.TestFramework.BindableProperty<System.Int64>.onChangeWithOldVal).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((ILRuntimeTest.TestFramework.BindableProperty<System.Int64>)o).OnChangeWithOldVal = @OnChangeWithOldVal;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 @val = *(long*)&ptr_of_this_method->Value;


            var result_of_this_method = new ILRuntimeTest.TestFramework.BindableProperty<System.Int64>(@val);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
