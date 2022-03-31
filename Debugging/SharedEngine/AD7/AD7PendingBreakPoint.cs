using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

using ILRuntime.Runtime.Debugger.Protocol;
namespace ILRuntimeDebugEngine.AD7
{
    class AD7PendingBreakPoint : IBreakPoint, IDebugPendingBreakpoint2
    {
        private readonly AD7Engine _engine;
        private readonly IDebugBreakpointRequest2 _pBPRequest;
        private BP_REQUEST_INFO _bpRequestInfo;
        private AD7BoundBreakpoint _boundBreakpoint;
        private AD7ErrorBreakpoint _errorBreakpoint;
        CSBindBreakpoint bindRequest;
        public enum_BP_STATE State { get; private set; }
        public override bool IsBound { get { return _boundBreakpoint != null; } }
        public override string ConditionExpression { get { return _bpRequestInfo.bpCondition.bstrCondition; } }

        public AD7PendingBreakPoint(AD7Engine engine, IDebugBreakpointRequest2 pBPRequest)
        {
            var requestInfo = new BP_REQUEST_INFO[1];
            pBPRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_ALLFIELDS, requestInfo);
            _bpRequestInfo = requestInfo[0];
            _pBPRequest = pBPRequest;
            _engine = engine;

            //Enabled = true;

            var docPosition =
                (IDebugDocumentPosition2)Marshal.GetObjectForIUnknown(_bpRequestInfo.bpLocation.unionmember2);

            string documentName;
            docPosition.GetFileName(out documentName);
            var startPosition = new TEXT_POSITION[1];
            var endPosition = new TEXT_POSITION[1];
            docPosition.GetRange(startPosition, endPosition);

            DocumentName = documentName;
            StartLine = (int)startPosition[0].dwLine;
            StartColumn = (int)startPosition[0].dwColumn;

            EndLine = (int)endPosition[0].dwLine;
            EndColumn = (int)endPosition[0].dwColumn;
        }

        public int Bind()
        {
            TryBind();
            return Constants.S_OK;
        }

        public int CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            throw new NotImplementedException();
        }

        public int Delete()
        {
            if (_engine != null && _engine.DebuggedProcess != null)
                _engine.DebuggedProcess.SendDeleteBreakpoint(GetHashCode());
            State = enum_BP_STATE.BPS_DELETED;
            return Constants.S_OK;
        }

        public int Enable(int fEnable)
        {
            State = fEnable != 0 ? enum_BP_STATE.BPS_ENABLED : enum_BP_STATE.BPS_DISABLED;
            if (bindRequest != null)
                _engine.DebuggedProcess.SendSetBreakpointEnabled(GetHashCode(), State == enum_BP_STATE.BPS_ENABLED);
            return Constants.S_OK;
        }

        public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            if (_boundBreakpoint != null)
                ppEnum = new AD7BoundBreakpointsEnum(new[] { _boundBreakpoint });
            else
                ppEnum = null;
            return Constants.S_OK;
        }

        public int EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            if (_errorBreakpoint != null)
                ppEnum = new AD7ErrorBreakpointsEnum(new[] { _errorBreakpoint });
            else
                ppEnum = null;
            return Constants.S_OK;
        }

        public int GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
        {
            ppBPRequest = _pBPRequest;
            return Constants.S_OK;
        }

        public int GetState(PENDING_BP_STATE_INFO[] pState)
        {
            pState[0].state = (enum_PENDING_BP_STATE)State;
            return Constants.S_OK;
        }

        public int SetCondition(BP_CONDITION bpCondition)
        {
            _bpRequestInfo.bpCondition = bpCondition;
            _engine.DebuggedProcess.SendSetBreakpointCondition(this.GetHashCode(), (BreakpointConditionStyle)bpCondition.styleCondition, bpCondition.bstrCondition);
            return Constants.S_OK;
        }

        public int SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        public int Virtualize(int fVirtualize)
        {
            return Constants.S_OK;
        }

        public override bool TryBind()
        {
            try
            {
                if (bindRequest == null)
                {
                    bindRequest = CreateBindRequest(State == enum_BP_STATE.BPS_ENABLED, (BreakpointConditionStyle)_bpRequestInfo.bpCondition.styleCondition, _bpRequestInfo.bpCondition.bstrCondition);
                }

                _engine.DebuggedProcess.SendBindBreakpoint(bindRequest);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        public override void Bound(BindBreakpointResults result)
        {
            if (result == BindBreakpointResults.OK)
            {
                _boundBreakpoint = new AD7BoundBreakpoint(_engine, this);
                _errorBreakpoint = null;
                _engine.Callback.BoundBreakpoint(this);
            }
            else
            {
                _errorBreakpoint = new AD7ErrorBreakpoint(_engine, this);
                _boundBreakpoint = null;
                _engine.Callback.ErrorBreakpoint(_errorBreakpoint);
            }
        }
    }
}