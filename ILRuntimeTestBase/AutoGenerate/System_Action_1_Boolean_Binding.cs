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
    unsafe class System_Action_1_Boolean_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Action<System.Boolean>);
            args = new Type[]{typeof(System.Boolean)};
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
            System.Action<System.Boolean> instance_of_this_method = (System.Action<System.Boolean>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Boolean @obj = ILIntepreter.ReadNeoBoolean(__frameBase, ref __curPrim);
            instance_of_this_method.Invoke(@obj);
        }
#else
        static StackObject* Invoke_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Boolean @obj = ptr_of_this_method->Value == 1;

            ptr_of_this_method = __esp - 2;
            System.Action<System.Boolean> instance_of_this_method = (System.Action<System.Boolean>)typeof(System.Action<System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Invoke(@obj);

            return __ret;
        }
#endif



    }
}
