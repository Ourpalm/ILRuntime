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
    unsafe class System_Collections_Generic_KeyValuePair_2_UInt32_ILTypeInstance_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>);
            args = new Type[]{};
            method = type.GetMethod("get_Key", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_Key_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_Key_0);
#endif

            app.RegisterCLRCreateDefaultInstance(type, () => new System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>());


        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, AutoList __mStack, ref System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }

#if ENABLE_NEO_MODE
        static void get_Key_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = default(System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>);
            // TODO: ValueType instance in Neo
            var result_of_this_method = instance_of_this_method.Key;
            if (__retDst != null) *(uint*)__retDst = (uint)result_of_this_method;
        }
#else
        static StackObject* get_Key_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = new System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_System_Collections_Generic_KeyValuePair_2_UInt32_ILTypeInstance_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_System_Collections_Generic_KeyValuePair_2_UInt32_ILTypeInstance_Binding_Binder.ParseValue(ref instance_of_this_method, __intp, ptr_of_this_method, __mStack, false);
            } else {
                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
                instance_of_this_method = (System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.KeyValuePair<System.UInt32, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)16);
            }

            var result_of_this_method = instance_of_this_method.Key;

            ptr_of_this_method = __esp - 1;
            if (ILRuntime.Runtime.Generated.CLRBindings.s_System_Collections_Generic_KeyValuePair_2_UInt32_ILTypeInstance_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_System_Collections_Generic_KeyValuePair_2_UInt32_ILTypeInstance_Binding_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            } else {
                WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            }

            __intp.Free(ptr_of_this_method);
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = (int)result_of_this_method;
            return __ret + 1;
        }
#endif



    }
}
