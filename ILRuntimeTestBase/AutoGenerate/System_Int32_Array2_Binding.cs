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
    unsafe class System_Int32_Array2_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Int32[,]);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Set", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Set_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Set_0);
#endif

            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetConstructor(flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Ctor_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Ctor_0);
#endif

        }


#if ENABLE_NEO_MODE
        static void Set_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int32[,] instance_of_this_method = (System.Int32[,])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 a1 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Int32 a2 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Int32 a3 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            instance_of_this_method[a1, a2] = a3;
        }
#else
        static StackObject* Set_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 4;

            ptr_of_this_method = __esp - 1;
            System.Int32 a3 = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Int32 a2 = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 3;
            System.Int32 a1 = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 4;
            System.Int32[,] instance_of_this_method = (System.Int32[,])typeof(System.Int32[,]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method[a1, a2] = a3;

            return __ret;
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
            System.Int32 a1 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Int32 a2 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Int32[,] result_of_this_method = new System.Int32[a1, a2];
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
            StackObject* __ret = __esp - 2;
            ptr_of_this_method = __esp - 1;
            System.Int32 a2 = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Int32 a1 = ptr_of_this_method->Value;


            if(!isNewObj) 
            {
                ptr_of_this_method = __esp - 3;
                System.Int32[,] __this = (System.Int32[,])typeof(System.Int32[,]).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));
                __intp.Free(ptr_of_this_method);
                if(__this is CrossBindingAdaptorType)
                    return __esp - 3;
            }
            var result_of_this_method = new System.Int32[a1, a2];

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif


    }
}
