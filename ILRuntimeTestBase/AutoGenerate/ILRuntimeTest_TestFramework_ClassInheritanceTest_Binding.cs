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
    unsafe class ILRuntimeTest_TestFramework_ClassInheritanceTest_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest);
            args = new Type[]{};
            method = type.GetMethod("TestAbstract", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, TestAbstract_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, TestAbstract_0);
#endif
            args = new Type[]{};
            method = type.GetMethod("TestVirtual", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, TestVirtual_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, TestVirtual_1);
#endif
            args = new Type[]{};
            method = type.GetMethod("TestField", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, TestField_2_Neo);
#else
            app.RegisterCLRMethodRedirection(method, TestField_2);
#endif
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.InterfaceTest)};
            method = type.GetMethod("Test3", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Test3_3_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Test3_3);
#endif
            args = new Type[]{typeof(System.Int64).MakeByRefType()};
            method = type.GetMethod("TestLongRef", flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, TestLongRef_4_Neo);
#else
            app.RegisterCLRMethodRedirection(method, TestLongRef_4);
#endif

            field = type.GetField("TestVal2", flag);
            app.RegisterCLRFieldGetter(field, get_TestVal2_0);
            app.RegisterCLRFieldSetter(field, set_TestVal2_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_TestVal2_0, AssignFromStack_TestVal2_0);
            field = type.GetField("staticField", flag);
            app.RegisterCLRFieldGetter(field, get_staticField_1);
            app.RegisterCLRFieldSetter(field, set_staticField_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_staticField_1, AssignFromStack_staticField_1);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Ctor_0_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Ctor_0);
#endif
            args = new Type[]{typeof(System.Int32), typeof(System.Int32)};
            method = type.GetConstructor(flag, null, args, null);
#if ENABLE_NEO_MODE
            app.RegisterCLRMethodRedirectionNeo(method, Ctor_1_Neo);
#else
            app.RegisterCLRMethodRedirection(method, Ctor_1);
#endif

        }


#if ENABLE_NEO_MODE
        static void TestAbstract_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            instance_of_this_method.TestAbstract();
        }
#else
        static StackObject* TestAbstract_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestAbstract();

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void TestVirtual_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            instance_of_this_method.TestVirtual();
        }
#else
        static StackObject* TestVirtual_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestVirtual();

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void TestField_2_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            instance_of_this_method.TestField();
        }
#else
        static StackObject* TestField_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.ClassInheritanceTest instance_of_this_method = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.TestField();

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void Test3_3_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            ILRuntimeTest.TestFramework.InterfaceTest @ins = (ILRuntimeTest.TestFramework.InterfaceTest)ILIntepreter.ReadNeoReference(__frameBase, ref __curPrim, __mStack);
            ILRuntimeTest.TestFramework.ClassInheritanceTest.Test3(@ins);
        }
#else
        static StackObject* Test3_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            ILRuntimeTest.TestFramework.InterfaceTest @ins = (ILRuntimeTest.TestFramework.InterfaceTest)typeof(ILRuntimeTest.TestFramework.InterfaceTest).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ILRuntimeTest.TestFramework.ClassInheritanceTest.Test3(@ins);

            return __ret;
        }
#endif

#if ENABLE_NEO_MODE
        static void TestLongRef_4_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            System.Int64 @i = ILIntepreter.ReadNeoInt64(__frameBase, ref __curPrim);
            ILRuntimeTest.TestFramework.ClassInheritanceTest.TestLongRef(ref @i);
        }
#else
        static StackObject* TestLongRef_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 1;

            ptr_of_this_method = __esp - 1;
            System.Int64 @i = __intp.RetriveInt64(ptr_of_this_method, __mStack);


            ILRuntimeTest.TestFramework.ClassInheritanceTest.TestLongRef(ref @i);

            ptr_of_this_method = __esp - 1;
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                        ___dst->ObjectType = ObjectTypes.Long;
                        *(long*)&___dst->Value = @i;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @i;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @i);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @i;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @i);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.Int64[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @i;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            return __ret;
        }
