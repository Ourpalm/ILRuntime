using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Debugger.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntimeDebugEngine
{
    interface IStackFrame
    {
        int ThreadID { get; }
        int Index { get; }

        IProperty GetPropertyByName(string name);
    }
}
