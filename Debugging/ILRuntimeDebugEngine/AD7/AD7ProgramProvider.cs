using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;

namespace ILRuntimeDebugEngine.AD7
{
    [ComVisible(true)]
    [Guid("795D4CA4-6029-48C2-B60E-84E7D6AA4458")]
    class AD7ProgramProvider : IDebugProgramProvider2
    {
        public int GetProviderProcessData(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, PROVIDER_PROCESS_DATA[] pProcess)
        {
            throw new NotImplementedException();
        }

        public int GetProviderProgramNode(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, ref Guid guidEngine, ulong programId, out IDebugProgramNode2 ppProgramNode)
        {
            throw new NotImplementedException();
        }

        public int SetLocale(ushort wLangID)
        {
            return 0;
        }

        public int WatchForProviderEvents(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 pEventCallback)
        {
            return 0;
        }
    }
}
