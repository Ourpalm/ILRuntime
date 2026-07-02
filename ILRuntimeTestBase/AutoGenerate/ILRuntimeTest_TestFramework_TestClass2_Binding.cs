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
    unsafe class ILRuntimeTest_TestFramework_TestClass2_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestClass2);
            args = new Type[]{};
            method = type.GetMethod("VMethod1", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, VMethod1_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, VMethod1_0);
#endif
            args = new Type[]{typeof(System.Int32).MakeByRefType()};
            method = type.GetMethod("VMethod3", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, VMethod3_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, VMethod3_1);
#endif
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("AbMethod2", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, AbMethod2_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, AbMethod2_2);
#endif
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestClass2)};
            method = type.GetMethod("Register", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Register_3_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Register_3);
#endif
            args = new Type[]{};
            method = type.GetMethod("Alloc", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Alloc_4_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Alloc_4);
#endif
            args = new Type[]{};
            method = type.GetMethod("VMethod2", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, VMethod2_5_Neo);
#else
            app.RegisterCLRMethodRedirection(method, VMethod2_5);
#endif
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Add", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Add_6_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Add_6);
#endif


        }


#if ENABLE_NEO_MODE
        static void VMethod1_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            instance_of_this_method.VMethod1();
        }
#else
        static StackObject* VMethod1_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)typeof(ILRuntimeTest.TestFramework.TestClass2).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.VMethod1();

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void VMethod3_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 @arg = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            instance_of_this_method.VMethod3(ref @arg);
        }
#else
        static StackObject* VMethod3_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Int32 @arg = __intp.RetriveInt32(ptr_of_this_method, __mStack);

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)typeof(ILRuntimeTest.TestFramework.TestClass2).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);

            instance_of_this_method.VMethod3(ref @arg);

            ptr_of_this_method = __esp - 1;
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = @arg;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @arg;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @arg);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @arg;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @arg);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.Int32[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @arg;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = __esp - 2;
            __intp.Free(ptr_of_this_method);
            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void AbMethod2_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 @arg1 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            var result_of_this_method = instance_of_this_method.AbMethod2(@arg1);
            if (__retDst != null) *(float*)__retDst = (float)result_of_this_method;
        }
#else
        static StackObject* AbMethod2_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Int32 @arg1 = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)typeof(ILRuntimeTest.TestFramework.TestClass2).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.AbMethod2(@arg1);

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif

#if ENABLE_NEO_MODE
        static void Register_3_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.TestClass2 @obj = (ILRuntimeTest.TestFramework.TestClass2)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestFramework.TestClass2.Register(@obj);
        }
#else
        static StackObject* Register_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.TestClass2 @obj = (ILRuntimeTest.TestFramework.TestClass2)typeof(ILRuntimeTest.TestFramework.TestClass2).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestFramework.TestClass2.Register(@obj);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void Alloc_4_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            var result_of_this_method = ILRuntimeTest.TestFramework.TestClass2.Alloc();
            if (__retDst != null)
            {
                if (__retRefBase >= __mStack.Count)
                    __mStack.Add(result_of_this_method);
                else
                    __mStack[__retRefBase] = result_of_this_method;
                *(int*)__retDst = __retRefBase;
            }
        }
#else
        static StackObject* Alloc_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = __esp - 0;


            var result_of_this_method = ILRuntimeTest.TestFramework.TestClass2.Alloc();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void VMethod2_5_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.VMethod2();
            if (__retDst != null) *(int*)__retDst = result_of_this_method ? 1 : 0;
        }
#else
        static StackObject* VMethod2_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.TestClass2 instance_of_this_method = (ILRuntimeTest.TestFramework.TestClass2)typeof(ILRuntimeTest.TestFramework.TestClass2).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.VMethod2();

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }
#endif

#if ENABLE_NEO_MODE
        static void Add_6_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int32 @a = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Int32 @b = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            var result_of_this_method = ILRuntimeTest.TestFramework.TestClass2.Add(@a, @b);
            if (__retDst != null) *(int*)__retDst = (int)result_of_this_method;
        }
#else
        static StackObject* Add_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Int32 @b = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Int32 @a = ptr_of_this_method->Value;


            var result_of_this_method = ILRuntimeTest.TestFramework.TestClass2.Add(@a, @b);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif



    }
}
