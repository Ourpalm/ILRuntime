using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Debugger
{
    public class DebugService
    {
        BreakPointContext curBreakpoint;
        static DebugService instance = new DebugService();

        public Action<string> OnBreakPoint;
        public static DebugService Instance { get { return instance; } }

        internal void Break(ILIntepreter intpreter, Exception ex = null)
        {
            BreakPointContext ctx = new BreakPointContext();
            ctx.Interpreter = intpreter;
            ctx.Exception = ex;

            curBreakpoint = ctx;

            if (OnBreakPoint != null)
                OnBreakPoint(ctx.DumpContext());
        }
    }
}
