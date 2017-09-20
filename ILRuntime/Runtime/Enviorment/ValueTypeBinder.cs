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
        CLRType clrType;
        int totalFieldCnt;

        public CLRType CLRType
        {
            get { return clrType; }
            set
            {
                if (clrType == null)
                {
                    clrType = value;
                    totalFieldCnt = clrType.TotalFieldCount;
                }
                else
                    throw new NotSupportedException();
            }
        }
        public int TotalFieldCount
        {
            get
            {
                return totalFieldCnt;
            }
        }

        public abstract void AssignFromStack(object ins, int fieldIdx, StackObject* esp, Enviorment.AppDomain appdomain, IList<object> managedStack);

        public abstract void CopyValueTypeToStack(object ins, StackObject* ptr, IList<object> mStack);

        public abstract object ToObject(StackObject* esp, Enviorment.AppDomain appdomain, IList<object> managedStack);

        public virtual void RegisterCLRRedirection(Enviorment.AppDomain appdomain)
        {

        }
    }
}
