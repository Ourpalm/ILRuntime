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
    unsafe class ILRuntimeTest_TestFramework_TestVectorStruct2_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestVectorStruct2);

            field = type.GetField("Vector", flag);
            app.RegisterCLRFieldGetter(field, get_Vector_0);
            app.RegisterCLRFieldSetter(field, set_Vector_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Vector_0, AssignFromStack_Vector_0);

            app.RegisterCLRMemberwiseClone(type, PerformMemberwiseClone);

            app.RegisterCLRCreateDefaultInstance(type, () => new ILRuntimeTest.TestFramework.TestVectorStruct2());


        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref ILRuntimeTest.TestFramework.TestVectorStruct2 instance_of_this_method)
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
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.TestVectorStruct2[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }


        static object get_Vector_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVectorStruct2)o).Vector;
        }

        static StackObject* CopyToStack_Vector_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.TestVectorStruct2)o).Vector;
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static void set_Vector_0(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.TestVectorStruct2 ins =(ILRuntimeTest.TestFramework.TestVectorStruct2)o;
            ins.Vector = (ILRuntimeTest.TestFramework.TestVector3)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Vector_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.TestVector3 @Vector = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref @Vector, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @Vector = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }
            ILRuntimeTest.TestFramework.TestVectorStruct2 ins =(ILRuntimeTest.TestFramework.TestVectorStruct2)o;
            ins.Vector = @Vector;
            o = ins;
            return ptr_of_this_method;
        }


        static object PerformMemberwiseClone(ref object o)
        {
            var ins = new ILRuntimeTest.TestFramework.TestVectorStruct2();
            ins = (ILRuntimeTest.TestFramework.TestVectorStruct2)o;
            return ins;
        }


    }
}
