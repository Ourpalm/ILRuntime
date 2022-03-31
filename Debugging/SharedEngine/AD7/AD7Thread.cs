using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

using ILRuntime.Runtime.Debugger;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Thread : IDebugThread2, IThread
    {
        private readonly AD7Engine _engine;
        private string _threadName = "ILRuntime Thread";
        int threadHash;
        AD7StackFrame[] frames;

        public StackFrameInfo[] StackFrames
        {
            set
            {
                frames = new AD7StackFrame[value.Length];
                for(int i = 0; i < value.Length; i++)
                {
                    AD7StackFrame f = new AD7StackFrame(_engine, this, value[i], i);
                    frames[i] = f;
                }
            }
        }

        public int ThreadID => threadHash;

        public AD7Thread(AD7Engine engine, int threadHash)//ThreadMirror threadMirror)
        {
            _engine = engine;
            this.threadHash = threadHash;
        }

        public int CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            return Constants.S_FALSE;
        }

        public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            if (frames != null)
            {
                FRAMEINFO[] info = new FRAMEINFO[frames.Length];
                for (int i = 0; i < info.Length; i++)
                {
                    info[i] = frames[i].GetFrameInfo(dwFieldSpec);
                }
                ppEnum = new AD7FrameInfoEnum(info);
            }
            else
                ppEnum = null;
            return Constants.S_OK;
        }

        public int GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
        {
            throw new NotImplementedException();
        }

        public int GetName(out string pbstrName)
        {
            pbstrName = _threadName;
            return Constants.S_OK;
        }

        public int GetProgram(out IDebugProgram2 ppProgram)
        {
            ppProgram = _engine;
            return Constants.S_OK;
        }

        public int GetThreadId(out uint pdwThreadId)
        {
            pdwThreadId = (uint)threadHash;
            return Constants.S_OK;
        }

        public int GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
        {   
            return Constants.S_OK;
        }

        public int Resume(out uint pdwSuspendCount)
        {
            pdwSuspendCount = 0;
            return Constants.E_NOTIMPL;
        }

        public int SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            return Constants.S_FALSE;
        }

        public int SetThreadName(string pszName)
        {
            return Constants.E_NOTIMPL;
        }

        public int Suspend(out uint pdwSuspendCount)
        {
            pdwSuspendCount = 0;
            return Constants.E_NOTIMPL;
        }
    }
}
