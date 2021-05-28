using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ILRuntimeTest_TestBase_StaticGenericMethods_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestBase.StaticGenericMethods);
            args = new Type[]{typeof(System.Action)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, StaticMethod_0);
            args = new Type[]{typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass>)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, StaticMethod_1);
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
            if (genericMethods.TryGetValue("StaticMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Func<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, StaticMethod_2);

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
                        app.RegisterCLRMethodRedirection(method, StaticMethod_3);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Func<System.Threading.Tasks.Task>)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, StaticMethod_4);
            args = new Type[]{typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>)};
            method = type.GetMethod("StaticMethod", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, StaticMethod_5);
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("StaticMethod", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Func<System.Threading.Tasks.Task<System.Int32>>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, StaticMethod_6);

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
                        app.RegisterCLRMethodRedirection(method, StaticMethod_7);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.String), typeof(System.Collections.Generic.KeyValuePair<System.String, System.String[]>[])};
            method = type.GetMethod("Method", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Method_8);


        }


        static StackObject* StaticMethod_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action @action = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@action);

            return __ret;
        }

        static StackObject* StaticMethod_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ILRuntimeTest.TestBase.ExtensionClass> @action = (System.Action<ILRuntimeTest.TestBase.ExtensionClass>)typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@action);

            return __ret;
        }

        static StackObject* StaticMethod_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Int32> @func = (System.Func<System.Int32>)typeof(System.Func<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);

            return __ret;
        }

        static StackObject* StaticMethod_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32> @action = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>)typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@action);

            return __ret;
        }

        static StackObject* StaticMethod_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Threading.Tasks.Task> @func = (System.Func<System.Threading.Tasks.Task>)typeof(System.Func<System.Threading.Tasks.Task>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@func);

            return __ret;
        }

        static StackObject* StaticMethod_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task> @func = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>)typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod(@func);

            return __ret;
        }

        static StackObject* StaticMethod_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Threading.Tasks.Task<System.Int32>> @func = (System.Func<System.Threading.Tasks.Task<System.Int32>>)typeof(System.Func<System.Threading.Tasks.Task<System.Int32>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);

            return __ret;
        }

        static StackObject* StaticMethod_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>> @func = (System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>)typeof(System.Func<ILRuntimeTest.TestBase.ExtensionClass, System.Threading.Tasks.Task<System.Int32>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.StaticMethod<System.Int32>(@func);

            return __ret;
        }

        static StackObject* Method_8(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.KeyValuePair<System.String, System.String[]>[] @panels = (System.Collections.Generic.KeyValuePair<System.String, System.String[]>[])typeof(System.Collections.Generic.KeyValuePair<System.String, System.String[]>[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @name = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.StaticGenericMethods.Method(@name, @panels);

            return __ret;
        }



    }
}
