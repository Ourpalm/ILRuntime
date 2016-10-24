using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Process : IDebugProcess2
    {
        Guid guid = Guid.NewGuid();
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
            pProcessInfo[0].bstrTitle = "Title";
            pProcessInfo[0].bstrBaseName = "ILRuntime Test";
            pProcessInfo[0].bstrFileName = "ffff.ttt";
            return Constants.S_OK;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
        {
            pbstrName = "ffff.ttt";
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
            throw new NotImplementedException();
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
    }
}
