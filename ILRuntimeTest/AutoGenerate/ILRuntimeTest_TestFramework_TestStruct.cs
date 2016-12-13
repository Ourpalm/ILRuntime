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
    unsafe class ILRuntimeTest_TestFramework_TestStruct
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo method;
            Type[] args;
            Type type = typeof(ILRuntimeTest.TestFramework.TestStruct);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestStruct).MakeByRefType()};
            method = type.GetMethod("DoTest", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DoTest_0);
            args = new Type[]{typeof(System.Int32).MakeByRefType()};
            method = type.GetMethod("DoTest", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DoTest_1);
            args = new Type[]{typeof(ILRuntimeTest.TestFramework.TestStruct)};
            method = type.GetMethod("DoTest2", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DoTest2_2);

        }

        static StackObject* DoTest_0(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.GetObjectAndResolveReference(p);
            ILRuntimeTest.TestFramework.TestStruct a = (ILRuntimeTest.TestFramework.TestStruct)StackObject.ToObject(p, domain, mStack);

            ILRuntimeTest.TestFramework.TestStruct.DoTest(ref a);

            p = ILIntepreter.Minus(esp, 1);
            switch(p->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var dst = *(StackObject**)&p->Value;
                        mStack[dst->Value] = a;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var obj = mStack[p->Value];
                        if(obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)obj)[p->ValueLow] = a;
                        }
                        else
                        {
                            var t = domain.GetType(obj.GetType()) as CLRType;
                            t.Fields[p->ValueLow].SetValue(obj, a);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = domain.GetType(p->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[p->ValueLow] = a;
                        }
                        else
                        {
                            ((CLRType)t).Fields[p->ValueLow].SetValue(null, a);
                        }
                    }
                    break;
            }

            return ret;
        }

        static StackObject* DoTest_1(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.GetObjectAndResolveReference(p);
            System.Int32 a = p->Value;

            ILRuntimeTest.TestFramework.TestStruct.DoTest(ref a);

            p = ILIntepreter.Minus(esp, 1);
            switch(p->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var dst = *(StackObject**)&p->Value;
                        dst->ObjectType = ObjectTypes.Integer;
                        dst->Value = a;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var obj = mStack[p->Value];
                        if(obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)obj)[p->ValueLow] = a;
                        }
                        else
                        {
                            var t = domain.GetType(obj.GetType()) as CLRType;
                            t.Fields[p->ValueLow].SetValue(obj, a);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = domain.GetType(p->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[p->ValueLow] = a;
                        }
                        else
                        {
                            ((CLRType)t).Fields[p->ValueLow].SetValue(null, a);
                        }
                    }
                    break;
            }

            return ret;
        }

        static StackObject* DoTest2_2(ILIntepreter intp, StackObject* esp, List<object> mStack, CLRMethod method)
        {
            ILRuntime.Runtime.Enviorment.AppDomain domain = intp.AppDomain;
            StackObject* p;
            StackObject* ret = ILIntepreter.Minus(esp, 1);
            p = ILIntepreter.Minus(esp, 1);
            ILRuntimeTest.TestFramework.TestStruct aaa = (ILRuntimeTest.TestFramework.TestStruct)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            ILRuntimeTest.TestFramework.TestStruct.DoTest2(aaa);

            return ret;
        }


    }
}
