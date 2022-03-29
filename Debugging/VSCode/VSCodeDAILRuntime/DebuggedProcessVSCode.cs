// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ILRuntimeDebugEngine;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Ookii.CommandLine;

namespace ILRuntime.VSCode
{
    class DebuggedProcessVSCode : DebuggedProcess
    {
        ILRuntimeDebugAdapter da;
        public DebuggedProcessVSCode(ILRuntimeDebugAdapter da, string host, int port)
            : base(host, port)
        {
            this.da = da;
        }
        protected override void HandleBreakpointHit(IBreakPoint bp, IThread bpThread)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        protected override IThread HandleTheadStarted(int threadHash)
        {
            var t = new VSCodeThread(threadHash, null);
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
            throw new NotImplementedException();
        }
    }
}
