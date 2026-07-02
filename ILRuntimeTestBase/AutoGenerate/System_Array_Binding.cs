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
    unsafe class System_Array_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Array);
            args = new Type[]{typeof(System.Type), typeof(System.Int32)};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, CreateInstance_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, CreateInstance_0);
#endif
            args = new Type[]{typeof(System.Object), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, SetValue_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, SetValue_1);
#endif
            args = new Type[]{};
            method = type.GetMethod("get_Length", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, get_Length_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, get_Length_2);
#endif
            args = new Type[]{};
            method = type.GetMethod("GetEnumerator", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, GetEnumerator_3_Neo);
#else
            app.RegisterCLRMethodRedirection(method, GetEnumerator_3);
#endif
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(System.Collections.Generic.KeyValuePair<System.String, System.String[]>)};
            if (genericMethods.TryGetValue("Empty", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.KeyValuePair<System.String, System.String[]>[])))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, Empty_4_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, Empty_4);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Byte)};
            if (genericMethods.TryGetValue("IndexOf", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Int32), typeof(System.Byte[]), typeof(System.Byte)))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, IndexOf_5_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, IndexOf_5);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("IndexOf", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Int32), typeof(System.Int32[]), typeof(System.Int32)))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, IndexOf_6_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, IndexOf_6);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Collections.Generic.List<System.Int32[]>)};
            if (genericMethods.TryGetValue("IndexOf", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Int32), typeof(System.Collections.Generic.List<System.Int32[]>[]), typeof(System.Collections.Generic.List<System.Int32[]>)))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, IndexOf_7_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, IndexOf_7);
#endif

                        break;
                    }
                }
            }


        }


#if ENABLE_NEO_MODE
        static void CreateInstance_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Type @elementType = (System.Type)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 @length = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            var result_of_this_method = System.Array.CreateInstance(@elementType, @length);
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
        static StackObject* CreateInstance_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Int32 @length = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Type @elementType = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Array.CreateInstance(@elementType, @length);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void SetValue_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Array instance_of_this_method = (System.Array)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Object @value = (System.Object)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 @index = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            instance_of_this_method.SetValue(@value, @index);
        }
#else
        static StackObject* SetValue_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.Int32 @index = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Object @value = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 3;
            System.Array instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(@value, @index);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void get_Length_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Array instance_of_this_method = (System.Array)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.Length;
            if (__retDst != null) *(int*)__retDst = (int)result_of_this_method;
        }
#else
        static StackObject* get_Length_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Array instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Length;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif

#if ENABLE_NEO_MODE
        static void GetEnumerator_3_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Array instance_of_this_method = (System.Array)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = instance_of_this_method.GetEnumerator();
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
        static StackObject* GetEnumerator_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Array instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetEnumerator();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void Empty_4_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            var result_of_this_method = System.Array.Empty<System.Collections.Generic.KeyValuePair<System.String, System.String[]>>();
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
        static StackObject* Empty_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = __esp - 0;


            var result_of_this_method = System.Array.Empty<System.Collections.Generic.KeyValuePair<System.String, System.String[]>>();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void IndexOf_5_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Byte[] @array = (System.Byte[])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Byte @value = ILIntepreter.ReadNeoUInt8(__frameBase, ref __curPrim);
            var result_of_this_method = System.Array.IndexOf<System.Byte>(@array, @value);
            if (__retDst != null) *(int*)__retDst = (int)result_of_this_method;
        }
#else
        static StackObject* IndexOf_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Byte @value = (byte)ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Byte[] @array = (System.Byte[])typeof(System.Byte[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Array.IndexOf<System.Byte>(@array, @value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif

#if ENABLE_NEO_MODE
        static void IndexOf_6_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int32[] @array = (System.Int32[])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Int32 @value = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            var result_of_this_method = System.Array.IndexOf<System.Int32>(@array, @value);
            if (__retDst != null) *(int*)__retDst = (int)result_of_this_method;
        }
#else
        static StackObject* IndexOf_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Int32 @value = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Int32[] @array = (System.Int32[])typeof(System.Int32[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Array.IndexOf<System.Int32>(@array, @value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif

#if ENABLE_NEO_MODE
        static void IndexOf_7_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Collections.Generic.List<System.Int32[]>[] @array = (System.Collections.Generic.List<System.Int32[]>[])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Collections.Generic.List<System.Int32[]> @value = (System.Collections.Generic.List<System.Int32[]>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            var result_of_this_method = System.Array.IndexOf<System.Collections.Generic.List<System.Int32[]>>(@array, @value);
            if (__retDst != null) *(int*)__retDst = (int)result_of_this_method;
        }
#else
        static StackObject* IndexOf_7(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Collections.Generic.List<System.Int32[]> @value = (System.Collections.Generic.List<System.Int32[]>)typeof(System.Collections.Generic.List<System.Int32[]>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.Collections.Generic.List<System.Int32[]>[] @array = (System.Collections.Generic.List<System.Int32[]>[])typeof(System.Collections.Generic.List<System.Int32[]>[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Array.IndexOf<System.Collections.Generic.List<System.Int32[]>>(@array, @value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }
#endif



    }
}
