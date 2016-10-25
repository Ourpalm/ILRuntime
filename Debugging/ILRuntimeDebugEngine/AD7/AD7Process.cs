using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Process : IDebugProcessEx2, IDebugProcess2
    {
        Guid guid = Guid.NewGuid();
        IDebugPort2 port;
        public AD7Process(IDebugPort2 port)
        {
            this.port = port;
        }
        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            throw new NotImplementedException();
        }

        public int CanDetach()
        {
            throw new NotImplementedException();
        }

        public int CauseBreak()
        {
            throw new NotImplementedException();
        }

        public int Detach()
        {
            throw new NotImplementedException();
        }

        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            throw new NotImplementedException();
        }

        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            throw new NotImplementedException();
        }

        public int GetAttachedSessionName(out string pbstrSessionName)
        {
            throw new NotImplementedException();
        }

        public int GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo)
        {
            pProcessInfo[0].Fields = Fields;
            pProcessInfo[0].bstrTitle = "ttt";
            pProcessInfo[0].bstrBaseName = "ttt";
            pProcessInfo[0].bstrFileName = "ttt";
            return Constants.S_OK;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
        {
            pbstrName = "ttt";
            return Constants.S_OK;
        }

        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            pProcessId[0].guidProcessId = guid;
            pProcessId[0].ProcessIdType = 1;   
            return 0;
        }

        public int GetPort(out IDebugPort2 ppPort)
        {
            ppPort = port;
            return Constants.S_OK;
        }

        public int GetProcessId(out Guid pguidProcessId)
        {
            pguidProcessId = guid;
            return 0;
        }

        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            throw new NotImplementedException();
        }

        public int Terminate()
        {
            return 0;
        }

        int IDebugProcessEx2.AddImplicitProgramNodes(ref Guid guidLaunchingEngine, Guid[] rgguidSpecificEngines, uint celtSpecificEngines)
        {
            return Constants.S_OK;
        }

        int IDebugProcessEx2.Attach(IDebugSession2 pSession)
        {
            return Constants.S_OK;
        }

        int IDebugProcessEx2.Detach(IDebugSession2 pSession)
        {
            throw new NotImplementedException();
        }
    }
}
