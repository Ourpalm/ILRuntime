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
    unsafe class System_Collections_Generic_Dictionary_2_UInt32_ILTypeInstance_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>);
            args = new Type[]{typeof(System.UInt32), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            method = type.GetMethod("Add", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Add_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Add_0);
#endif
            args = new Type[]{};
            method = type.GetMethod("GetEnumerator", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, GetEnumerator_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, GetEnumerator_1);
#endif

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Ctor_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Ctor_0);
#endif

        }


#if ENABLE_NEO_MODE
        static void Add_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.UInt32 @key = (uint)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            ILRuntime.Runtime.Intepreter.ILTypeInstance @value = (ILRuntime.Runtime.Intepreter.ILTypeInstance)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            instance_of_this_method.Add(@key, @value);
        }
#else
        static StackObject* Add_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            ILRuntime.Runtime.Intepreter.ILTypeInstance @value = (ILRuntime.Runtime.Intepreter.ILTypeInstance)typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.UInt32 @key = (uint)ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 3;
            System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Add(@key, @value);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void GetEnumerator_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.GetEnumerator();
            // TODO: CLR value type return in reflection fallback: Step 13
        }
#else
        static StackObject* GetEnumerator_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetEnumerator();

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
            System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> result_of_this_method = new System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>();
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
            StackObject* __ret = __esp - 0;

            if(!isNewObj) 
            {
                StackObject* ptr_of_this_method = __esp - 1;
                System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> __this = (System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));
                __intp.Free(ptr_of_this_method);
                if(__this is CrossBindingAdaptorType)
                    return __esp - 1;
            }
            var result_of_this_method = new System.Collections.Generic.Dictionary<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif


    }
}
