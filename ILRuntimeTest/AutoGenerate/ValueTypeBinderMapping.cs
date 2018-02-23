using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Stack;

public static unsafe class ValueTypeBinderMapping
{

        public static ILRuntimeTest.TestFramework.TestVector3 Parse_ILRuntimeTest_TestFramework_TestVector3_Binding(ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            var clrType = (CLRType)__intp.AppDomain.GetType(typeof(ILRuntimeTest.TestFramework.TestVector3));
            var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVector3>)clrType.ValueTypeBinder;

            ILRuntimeTest.TestFramework.TestVector3 value = new ILRuntimeTest.TestFramework.TestVector3();

            var a = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var ptr = *(StackObject**)&a->Value;
                binder.AssignFromStack(ref value, ptr, __mStack);
                __intp.FreeStackValueType(ptr_of_this_method);
            }
            else
            {
                value = (ILRuntimeTest.TestFramework.TestVector3)StackObject.ToObject(a, __intp.AppDomain, __mStack);
                __intp.Free(ptr_of_this_method);
            }

            return value;
        }

        public static void WriteBack_ILRuntimeTest_TestFramework_TestVector3_Binding(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref ILRuntimeTest.TestFramework.TestVector3 instance_of_this_method)
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
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var clrType = (CLRType)__domain.GetType(typeof(ILRuntimeTest.TestFramework.TestVector3));
                        var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVector3>)clrType.ValueTypeBinder;

                        var dst = *((StackObject**)&ptr_of_this_method->Value);
                        binder.CopyValueTypeToStack (ref instance_of_this_method, dst, __mStack);
                    }
                    break;
            }
        }

        public static void Push_ILRuntimeTest_TestFramework_TestVector3_Binding(ref ILRuntimeTest.TestFramework.TestVector3 value, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            var clrType = (CLRType)__intp.AppDomain.GetType(typeof(ILRuntimeTest.TestFramework.TestVector3));
            var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVector3>)clrType.ValueTypeBinder;

            __intp.AllocValueType(ptr_of_this_method, binder.CLRType);
            var dst = *((StackObject**)&ptr_of_this_method->Value);
            binder.CopyValueTypeToStack(ref value, dst, __mStack);
        }


        public static ILRuntimeTest.TestFramework.TestVectorStruct Parse_ILRuntimeTest_TestFramework_TestVectorStruct_Binding(ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            var clrType = (CLRType)__intp.AppDomain.GetType(typeof(ILRuntimeTest.TestFramework.TestVectorStruct));
            var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVectorStruct>)clrType.ValueTypeBinder;

            ILRuntimeTest.TestFramework.TestVectorStruct value = new ILRuntimeTest.TestFramework.TestVectorStruct();

            var a = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var ptr = *(StackObject**)&a->Value;
                binder.AssignFromStack(ref value, ptr, __mStack);
                __intp.FreeStackValueType(ptr_of_this_method);
            }
            else
            {
                value = (ILRuntimeTest.TestFramework.TestVectorStruct)StackObject.ToObject(a, __intp.AppDomain, __mStack);
                __intp.Free(ptr_of_this_method);
            }

            return value;
        }

        public static void WriteBack_ILRuntimeTest_TestFramework_TestVectorStruct_Binding(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref ILRuntimeTest.TestFramework.TestVectorStruct instance_of_this_method)
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
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.TestVectorStruct[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var clrType = (CLRType)__domain.GetType(typeof(ILRuntimeTest.TestFramework.TestVectorStruct));
                        var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVectorStruct>)clrType.ValueTypeBinder;

                        var dst = *((StackObject**)&ptr_of_this_method->Value);
                        binder.CopyValueTypeToStack (ref instance_of_this_method, dst, __mStack);
                    }
                    break;
            }
        }

        public static void Push_ILRuntimeTest_TestFramework_TestVectorStruct_Binding(ref ILRuntimeTest.TestFramework.TestVectorStruct value, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            var clrType = (CLRType)__intp.AppDomain.GetType(typeof(ILRuntimeTest.TestFramework.TestVectorStruct));
            var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVectorStruct>)clrType.ValueTypeBinder;

            __intp.AllocValueType(ptr_of_this_method, binder.CLRType);
            var dst = *((StackObject**)&ptr_of_this_method->Value);
            binder.CopyValueTypeToStack(ref value, dst, __mStack);
        }


        public static ILRuntimeTest.TestFramework.TestVectorStruct2 Parse_ILRuntimeTest_TestFramework_TestVectorStruct2_Binding(ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            var clrType = (CLRType)__intp.AppDomain.GetType(typeof(ILRuntimeTest.TestFramework.TestVectorStruct2));
            var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVectorStruct2>)clrType.ValueTypeBinder;

            ILRuntimeTest.TestFramework.TestVectorStruct2 value = new ILRuntimeTest.TestFramework.TestVectorStruct2();

            var a = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var ptr = *(StackObject**)&a->Value;
                binder.AssignFromStack(ref value, ptr, __mStack);
                __intp.FreeStackValueType(ptr_of_this_method);
            }
            else
            {
                value = (ILRuntimeTest.TestFramework.TestVectorStruct2)StackObject.ToObject(a, __intp.AppDomain, __mStack);
                __intp.Free(ptr_of_this_method);
            }

            return value;
        }

        public static void WriteBack_ILRuntimeTest_TestFramework_TestVectorStruct2_Binding(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, IList<object> __mStack, ref ILRuntimeTest.TestFramework.TestVectorStruct2 instance_of_this_method)
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
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.TestVectorStruct2[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var clrType = (CLRType)__domain.GetType(typeof(ILRuntimeTest.TestFramework.TestVectorStruct2));
                        var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVectorStruct2>)clrType.ValueTypeBinder;

                        var dst = *((StackObject**)&ptr_of_this_method->Value);
                        binder.CopyValueTypeToStack (ref instance_of_this_method, dst, __mStack);
                    }
                    break;
            }
        }

        public static void Push_ILRuntimeTest_TestFramework_TestVectorStruct2_Binding(ref ILRuntimeTest.TestFramework.TestVectorStruct2 value, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            var clrType = (CLRType)__intp.AppDomain.GetType(typeof(ILRuntimeTest.TestFramework.TestVectorStruct2));
            var binder = (ValueTypeBinder<ILRuntimeTest.TestFramework.TestVectorStruct2>)clrType.ValueTypeBinder;

            __intp.AllocValueType(ptr_of_this_method, binder.CLRType);
            var dst = *((StackObject**)&ptr_of_this_method->Value);
            binder.CopyValueTypeToStack(ref value, dst, __mStack);
        }



}
