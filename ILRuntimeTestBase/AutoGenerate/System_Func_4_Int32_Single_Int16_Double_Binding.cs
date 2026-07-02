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
    unsafe class System_Func_4_Int32_Single_Int16_Double_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Func<System.Int32, System.Single, System.Int16, System.Double>);
            args = new Type[]{typeof(System.Int32), typeof(System.Single), typeof(System.Int16)};
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
            System.Func<System.Int32, System.Single, System.Int16, System.Double> instance_of_this_method = (System.Func<System.Int32, System.Single, System.Int16, System.Double>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 @arg1 = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Single @arg2 = ILIntepreter.ReadNeoFloat(__frameBase, ref __curPrim);
            System.Int16 @arg3 = ILIntepreter.ReadNeoInt16(__frameBase, ref __curPrim);
            var result_of_this_method = instance_of_this_method.Invoke(@arg1, @arg2, @arg3);
            if (__retDst != null) *(double*)__retDst = (double)result_of_this_method;
        }
#else
        static StackObject* Invoke_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 4;

            ptr_of_this_method = __esp - 1;
            System.Int16 @arg3 = (short)ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Single @arg2 = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 3;
            System.Int32 @arg1 = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 4;
            System.Func<System.Int32, System.Single, System.Int16, System.Double> instance_of_this_method = (System.Func<System.Int32, System.Single, System.Int16, System.Double>)typeof(System.Func<System.Int32, System.Single, System.Int16, System.Double>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Invoke(@arg1, @arg2, @arg3);

            __ret->ObjectType = ObjectTypes.Double;
            *(double*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif



    }
}
