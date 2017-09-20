using System;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntimeTest.TestFramework
{
    public class TestVector3Binder : ValueTypeBinder
    {
        public override unsafe void AssignFromStack(object ins, int fieldIdx, StackObject* esp, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IList<object> managedStack)
        {
            throw new NotImplementedException();
        }

        public override unsafe void CopyValueTypeToStack(object ins, StackObject* ptr, IList<object> mStack)
        {
            TestVector3 vec = (TestVector3)ins;
            var v = ILIntepreter.Minus(ptr, 1);
            *(float*)&v->Value = vec.X;
            v = ILIntepreter.Minus(ptr, 2);
            *(float*)&v->Value = vec.Y;
            v = ILIntepreter.Minus(ptr, 3);
            *(float*)&v->Value = vec.Z;
        }

        public override unsafe object ToObject(StackObject* ptr, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IList<object> managedStack)
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
            base.RegisterCLRRedirection(appdomain);
        }

        public unsafe static StackObject* NewVector3(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var instance = ILIntepreter.Minus(esp, 4);
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

            intp.FreeStackValueType(instance);
            return instance;
        }
    }
}