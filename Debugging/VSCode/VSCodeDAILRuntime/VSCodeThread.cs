// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ILRuntime.Runtime.Debugger;
using ILRuntimeDebugEngine;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Ookii.CommandLine;

namespace ILRuntime.VSCode
{
    class VSCodeThread : IThread
    {
        Thread thread;
        StackFrameInfo[] frames;

        public Thread Thread => thread;

        public StackFrameInfo[] StackFrames { set { frames = value; } }

        public VSCodeThread(int id, string threadName)
        {
            thread = new Thread(id, threadName);
        }
    }
}
