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
    unsafe class System_Action_2_Int32_ILTypeInstance_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Action<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>);
            args = new Type[]{typeof(System.Int32), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
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
            System.Action<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (System.Action<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 @arg1 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            ILRuntime.Runtime.Intepreter.ILTypeInstance @arg2 = (ILRuntime.Runtime.Intepreter.ILTypeInstance)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            instance_of_this_method.Invoke(@arg1, @arg2);
        }
#else
        static StackObject* Invoke_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            ILRuntime.Runtime.Intepreter.ILTypeInstance @arg2 = (ILRuntime.Runtime.Intepreter.ILTypeInstance)typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.Int32 @arg1 = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 3;
            System.Action<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (System.Action<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Action<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Invoke(@arg1, @arg2);

            return __ret;
        }
#endif



    }
}
