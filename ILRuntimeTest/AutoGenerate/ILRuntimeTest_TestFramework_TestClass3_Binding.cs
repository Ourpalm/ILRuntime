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
    unsafe class ILRuntimeTest_TestFramework_TestClass3_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestClass3);
            args = new Type[]{typeof(System.Int32).MakeByRefType(), typeof(System.Int32)};
            method = type.GetMethod("getString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, getString_0);

            field = type.GetField("Struct", flag);
            app.RegisterCLRFieldGetter(field, get_Struct_0);
            app.RegisterCLRFieldSetter(field, set_Struct_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Struct_0, AssignFromStack_Struct_0);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* getString_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @length = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 @startIndex = __intp.RetriveInt32(ptr_of_this_method, __mStack);


            var result_of_this_method = ILRuntimeTest.TestFramework.TestClass3.getString(ref @startIndex, @length);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = @startIndex;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @startIndex;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @startIndex);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @startIndex;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @startIndex);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.Int32[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @startIndex;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_Struct_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestClass3)o).Struct;
        }

        static StackObject* CopyToStack_Struct_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.TestClass3)o).Struct;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Struct_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.TestClass3)o).Struct = (ILRuntimeTest.TestFramework.TestStruct)v;
        }

        static StackObject* AssignFromStack_Struct_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.TestStruct @Struct = (ILRuntimeTest.TestFramework.TestStruct)typeof(ILRuntimeTest.TestFramework.TestStruct).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            ((ILRuntimeTest.TestFramework.TestClass3)o).Struct = @Struct;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new ILRuntimeTest.TestFramework.TestClass3();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
