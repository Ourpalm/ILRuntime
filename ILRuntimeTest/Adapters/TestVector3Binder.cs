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
        public override int TotalFieldCount => throw new NotImplementedException();

        public override unsafe void AssignFromStack(object ins, int fieldIdx, StackObject* esp, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IList<object> managedStack)
        {
            throw new NotImplementedException();
        }

        public override unsafe void CopyValueTypeToStack(object ins, StackObject* ptr, IList<object> mStack)
        {
            throw new NotImplementedException();
        }

        public override unsafe void InitializeValueTypeObject(StackObject* ptr)
        {
            throw new NotImplementedException();
        }
    }


}