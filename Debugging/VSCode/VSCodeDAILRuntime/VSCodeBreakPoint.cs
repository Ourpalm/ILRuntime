// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Debugger.Protocol;
using ILRuntimeDebugEngine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Ookii.CommandLine;

namespace ILRuntime.VSCode
{
    class VSCodeBreakPoint : IBreakPoint
    {
        string condition;
        Breakpoint bp;
        DebuggedProcessVSCode debugged;
        CSBindBreakpoint bindRequest;

        public Breakpoint BreakPoint => bp;

        public VSCodeBreakPoint(DebuggedProcessVSCode debugged, Source source, int line, int column, string condition)
        {
            this.debugged = debugged;
            bp = new Breakpoint(false);
            bp.Id = GetHashCode();
            bp.Source = source;
            bp.Line = line;
            bp.Column = column;
            this.condition = condition;
            DocumentName = source.Path;
            StartLine = line - 1;
            StartColumn = column > 0 ? column - 1 : 0;
        }
        public override string ConditionExpression => condition;

        public override bool IsBound => bp.Verified;

        public override void Bound(BindBreakpointResults res)
        {
            switch (res)
            {
                case BindBreakpointResults.OK:
                    bp.Verified = true;
                    break;
                case BindBreakpointResults.CodeNotFound:
                case BindBreakpointResults.TypeNotFound:
                    bp.Verified = false;
                    bp.Message = "Code not loaded";
                    break;                    
            }
            debugged.Adapter.Protocol.SendEvent(new BreakpointEvent()
            {
                Breakpoint = bp,
                Reason = BreakpointEvent.ReasonValue.Changed
            });
        }

        public override bool TryBind()
        {
            try
            {
                if (bindRequest == null)
                {
                    bindRequest = CreateBindRequest(true, BreakpointConditionStyle.None, null);
                }

                debugged.SendBindBreakpoint(bindRequest);
                return true;
            }
            catch (Exception ex)
            {
                bp.Verified = false;
                bp.Message = ex.ToString();
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return false;
        }
    }
}
