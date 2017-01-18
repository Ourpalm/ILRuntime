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
    unsafe class System_Array_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Array);
            args = new Type[]{};
            method = type.GetMethod("GetEnumerator", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetEnumerator_0);
            args = new Type[]{typeof(System.Array), typeof(System.Object)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_1);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_2);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_3);
            args = new Type[]{typeof(System.Array), typeof(System.Object)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_4);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_5);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_6);
            args = new Type[]{typeof(System.Array)};
            method = type.GetMethod("Reverse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Reverse_7);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Reverse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Reverse_8);
            args = new Type[]{typeof(System.Array)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_9);
            args = new Type[]{typeof(System.Array), typeof(System.Array)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_10);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_11);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_12);
            args = new Type[]{typeof(System.Array), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_13);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_14);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_15);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_16);
            args = new Type[]{};
            method = type.GetMethod("Initialize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Initialize_17);
            args = new Type[]{typeof(System.Type), typeof(System.Int32)};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_18);
            args = new Type[]{typeof(System.Type), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_19);
            args = new Type[]{typeof(System.Type), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_20);
            args = new Type[]{typeof(System.Type), typeof(System.Int32[])};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_21);
            args = new Type[]{typeof(System.Type), typeof(System.Int64[])};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_22);
            args = new Type[]{typeof(System.Type), typeof(System.Int32[]), typeof(System.Int32[])};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_23);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int32)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_24);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_25);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("ConstrainedCopy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ConstrainedCopy_26);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int64)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_27);
            args = new Type[]{typeof(System.Array), typeof(System.Int64), typeof(System.Array), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_28);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Clear", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clear_29);
            args = new Type[]{typeof(System.Int32[])};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_30);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_31);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_32);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_33);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_34);
            args = new Type[]{typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_35);
            args = new Type[]{typeof(System.Int64), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_36);
            args = new Type[]{typeof(System.Int64[])};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_37);
            args = new Type[]{typeof(System.Object), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_38);
            args = new Type[]{typeof(System.Object), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_39);
            args = new Type[]{typeof(System.Object), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_40);
            args = new Type[]{typeof(System.Object), typeof(System.Int32[])};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_41);
            args = new Type[]{typeof(System.Object), typeof(System.Int64)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_42);
            args = new Type[]{typeof(System.Object), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_43);
            args = new Type[]{typeof(System.Object), typeof(System.Int64), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_44);
            args = new Type[]{typeof(System.Object), typeof(System.Int64[])};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_45);
            args = new Type[]{};
            method = type.GetMethod("get_Length", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Length_46);
            args = new Type[]{};
            method = type.GetMethod("get_LongLength", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_LongLength_47);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetLength", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetLength_48);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetLongLength", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetLongLength_49);
            args = new Type[]{};
            method = type.GetMethod("get_Rank", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Rank_50);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetUpperBound", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetUpperBound_51);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetLowerBound", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetLowerBound_52);
            args = new Type[]{};
            method = type.GetMethod("get_SyncRoot", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_SyncRoot_53);
            args = new Type[]{};
            method = type.GetMethod("get_IsReadOnly", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_IsReadOnly_54);
            args = new Type[]{};
            method = type.GetMethod("get_IsFixedSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_IsFixedSize_55);
            args = new Type[]{};
            method = type.GetMethod("get_IsSynchronized", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_IsSynchronized_56);
            args = new Type[]{};
            method = type.GetMethod("Clone", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clone_57);
            args = new Type[]{typeof(System.Array), typeof(System.Object)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_58);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Object)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_59);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Collections.IComparer)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_60);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Object), typeof(System.Collections.IComparer)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_61);
            args = new Type[]{typeof(System.Array), typeof(System.Int32)};
            method = type.GetMethod("CopyTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CopyTo_62);
            args = new Type[]{typeof(System.Array), typeof(System.Int64)};
            method = type.GetMethod("CopyTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CopyTo_63);


        }


        static StackObject* GetEnumerator_0(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetEnumerator();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* IndexOf_1(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.IndexOf(array, value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* IndexOf_2(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 startIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.IndexOf(array, value, startIndex);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* IndexOf_3(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 count = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 startIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.IndexOf(array, value, startIndex, count);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* LastIndexOf_4(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.LastIndexOf(array, value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* LastIndexOf_5(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 startIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.LastIndexOf(array, value, startIndex);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* LastIndexOf_6(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 count = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 startIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.LastIndexOf(array, value, startIndex, count);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* Reverse_7(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Reverse(array);

            return __ret;
        }

        static StackObject* Reverse_8(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Reverse(array, index, length);

            return __ret;
        }

        static StackObject* Sort_9(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(array);

            return __ret;
        }

        static StackObject* Sort_10(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array items = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array keys = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(keys, items);

            return __ret;
        }

        static StackObject* Sort_11(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(array, index, length);

            return __ret;
        }

        static StackObject* Sort_12(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array items = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array keys = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(keys, items, index, length);

            return __ret;
        }

        static StackObject* Sort_13(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)typeof(System.Collections.IComparer).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(array, comparer);

            return __ret;
        }

        static StackObject* Sort_14(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)typeof(System.Collections.IComparer).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array items = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array keys = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(keys, items, comparer);

            return __ret;
        }

        static StackObject* Sort_15(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)typeof(System.Collections.IComparer).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(array, index, length, comparer);

            return __ret;
        }

        static StackObject* Sort_16(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)typeof(System.Collections.IComparer).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array items = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Array keys = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Sort(keys, items, index, length, comparer);

            return __ret;
        }

        static StackObject* Initialize_17(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Initialize();

            return __ret;
        }

        static StackObject* CreateInstance_18(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Type elementType = (System.Type)typeof(System.Type).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.CreateInstance(elementType, length);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_19(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length2 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 length1 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Type elementType = (System.Type)typeof(System.Type).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.CreateInstance(elementType, length1, length2);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_20(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length3 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 length2 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 length1 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Type elementType = (System.Type)typeof(System.Type).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.CreateInstance(elementType, length1, length2, length3);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_21(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32[] lengths = (System.Int32[])typeof(System.Int32[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Type elementType = (System.Type)typeof(System.Type).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.CreateInstance(elementType, lengths);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_22(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64[] lengths = (System.Int64[])typeof(System.Int64[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Type elementType = (System.Type)typeof(System.Type).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.CreateInstance(elementType, lengths);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_23(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32[] lowerBounds = (System.Int32[])typeof(System.Int32[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32[] lengths = (System.Int32[])typeof(System.Int32[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Type elementType = (System.Type)typeof(System.Type).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.CreateInstance(elementType, lengths, lowerBounds);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Copy_24(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array destinationArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array sourceArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Copy(sourceArray, destinationArray, length);

            return __ret;
        }

        static StackObject* Copy_25(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 destinationIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array destinationArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Int32 sourceIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Array sourceArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

            return __ret;
        }

        static StackObject* ConstrainedCopy_26(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 destinationIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array destinationArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Int32 sourceIndex = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Array sourceArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.ConstrainedCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

            return __ret;
        }

        static StackObject* Copy_27(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 length = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array destinationArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array sourceArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Copy(sourceArray, destinationArray, length);

            return __ret;
        }

        static StackObject* Copy_28(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 length = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int64 destinationIndex = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array destinationArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Int64 sourceIndex = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Array sourceArray = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

            return __ret;
        }

        static StackObject* Clear_29(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            System.Array.Clear(array, index, length);

            return __ret;
        }

        static StackObject* GetValue_30(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32[] indices = (System.Int32[])typeof(System.Int32[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(indices);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetValue_31(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(index);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetValue_32(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 index2 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index1 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(index1, index2);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetValue_33(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 index3 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index2 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 index1 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(index1, index2, index3);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetValue_34(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 index = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(index);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetValue_35(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 index2 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int64 index1 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(index1, index2);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetValue_36(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 index3 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int64 index2 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int64 index1 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(index1, index2, index3);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetValue_37(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64[] indices = (System.Int64[])typeof(System.Int64[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetValue(indices);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* SetValue_38(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, index);

            return __ret;
        }

        static StackObject* SetValue_39(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 index2 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index1 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, index1, index2);

            return __ret;
        }

        static StackObject* SetValue_40(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 index3 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 index2 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 index1 = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, index1, index2, index3);

            return __ret;
        }

        static StackObject* SetValue_41(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32[] indices = (System.Int32[])typeof(System.Int32[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, indices);

            return __ret;
        }

        static StackObject* SetValue_42(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 index = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, index);

            return __ret;
        }

        static StackObject* SetValue_43(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 index2 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int64 index1 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, index1, index2);

            return __ret;
        }

        static StackObject* SetValue_44(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 index3 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int64 index2 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int64 index1 = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, index1, index2, index3);

            return __ret;
        }

        static StackObject* SetValue_45(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64[] indices = (System.Int64[])typeof(System.Int64[]).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(value, indices);

            return __ret;
        }

        static StackObject* get_Length_46(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Length;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_LongLength_47(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.LongLength;

            __ret->ObjectType = ObjectTypes.Long;
            *(long*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* GetLength_48(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 dimension = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetLength(dimension);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* GetLongLength_49(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 dimension = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetLongLength(dimension);

            __ret->ObjectType = ObjectTypes.Long;
            *(long*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_Rank_50(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Rank;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* GetUpperBound_51(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 dimension = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetUpperBound(dimension);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* GetLowerBound_52(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 dimension = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetLowerBound(dimension);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_SyncRoot_53(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.SyncRoot;

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_IsReadOnly_54(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.IsReadOnly;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* get_IsFixedSize_55(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.IsFixedSize;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* get_IsSynchronized_56(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.IsSynchronized;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* Clone_57(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Clone();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* BinarySearch_58(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.BinarySearch(array, value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* BinarySearch_59(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.BinarySearch(array, index, length, value);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* BinarySearch_60(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)typeof(System.Collections.IComparer).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.BinarySearch(array, value, comparer);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* BinarySearch_61(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)typeof(System.Collections.IComparer).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Object value = (System.Object)typeof(System.Object).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 length = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = System.Array.BinarySearch(array, index, length, value, comparer);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* CopyTo_62(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 index = ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.CopyTo(array, index);

            return __ret;
        }

        static StackObject* CopyTo_63(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 index = *(long*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Array array = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Array instance_of_this_method;
            instance_of_this_method = (System.Array)typeof(System.Array).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.CopyTo(array, index);

            return __ret;
        }



    }
}
