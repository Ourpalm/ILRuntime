using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Debugger.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntimeDebugEngine
{
    interface IProperty
    {
        IProperty Parent { get; set; }
        VariableInfo Info { get; }
        VariableReference[] Parameters { get; set; }
        VariableReference GetVariableReference();
    }
}
