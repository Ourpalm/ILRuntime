using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Intepreter
{
    public class ILRuntimeException : Exception
    {
        string stackTrace;
        string thisInfo, localInfo;
        internal ILRuntimeException(string message, ILIntepreter intepreter, CLR.Method.ILMethod method, Exception innerException = null)
            : base(message, innerException)
        
        {
            if (innerException is ILRuntimeException)
            {
                ILRuntimeException e = innerException as ILRuntimeException;
                stackTrace = e.stackTrace;
                thisInfo = e.thisInfo;
                localInfo = e.localInfo;
            }
            else
            {
                stackTrace = Debugger.DebugService.Instance.GetStackTrance(intepreter);
                if (method.HasThis)
                    thisInfo = Debugger.DebugService.Instance.GetThisInfo(intepreter);
                else
                    thisInfo = "";
                localInfo = Debugger.DebugService.Instance.GetLocalVariableInfo(intepreter);
            }
        }

        public override string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }

        public string ThisInfo
        {
            get { return thisInfo; }
        }

        public string LocalInfo
        {
            get
            {
                return localInfo;
            }
        }
    }
}
