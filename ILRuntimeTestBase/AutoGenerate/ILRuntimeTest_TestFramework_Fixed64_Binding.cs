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
    unsafe class ILRuntimeTest_TestFramework_Fixed64_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.Fixed64);
            args = new Type[]{};
            method = type.GetMethod("get_RawValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_RawValue_0);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.Fixed64), typeof(ILRuntimeTest.TestFramework.Fixed64)};
            method = type.GetMethod("op_LessThan", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_LessThan_1);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.Fixed64), typeof(ILRuntimeTest.TestFramework.Fixed64)};
            method = type.GetMethod("op_GreaterThan", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_GreaterThan_2);

            app.RegisterCLRCreateDefaultInstance(type, () => new ILRuntimeTest.TestFramework.Fixed64());

            args = new Type[]{typeof(System.Int64)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, AutoList __mStack, ref ILRuntimeTest.TestFramework.Fixed64 instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.Fixed64[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }

        static StackObject* get_RawValue_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.Fixed64 instance_of_this_method = new ILRuntimeTest.TestFramework.Fixed64();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.ParseValue(ref instance_of_this_method, __intp, ptr_of_this_method, __mStack, false);
            } else {
                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
                instance_of_this_method = (ILRuntimeTest.TestFramework.Fixed64)typeof(ILRuntimeTest.TestFramework.Fixed64).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }

            var result_of_this_method = instance_of_this_method.RawValue;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            } else {
                WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            }

            __intp.Free(ptr_of_this_method);
            __ret->ObjectType = ObjectTypes.Long;
            *(long*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* op_LessThan_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.Fixed64 @y = new ILRuntimeTest.TestFramework.Fixed64();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.ParseValue(ref @y, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @y = (ILRuntimeTest.TestFramework.Fixed64)typeof(ILRuntimeTest.TestFramework.Fixed64).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.Fixed64 @x = new ILRuntimeTest.TestFramework.Fixed64();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.ParseValue(ref @x, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @x = (ILRuntimeTest.TestFramework.Fixed64)typeof(ILRuntimeTest.TestFramework.Fixed64).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }


            var result_of_this_method = x < y;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* op_GreaterThan_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.Fixed64 @y = new ILRuntimeTest.TestFramework.Fixed64();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.ParseValue(ref @y, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @y = (ILRuntimeTest.TestFramework.Fixed64)typeof(ILRuntimeTest.TestFramework.Fixed64).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.Fixed64 @x = new ILRuntimeTest.TestFramework.Fixed64();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.ParseValue(ref @x, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @x = (ILRuntimeTest.TestFramework.Fixed64)typeof(ILRuntimeTest.TestFramework.Fixed64).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }


            var result_of_this_method = x > y;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 @value = *(long*)&ptr_of_this_method->Value;


            var result_of_this_method = new ILRuntimeTest.TestFramework.Fixed64(@value);

            if(!isNewObj)
            {
                __ret--;
                if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                    ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.WriteBackValue(__domain, __ret, __mStack, ref result_of_this_method);
                } else {
                    WriteBackInstance(__domain, __ret, __mStack, ref result_of_this_method);
                }
                return __ret;
            }

            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
                return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }


    }
}
