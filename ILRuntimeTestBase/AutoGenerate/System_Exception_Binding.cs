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
    unsafe class System_Exception_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Exception);
            args = new Type[]{};
            method = type.GetMethod("get_Message", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_Message_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_Message_0);
#endif
            args = new Type[]{};
            method = type.GetMethod("get_Data", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_Data_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_Data_1);
#endif
            args = new Type[]{};
            method = type.GetMethod("get_StackTrace", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_StackTrace_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_StackTrace_2);
#endif

            args = new Type[]{typeof(System.String)};
            method = type.GetConstructor(flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Ctor_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Ctor_0);
#endif
            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Ctor_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Ctor_1);
#endif

        }


#if ENABLE_NEO_MODE
        static void get_Message_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Exception instance_of_this_method = (System.Exception)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.Message;
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
        static StackObject* get_Message_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Exception instance_of_this_method = (System.Exception)typeof(System.Exception).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Message;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void get_Data_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Exception instance_of_this_method = (System.Exception)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.Data;
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
        static StackObject* get_Data_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Exception instance_of_this_method = (System.Exception)typeof(System.Exception).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Data;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void get_StackTrace_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Exception instance_of_this_method = (System.Exception)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.StackTrace;
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
        static StackObject* get_StackTrace_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Exception instance_of_this_method = (System.Exception)typeof(System.Exception).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.StackTrace;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif


#if ENABLE_NEO_MODE
        static void Ctor_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            if (isNewObj)
            {
                __curPrim += 4; // Skip retRefBase
            }
            else
            {
                // TODO: Constructor binding for non-newObj (e.g. value type init) in Neo
            }
            System.String @message = (System.String)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Exception result_of_this_method = new System.Exception(@message);
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
        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;
            ptr_of_this_method = __esp - 1;
            System.String @message = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            if(!isNewObj) 
            {
                ptr_of_this_method = __esp - 2;
                System.Exception __this = (System.Exception)typeof(System.Exception).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));
                __intp.Free(ptr_of_this_method);
                if(__this is CrossBindingAdaptorType)
                    return __esp - 2;
            }
            var result_of_this_method = new System.Exception(@message);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void Ctor_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            if (isNewObj)
            {
                __curPrim += 4; // Skip retRefBase
            }
            else
            {
                // TODO: Constructor binding for non-newObj (e.g. value type init) in Neo
            }
            System.Exception result_of_this_method = new System.Exception();
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
        static StackObject* Ctor_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = __esp - 0;

            if(!isNewObj) 
            {
                StackObject* ptr_of_this_method = __esp - 1;
                System.Exception __this = (System.Exception)typeof(System.Exception).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));
                __intp.Free(ptr_of_this_method);
                if(__this is CrossBindingAdaptorType)
                    return __esp - 1;
            }
            var result_of_this_method = new System.Exception();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif


    }
}
