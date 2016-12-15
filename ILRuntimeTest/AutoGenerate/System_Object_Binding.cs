using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_Object_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(System.Object);
            args = new Type[]{};
            method = type.GetMethod("ToString", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ToString_0);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_1);
            args = new Type[]{typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("Equals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Equals_2);
            args = new Type[]{typeof(System.Object), typeof(System.Object)};
            method = type.GetMethod("ReferenceEquals", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReferenceEquals_3);
            args = new Type[]{};
            method = type.GetMethod("GetHashCode", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetHashCode_4);
            args = new Type[]{};
            method = type.GetMethod("GetType", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetType_5);

        }

        static StackObject* ToString_0(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Object instance = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance.ToString();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Equals_1(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object obj = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object instance = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance.Equals(obj);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Equals_2(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object objB = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object objA = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Object.Equals(objA, objB);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* ReferenceEquals_3(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object objB = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object objA = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = System.Object.ReferenceEquals(objA, objB);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* GetHashCode_4(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Object instance = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance.GetHashCode();

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* GetType_5(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Object instance = (System.Object)typeof(System.Object).CheckCLRTypes(domain, StackObject.ToObject(p, domain, mStack));
            intp.Free(p);

            var result_of_this_method = instance.GetType();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }


    }
}
