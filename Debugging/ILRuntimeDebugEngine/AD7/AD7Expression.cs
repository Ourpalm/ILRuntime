using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Expression : IDebugExpression2
    {
        private readonly ILProperty property;

        public AD7Expression(ILProperty property)
        {
            this.property = property;
        }

        public int Abort()
        {
            return Constants.E_NOTIMPL;
        }

        public int EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback)
        {
            return Constants.E_NOTIMPL;
        }

        public int EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback,
            out IDebugProperty2 ppResult)
        {
            ppResult = property;
            return Constants.S_OK;
        }
    }
}
