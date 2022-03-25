using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7BoundBreakpoint : IDebugBoundBreakpoint2, IDebugBreakpointResolution2
    {
        private readonly AD7Engine _engine;
        private readonly AD7PendingBreakPoint _pendingBreakpoint;

        public AD7BoundBreakpoint(AD7Engine engine, AD7PendingBreakPoint pendingBreakpoint)
        {
            _engine = engine;
            _pendingBreakpoint = pendingBreakpoint;
        }

        public int Delete()
        {
            return Constants.S_OK;
        }

        public int Enable(int fEnable)
        {
            return Constants.S_OK;
        }

        public int GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution)
        {
            ppBPResolution = this;
            return Constants.S_OK;
        }

        public int GetHitCount(out uint pdwHitCount)
        {
            pdwHitCount = 0;
            return Constants.S_OK;
        }

        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            ppPendingBreakpoint = _pendingBreakpoint;
            return Constants.S_OK;
        }

        public int GetState(enum_BP_STATE[] pState)
        {
            pState[0] = _pendingBreakpoint.State;
            return Constants.S_OK;
        }

        public int SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        public int SetHitCount(uint dwHitCount)
        {
            throw new NotImplementedException();
        }

        public int SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        public int GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return Constants.S_OK;
        }

        public int GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo)
        {
            if (dwFields == enum_BPRESI_FIELDS.BPRESI_ALLFIELDS)
            {
                pBPResolutionInfo[0].dwFields = enum_BPRESI_FIELDS.BPRESI_PROGRAM;
                pBPResolutionInfo[0].pProgram = _engine;
            }

            return Constants.S_OK;
        }
    }
}