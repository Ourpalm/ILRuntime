using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.CLR.Method;

namespace ILRuntime.Runtime.Stack
{
    unsafe struct StackFrame
    {
        public ILMethod Method;
        public StackObject* LocalVarPointer;
        public StackObject* BasePointer;

    }
}
