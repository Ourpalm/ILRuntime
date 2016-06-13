using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntime.Runtime.Intepreter
{
    public class ILRuntimeException : Exception
    {
        string stackTrace;
        internal ILRuntimeException(string message, ILIntepreter intepreter, Exception innerException = null)
            : base(message, innerException)
        {
            stackTrace = Debugger.DebugService.Instance.GetStackTrance(intepreter);
        }

        public override string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }
    }
}