#endif


        static object get_TestVal2_0(ref object o)
        {
            return ((ILRuntimeTest.TestFramework.ClassInheritanceTest)o).TestVal2;
        }

        static StackObject* CopyToStack_TestVal2_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((ILRuntimeTest.TestFramework.ClassInheritanceTest)o).TestVal2;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_TestVal2_0(ref object o, object v)
        {
            ((ILRuntimeTest.TestFramework.ClassInheritanceTest)o).TestVal2 = (System.Int32)v;
        }

        static StackObject* AssignFromStack_TestVal2_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @TestVal2 = ptr_of_this_method->Value;
            ((ILRuntimeTest.TestFramework.ClassInheritanceTest)o).TestVal2 = @TestVal2;
            return ptr_of_this_method;
        }

        static object get_staticField_1(ref object o)
        {
            return ILRuntimeTest.TestFramework.ClassInheritanceTest.staticField;
        }

        static StackObject* CopyToStack_staticField_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ILRuntimeTest.TestFramework.ClassInheritanceTest.staticField;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_staticField_1(ref object o, object v)
        {
            ILRuntimeTest.TestFramework.ClassInheritanceTest.staticField = (System.IDisposable)v;
        }

        static StackObject* AssignFromStack_staticField_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.IDisposable @staticField = (System.IDisposable)typeof(System.IDisposable).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (ILRuntime.CLR.Utils.Extensions.TypeFlags)0);
            ILRuntimeTest.TestFramework.ClassInheritanceTest.staticField = @staticField;
            return ptr_of_this_method;
        }


#if ENABLE_NEO_MODE
        static void Ctor_0_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            if (isNewObj)
            {
                __curPrim += 4; // Skip retRefBase
            }
            else
            {
                // TODO: Constructor binding for non-newObj (e.g. value type init) in Neo
            }
            ILRuntimeTest.TestFramework.ClassInheritanceTest result_of_this_method = new ILRuntimeTest.TestFramework.ClassInheritanceTest();
            if (__retDst != null)
            {
                if (__retRefBase >= __mStack.Count)
                    __mStack.Add(result_of_this_method);
                else
                    __mStack[__retRefBase] = result_of_this_method;
                *(int*)__retDst = __retRefBase;
            }
        }
#else
        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = __esp - 0;

            if(!isNewObj) 
            {
                StackObject* ptr_of_this_method = __esp - 1;
                ILRuntimeTest.TestFramework.ClassInheritanceTest __this = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));
                __intp.Free(ptr_of_this_method);
                if(__this is CrossBindingAdaptorType)
                    return __esp - 1;
            }
            var result_of_this_method = new ILRuntimeTest.TestFramework.ClassInheritanceTest();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif

#if ENABLE_NEO_MODE
        static void Ctor_1_Neo(ILIntepreter __intp, byte* __frameBase, AutoList __mStack, CLRMethod __method, bool isNewObj, byte* __retDst, int __retRefBase)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            int __curPrim = 0;
            if (isNewObj)
            {
                __curPrim += 4; // Skip retRefBase
            }
            else
            {
                // TODO: Constructor binding for non-newObj (e.g. value type init) in Neo
            }
            System.Int32 @a = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            System.Int32 @b = (System.Int32)ILIntepreter.ReadNeoInt32(__frameBase, ref __curPrim);
            ILRuntimeTest.TestFramework.ClassInheritanceTest result_of_this_method = new ILRuntimeTest.TestFramework.ClassInheritanceTest(@a, @b);
            if (__retDst != null)
            {
                if (__retRefBase >= __mStack.Count)
                    __mStack.Add(result_of_this_method);
                else
                    __mStack[__retRefBase] = result_of_this_method;
                *(int*)__retDst = __retRefBase;
            }
        }
#else
        static StackObject* Ctor_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = __esp - 2;
            ptr_of_this_method = __esp - 1;
            System.Int32 @b = ptr_of_this_method->Value;

            ptr_of_this_method = __esp - 2;
            System.Int32 @a = ptr_of_this_method->Value;


            if(!isNewObj) 
            {
                ptr_of_this_method = __esp - 3;
                ILRuntimeTest.TestFramework.ClassInheritanceTest __this = (ILRuntimeTest.TestFramework.ClassInheritanceTest)typeof(ILRuntimeTest.TestFramework.ClassInheritanceTest).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));
                __intp.Free(ptr_of_this_method);
                if(__this is CrossBindingAdaptorType)
                    return __esp - 3;
            }
            var result_of_this_method = new ILRuntimeTest.TestFramework.ClassInheritanceTest(@a, @b);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
#endif


    }
}
