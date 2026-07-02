using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime;
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
    [ILRuntimePatchIgnore]
    unsafe class ILRuntimeTest_TestFramework_JInt_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.JInt);
            MethodInfo[] methods = type.GetMethods(flag).Where(t => !t.IsGenericMethod).ToArray();
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.JInt)};
            method = type.GetMethod("op_Increment", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, op_Increment_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, op_Increment_0);
#endif
            args = new Type[]{typeof(System.Int32)};
            method = methods.Where(t => t.Name.Equals("op_Implicit") && t.ReturnType == typeof(ILRuntimeTest.TestFramework.JInt) && t.CheckMethodParams(args)).Single();
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, op_Implicit_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, op_Implicit_1);
#endif
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.JInt), typeof(ILRuntimeTest.TestFramework.JInt)};
            method = type.GetMethod("op_Inequality", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, op_Inequality_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, op_Inequality_2);
#endif
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.JInt)};
            method = type.GetMethod("op_Decrement", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, op_Decrement_3_Neo);
#else
            app.RegisterCLRMethodRedirection(method, op_Decrement_3);
#endif
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.JInt), typeof(System.Int32)};
            method = type.GetMethod("op_Subtraction", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, op_Subtraction_4_Neo);
#else
            app.RegisterCLRMethodRedirection(method, op_Subtraction_4);
#endif
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.JInt)};
            method = methods.Where(t => t.Name.Equals("op_Implicit") && t.ReturnType == typeof(System.Int32) && t.CheckMethodParams(args)).Single();
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, op_Implicit_5_Neo);
#else
            app.RegisterCLRMethodRedirection(method, op_Implicit_5);
#endif

            app.RegisterCLRCreateDefaultInstance(type, () => new ILRuntimeTest.TestFramework.JInt());


        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, AutoList __mStack, ref ILRuntimeTest.TestFramework.JInt instance_of_this_method)
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
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.JInt[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }

#if ENABLE_NEO_MODE
        static void op_Increment_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.JInt @a = default(ILRuntimeTest.TestFramework.JInt);
            // TODO: ByRef or unsupported ValueType parameters in Neo
            var result_of_this_method = ++@a;
            // TODO: CLR value type return in reflection fallback: Step 13
        }
#else
        static StackObject* op_Increment_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.JInt @a = (ILRuntimeTest.TestFramework.JInt)typeof(ILRuntimeTest.TestFramework.JInt).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = ++a;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void op_Implicit_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int32 @val = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            var result_of_this_method = (ILRuntimeTest.TestFramework.JInt)@val;
            // TODO: CLR value type return in reflection fallback: Step 13
        }
#else
        static StackObject* op_Implicit_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Int32 @val = ptr_of_this_method->Value;


            var result_of_this_method = (ILRuntimeTest.TestFramework.JInt)val;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void op_Inequality_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.JInt @a = default(ILRuntimeTest.TestFramework.JInt);
            // TODO: ByRef or unsupported ValueType parameters in Neo
            ILRuntimeTest.TestFramework.JInt @b = default(ILRuntimeTest.TestFramework.JInt);
            // TODO: ByRef or unsupported ValueType parameters in Neo
            var result_of_this_method = @a != @b;
            if (__retDst != null) *(int*)__retDst = result_of_this_method ? 1 : 0;
        }
#else
        static StackObject* op_Inequality_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.JInt @b = (ILRuntimeTest.TestFramework.JInt)typeof(ILRuntimeTest.TestFramework.JInt).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.JInt @a = (ILRuntimeTest.TestFramework.JInt)typeof(ILRuntimeTest.TestFramework.JInt).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = a != b;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }
#endif

#if ENABLE_NEO_MODE
        static void op_Decrement_3_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.JInt @a = default(ILRuntimeTest.TestFramework.JInt);
            // TODO: ByRef or unsupported ValueType parameters in Neo
            var result_of_this_method = --@a;
            // TODO: CLR value type return in reflection fallback: Step 13
        }
#else
        static StackObject* op_Decrement_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.JInt @a = (ILRuntimeTest.TestFramework.JInt)typeof(ILRuntimeTest.TestFramework.JInt).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = --a;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void op_Subtraction_4_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.JInt @a = default(ILRuntimeTest.TestFramework.JInt);
            // TODO: ByRef or unsupported ValueType parameters in Neo
            System.Int32 @b = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            var result_of_this_method = @a - @b;
            // TODO: CLR value type return in reflection fallback: Step 13
        }
#else
        static StackObject* op_Subtraction_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Int32 @b = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.JInt @a = (ILRuntimeTest.TestFramework.JInt)typeof(ILRuntimeTest.TestFramework.JInt).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = a - b;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void op_Implicit_5_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.JInt @val = default(ILRuntimeTest.TestFramework.JInt);
            // TODO: ByRef or unsupported ValueType parameters in Neo
            var result_of_this_method = (System.Int32)@val;
            if (__retDst != null) *(int*)__retDst = (int)result_of_this_method;
        }
#else
        static StackObject* op_Implicit_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.JInt @val = (ILRuntimeTest.TestFramework.JInt)typeof(ILRuntimeTest.TestFramework.JInt).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = (System.Int32)val;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif



    }
}
