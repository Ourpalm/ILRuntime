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
    unsafe class ILRuntimeTest_TestFramework_Fixed64Vector2_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.Fixed64Vector2);

            field = type.GetField("x", flag);
            app.RegisterCLRFieldGetter(field, get_x_0);
            app.RegisterCLRFieldSetter(field, set_x_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_x_0, AssignFromStack_x_0);

            app.RegisterCLRMemberwiseClone(type, PerformMemberwiseClone);

            app.RegisterCLRCreateDefaultInstance(type, () => new ILRuntimeTest.TestFramework.Fixed64Vector2());

            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, AutoList __mStack, ref ILRuntimeTest.TestFramework.Fixed64Vector2 instance_of_this_method)
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
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.Fixed64Vector2[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }


        static object get_x_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.Fixed64Vector2)o).x;
        }

        static StackObject* CopyToStack_x_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.Fixed64Vector2)o).x;
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static void set_x_0(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.Fixed64Vector2 ins =(ILRuntimeTest.TestFramework.Fixed64Vector2)o;
            ins.x = (ILRuntimeTest.TestFramework.Fixed64)v;
            o = ins;
        }

        static StackObject* AssignFromStack_x_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.Fixed64 @x = new ILRuntimeTest.TestFramework.Fixed64();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64_Binding_Binder.ParseValue(ref @x, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @x = (ILRuntimeTest.TestFramework.Fixed64)typeof(ILRuntimeTest.TestFramework.Fixed64).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }
            ILRuntimeTest.TestFramework.Fixed64Vector2 ins =(ILRuntimeTest.TestFramework.Fixed64Vector2)o;
            ins.x = @x;
            o = ins;
            return ptr_of_this_method;
        }


        static object PerformMemberwiseClone(ref object o)
        {
            var ins = new ILRuntimeTest.TestFramework.Fixed64Vector2();
            ins = (ILRuntimeTest.TestFramework.Fixed64Vector2)o;
            return ins;
        }

        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @y = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 @x = ptr_of_this_method->Value;


            var result_of_this_method = new ILRuntimeTest.TestFramework.Fixed64Vector2(@x, @y);

            if(!isNewObj)
            {
                __ret--;
                if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64Vector2_Binding_Binder != null) {
                    ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64Vector2_Binding_Binder.WriteBackValue(__domain, __ret, __mStack, ref result_of_this_method);
                } else {
                    WriteBackInstance(__domain, __ret, __mStack, ref result_of_this_method);
                }
                return __ret;
            }

            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64Vector2_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_Fixed64Vector2_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
                return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }


    }
}
