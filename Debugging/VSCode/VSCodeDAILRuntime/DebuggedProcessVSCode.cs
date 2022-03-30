// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ILRuntime.Runtime.Debugger;
using ILRuntimeDebugEngine;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Ookii.CommandLine;

namespace ILRuntime.VSCode
{
    class DebuggedProcessVSCode : DebuggedProcess
    {
        ILRuntimeDebugAdapter da;
        Dictionary<string, HashSet<VSCodeBreakPoint>> currentBreakpoints = new Dictionary<string, HashSet<VSCodeBreakPoint>>();

        public ILRuntimeDebugAdapter Adapter => da;
        public Dictionary<string, HashSet<VSCodeBreakPoint>> BreakPoints => currentBreakpoints;

        public VSCodeBreakPoint FindBreakpoint(string document, int line)
        {
            if (currentBreakpoints.TryGetValue(document, out HashSet<VSCodeBreakPoint> breakpoints))
            {
                foreach(var i in breakpoints)
                {
                    if (i.StartLine == line)
                        return i;
                }
            }
            return null;
        }

        public void UpdateBreakpoints(string document, HashSet<VSCodeBreakPoint> validBPs)
        {            
            if (currentBreakpoints.TryGetValue(document, out HashSet<VSCodeBreakPoint> breakpoints))
            {
                foreach(var i in breakpoints)
                {
                    if (!validBPs.Contains(i))
                    {
                        SendDeleteBreakpoint(i.GetHashCode());
                    }
                }
            }
            currentBreakpoints[document] = validBPs;
        }
        public DebuggedProcessVSCode(ILRuntimeDebugAdapter da, string host, int port)
            : base(host, port)
        {
            this.da = da;
        }
        protected override void HandleBreakpointHit(IBreakPoint bp, IThread bpThread)
        {
            VSCodeBreakPoint codeBp = (VSCodeBreakPoint)bp;
            da.Protocol.SendEvent(new StoppedEvent()
            {
                AllThreadsStopped = true,
                HitBreakpointIds = new List<int>() { codeBp.GetHashCode() },
                Reason = StoppedEvent.ReasonValue.Breakpoint,
                ThreadId = bpThread.ThreadID,                
            });             
        }

        protected override void HandleModuleLoaded(string moduleName)
        {
            Module module = new Module(moduleName, moduleName);
            da.Protocol.SendEvent(new ModuleEvent(ModuleEvent.ReasonValue.New, module));
        }

        protected override void HandleShowErrorMessageBox(string errorMsg)
        {
            da.Protocol.SendEvent(new OutputEvent()
            {
                Output = errorMsg,
                Category = OutputEvent.CategoryValue.Stderr,
            });
        }

        protected override void HandleStepCompelte(IThread bpThread)
        {
            da.Protocol.SendEvent(new StoppedEvent()
            {
                AllThreadsStopped = true,
                Reason = StoppedEvent.ReasonValue.Step,
                ThreadId = bpThread.ThreadID,
            });
        }

        protected override IThread HandleTheadStarted(int threadHash)
        {
            var t = new VSCodeThread(this, threadHash, null);
            da.Protocol.SendEvent(new ThreadEvent()
            {
                Reason = ThreadEvent.ReasonValue.Started,
                ThreadId = t.Thread.Id
            });
            return t;
        }

        protected override void HandleThreadEnded(IThread t)
        {
            da.Protocol.SendEvent(new ThreadEvent()
            {
                Reason = ThreadEvent.ReasonValue.Exited,
                ThreadId = ((VSCodeThread)t).Thread.Id
            }) ;
        }

        protected override IProperty CreateProperty(IStackFrame frame, VariableInfo info)
        {
            return new VSCodeVariable((VSCodeStackFrame)frame, info);
        }
    }
}
