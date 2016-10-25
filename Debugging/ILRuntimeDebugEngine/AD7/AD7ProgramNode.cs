using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7ProgramNode : IDebugProgramNode2
    {
        Guid pID;
        public AD7ProgramNode(Guid pID)
        {
            this.pID = pID;
        }
        public int Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            throw new NotImplementedException();
        }

        public int DetachDebugger_V7()
        {
            throw new NotImplementedException();
        }

        public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            pbstrEngine = EngineConstants.EngineName;
            pguidEngine = new Guid(EngineConstants.EngineGUID);
            return Constants.S_OK;
        }

        public int GetHostMachineName_V7(out string pbstrHostMachineName)
        {
            throw new NotImplementedException();
        }

        public int GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName)
        {
            pbstrHostName = null;
           
            return Constants.S_OK;
        }

        public int GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            pHostProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
            pHostProcessId[0].guidProcessId = pID;
            return Constants.S_OK;
        }

        public int GetProgramName(out string pbstrProgramName)
        {
            throw new NotImplementedException();
        }
    }
}
