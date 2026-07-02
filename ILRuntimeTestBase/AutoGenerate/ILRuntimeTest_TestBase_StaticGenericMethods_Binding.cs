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
    unsafe class ILRuntimeTest_TestBase_StaticGenericMethods_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestBase.StaticGenericMethods);
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
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("ArrayMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Int32[][])))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, ArrayMethod_0_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, ArrayMethod_0);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32[])};
            if (genericMethods.TryGetValue("ArrayMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Int32[][][])))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, ArrayMethod_1_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, ArrayMethod_1);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Action)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, StaticMethod_2);
#endif
            args = new Type[]{typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass>)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_3_Neo);
#else
            app.RegisterCLRMethodRedirection(method, StaticMethod_3);
#endif
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("StaticMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Func<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_4_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, StaticMethod_4);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("StaticMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_5_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, StaticMethod_5);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Func<System.Threading.Tasks.Task>)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_6_Neo);
#else
            app.RegisterCLRMethodRedirection(method, StaticMethod_6);
#endif
            args = new Type[]{typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_7_Neo);
#else
            app.RegisterCLRMethodRedirection(method, StaticMethod_7);
#endif
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("StaticMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Func<System.Threading.Tasks.Task<System.Int32>>)))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_8_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, StaticMethod_8);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("StaticMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>)))
                    {
                        method = m.MakeGenericMethod(args);
#if ENABLE_NEO_MODE
                        app.RegisterCLRMethodRedirectionNeo(method, StaticMethod_9_Neo);
#else
                        app.RegisterCLRMethodRedirection(method, StaticMethod_9);
#endif

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.String), typeof(System.Collections.Generic.KeyValuePair<System.String, System.String[]>[])};
            method = type.GetMethod("Method", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Method_10_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Method_10);
#endif


        }


#if ENABLE_NEO_MODE
        static void ArrayMethod_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int32[][] @val = (System.Int32[][])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.ArrayMethod<System.Int32>(@val);
        }
#else
        static StackObject* ArrayMethod_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Int32[][] @val = (System.Int32[][])typeof(System.Int32[][]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.ArrayMethod<System.Int32>(@val);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void ArrayMethod_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int32[][][] @val = (System.Int32[][][])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.ArrayMethod<System.Int32[]>(@val);
        }
#else
        static StackObject* ArrayMethod_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Int32[][][] @val = (System.Int32[][][])typeof(System.Int32[][][]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.ArrayMethod<System.Int32[]>(@val);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Action @action = (System.Action)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@action);
        }
#else
        static StackObject* StaticMethod_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Action @action = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@action);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_3_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Action<ILRuntimeTest.TestBase.ExtensionClass> @action = (System.Action<ILRuntimeTest.TestBase.ExtensionClass>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@action);
        }
#else
        static StackObject* StaticMethod_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Action<ILRuntimeTest.TestBase.ExtensionClass> @action = (System.Action<ILRuntimeTest.TestBase.ExtensionClass>)typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@action);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_4_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Func<System.Int32> @func = (System.Func<System.Int32>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);
        }
#else
        static StackObject* StaticMethod_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Func<System.Int32> @func = (System.Func<System.Int32>)typeof(System.Func<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_5_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32> @action = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@action);
        }
#else
        static StackObject* StaticMethod_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32> @action = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>)typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@action);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_6_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Func<System.Threading.Tasks.Task> @func = (System.Func<System.Threading.Tasks.Task>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@func);
        }
#else
        static StackObject* StaticMethod_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Func<System.Threading.Tasks.Task> @func = (System.Func<System.Threading.Tasks.Task>)typeof(System.Func<System.Threading.Tasks.Task>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@func);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_7_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task> @func = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@func);
        }
#else
        static StackObject* StaticMethod_7(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task> @func = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>)typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@func);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_8_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Func<System.Threading.Tasks.Task<System.Int32>> @func = (System.Func<System.Threading.Tasks.Task<System.Int32>>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);
        }
#else
        static StackObject* StaticMethod_8(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Func<System.Threading.Tasks.Task<System.Int32>> @func = (System.Func<System.Threading.Tasks.Task<System.Int32>>)typeof(System.Func<System.Threading.Tasks.Task<System.Int32>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void StaticMethod_9_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>> @func = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);
        }
#else
        static StackObject* StaticMethod_9(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>> @func = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>)typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void Method_10_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.String @name = (System.String)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            System.Collections.Generic.KeyValuePair<System.String, System.String[]>[] @panels = (System.Collections.Generic.KeyValuePair<System.String, System.String[]>[])ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestBase.StaticGenericMethods.Method(@name, @panels);
        }
#else
        static StackObject* Method_10(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            System.Collections.Generic.KeyValuePair<System.String, System.String[]>[] @panels = (System.Collections.Generic.KeyValuePair<System.String, System.String[]>[])typeof(System.Collections.Generic.KeyValuePair<System.String, System.String[]>[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.String @name = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.Method(@name, @panels);

            return __ret;
        }
#endif



    }
}
