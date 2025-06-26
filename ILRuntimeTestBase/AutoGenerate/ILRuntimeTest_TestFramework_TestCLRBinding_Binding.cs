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
    unsafe class ILRuntimeTest_TestFramework_TestCLRBinding_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestCLRBinding);
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
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestCLRBinding)};
            if (genericMethods.TryGetValue("Emit", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestFramework.TestCLRBinding)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Emit_0);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestCLRBinding)};
            if (genericMethods.TryGetValue("LoadAsset", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.String), typeof(ILRuntimeTest.TestFramework.TestCLRBinding)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, LoadAsset_1);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.String)};
            if (genericMethods.TryGetValue("LoadAsset", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.String), typeof(System.String)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, LoadAsset_2);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("LoadAsset", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.String), typeof(System.Int32)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, LoadAsset_3);

                        break;
                    }
                }
            }
            args = new Type[]{};
            method = type.GetMethod("MissingMethod", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, MissingMethod_4);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.ClassInheritanceTestAdaptor.Adaptor)};
            if (genericMethods.TryGetValue("MissingMethodGeneric", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestFramework.ClassInheritanceTestAdaptor.Adaptor)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, MissingMethodGeneric_5);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.MissingType)};
            if (genericMethods.TryGetValue("Emit", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestFramework.MissingType)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Emit_6);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.MissingType)};
            if (genericMethods.TryGetValue("MissingMethodGeneric", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ILRuntimeTest.TestFramework.MissingType)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, MissingMethodGeneric_7);

                        break;
                    }
                }
            }

            field = type.GetField("missingField", flag);
            app.RegisterCLRFieldGetter(field, get_missingField_0);
            app.RegisterCLRFieldSetter(field, set_missingField_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_missingField_0, AssignFromStack_missingField_0);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* Emit_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.TestCLRBinding @obj = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Emit<ILRuntimeTest.TestFramework.TestCLRBinding>(@obj);

            return __ret;
        }

        static StackObject* LoadAsset_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.TestCLRBinding @obj = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.String @name = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 3;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.LoadAsset<ILRuntimeTest.TestFramework.TestCLRBinding>(@name, @obj);

            return __ret;
        }

        static StackObject* LoadAsset_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.String @obj = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            System.String @name = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 3;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.LoadAsset<System.String>(@name, @obj);

            return __ret;
        }

        static StackObject* LoadAsset_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 3;

            ptr_of_this_method = __esp - 1;
            System.Int32 @obj = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.String @name = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 3;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.LoadAsset<System.Int32>(@name, @obj);

            return __ret;
        }

        static StackObject* MissingMethod_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.MissingMethod();

            return __ret;
        }

        static StackObject* MissingMethodGeneric_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.ClassInheritanceTestAdaptor.Adaptor @obj = (ILRuntimeTest.TestFramework.ClassInheritanceTestAdaptor.Adaptor)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTestAdaptor.Adaptor).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.MissingMethodGeneric<ILRuntimeTest.TestFramework.ClassInheritanceTestAdaptor.Adaptor>(@obj);

            return __ret;
        }

        static StackObject* Emit_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.MissingType @obj = (ILRuntimeTest.TestFramework.MissingType)typeof(ILRuntimeTest.TestFramework.MissingType).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Emit<ILRuntimeTest.TestFramework.MissingType>(@obj);

            return __ret;
        }

        static StackObject* MissingMethodGeneric_7(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.MissingType @obj = (ILRuntimeTest.TestFramework.MissingType)typeof(ILRuntimeTest.TestFramework.MissingType).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = __esp - 2;
            ILRuntimeTest.TestFramework.TestCLRBinding instance_of_this_method = (ILRuntimeTest.TestFramework.TestCLRBinding)typeof(ILRuntimeTest.TestFramework.TestCLRBinding).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.MissingMethodGeneric<ILRuntimeTest.TestFramework.MissingType>(@obj);

            return __ret;
        }


        static object get_missingField_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestCLRBinding)o).missingField;
        }

        static StackObject* CopyToStack_missingField_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.TestCLRBinding)o).missingField;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_missingField_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.TestCLRBinding)o).missingField = (System.Int32)v;
        }

        static StackObject* AssignFromStack_missingField_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @missingField = ptr_of_this_method->Value;
            ((ILRuntimeTest.TestFramework.TestCLRBinding)o).missingField = @missingField;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = __esp - 0;

            var result_of_this_method = new ILRuntimeTest.TestFramework.TestCLRBinding();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
