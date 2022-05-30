using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Debugger.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntimeDebugEngine
{
    interface IThread
    {
        int ThreadID { get; }
        StackFrameInfo[] StackFrames { set; }
    }
}
