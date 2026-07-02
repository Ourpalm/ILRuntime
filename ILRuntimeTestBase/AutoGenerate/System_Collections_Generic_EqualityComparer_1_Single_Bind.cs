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
    unsafe class System_Collections_Generic_EqualityComparer_1_Single_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Collections.Generic.EqualityComparer<System.Single>);
            args = new Type[]{};
            method = type.GetMethod("get_Default", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_Default_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_Default_0);
#endif
            args = new Type[]{typeof(System.Single), typeof(System.Single)};
            method = type.GetMethod("Equals", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Equals_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Equals_1);
#endif


        }


#if ENABLE_NEO_MODE
        static void get_Default_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            var result_of_this_method = System.Collections.Generic.EqualityComparer<System.Single>.Default;
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
        static StackObject* get_Default_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = __esp - 0;


            var result_of_this_method = System.Collections.Generic.EqualityComparer<System.Single>.Default;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void Equals_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Collections.Generic.EqualityComparer<System.Single> instance_of_this_method = (System.Collections.Generic.EqualityComparer<System.Single>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Single @x = ILIntepreter.ReadNeoFloat(__frameBase, ref __curPrim);
            System.Single @y = ILIntepreter.ReadNeoFloat(__frameBase, ref __curPrim);
            var result_of_this_method = instance_of_this_method.Equals(@x, @y);
            if (__retDst != null) *(int*)__retDst = result_of_this_method ? 1 : 0;
        }
#else
        static StackObject* Equals_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.Single @y = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Single @x = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 3;
            System.Collections.Generic.EqualityComparer<System.Single> instance_of_this_method = (System.Collections.Generic.EqualityComparer<System.Single>)typeof(System.Collections.Generic.EqualityComparer<System.Single>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Equals(@x, @y);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }
#endif



    }
}
