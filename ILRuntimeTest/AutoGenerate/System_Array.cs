using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_Array
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(System.Array);
            args = new Type[]{typeof(System.Type), typeof(System.Int32)};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_0);
            args = new Type[]{typeof(System.Type), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_1);
            args = new Type[]{typeof(System.Type), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_2);
            args = new Type[]{typeof(System.Type), typeof(System.Int32[])};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_3);
            args = new Type[]{typeof(System.Type), typeof(System.Int64[])};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_4);
            args = new Type[]{typeof(System.Type), typeof(System.Int32[]), typeof(System.Int32[])};
            method = type.GetMethod("CreateInstance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateInstance_5);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int32)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_6);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_7);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("ConstrainedCopy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ConstrainedCopy_8);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int64)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_9);
            args = new Type[]{typeof(System.Array), typeof(System.Int64), typeof(System.Array), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("Copy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Copy_10);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Clear", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clear_11);
            args = new Type[]{typeof(System.Int32[])};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_12);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_13);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_14);
            args = new Type[]{typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_15);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_16);
            args = new Type[]{typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_17);
            args = new Type[]{typeof(System.Int64), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_18);
            args = new Type[]{typeof(System.Int64[])};
            method = type.GetMethod("GetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetValue_19);
            args = new Type[]{typeof(System.Object), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_20);
            args = new Type[]{typeof(System.Object), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_21);
            args = new Type[]{typeof(System.Object), typeof(System.Int32), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_22);
            args = new Type[]{typeof(System.Object), typeof(System.Int32[])};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_23);
            args = new Type[]{typeof(System.Object), typeof(System.Int64)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_24);
            args = new Type[]{typeof(System.Object), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_25);
            args = new Type[]{typeof(System.Object), typeof(System.Int64), typeof(System.Int64), typeof(System.Int64)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_26);
            args = new Type[]{typeof(System.Object), typeof(System.Int64[])};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_27);
            args = new Type[]{};
            method = type.GetMethod("get_Length", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Length_28);
            args = new Type[]{};
            method = type.GetMethod("get_LongLength", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_LongLength_29);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetLength", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetLength_30);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetLongLength", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetLongLength_31);
            args = new Type[]{};
            method = type.GetMethod("get_Rank", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Rank_32);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetUpperBound", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetUpperBound_33);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("GetLowerBound", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetLowerBound_34);
            args = new Type[]{};
            method = type.GetMethod("get_SyncRoot", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_SyncRoot_35);
            args = new Type[]{};
            method = type.GetMethod("get_IsReadOnly", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_IsReadOnly_36);
            args = new Type[]{};
            method = type.GetMethod("get_IsFixedSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_IsFixedSize_37);
            args = new Type[]{};
            method = type.GetMethod("get_IsSynchronized", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_IsSynchronized_38);
            args = new Type[]{};
            method = type.GetMethod("Clone", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clone_39);
            args = new Type[]{typeof(System.Array), typeof(System.Object)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_40);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Object)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_41);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Collections.IComparer)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_42);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Object), typeof(System.Collections.IComparer)};
            method = type.GetMethod("BinarySearch", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BinarySearch_43);
            args = new Type[]{typeof(System.Array), typeof(System.Int32)};
            method = type.GetMethod("CopyTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CopyTo_44);
            args = new Type[]{typeof(System.Array), typeof(System.Int64)};
            method = type.GetMethod("CopyTo", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CopyTo_45);
            args = new Type[]{};
            method = type.GetMethod("GetEnumerator", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetEnumerator_46);
            args = new Type[]{typeof(System.Array), typeof(System.Object)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_47);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_48);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("IndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IndexOf_49);
            args = new Type[]{typeof(System.Array), typeof(System.Object)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_50);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_51);
            args = new Type[]{typeof(System.Array), typeof(System.Object), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("LastIndexOf", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LastIndexOf_52);
            args = new Type[]{typeof(System.Array)};
            method = type.GetMethod("Reverse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Reverse_53);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Reverse", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Reverse_54);
            args = new Type[]{typeof(System.Array)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_55);
            args = new Type[]{typeof(System.Array), typeof(System.Array)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_56);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_57);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_58);
            args = new Type[]{typeof(System.Array), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_59);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_60);
            args = new Type[]{typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_61);
            args = new Type[]{typeof(System.Array), typeof(System.Array), typeof(System.Int32), typeof(System.Int32), typeof(System.Collections.IComparer)};
            method = type.GetMethod("Sort", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Sort_62);
            args = new Type[]{};
            method = type.GetMethod("Initialize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Initialize_63);

        }

        static StackObject* CreateInstance_0(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Type elementType = (System.Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.CreateInstance(elementType, length);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_1(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length2 = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 length1 = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Type elementType = (System.Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.CreateInstance(elementType, length1, length2);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_2(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length3 = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 length2 = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 length1 = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Type elementType = (System.Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.CreateInstance(elementType, length1, length2, length3);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_3(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32[] lengths = (System.Int32[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Type elementType = (System.Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.CreateInstance(elementType, lengths);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_4(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64[] lengths = (System.Int64[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Type elementType = (System.Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.CreateInstance(elementType, lengths);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* CreateInstance_5(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32[] lowerBounds = (System.Int32[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32[] lengths = (System.Int32[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Type elementType = (System.Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.CreateInstance(elementType, lengths, lowerBounds);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* Copy_6(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array destinationArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array sourceArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Copy(sourceArray, destinationArray, length);

            return ret;
        }

        static StackObject* Copy_7(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 destinationIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array destinationArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 sourceIndex = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.Array sourceArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

            return ret;
        }

        static StackObject* ConstrainedCopy_8(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 destinationIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array destinationArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 sourceIndex = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.Array sourceArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.ConstrainedCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

            return ret;
        }

        static StackObject* Copy_9(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 length = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array destinationArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array sourceArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Copy(sourceArray, destinationArray, length);

            return ret;
        }

        static StackObject* Copy_10(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 length = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 destinationIndex = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array destinationArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Int64 sourceIndex = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.Array sourceArray = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

            return ret;
        }

        static StackObject* Clear_11(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Clear(array, index, length);

            return ret;
        }

        static StackObject* GetValue_12(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32[] indices = (System.Int32[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(indices);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetValue_13(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(index);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetValue_14(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index2 = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index1 = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(index1, index2);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetValue_15(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index3 = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index2 = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 index1 = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(index1, index2, index3);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetValue_16(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 index = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(index);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetValue_17(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 index2 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 index1 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(index1, index2);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetValue_18(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 index3 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 index2 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int64 index1 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(index1, index2, index3);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* GetValue_19(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64[] indices = (System.Int64[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetValue(indices);

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* SetValue_20(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, index);

            return ret;
        }

        static StackObject* SetValue_21(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index2 = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index1 = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, index1, index2);

            return ret;
        }

        static StackObject* SetValue_22(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index3 = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index2 = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 index1 = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, index1, index2, index3);

            return ret;
        }

        static StackObject* SetValue_23(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32[] indices = (System.Int32[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, indices);

            return ret;
        }

        static StackObject* SetValue_24(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 index = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, index);

            return ret;
        }

        static StackObject* SetValue_25(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 index2 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 index1 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, index1, index2);

            return ret;
        }

        static StackObject* SetValue_26(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 index3 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int64 index2 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int64 index1 = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, index1, index2, index3);

            return ret;
        }

        static StackObject* SetValue_27(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64[] indices = (System.Int64[])StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.SetValue(value, indices);

            return ret;
        }

        static StackObject* get_Length_28(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Length;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* get_LongLength_29(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.LongLength;

            ret->ObjectType = ObjectTypes.Long;
            *(long*)&ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* GetLength_30(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 dimension = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetLength(dimension);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* GetLongLength_31(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 dimension = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetLongLength(dimension);

            ret->ObjectType = ObjectTypes.Long;
            *(long*)&ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* get_Rank_32(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Rank;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* GetUpperBound_33(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 dimension = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetUpperBound(dimension);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* GetLowerBound_34(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 dimension = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetLowerBound(dimension);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* get_SyncRoot_35(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.SyncRoot;

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* get_IsReadOnly_36(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.IsReadOnly;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* get_IsFixedSize_37(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.IsFixedSize;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* get_IsSynchronized_38(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.IsSynchronized;

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method ? 1 : 0;
            return ret + 1;
        }

        static StackObject* Clone_39(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.Clone();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* BinarySearch_40(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.BinarySearch(array, value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* BinarySearch_41(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.BinarySearch(array, index, length, value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* BinarySearch_42(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.BinarySearch(array, value, comparer);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* BinarySearch_43(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 5);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.BinarySearch(array, index, length, value, comparer);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* CopyTo_44(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.CopyTo(array, index);

            return ret;
        }

        static StackObject* CopyTo_45(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int64 index = *(long*)&p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.CopyTo(array, index);

            return ret;
        }

        static StackObject* GetEnumerator_46(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = instance.GetEnumerator();

            return ILIntepreter.PushObject(ret, mStack, result_of_this_method);
        }

        static StackObject* IndexOf_47(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.IndexOf(array, value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_48(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.IndexOf(array, value, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* IndexOf_49(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.IndexOf(array, value, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_50(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.LastIndexOf(array, value);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_51(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.LastIndexOf(array, value, startIndex);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* LastIndexOf_52(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 count = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 startIndex = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Object value = (System.Object)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            var result_of_this_method = System.Array.LastIndexOf(array, value, startIndex, count);

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = result_of_this_method;
            return ret + 1;
        }

        static StackObject* Reverse_53(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Reverse(array);

            return ret;
        }

        static StackObject* Reverse_54(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Reverse(array, index, length);

            return ret;
        }

        static StackObject* Sort_55(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(array);

            return ret;
        }

        static StackObject* Sort_56(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Array items = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array keys = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(keys, items);

            return ret;
        }

        static StackObject* Sort_57(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(array, index, length);

            return ret;
        }

        static StackObject* Sort_58(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Array items = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 4);
            System.Array keys = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(keys, items, index, length);

            return ret;
        }

        static StackObject* Sort_59(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 2);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(array, comparer);

            return ret;
        }

        static StackObject* Sort_60(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 3);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Array items = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 3);
            System.Array keys = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(keys, items, comparer);

            return ret;
        }

        static StackObject* Sort_61(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 4);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Array array = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(array, index, length, comparer);

            return ret;
        }

        static StackObject* Sort_62(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 5);
            p = ILIntepreter.Minus(esp, 1);
            System.Collections.IComparer comparer = (System.Collections.IComparer)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            System.Int32 length = p->Value;
            p = ILIntepreter.Minus(esp, 3);
            System.Int32 index = p->Value;
            p = ILIntepreter.Minus(esp, 4);
            System.Array items = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 5);
            System.Array keys = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            System.Array.Sort(keys, items, index, length, comparer);

            return ret;
        }

        static StackObject* Initialize_63(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            System.Array instance = (System.Array)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            instance.Initialize();

            return ret;
        }


    }
}
