using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;
namespace ILRuntimeDebugEngine.AD7
{
    class AD7Port : IDebugDefaultPort2, IDebugPortEx2, IDebugPortNotify2
    {
        List<IDebugProcess2> processes = new List<IDebugProcess2>();
        AD7PortSupplier supplier;
        IDebugPortRequest2 request;
        string portName;
        Guid guid = Guid.NewGuid();
        public AD7Port(AD7PortSupplier supplier, IDebugPortRequest2 request)
        {
            this.supplier = supplier;
            this.request = request;
            request.GetPortName(out portName);
            AD7Process p = new AD7Process(this);
            processes.Add(p);
        }

        public int EnumProcesses(out IEnumDebugProcesses2 ppEnum)
        {
            ppEnum = new AD7ProcessEnum(processes.ToArray());
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
            ppProcess = processes[0];
            return Constants.S_OK;
        }

        public int GetPortNotify(out IDebugPortNotify2 ppPortNotify)
        {
            throw new NotImplementedException();
        }

        public int GetServer(out IDebugCoreServer3 ppServer)
        {
            throw new NotImplementedException();
        }

        public int QueryIsLocal()
        {
            throw new NotImplementedException();
        }

        int IDebugPortEx2.LaunchSuspended(string pszExe, string pszArgs, string pszDir, string bstrEnv, uint hStdInput, uint hStdOutput, uint hStdError, out IDebugProcess2 ppPortProcess)
        {
            throw new NotImplementedException();
        }

        int IDebugPortEx2.ResumeProcess(IDebugProcess2 pPortProcess)
        {
            throw new NotImplementedException();
        }

        int IDebugPortEx2.CanTerminateProcess(IDebugProcess2 pPortProcess)
        {
            throw new NotImplementedException();
        }

        int IDebugPortEx2.TerminateProcess(IDebugProcess2 pPortProcess)
        {
            throw new NotImplementedException();
        }

        int IDebugPortEx2.GetPortProcessId(out uint pdwProcessId)
        {
            throw new NotImplementedException();
        }

        int IDebugPortEx2.GetProgram(IDebugProgramNode2 pProgramNode, out IDebugProgram2 ppProgram)
        {
            throw new NotImplementedException();
        }

        int IDebugPortNotify2.AddProgramNode(IDebugProgramNode2 pProgramNode)
        {
            throw new NotImplementedException();
        }

        int IDebugPortNotify2.RemoveProgramNode(IDebugProgramNode2 pProgramNode)
        {
            throw new NotImplementedException();
        }
    }
}
