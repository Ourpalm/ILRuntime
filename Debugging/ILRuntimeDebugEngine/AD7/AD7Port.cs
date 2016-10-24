using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;
namespace ILRuntimeDebugEngine.AD7
{
    class AD7Port : IDebugPort2
    {
        AD7PortSupplier supplier;
        IDebugPortRequest2 request;
        string portName;
        Guid guid = Guid.NewGuid();
        public AD7Port(AD7PortSupplier supplier, IDebugPortRequest2 request)
        {
            this.supplier = supplier;
            this.request = request;
            request.GetPortName(out portName);
        }

        public int EnumProcesses(out IEnumDebugProcesses2 ppEnum)
        {
            ppEnum = null;
            return Constants.S_OK;
        }

        public int GetPortId(out Guid pguidPort)
        {
            pguidPort = guid;
            return Constants.S_OK;
        }

        public int GetPortName(out string pbstrName)
        {
            pbstrName = portName;
            return Constants.S_OK;
        }

        public int GetPortRequest(out IDebugPortRequest2 ppRequest)
        {
            ppRequest = request;
            return Constants.S_OK;
        }

        public int GetPortSupplier(out IDebugPortSupplier2 ppSupplier)
        {
            ppSupplier = supplier;
            return Constants.S_OK;
        }

        public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
        {
            throw new NotImplementedException();
        }
    }
}
