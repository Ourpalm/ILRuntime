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
    unsafe class ILRuntimeTest_TestFramework_ClassInheritanceTest2_1_ILRuntimeTest_TestFramework_ClassInheritanceTest2Adaptor_Binding_Adaptor_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest2<ILRuntimeTest.TestFramework.ClassInheritanceTest2Adaptor.Adaptor>);
            args = new Type[]{};
            method = type.GetMethod("TestVirtual", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TestVirtual_0);


        }


        static StackObject* TestVirtual_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.ClassInheritanceTest2<ILRuntimeTest.TestFramework.ClassInheritanceTest2Adaptor.Adaptor> instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest2<ILRuntimeTest.TestFramework.ClassInheritanceTest2Adaptor.Adaptor>)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest2<ILRuntimeTest.TestFramework.ClassInheritanceTest2Adaptor.Adaptor>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestVirtual();

            return __ret;
        }



    }
}
