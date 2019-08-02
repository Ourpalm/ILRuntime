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
    unsafe class ILRuntimeTest_TestFramework_TestVectorClass_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestVectorClass);
            args = new Type[]{};
            method = type.GetMethod("get_Vector2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Vector2_0);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestVector3)};
            method = type.GetMethod("set_Vector2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Vector2_1);

            field = type.GetField("vector", flag);
            app.RegisterCLRFieldGetter(field, get_vector_0);
            app.RegisterCLRFieldSetter(field, set_vector_0);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* get_Vector2_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.TestVectorClass instance_of_this_method = (ILRuntimeTest.TestFramework.TestVectorClass)typeof(ILRuntimeTest.TestFramework.TestVectorClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Vector2;

            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static StackObject* set_Vector2_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.TestVector3 @value = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref @value, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @value = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
                __intp.Free(ptr_of_this_method);
            }

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.TestVectorClass instance_of_this_method = (ILRuntimeTest.TestFramework.TestVectorClass)typeof(ILRuntimeTest.TestFramework.TestVectorClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Vector2 = value;

            return __ret;
        }


        static object get_vector_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVectorClass)o).vector;
        }
        static void set_vector_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.TestVectorClass)o).vector = (ILRuntimeTest.TestFramework.TestVector3)v;
        }

        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new ILRuntimeTest.TestFramework.TestVectorClass();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
