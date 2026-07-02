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
    unsafe class System_Collections_Generic_Dictionary_2_Int64_Int32_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Collections.Generic.Dictionary<System.Int64, System.Int32>);
            args = new Type[]{typeof(System.Int64), typeof(System.Int32)};
            method = type.GetMethod("set_Item", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, set_Item_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, set_Item_0);
#endif
            args = new Type[]{typeof(System.Int64), typeof(System.Int32).MakeByRefType()};
            method = type.GetMethod("TryGetValue", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, TryGetValue_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, TryGetValue_1);
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
        static void set_Item_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Collections.Generic.Dictionary<System.Int64, System.Int32> instance_of_this_method = (System.Collections.Generic.Dictionary<System.Int64, System.Int32>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int64 @key = ILIntepreter.ReadNeoInt64(__frameBase, ref __curPrim);
            System.Int32 @value = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            instance_of_this_method[key] = value;
        }
#else
        static StackObject* set_Item_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.Int32 @value = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Int64 @key = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 3;
            System.Collections.Generic.Dictionary<System.Int64, System.Int32> instance_of_this_method = (System.Collections.Generic.Dictionary<System.Int64, System.Int32>)typeof(System.Collections.Generic.Dictionary<System.Int64, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method[key] = value;

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void TryGetValue_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Collections.Generic.Dictionary<System.Int64, System.Int32> instance_of_this_method = (System.Collections.Generic.Dictionary<System.Int64, System.Int32>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int64 @key = ILIntepreter.ReadNeoInt64(__frameBase, ref __curPrim);
            System.Int32 @value = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            var result_of_this_method = instance_of_this_method.TryGetValue(@key, out @value);
            if (__retDst != null) *(int*)__retDst = result_of_this_method ? 1 : 0;
        }
#else
        static StackObject* TryGetValue_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.Int32 @value = __intp.RetriveInt32(ptr_of_this_method, __mStack);

            ptr_of_this_method = __esp - 2;
            System.Int64 @key = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 3;
            System.Collections.Generic.Dictionary<System.Int64, System.Int32> instance_of_this_method = (System.Collections.Generic.Dictionary<System.Int64, System.Int32>)typeof(System.Collections.Generic.Dictionary<System.Int64, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);

            var result_of_this_method = instance_of_this_method.TryGetValue(@key, out @value);

            ptr_of_this_method = __esp - 1;
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = @value;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @value;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @value);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @value;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @value);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.Int32[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @value;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = __esp - 2;
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = __esp - 3;
            __intp.Free(ptr_of_this_method);
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
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
            System.Collections.Generic.Dictionary<System.Int64, System.Int32> result_of_this_method = new System.Collections.Generic.Dictionary<System.Int64, System.Int32>();
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
                System.Collections.Generic.Dictionary<System.Int64, System.Int32> __this = (System.Collections.Generic.Dictionary<System.Int64, System.Int32>)typeof(System.Collections.Generic.Dictionary<System.Int64, System.Int32>).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));
                __intp.Free(ptr_of_this_method);
                if(__this is CrossBindingAdaptorType)
                    return __esp - 1;
            }
            var result_of_this_method = new System.Collections.Generic.Dictionary<System.Int64, System.Int32>();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif


    }
}
