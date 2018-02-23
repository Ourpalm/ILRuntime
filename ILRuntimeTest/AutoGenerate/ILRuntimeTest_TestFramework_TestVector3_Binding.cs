using System;
using System.Collections.Generic;
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
    unsafe class ILRuntimeTest_TestFramework_TestVector3_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestVector3);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestVector3), typeof(ILRuntimeTest.TestFramework.TestVector3)};
            method = type.GetMethod("op_Addition", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_Addition_0);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestVector3), typeof(System.Single)};
            method = type.GetMethod("op_Multiply", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_Multiply_1);

            field = type.GetField("One", flag);
            app.RegisterCLRFieldGetter(field, get_One_0);
            app.RegisterCLRFieldSetter(field, set_One_0);
            field = type.GetField("X", flag);
            app.RegisterCLRFieldGetter(field, get_X_1);
            app.RegisterCLRFieldSetter(field, set_X_1);
            field = type.GetField("Y", flag);
            app.RegisterCLRFieldGetter(field, get_Y_2);
            app.RegisterCLRFieldSetter(field, set_Y_2);
            field = type.GetField("Z", flag);
            app.RegisterCLRFieldGetter(field, get_Z_3);
            app.RegisterCLRFieldSetter(field, set_Z_3);

            app.RegisterCLRMemberwiseClone(type, PerformMemberwiseClone);

            app.RegisterCLRCreateDefaultInstance(type, () => new ILRuntimeTest.TestFramework.TestVector3());
            app.RegisterCLRCreateArrayInstance(type, s => new ILRuntimeTest.TestFramework.TestVector3[s]);

            args = new Type[]{typeof(System.Single), typeof(System.Single), typeof(System.Single)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref ILRuntimeTest.TestFramework.TestVector3 instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.TestVector3[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }

        static StackObject* op_Addition_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.TestVector3 b = ValueTypeBinderMapping.Parse_ILRuntimeTest_TestFramework_TestVector3_Binding (__intp, ptr_of_this_method, __mStack);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.TestVector3 a = ValueTypeBinderMapping.Parse_ILRuntimeTest_TestFramework_TestVector3_Binding (__intp, ptr_of_this_method, __mStack);

            var result_of_this_method = a + b;

            ValueTypeBinderMapping.Push_ILRuntimeTest_TestFramework_TestVector3_Binding(ref result_of_this_method, __intp, __ret, __mStack);
            return __ret + 1;
        }

        static StackObject* op_Multiply_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single b = *(float*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.TestVector3 a = ValueTypeBinderMapping.Parse_ILRuntimeTest_TestFramework_TestVector3_Binding (__intp, ptr_of_this_method, __mStack);

            var result_of_this_method = a * b;

            ValueTypeBinderMapping.Push_ILRuntimeTest_TestFramework_TestVector3_Binding(ref result_of_this_method, __intp, __ret, __mStack);
            return __ret + 1;
        }


        static object get_One_0(ref object o)
        {
            return ILRuntimeTest.TestFramework.TestVector3.One;
        }
        static void set_One_0(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.TestVector3.One = (ILRuntimeTest.TestFramework.TestVector3)v;
        }
        static object get_X_1(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVector3)o).X;
        }
        static void set_X_1(ref object o, object v)
        {
            var h = GCHandle.Alloc(o, GCHandleType.Pinned);
            ILRuntimeTest.TestFramework.TestVector3* p = (ILRuntimeTest.TestFramework.TestVector3 *)(void *)h.AddrOfPinnedObject();
            p->X = (System.Single)v;
            h.Free();
        }
        static object get_Y_2(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVector3)o).Y;
        }
        static void set_Y_2(ref object o, object v)
        {
            var h = GCHandle.Alloc(o, GCHandleType.Pinned);
            ILRuntimeTest.TestFramework.TestVector3* p = (ILRuntimeTest.TestFramework.TestVector3 *)(void *)h.AddrOfPinnedObject();
            p->Y = (System.Single)v;
            h.Free();
        }
        static object get_Z_3(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVector3)o).Z;
        }
        static void set_Z_3(ref object o, object v)
        {
            var h = GCHandle.Alloc(o, GCHandleType.Pinned);
            ILRuntimeTest.TestFramework.TestVector3* p = (ILRuntimeTest.TestFramework.TestVector3 *)(void *)h.AddrOfPinnedObject();
            p->Z = (System.Single)v;
            h.Free();
        }

        static object PerformMemberwiseClone(ref object o)
        {
            var ins = new ILRuntimeTest.TestFramework.TestVector3();
            ins = (ILRuntimeTest.TestFramework.TestVector3)o;
            return ins;
        }

        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single z = *(float*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Single y = *(float*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Single x = *(float*)&ptr_of_this_method->Value;

            var result_of_this_method = new ILRuntimeTest.TestFramework.TestVector3(x, y, z);

            if(!isNewObj)
            {
                __ret--;
                ValueTypeBinderMapping.WriteBack_ILRuntimeTest_TestFramework_TestVector3_Binding(__domain, __ret, __mStack, ref result_of_this_method);
                return __ret;
            }
            ValueTypeBinderMapping.Push_ILRuntimeTest_TestFramework_TestVector3_Binding(ref result_of_this_method, __intp, __ret, __mStack);
            return __ret + 1;
        }


    }
}
