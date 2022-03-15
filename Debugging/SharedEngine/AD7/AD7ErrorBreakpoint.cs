using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7ErrorBreakpoint : IDebugErrorBreakpoint2, IDebugErrorBreakpointResolution2
    {
        private readonly AD7Engine _engine;
        private readonly AD7PendingBreakPoint _pendingBreakpoint;

        public AD7ErrorBreakpoint(AD7Engine engine, AD7PendingBreakPoint pendingBreakpoint)
        {
            _engine = engine;
            _pendingBreakpoint = pendingBreakpoint;
        }

        public int GetBreakpointResolution(out IDebugErrorBreakpointResolution2 ppBPResolution)
        {
            ppBPResolution = this;
            return Constants.S_OK;
        }

        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            ppPendingBreakpoint = _pendingBreakpoint;
            return Constants.S_OK;
        }

        public int GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return Constants.S_OK;
        }

        public int GetResolutionInfo(enum_BPERESI_FIELDS dwFields, BP_ERROR_RESOLUTION_INFO[] pBPResolutionInfo)
        {
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_PROGRAM) == enum_BPERESI_FIELDS.BPERESI_PROGRAM)
            {
                pBPResolutionInfo[0].dwFields |= enum_BPERESI_FIELDS.BPERESI_PROGRAM;
                pBPResolutionInfo[0].pProgram = _engine;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_TYPE) == enum_BPERESI_FIELDS.BPERESI_TYPE)
            {
                pBPResolutionInfo[0].dwFields |= enum_BPERESI_FIELDS.BPERESI_TYPE;
                pBPResolutionInfo[0].dwType = enum_BP_ERROR_TYPE.BPET_TYPE_WARNING;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_MESSAGE) == enum_BPERESI_FIELDS.BPERESI_MESSAGE)
            {
                pBPResolutionInfo[0].dwFields |= enum_BPERESI_FIELDS.BPERESI_MESSAGE;
                pBPResolutionInfo[0].bstrMessage = "Current code is not loaded yet";
            }

            return Constants.S_OK;
        }
    }
}
