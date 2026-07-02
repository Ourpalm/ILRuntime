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
    unsafe class System_Reflection_MemberInfo_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Reflection.MemberInfo);
            args = new Type[]{};
            method = type.GetMethod("get_Name", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_Name_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_Name_0);
#endif
            args = new Type[]{typeof(System.Type), typeof(System.Boolean)};
            method = type.GetMethod("GetCustomAttributes", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, GetCustomAttributes_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, GetCustomAttributes_1);
#endif
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("GetCustomAttributes", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, GetCustomAttributes_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, GetCustomAttributes_2);
#endif
            args = new Type[]{typeof(System.Type), typeof(System.Boolean)};
            method = type.GetMethod("IsDefined", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, IsDefined_3_Neo);
#else
            app.RegisterCLRMethodRedirection(method, IsDefined_3);
#endif
            args = new Type[]{};
            method = type.GetMethod("get_DeclaringType", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_DeclaringType_4_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_DeclaringType_4);
#endif


        }


#if ENABLE_NEO_MODE
        static void get_Name_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.Name;
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
        static StackObject* get_Name_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)typeof(System.Reflection.MemberInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Name;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void GetCustomAttributes_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Type @attributeType = (System.Type)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Boolean @inherit = ILIntepreter.ReadNeoBoolean(__frameBase, ref __curPrim);
            var result_of_this_method = instance_of_this_method.GetCustomAttributes(@attributeType, @inherit);
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
        static StackObject* GetCustomAttributes_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.Boolean @inherit = ptr_of_this_method->Value == 1;

            ptr_of_this_method = __esp - 2;
            System.Type @attributeType = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 3;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)typeof(System.Reflection.MemberInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetCustomAttributes(@attributeType, @inherit);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void GetCustomAttributes_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Boolean @inherit = ILIntepreter.ReadNeoBoolean(__frameBase, ref __curPrim);
            var result_of_this_method = instance_of_this_method.GetCustomAttributes(@inherit);
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
        static StackObject* GetCustomAttributes_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Boolean @inherit = ptr_of_this_method->Value == 1;

            ptr_of_this_method = __esp - 2;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)typeof(System.Reflection.MemberInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetCustomAttributes(@inherit);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void IsDefined_3_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Type @attributeType = (System.Type)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Boolean @inherit = ILIntepreter.ReadNeoBoolean(__frameBase, ref __curPrim);
            var result_of_this_method = instance_of_this_method.IsDefined(@attributeType, @inherit);
            if (__retDst != null) *(int*)__retDst = result_of_this_method ? 1 : 0;
        }
#else
        static StackObject* IsDefined_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.Boolean @inherit = ptr_of_this_method->Value == 1;

            ptr_of_this_method = __esp - 2;
            System.Type @attributeType = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 3;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)typeof(System.Reflection.MemberInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.IsDefined(@attributeType, @inherit);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }
#endif

#if ENABLE_NEO_MODE
        static void get_DeclaringType_4_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.DeclaringType;
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
        static StackObject* get_DeclaringType_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Reflection.MemberInfo instance_of_this_method = (System.Reflection.MemberInfo)typeof(System.Reflection.MemberInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.DeclaringType;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif



    }
}
