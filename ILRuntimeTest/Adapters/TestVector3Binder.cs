using System;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntimeTest.TestFramework
{
    public unsafe class TestVector3Binder : ValueTypeBinder
    {
        public override void AssignFromStack(object ins, int fieldIdx, StackObject* esp, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IList<object> managedStack)
        {
            throw new NotImplementedException();
        }

        public override void CopyValueTypeToStack(object ins, StackObject* ptr, IList<object> mStack)
        {
            TestVector3 vec = (TestVector3)ins;
            var v = ILIntepreter.Minus(ptr, 1);
            *(float*)&v->Value = vec.X;
            v = ILIntepreter.Minus(ptr, 2);
            *(float*)&v->Value = vec.Y;
            v = ILIntepreter.Minus(ptr, 3);
            *(float*)&v->Value = vec.Z;
        }

        public override object ToObject(StackObject* ptr, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IList<object> managedStack)
        {
            TestVector3 vec = new TestVector3();
            var v = ILIntepreter.Minus(ptr, 1);
            vec.X = *(float*)&v->Value;
            v = ILIntepreter.Minus(ptr, 2);
            vec.Y = *(float*)&v->Value;
            v = ILIntepreter.Minus(ptr, 3);
            vec.Z = *(float*)&v->Value;

            return vec;
        }

        public override void RegisterCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(TestVector3);
            args = new Type[] { typeof(float), typeof(float), typeof(float) };
            method = type.GetConstructor(flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, NewVector3);

            args = new Type[] { typeof(TestVector3), typeof(TestVector3) };
            method = type.GetMethod("op_Addition", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, Vector3_Add);
        }

        public StackObject* Vector3_Add(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 2);
            var a = ILIntepreter.GetObjectAndResolveReference(ret);
            var b = ILIntepreter.GetObjectAndResolveReference(ILIntepreter.Minus(esp, 1));

            float x, y, z, x2, y2, z2;
            if(a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = *(StackObject**)&a->Value;
                x = *(float*)&ILIntepreter.Minus(src, 1)->Value;
                y = *(float*)&ILIntepreter.Minus(src, 2)->Value;
                z = *(float*)&ILIntepreter.Minus(src, 3)->Value;
            }
            else
            {
                var src = (TestVector3)mStack[a->Value];
                x = src.X;
                y = src.Y;
                z = src.Z;
            }

            if (b->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = *(StackObject**)&b->Value;
                x2 = *(float*)&ILIntepreter.Minus(src, 1)->Value;
                y2 = *(float*)&ILIntepreter.Minus(src, 2)->Value;
                z2 = *(float*)&ILIntepreter.Minus(src, 3)->Value;
            }
            else
            {
                var src = (TestVector3)mStack[b->Value];
                x2 = src.X;
                y2 = src.Y;
                z2 = src.Z;
            }

            intp.AllocValueType(ret, CLRType);
            var dst = *((StackObject**)&ret->Value);

            *(float*)&ILIntepreter.Minus(dst, 1)->Value = x + x2;
            *(float*)&ILIntepreter.Minus(dst, 2)->Value = y + y2;
            *(float*)&ILIntepreter.Minus(dst, 3)->Value = z + z2;

            return ret + 1;
        }

        public static StackObject* NewVector3(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 4);
            var instance = ILIntepreter.GetObjectAndResolveReference(ret);
            var dst = *(StackObject**)&instance->Value;
            var f = ILIntepreter.Minus(dst, 1);
            var v = ILIntepreter.Minus(esp, 3);
            *f = *v;

            f = ILIntepreter.Minus(dst, 2);
            v = ILIntepreter.Minus(esp, 2);
            *f = *v;

            f = ILIntepreter.Minus(dst, 3);
            v = ILIntepreter.Minus(esp, 1);
            *f = *v;

            return ret;
        }
    }
}