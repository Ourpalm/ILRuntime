using ILRuntime.Runtime.Debugger.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntimeDebugEngine
{
    internal interface IBreakPoint
    {
        string DocumentName { get; }
        string ConditionExpression { get; }
        int StartLine { get; }
        int StartColumn { get; }
        bool IsBound { get; }
        bool TryBind();
        void Bound(BindBreakpointResults res);
    }
}
