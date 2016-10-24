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
    [Guid("552856E6-0F4F-4D79-83A5-9AF4A1A7A4B0")]
    public class AD7Engine : IDebugEngine2, IDebugEngineLaunch2
    {
        IDebugEventCallback2 callback;
        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason)
        {
            throw new NotImplementedException();
        }

        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 pProcess)
        {
            return 0;
        }

        int IDebugEngine2.CauseBreak()
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 pEvent)
        {
            return Constants.S_OK;
        }

        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.GetEngineId(out Guid pguidEngine)
        {
            throw new NotImplementedException();
        }

        int IDebugEngineLaunch2.LaunchSuspended(string pszServer, IDebugPort2 pPort, string pszExe, string pszArgs, string pszDir, string bstrEnv, string pszOptions, enum_LAUNCH_FLAGS dwLaunchFlags, uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 pCallback, out IDebugProcess2 ppProcess)
        {
            callback = pCallback;
            ppProcess = new AD7Process();
            IDebugEvent2 evt = new AD7LoadCompleteEvent();
            uint attribute;
            evt.GetAttributes(out attribute);
            callback.Event(this, ppProcess, null, null, evt, new Guid(AD7LoadCompleteEvent.IID), attribute);
            return 0;
        }

        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException)
        {
            throw new NotImplementedException();
        }

        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 pProcess)
        {
            return 0;
        }

        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.SetLocale(ushort wLangID)
        {
            return 0;
        }

        int IDebugEngine2.SetMetric(string pszMetric, object varValue)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot)
        {
            return 0;
        }

        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 pProcess)
        {
            IDebugEvent2 evt = new AD7.AD7ProgramDestroyEvent(0);
            uint attribute;
            evt.GetAttributes(out attribute);
            callback.Event(this, pProcess, null, null, evt, new Guid(AD7ProgramDestroyEvent.IID), attribute);
            return Constants.S_OK;
        }
    }
}
