using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Enviorment
{
    public unsafe abstract class ValueTypeBinder
    {
        public abstract void AssignFromStack(object ins, int fieldIdx, StackObject* esp, Enviorment.AppDomain appdomain, IList<object> managedStack);
        public abstract void CopyValueTypeToStack(object ins, StackObject* ptr, IList<object> mStack);
        public abstract int TotalFieldCount { get; }
        public abstract void InitializeValueTypeObject(StackObject* ptr);
    }
}
