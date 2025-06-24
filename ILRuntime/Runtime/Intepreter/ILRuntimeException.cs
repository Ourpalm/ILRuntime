using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Intepreter
{
    public class ILRuntimeException : Exception
    {
        string message;
        string stackTrace;
        string thisInfo, localInfo;
        Exception innerException;
        internal unsafe ILRuntimeException(string message, ILIntepreter intepreter, CLR.Method.ILMethod method, StackObject* esp, Exception innerException = null)
            : base(message)
        
        {
            this.innerException = innerException;
            this.message = message;
            var ds = intepreter.AppDomain.DebugService;
            if (innerException is ILRuntimeException)
            {
                ILRuntimeException e = innerException as ILRuntimeException;
                stackTrace = e.stackTrace;
                thisInfo = e.thisInfo;
                localInfo = e.localInfo;
            }
            else
            {
                stackTrace = ds.GetStackTrace(intepreter);
                try
                {
                    if (method.HasThis)
                        thisInfo = ds.GetThisInfo(intepreter);
                    else
                        thisInfo = "";
                    localInfo = ds.GetLocalVariableInfo(intepreter);
                }
                catch
                {

                }
#if DEBUG && DETAILED_STACK_INFO
                stackTrace += "\n\n" + ds.DumpStack(esp, intepreter.Stack).ToString();
#endif
                if (innerException != null)
                {
                    stackTrace += "\n\nRethrown as Exception:" + innerException.ToString();
                }
            }

            if (ds.OnILRuntimeException != null) {
                ds.OnILRuntimeException(ToString());
            }
        }

        public override string Message
        {
            get
            {
                return message;
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

        public Exception GetInnerException()
        {
            return innerException;
        }

        public override string ToString()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(this.message);
            if (!string.IsNullOrEmpty(ThisInfo))
            {
                message.AppendLine("this:");
                message.AppendLine(ThisInfo);
            }
            message.AppendLine("Local Variables:");
            message.AppendLine(LocalInfo);
            message.AppendLine(stackTrace);
            if (InnerException != null)
                message.AppendLine(InnerException.ToString());
            return message.ToString();
        }
    }
}
