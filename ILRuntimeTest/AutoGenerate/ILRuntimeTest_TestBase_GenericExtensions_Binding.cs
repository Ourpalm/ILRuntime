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
    unsafe class ILRuntimeTest_TestBase_GenericExtensions_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestBase.GenericExtensions);
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
            args = new Type[]{typeof(ILRuntimeTest.TestBase.ExtensionClass)};
            if (genericMethods.TryGetValue("Method1", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.Action<System.Object>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method1_0);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.ExtensionClass)};
            if (genericMethods.TryGetValue("Method1", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Object>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method1_1);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.Action<System.Exception>)};
            method = type.GetMethod("Method2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Method2_2);
            args = new Type[]{typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Exception>)};
            method = type.GetMethod("Method2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Method2_3);
            args = new Type[]{typeof(System.ArgumentException)};
            if (genericMethods.TryGetValue("Method2", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.Action<System.ArgumentException>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method2_4);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.ArgumentException)};
            if (genericMethods.TryGetValue("Method2", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.ArgumentException>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method2_5);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("Method2", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>), typeof(System.Action<System.Exception>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method2_6);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("Method2", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>), typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.Exception>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method2_7);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32), typeof(System.ArgumentException)};
            if (genericMethods.TryGetValue("Method2", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>), typeof(System.Action<System.ArgumentException>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method2_8);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32), typeof(System.ArgumentException)};
            if (genericMethods.TryGetValue("Method2", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>), typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.ArgumentException>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method2_9);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.ExtensionClass)};
            if (genericMethods.TryGetValue("Method3", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.Exception)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method3_10);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>)};
            if (genericMethods.TryGetValue("Method3", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>), typeof(System.Exception)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method3_11);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.SubExtensionClass)};
            if (genericMethods.TryGetValue("Method3", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.SubExtensionClass), typeof(System.Exception)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method3_12);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>)};
            if (genericMethods.TryGetValue("Method3", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>), typeof(System.Exception)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method3_13);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.ArgumentException)};
            if (genericMethods.TryGetValue("Method3", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass), typeof(System.ArgumentException)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method3_14);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>), typeof(System.ArgumentException)};
            if (genericMethods.TryGetValue("Method3", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>), typeof(System.ArgumentException)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method3_15);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>), typeof(System.ArgumentException)};
            if (genericMethods.TryGetValue("Method3", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>), typeof(System.ArgumentException)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Method3_16);

                        break;
                    }
                }
            }


        }


        static StackObject* Method1_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<System.Object> @a = (System.Action<System.Object>)typeof(System.Action<System.Object>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method1<ILRuntimeTest.TestBase.ExtensionClass>(@i, @a);

            return __ret;
        }

        static StackObject* Method1_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Object> @a = (System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Object>)typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Object>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method1<ILRuntimeTest.TestBase.ExtensionClass>(@i, @a);

            return __ret;
        }

        static StackObject* Method2_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<System.Exception> @a = (System.Action<System.Exception>)typeof(System.Action<System.Exception>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2(@i, @a);

            return __ret;
        }

        static StackObject* Method2_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Exception> @a = (System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Exception>)typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.Exception>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2(@i, @a);

            return __ret;
        }

        static StackObject* Method2_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<System.ArgumentException> @a = (System.Action<System.ArgumentException>)typeof(System.Action<System.ArgumentException>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2<System.ArgumentException>(@i, @a);

            return __ret;
        }

        static StackObject* Method2_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.ArgumentException> @a = (System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.ArgumentException>)typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass, System.ArgumentException>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2<System.ArgumentException>(@i, @a);

            return __ret;
        }

        static StackObject* Method2_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<System.Exception> @a = (System.Action<System.Exception>)typeof(System.Action<System.Exception>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.ExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2<System.Int32>(@i, @a);

            return __ret;
        }

        static StackObject* Method2_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.Exception> @a = (System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.Exception>)typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.Exception>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.ExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2<System.Int32>(@i, @a);

            return __ret;
        }

        static StackObject* Method2_8(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<System.ArgumentException> @a = (System.Action<System.ArgumentException>)typeof(System.Action<System.ArgumentException>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.ExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2<System.Int32, System.ArgumentException>(@i, @a);

            return __ret;
        }

        static StackObject* Method2_9(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.ArgumentException> @a = (System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.ArgumentException>)typeof(System.Action<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.ArgumentException>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.ExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method2<System.Int32, System.ArgumentException>(@i, @a);

            return __ret;
        }

        static StackObject* Method3_10(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Exception @ex = (System.Exception)typeof(System.Exception).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method3<ILRuntimeTest.TestBase.ExtensionClass>(@i, @ex);

            return __ret;
        }

        static StackObject* Method3_11(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Exception @ex = (System.Exception)typeof(System.Exception).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.ExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method3<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>>(@i, @ex);

            return __ret;
        }

        static StackObject* Method3_12(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Exception @ex = (System.Exception)typeof(System.Exception).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.SubExtensionClass @i = (ILRuntimeTest.TestBase.SubExtensionClass)typeof(ILRuntimeTest.TestBase.SubExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method3<ILRuntimeTest.TestBase.SubExtensionClass>(@i, @ex);

            return __ret;
        }

        static StackObject* Method3_13(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Exception @ex = (System.Exception)typeof(System.Exception).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.SubExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method3<ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>>(@i, @ex);

            return __ret;
        }

        static StackObject* Method3_14(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.ArgumentException @ex = (System.ArgumentException)typeof(System.ArgumentException).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass @i = (ILRuntimeTest.TestBase.ExtensionClass)typeof(ILRuntimeTest.TestBase.ExtensionClass).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method3<System.ArgumentException>(@i, @ex);

            return __ret;
        }

        static StackObject* Method3_15(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.ArgumentException @ex = (System.ArgumentException)typeof(System.ArgumentException).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.ExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.ExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.ExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method3<ILRuntimeTest.TestBase.ExtensionClass<System.Int32>, System.ArgumentException>(@i, @ex);

            return __ret;
        }

        static StackObject* Method3_16(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.ArgumentException @ex = (System.ArgumentException)typeof(System.ArgumentException).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestBase.SubExtensionClass<System.Int32> @i = (ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>)typeof(ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestBase.GenericExtensions.Method3<ILRuntimeTest.TestBase.SubExtensionClass<System.Int32>, System.ArgumentException>(@i, @ex);

            return __ret;
        }



    }
}
