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
    public unsafe class Fixed64Binder : ValueTypeBinder<Fixed64>
    {
        public override unsafe void AssignFromStack(ref Fixed64 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            ins = new Fixed64(*(long*)&v->Value);
        }

        public override unsafe void CopyValueTypeToStack(ref Fixed64 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            *(long*)&v->Value = ins.RawValue;
        }
        public override void RegisterCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(Fixed64);
            args = new Type[] { typeof(long) };
            method = type.GetConstructor(flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, NewFixed64);
        }

        StackObject* NewFixed64(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            StackObject* ret;
            if (isNewObj)
            {
                ret = esp;
                Fixed64 vec;
                var ptr = ILIntepreter.Minus(esp, 1);
                var val = *(long*)&ptr->Value;
                vec = new Fixed64(val);
                PushFixed64(ref vec, intp, ptr, mStack);
            }
            else
            {
                ret = ILIntepreter.Minus(esp, 2);
                var instance = ILIntepreter.GetObjectAndResolveReference(ret);
                var dst = *(StackObject**)&instance->Value;
                var f = ILIntepreter.Minus(dst, 1);
                var v = ILIntepreter.Minus(esp, 1);
                var val = *(long*)&v->Value;
                Fixed64 vec = new Fixed64(val);
                *(long*)&f->Value = vec.RawValue;
            }
            return ret;
        }

        public static void ParseFixed64(out Fixed64 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
        {
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = *(StackObject**)&a->Value;
                long value = *(long*)&ILIntepreter.Minus(src, 1)->Value;
                vec = new Fixed64(value);
                intp.FreeStackValueType(ptr);
            }
            else
            {
                vec = (Fixed64)StackObject.ToObject(a, intp.AppDomain, mStack);
                intp.Free(ptr);
            }
        }

        public void PushFixed64(ref Fixed64 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
        {
            intp.AllocValueType(ptr, CLRType);
            var dst = *((StackObject**)&ptr->Value);
            CopyValueTypeToStack(ref vec, dst, mStack);
        }
    }

    public unsafe class FixedVector2Binder : ValueTypeBinder<Fixed64Vector2>
    {
        public override unsafe void AssignFromStack(ref Fixed64Vector2 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            ins.x = GetFixed64(v, mStack);
            v = ILIntepreter.Minus(ptr, 2);
            ins.y = GetFixed64(v, mStack);
        }

        private Fixed64 GetFixed64(StackObject* ptr, IList<object> mStack)
        {
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            Fixed64 res;
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = *(StackObject**)&a->Value;
                var val = *(long*)&ILIntepreter.Minus(src, 1)->Value;
                res = new Fixed64(val);
            }
            else
            {
                var raw = (Fixed64)StackObject.ToObject(a, domain, mStack);
                res = raw;
            }
            return res;
        }

        public override unsafe void CopyValueTypeToStack(ref Fixed64Vector2 ins, StackObject* ptr, IList<object> mStack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            var fix64Ptr = GetFix64Ptr(v, mStack);
            *(long*)&fix64Ptr->Value = ins.x.RawValue;

            v = ILIntepreter.Minus(ptr, 2);
            fix64Ptr = GetFix64Ptr(v, mStack);
            *(long*)&fix64Ptr->Value = ins.y.RawValue;
        }

        private StackObject* GetFix64Ptr(StackObject* ptr, IList<object> mStack)
        {
            StackObject* res = null;
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = *(StackObject**)&a->Value;
                res = ILIntepreter.Minus(src, 1);
            }
            return res;
        }

        public override void RegisterCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(Fixed64Vector2);
            args = new Type[] { typeof(int), typeof(int) };
            method = type.GetConstructor(flag, null, args, null);
            appdomain.RegisterCLRMethodRedirection(method, NewFixed64Vector2);
        }

        StackObject* NewFixed64Vector2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            StackObject* ret;
            if (isNewObj)
            {
                ret = ILIntepreter.Minus(esp, 1);
                Fixed64Vector2 vec;
                var ptr = ILIntepreter.Minus(esp, 1);
                var y = ptr->Value;
                ptr = ILIntepreter.Minus(esp, 2);
                var x = ptr->Value;
                vec = new Fixed64Vector2(x, y);

                PushVector2(ref vec, intp, ptr, mStack);
            }
            else
            {
                ret = ILIntepreter.Minus(esp, 3);
                var instance = ILIntepreter.GetObjectAndResolveReference(ret);
                var dst = *(StackObject**)&instance->Value;
                var ptr = ILIntepreter.Minus(dst, 1);
                var f = GetFix64Ptr(ptr, mStack);
                var v = ILIntepreter.Minus(esp, 2);
                Fixed64 fix = new Fixed64(v->Value);
                *(long*)&f->Value = fix.RawValue;

                ptr = ILIntepreter.Minus(dst, 2);
                f = GetFix64Ptr(ptr, mStack);
                v = ILIntepreter.Minus(esp, 1);
                fix = new Fixed64(v->Value);
                *(long*)&f->Value = fix.RawValue;
            }
            return ret;
        }

        public void ParseFixed64Vector2(out Fixed64Vector2 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
        {
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = *(StackObject**)&a->Value;
                var fixPtr = ILIntepreter.Minus(src, 1);
                vec.x = GetFixed64(fixPtr, mStack);
                fixPtr = ILIntepreter.Minus(src, 2);
                vec.y = GetFixed64(fixPtr, mStack);

                intp.FreeStackValueType(ptr);
            }
            else
            {
                vec = (Fixed64Vector2)StackObject.ToObject(a, intp.AppDomain, mStack);
                intp.Free(ptr);
            }
        }

        public void PushVector2(ref Fixed64Vector2 vec, ILIntepreter intp, StackObject* ptr, IList<object> mStack)
        {
            intp.AllocValueType(ptr, CLRType);
            var dst = *((StackObject**)&ptr->Value);
            CopyValueTypeToStack(ref vec, dst, mStack);
        }
    }
    public unsafe class TestStructABinder : ValueTypeBinder<TestStructA>
    {
        public override unsafe void CopyValueTypeToStack(ref TestStructA ins, StackObject* ptr, IList<object> stack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            *(long*)&v->Value = ins.value;
        }

        public override unsafe void AssignFromStack(ref TestStructA ins, StackObject* ptr, IList<object> stack)
        {
            var v = ILIntepreter.Minus(ptr, 1);
            ins.value = *(long*)&v->Value;
        }

        public static TestStructA OnlyParseTestStructA(StackObject* ptr, IList<object> mStack)
        {
            TestStructA value;
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            //Debug.Log("执行ParseFP的时候指针地址类型：" + a->ObjectType);

            switch (a->ObjectType)
            {
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var src = ILIntepreter.ResolveReference(a);
                        value.value = *(long*)&ILIntepreter.Minus(src, 1)->Value;
                        //intp.FreeStackValueType(ptr);
                    }
                    break;
                case ObjectTypes.Object:
                    {
                        value = (TestStructA)mStack[a->Value];
                    }
                    break;
                case ObjectTypes.Integer:
                    {
                        value.value = a->Value;
                    }
                    break;
                case ObjectTypes.Long:
                    {
                        value.value = *(long*)&a->Value;
                    }
                    break;
                //case ObjectTypes.Float:
                //    {
                //        value.a = *(float*)&a->Value;
                //    }
                //    break;
                //case ObjectTypes.Double:
                //    {
                //        value.a = *(double*)&a->Value;
                //    }
                //    break;
                default:
                    {
                        throw new NotSupportedException("执行ParseTestStructA的时候出错，指针地址类型超出预期,需补充：" + a->ObjectType);
                        value.value = -999999;
                    }
                    break;
            }

            return value;
        }

        public static void OnlyPushTestStructA(TestStructA value, StackObject* ptr)
        {
            var a = ILIntepreter.GetObjectAndResolveReference(ptr);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var src = ILIntepreter.ResolveReference(a);
                *(long*)&ILIntepreter.Minus(src, 1)->Value = value.value;
                //intp.FreeStackValueType(ptr);
            }
            else
            {
                throw new NotSupportedException("执行SetTestStructA的时候出错，指针地址不是valueType");
                //intp.Free(ptr);
            }
        }
    }


    public unsafe class TestStructBBinder : ValueTypeBinder<TestStructB>
    {
        public override unsafe void AssignFromStack(ref TestStructB ins, StackObject* ptr, IList<object> mStack)
        {
            //Debug.Log("Assign From Stack");
            var v = ILIntepreter.Minus(ptr, 1);
            ins.m1 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 2);
            ins.m2 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 3);
            ins.m3 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 4);
            ins.m4 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 5);
            ins.m5 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 6);
            ins.m6 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 7);
            ins.m7 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 8);
            ins.m8 = TestStructABinder.OnlyParseTestStructA(v, mStack);
            v = ILIntepreter.Minus(ptr, 9);
            ins.m9 = TestStructABinder.OnlyParseTestStructA(v, mStack);
        }

        public override unsafe void CopyValueTypeToStack(ref TestStructB ins, StackObject* ptr, IList<object> mStack)
        {
            //Debug.Log("Copy Value To Stack");
            var v = ILIntepreter.Minus(ptr, 1);
            TestStructABinder.OnlyPushTestStructA(ins.m1, v);
            v = ILIntepreter.Minus(ptr, 2);
            TestStructABinder.OnlyPushTestStructA(ins.m2, v);
            v = ILIntepreter.Minus(ptr, 3);
            TestStructABinder.OnlyPushTestStructA(ins.m3, v);
            v = ILIntepreter.Minus(ptr, 4);
            TestStructABinder.OnlyPushTestStructA(ins.m4, v);
            v = ILIntepreter.Minus(ptr, 5);
            TestStructABinder.OnlyPushTestStructA(ins.m5, v);
            v = ILIntepreter.Minus(ptr, 6);
            TestStructABinder.OnlyPushTestStructA(ins.m6, v);
            v = ILIntepreter.Minus(ptr, 7);
            TestStructABinder.OnlyPushTestStructA(ins.m7, v);
            v = ILIntepreter.Minus(ptr, 8);
            TestStructABinder.OnlyPushTestStructA(ins.m8, v);
            v = ILIntepreter.Minus(ptr, 9);
            TestStructABinder.OnlyPushTestStructA(ins.m9, v);
        }
    }

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