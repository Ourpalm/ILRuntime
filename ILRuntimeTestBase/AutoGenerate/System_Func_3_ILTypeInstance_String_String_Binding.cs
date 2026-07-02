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
    unsafe class System_Func_3_ILTypeInstance_String_String_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.String, System.String>);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.String)};
            method = type.GetMethod("Invoke", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Invoke_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Invoke_0);
#endif


        }


#if ENABLE_NEO_MODE
        static void Invoke_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.String, System.String> instance_of_this_method = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.String, System.String>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntime.Runtime.Intepreter.ILTypeInstance @arg1 = (ILRuntime.Runtime.Intepreter.ILTypeInstance)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.String @arg2 = (System.String)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.Invoke(@arg1, @arg2);
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
        static StackObject* Invoke_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.String @arg2 = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            ILRuntime.Runtime.Intepreter.ILTypeInstance @arg1 = (ILRuntime.Runtime.Intepreter.ILTypeInstance)typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 3;
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.String, System.String> instance_of_this_method = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.String, System.String>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.String, System.String>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Invoke(@arg1, @arg2);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif



    }
}
