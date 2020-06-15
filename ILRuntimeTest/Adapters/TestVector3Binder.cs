using System;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntimeTest.TestFramework
{
    public unsafe class TestVector3Binder : ValueTypeBinder<TestVector3>
    {
        public override unsafe void CopyValueTypeToStack(ref TestVector3 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            *(float*)&v->Value = ins.X;
            v = ILIntepreter.Minus(ptr, 2);
            *(float*)&v->Value = ins.Y;
            v = ILIntepreter.Minus(ptr, 3);
            *(float*)&v->Value = ins.Z;
        }

        public override unsafe void AssignFromStack(ref TestVector3 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            ins.X = *(float*)&v->Value;
            v = ILIntepreter.Minus(ptr, 2);
            ins.Y = *(float*)&v->Value;
            v = ILIntepreter.Minus(ptr, 3);
            ins.Z = *(float*)&v->Value;
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

            args = new Type[] { typeof(TestVector3), typeof(float) };
            method = type.GetMethod("op_Multiply", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, Vector3_Multiply);

            args = new Type[] { };
            method = type.GetMethod("get_One2", flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, Vector3_One2);
        }

        public StackObject* Vector3_One2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            TestVector3 res = TestVector3.One;
            PushVector3(ref res, intp, esp, mStack);
            return esp + 1;
        }

        public StackObject* Vector3_Add(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 2);
            var ptrB = ILIntepreter.Minus(esp, 1);
            var b = ILIntepreter.GetObjectAndResolveReference(ptrB);

            float x, y, z, x2, y2, z2;
            if (b->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = ILIntepreter.ResolveReference(b);
                x2 = *(float*)&ILIntepreter.Minus(src, 1)->Value;
                y2 = *(float*)&ILIntepreter.Minus(src, 2)->Value;
                z2 = *(float*)&ILIntepreter.Minus(src, 3)->Value;
                intp.FreeStackValueType(ptrB);
            }
            else
            {
                var src = (TestVector3)mStack[b->Value];
                x2 = src.X;
                y2 = src.Y;
                z2 = src.Z;
                intp.Free(ptrB);
            }

            var ptrA = ILIntepreter.Minus(esp, 2);
            var a = ILIntepreter.GetObjectAndResolveReference(ptrA);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = ILIntepreter.ResolveReference(a);
                x = *(float*)&ILIntepreter.Minus(src, 1)->Value;
                y = *(float*)&ILIntepreter.Minus(src, 2)->Value;
                z = *(float*)&ILIntepreter.Minus(src, 3)->Value;

                intp.FreeStackValueType(ptrA);
            }
            else
            {
                var src = (TestVector3)mStack[a->Value];
                x = src.X;
                y = src.Y;
                z = src.Z;
                intp.Free(ptrA);
            }

            intp.AllocValueType(ret, CLRType);
            var dst = ILIntepreter.ResolveReference(ret);

            *(float*)&ILIntepreter.Minus(dst, 1)->Value = x + x2;
            *(float*)&ILIntepreter.Minus(dst, 2)->Value = y + y2;
            *(float*)&ILIntepreter.Minus(dst, 3)->Value = z + z2;

            return ret + 1;
        }

        public StackObject* Vector3_Multiply(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 2);

            var ptr = ILIntepreter.Minus(esp, 1);
            var b = ILIntepreter.GetObjectAndResolveReference(ptr);

            float val = *(float*)&b->Value;

            float x, y, z;

            ptr = ILIntepreter.Minus(esp, 2);
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = ILIntepreter.ResolveReference(a);
                x = *(float*)&ILIntepreter.Minus(src, 1)->Value;
                y = *(float*)&ILIntepreter.Minus(src, 2)->Value;
                z = *(float*)&ILIntepreter.Minus(src, 3)->Value;
                intp.FreeStackValueType(ptr);
            }
            else
            {
                var src = (TestVector3)mStack[a->Value];
                x = src.X;
                y = src.Y;
                z = src.Z;
                intp.Free(ptr);
            }

            intp.AllocValueType(ret, CLRType);
            var dst = ILIntepreter.ResolveReference(ret);

            *(float*)&ILIntepreter.Minus(dst, 1)->Value = x * val;
            *(float*)&ILIntepreter.Minus(dst, 2)->Value = y * val;
            *(float*)&ILIntepreter.Minus(dst, 3)->Value = z * val;

            return ret + 1;
        }

        public StackObject* NewVector3(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            StackObject* ret = null;
            if (isNewObj)
            {
                ret = ILIntepreter.Minus(esp, 2);
                TestVector3 vec;
                var ptr = ILIntepreter.Minus(esp, 1);
                vec.Z = *(float*)&ptr->Value;
                ptr = ILIntepreter.Minus(esp, 2);
                vec.Y = *(float*)&ptr->Value;
                ptr = ILIntepreter.Minus(esp, 3);
                vec.X = *(float*)&ptr->Value;

                PushVector3(ref vec, intp, ptr, mStack);
            }
            else
            {
                ret = ILIntepreter.Minus(esp, 4);
                var instance = ILIntepreter.GetObjectAndResolveReference(ret);
                if (instance->ObjectType == ObjectTypes.ValueTypeObjectReference)
                {
                    var dst = ILIntepreter.ResolveReference(instance);
                    var f = ILIntepreter.Minus(dst, 1);
                    var v = ILIntepreter.Minus(esp, 3);
                    *f = *v;

                    f = ILIntepreter.Minus(dst, 2);
                    v = ILIntepreter.Minus(esp, 2);
                    *f = *v;

                    f = ILIntepreter.Minus(dst, 3);
                    v = ILIntepreter.Minus(esp, 1);
                    *f = *v;
                }
                else
                {

                }
            }
            return ret;
        }

        public void PushVector3(ref TestVector3 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
        {
            intp.AllocValueType(ptr, CLRType);
            var dst = ILIntepreter.ResolveReference(ptr);
            CopyValueTypeToStack(ref vec, dst, mStack);
        }
    }

    public unsafe class TestVectorStructBinder : ValueTypeBinder<TestVectorStruct>
    {
        public override unsafe void CopyValueTypeToStack(ref TestVectorStruct ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            v->Value = ins.A;
            v = ILIntepreter.Minus(ptr, 2);
            CopyValueTypeToStack(ref ins.B, v, mStack);
            v = ILIntepreter.Minus(ptr, 3);
            CopyValueTypeToStack(ref ins.C, v, mStack);
        }

        public override unsafe void AssignFromStack(ref TestVectorStruct ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            ins.A = v->Value;
            v = ILIntepreter.Minus(ptr, 2);
            AssignFromStack(ref ins.B, v, mStack);
            v = ILIntepreter.Minus(ptr, 3);
            AssignFromStack(ref ins.C, v, mStack);
        }
    }

    public unsafe class TestVectorStruct2Binder : ValueTypeBinder<TestVectorStruct2>
    {
        public override unsafe void CopyValueTypeToStack(ref TestVectorStruct2 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            CopyValueTypeToStack(ref ins.A, v, mStack);
            v = ILIntepreter.Minus(ptr, 2);
            CopyValueTypeToStack(ref ins.Vector, v, mStack);
        }

        public override unsafe void AssignFromStack(ref TestVectorStruct2 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            AssignFromStack(ref ins.A, v, mStack);
            v = ILIntepreter.Minus(ptr, 2);
            AssignFromStack(ref ins.Vector, v, mStack);
        }
    }

    public unsafe class KeyValuePairUInt32ILTypeInstanceBinder : ValueTypeBinder<KeyValuePair<UInt32, ILTypeInstance>>
    {
        public override unsafe void AssignFromStack(ref KeyValuePair<UInt32, ILTypeInstance> ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            var key = *(UInt32*)&v->Value;
            v = ILIntepreter.Minus(ptr, 2);
            object val = mStack[v->Value];
            ins = new KeyValuePair<uint, ILTypeInstance>(key, (ILTypeInstance)val);
        }

        public override unsafe void CopyValueTypeToStack(ref KeyValuePair<UInt32, ILTypeInstance> ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            *(UInt32*)&v->Value = ins.Key;
            v = ILIntepreter.Minus(ptr, 2);
            mStack[v->Value] = ins.Value;
        }
        public override void RegisterCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(KeyValuePair<UInt32, ILTypeInstance>);
            args = new Type[] { typeof(UInt32), typeof(ILTypeInstance) };
            method = type.GetConstructor(flag, null, args, null);
            //appdomain.RegisterCLRMethodRedirection(method, NewKV);
            //_appdomain_ = appdomain;
        }
    }
}