using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace ILRuntimeDebugEngine.AD7
{
    [ComVisible(true)]
    [Guid("C90C1326-0C6D-4334-BA0D-E94D0F91D440")]
    public class AD7PortSupplier : IDebugPortSupplier2
    {
        List<IDebugPort2> ports = new List<IDebugPort2>();
        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
        {
            ppPort = new AD7Port(this, pRequest);
            ports.Add(ppPort);
            return 0;
        }

        public int CanAddPort()
        {
            throw new NotImplementedException();
        }

        public int EnumPorts(out IEnumDebugPorts2 ppEnum)
        {
            ppEnum = new AD7PortEnum(ports.ToArray());
            return Constants.S_OK;
        }

        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
        {
            throw new NotImplementedException();
        }

        public int GetPortSupplierId(out Guid pguidPortSupplier)
        {
            pguidPortSupplier = new Guid(EngineConstants.PortSupplier);
            return 0;
        }

        public int GetPortSupplierName(out string pbstrName)
        {
            pbstrName = "ILRuntime PortSupplier";
            return Constants.S_OK;
        }

        public int RemovePort(IDebugPort2 pPort)
        {
            throw new NotImplementedException();
        }
    }
}
