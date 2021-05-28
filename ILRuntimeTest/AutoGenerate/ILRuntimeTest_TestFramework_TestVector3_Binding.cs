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
    unsafe class ILRuntimeTest_TestFramework_TestVector3_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestVector3);
            args = new Type[]{};
            method = type.GetMethod("get_One2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_One2_0);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestVector3), typeof(ILRuntimeTest.TestFramework.TestVector3)};
            method = type.GetMethod("op_Addition", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_Addition_1);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestVector3), typeof(System.Single)};
            method = type.GetMethod("op_Multiply", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, op_Multiply_2);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestVector3).MakeByRefType(), typeof(System.Single).MakeByRefType()};
            method = type.GetMethod("Test", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Test_3);
            args = new Type[]{};
            method = type.GetMethod("Normalize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Normalize_4);

            field = type.GetField("X", flag);
            app.RegisterCLRFieldGetter(field, get_X_0);
            app.RegisterCLRFieldSetter(field, set_X_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_X_0, AssignFromStack_X_0);
            field = type.GetField("One", flag);
            app.RegisterCLRFieldGetter(field, get_One_1);
            app.RegisterCLRFieldSetter(field, set_One_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_One_1, AssignFromStack_One_1);
            field = type.GetField("Y", flag);
            app.RegisterCLRFieldGetter(field, get_Y_2);
            app.RegisterCLRFieldSetter(field, set_Y_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_Y_2, AssignFromStack_Y_2);
            field = type.GetField("Z", flag);
            app.RegisterCLRFieldGetter(field, get_Z_3);
            app.RegisterCLRFieldSetter(field, set_Z_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_Z_3, AssignFromStack_Z_3);

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

        static StackObject* get_One2_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = ILRuntimeTest.TestFramework.TestVector3.One2;

            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static StackObject* op_Addition_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.TestVector3 @b = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref @b, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @b = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.TestVector3 @a = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref @a, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @a = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }


            var result_of_this_method = a + b;

            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static StackObject* op_Multiply_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @b = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.TestVector3 @a = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref @a, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @a = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
                __intp.Free(ptr_of_this_method);
            }


            var result_of_this_method = a * b;

            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static StackObject* Test_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @b = __intp.RetriveFloat(ptr_of_this_method, __mStack);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ILRuntimeTest.TestFramework.TestVector3 @a = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref @a, __intp, ptr_of_this_method, __mStack, false);
            } else {
                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
                @a = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            ILRuntimeTest.TestFramework.TestVector3 instance_of_this_method = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref instance_of_this_method, __intp, ptr_of_this_method, __mStack, false);
            } else {
                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
                instance_of_this_method = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }

            instance_of_this_method.Test(out @a, out @b);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                        ___dst->ObjectType = ObjectTypes.Float;
                        *(float*)&___dst->Value = @b;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @b;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @b);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @b;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @b);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.Single[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @b;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                        ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref a);
                } else {
                        object ___obj = @a;
                        if (___dst->ObjectType >= ObjectTypes.Object)
                        {
                            if (___obj is CrossBindingAdaptorType)
                                ___obj = ((CrossBindingAdaptorType)___obj).ILInstance;
                            __mStack[___dst->Value] = ___obj;
                        }
                        else
                        {
                            ILIntepreter.UnboxObject(___dst, ___obj, __mStack, __domain);
                        }
                }
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @a;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @a);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @a;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @a);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as ILRuntimeTest.TestFramework.TestVector3[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @a;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            } else {
                WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            }

            __intp.Free(ptr_of_this_method);
            return __ret;
        }

        static StackObject* Normalize_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntimeTest.TestFramework.TestVector3 instance_of_this_method = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref instance_of_this_method, __intp, ptr_of_this_method, __mStack, false);
            } else {
                ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
                instance_of_this_method = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }

            instance_of_this_method.Normalize();

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.WriteBackValue(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            } else {
                WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);
            }

            __intp.Free(ptr_of_this_method);
            return __ret;
        }


        static object get_X_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVector3)o).X;
        }

        static StackObject* CopyToStack_X_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.TestVector3)o).X;
            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_X_0(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.TestVector3 ins =(ILRuntimeTest.TestFramework.TestVector3)o;
            ins.X = (System.Single)v;
            o = ins;
        }

        static StackObject* AssignFromStack_X_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Single @X = *(float*)&ptr_of_this_method->Value;
            ILRuntimeTest.TestFramework.TestVector3 ins =(ILRuntimeTest.TestFramework.TestVector3)o;
            ins.X = @X;
            o = ins;
            return ptr_of_this_method;
        }

        static object get_One_1(ref object o)
        {
            return ILRuntimeTest.TestFramework.TestVector3.One;
        }

        static StackObject* CopyToStack_One_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.TestVector3.One;
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }

        static void set_One_1(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.TestVector3.One = (ILRuntimeTest.TestFramework.TestVector3)v;
        }

        static StackObject* AssignFromStack_One_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ILRuntimeTest.TestFramework.TestVector3 @One = new ILRuntimeTest.TestFramework.TestVector3();
            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.ParseValue(ref @One, __intp, ptr_of_this_method, __mStack, true);
            } else {
                @One = (ILRuntimeTest.TestFramework.TestVector3)typeof(ILRuntimeTest.TestFramework.TestVector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            }
            ILRuntimeTest.TestFramework.TestVector3.One = @One;
            return ptr_of_this_method;
        }

        static object get_Y_2(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVector3)o).Y;
        }

        static StackObject* CopyToStack_Y_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.TestVector3)o).Y;
            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_Y_2(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.TestVector3 ins =(ILRuntimeTest.TestFramework.TestVector3)o;
            ins.Y = (System.Single)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Y_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Single @Y = *(float*)&ptr_of_this_method->Value;
            ILRuntimeTest.TestFramework.TestVector3 ins =(ILRuntimeTest.TestFramework.TestVector3)o;
            ins.Y = @Y;
            o = ins;
            return ptr_of_this_method;
        }

        static object get_Z_3(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.TestVector3)o).Z;
        }

        static StackObject* CopyToStack_Z_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.TestVector3)o).Z;
            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_Z_3(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.TestVector3 ins =(ILRuntimeTest.TestFramework.TestVector3)o;
            ins.Z = (System.Single)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Z_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Single @Z = *(float*)&ptr_of_this_method->Value;
            ILRuntimeTest.TestFramework.TestVector3 ins =(ILRuntimeTest.TestFramework.TestVector3)o;
            ins.Z = @Z;
            o = ins;
            return ptr_of_this_method;
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
            System.Single @z = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Single @y = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Single @x = *(float*)&ptr_of_this_method->Value;


            var result_of_this_method = new ILRuntimeTest.TestFramework.TestVector3(@x, @y, @z);

            if(!isNewObj)
            {
                __ret--;
                if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                    ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.WriteBackValue(__domain, __ret, __mStack, ref result_of_this_method);
                } else {
                    WriteBackInstance(__domain, __ret, __mStack, ref result_of_this_method);
                }
                return __ret;
            }

            if (ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder != null) {
                ILRuntime.Runtime.Generated.CLRBindings.s_ILRuntimeTest_TestFramework_TestVector3_Binding_Binder.PushValue(ref result_of_this_method, __intp, __ret, __mStack);
                return __ret + 1;
            } else {
                return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
            }
        }


    }
}
