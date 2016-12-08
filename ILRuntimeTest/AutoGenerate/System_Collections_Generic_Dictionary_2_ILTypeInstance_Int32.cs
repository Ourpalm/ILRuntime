using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_Collections_Generic_Dictionary_2_ILTypeInstance_Int32
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>);
            args = new Type[]{};
            method = type.GetMethod("get_Comparer", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Comparer_0);
            args = new Type[]{};
            method = type.GetMethod("get_Count", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Count_1);
            args = new Type[]{};
            method = type.GetMethod("get_Keys", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Keys_2);
            args = new Type[]{};
            method = type.GetMethod("get_Values", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Values_3);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            method = type.GetMethod("get_Item", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Item_4);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Int32)};
            method = type.GetMethod("set_Item", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Item_5);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Int32)};
            method = type.GetMethod("Add", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Add_6);
            args = new Type[]{};
            method = type.GetMethod("Clear", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clear_7);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            method = type.GetMethod("ContainsKey", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ContainsKey_8);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("ContainsValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ContainsValue_9);
            args = new Type[]{};
            method = type.GetMethod("GetEnumerator", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetEnumerator_10);
            args = new Type[]{typeof(System.Runtime.Serialization.SerializationInfo), typeof(System.Runtime.Serialization.StreamingContext)};
            method = type.GetMethod("GetObjectData", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetObjectData_11);
            args = new Type[]{typeof(System.Object)};
            method = type.GetMethod("OnDeserialization", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, OnDeserialization_12);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            method = type.GetMethod("Remove", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Remove_13);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Int32).MakeByRefType()};
            method = type.GetMethod("TryGetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, TryGetValue_14);

        }

        static StackObject* get_Comparer_0(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Comparer;
            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* get_Count_1(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Count;
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* get_Keys_2(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Keys;
            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* get_Values_3(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Values;
            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* get_Item_4(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            ILRuntime.Runtime.Intepreter.ILTypeInstance key = (ILRuntime.Runtime.Intepreter.ILTypeInstance)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance[key];
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* set_Item_5(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            ILRuntime.Runtime.Intepreter.ILTypeInstance key = (ILRuntime.Runtime.Intepreter.ILTypeInstance)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 value = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance[key] = value;
            return ret;
        }

        static StackObject* Add_6(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            ILRuntime.Runtime.Intepreter.ILTypeInstance key = (ILRuntime.Runtime.Intepreter.ILTypeInstance)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 value = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.Add(key, value);
            return ret;
        }

        static StackObject* Clear_7(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.Clear();
            return ret;
        }

        static StackObject* ContainsKey_8(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            ILRuntime.Runtime.Intepreter.ILTypeInstance key = (ILRuntime.Runtime.Intepreter.ILTypeInstance)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.ContainsKey(key);
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* ContainsValue_9(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 value = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.ContainsValue(value);
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* GetEnumerator_10(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetEnumerator();
            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetObjectData_11(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Runtime.Serialization.SerializationInfo info = (System.Runtime.Serialization.SerializationInfo)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Runtime.Serialization.StreamingContext context = (System.Runtime.Serialization.StreamingContext)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.GetObjectData(info, context);
            return ret;
        }

        static StackObject* OnDeserialization_12(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object sender = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.OnDeserialization(sender);
            return ret;
        }

        static StackObject* Remove_13(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            ILRuntime.Runtime.Intepreter.ILTypeInstance key = (ILRuntime.Runtime.Intepreter.ILTypeInstance)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Remove(key);
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* TryGetValue_14(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            ILRuntime.Runtime.Intepreter.ILTypeInstance key = (ILRuntime.Runtime.Intepreter.ILTypeInstance)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.GetObjectAndResolveReference(p);
            System.Int32 value = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> instance = (System.Collections.Generic.Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.TryGetValue(key, out value);
            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }


    }
}
