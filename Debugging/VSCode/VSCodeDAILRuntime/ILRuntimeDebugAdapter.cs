using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Serialization;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ookii.CommandLine;
//using SampleDebugAdapter.Directives;
using static System.FormattableString;
using SysThread = System.Threading.Thread;
namespace ILRuntime.VSCode
{
    internal class ILRuntimeDebugAdapter : DebugAdapterBase
    {
        private int currentLineNum;
        private bool stopAtEntry;
        private int hyperStepSpeed;
        private bool stopped;

        private ReadOnlyCollection<string> lines;
        private int nextId = 999;

        private SysThread debugThread;
        private ManualResetEvent runEvent;
        private StoppedEvent.ReasonValue? stopReason;
        private int stopThreadId;

        private object syncObject = new object();

        internal ILRuntimeDebugAdapter(Stream stdIn, Stream stdOut)
        {
            base.InitializeProtocolClient(stdIn, stdOut);
        }

        internal void Run()
        {
            this.Protocol.Run();
        }
        #region Initialize/Disconnect

        protected override InitializeResponse HandleInitializeRequest(InitializeArguments arguments)
        {
            if (arguments.LinesStartAt1 == true)
                this.clientsFirstLine = 1;

            this.Protocol.SendEvent(new InitializedEvent());
            var res = new InitializeResponse()
            {
                SupportsConfigurationDoneRequest = true,
                SupportsSetVariable = false,
                SupportsConditionalBreakpoints = true,
                SupportsDebuggerProperties = true,
                SupportsSetExpression = true,
                SupportsExceptionOptions = true,
                SupportsExceptionConditions = true,
                SupportsExceptionInfoRequest = true,
                SupportsValueFormattingOptions = true,
                SupportsEvaluateForHovers = true,
            };

            return res;
        }

        protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments)
        {
            this.currentLineNum = this.lines.Count + 1;
            this.Continue(step: false);

            // Ensure the debug thread has stopped before sending the response
            this.debugThread.Join();

            return new DisconnectResponse();
        }

        #endregion

        #region Launch

        protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments)
        {
            string fileName = arguments.ConfigurationProperties.GetValueAsString("program");
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ProtocolException("Launch failed because launch configuration did not specify 'program'.");
            }

            fileName = Path.GetFullPath(fileName);
            if (!File.Exists(fileName))
            {
                throw new ProtocolException("Launch failed because 'program' files does not exist.");
            }

            this.stopAtEntry = arguments.ConfigurationProperties.GetValueAsBool("stopAtEntry") ?? false;
            this.hyperStepSpeed = arguments.ConfigurationProperties.GetValueAsInt("hyperStepSpeed") ?? 0;

            return new LaunchResponse();
        }

        #endregion

        #region Continue/Stepping

        protected override ConfigurationDoneResponse HandleConfigurationDoneRequest(ConfigurationDoneArguments arguments)
        {
            if (this.stopAtEntry)
            {
                // Clear the event so we'll break at startup
                this.RequestStop(StoppedEvent.ReasonValue.Step);
            }

            this.debugThread = new SysThread(this.DebugThreadProc);
            this.debugThread.Name = "Debug Loop Thread";
            this.debugThread.Start();

            return new ConfigurationDoneResponse();
        }

        protected override ContinueResponse HandleContinueRequest(ContinueArguments arguments)
        {
            this.Continue(step: false);
            return new ContinueResponse();
        }

        protected override StepInResponse HandleStepInRequest(StepInArguments arguments)
        {
            this.Continue(step: true);
            return new StepInResponse();
        }

        protected override StepOutResponse HandleStepOutRequest(StepOutArguments arguments)
        {
            this.Continue(step: true);
            return new StepOutResponse();
        }

        protected override NextResponse HandleNextRequest(NextArguments arguments)
        {
            this.Continue(step: true);
            return new NextResponse();
        }

        /// <summary>
        /// Continues "debugging". This will either step or run until the next breakpoint or until
        /// the end of the file.
        /// </summary>
        private void Continue(bool step)
        {
            lock (this.syncObject)
            {
                // Reset all state before continuing
                
                if (step)
                {
                    this.stopReason = StoppedEvent.ReasonValue.Step;
                }
                else
                {
                    this.stopReason = null;
                }
            }

            this.stopped = false;
            this.runEvent.Set();
        }

        protected override PauseResponse HandlePauseRequest(PauseArguments arguments)
        {
            this.RequestStop(StoppedEvent.ReasonValue.Pause);
            return new PauseResponse();
        }

        #endregion

        #region Debug Thread

        private void DebugThreadProc()
        {
            this.Protocol.SendEvent(new ExitedEvent(exitCode: 0));
            this.Protocol.SendEvent(new TerminatedEvent());
        }

        private void RequestStop(StoppedEvent.ReasonValue reason, int threadId = 0)
        {
            lock (this.syncObject)
            {
                this.stopReason = reason;
                this.stopThreadId = threadId;
                this.runEvent.Reset();
            }
        }

        private void SendOutput(string message)
        {
            string outputText = !String.IsNullOrEmpty(message) ? message.Trim() : String.Empty;

            this.Protocol.SendEvent(new OutputEvent()
            {
                Output = Invariant($"{outputText}{Environment.NewLine}"),
                Category = OutputEvent.CategoryValue.Stdout
            });
        }

        #endregion

        #region Breakpoints

        protected override SetBreakpointsResponse HandleSetBreakpointsRequest(SetBreakpointsArguments arguments)
        {
            //return this.BreakpointManager.HandleSetBreakpointsRequest(arguments);
            throw new NotImplementedException();
        }

        #endregion

        #region Debugger Properties

        internal bool? IsJustMyCodeOn { get; private set; }
        internal bool? IsStepFilteringOn { get; private set; }

        protected override SetDebuggerPropertyResponse HandleSetDebuggerPropertyRequest(SetDebuggerPropertyArguments arguments)
        {
            this.IsJustMyCodeOn = GetValueAsVariantBool(arguments.DebuggerProperties, "JustMyCodeStepping") ?? this.IsJustMyCodeOn;
            this.IsStepFilteringOn = GetValueAsVariantBool(arguments.DebuggerProperties, "EnableStepFiltering") ?? this.IsStepFilteringOn;

            return new SetDebuggerPropertyResponse();
        }

        /// <summary>
        /// Turns a debugger property value into a bool.
        /// Debugger properties use variants, so bools come as integers
        /// </summary>
        private static bool? GetValueAsVariantBool(Dictionary<string, JToken> properties, string propertyName)
        {
            int? value = properties.GetValueAsInt(propertyName);

            if (!value.HasValue)
            {
                return null;
            }

            return (int)value != 0;
        }

        #endregion

        #region Inspection

        protected override ThreadsResponse HandleThreadsRequest(ThreadsArguments arguments)
        {
            throw new NotImplementedException();
        }

        protected override ScopesResponse HandleScopesRequest(ScopesArguments arguments)
        {
            throw new NotImplementedException();
        }

        protected override StackTraceResponse HandleStackTraceRequest(StackTraceArguments arguments)
        {
            throw new NotImplementedException();
        }

        protected override VariablesResponse HandleVariablesRequest(VariablesArguments arguments)
        {
            throw new NotImplementedException();
        }

        protected override SetVariableResponse HandleSetVariableRequest(SetVariableArguments arguments)
        {
            throw new NotImplementedException();
        }

        protected override EvaluateResponse HandleEvaluateRequest(EvaluateArguments arguments)
        {
            throw new NotImplementedException();
        }

        protected override SetExpressionResponse HandleSetExpressionRequest(SetExpressionArguments arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Modules

        protected override ModulesResponse HandleModulesRequest(ModulesArguments arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Source Code Requests

        protected override SourceResponse HandleSourceRequest(SourceArguments arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Exceptions

        protected override ExceptionInfoResponse HandleExceptionInfoRequest(ExceptionInfoArguments arguments)
        {
            throw new NotImplementedException();
        }

        protected override SetExceptionBreakpointsResponse HandleSetExceptionBreakpointsRequest(SetExceptionBreakpointsArguments arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Convert Line Numbering To/From Client

        private int clientsFirstLine = 0;

        internal int LineToClient(int line)
        {
            return line + this.clientsFirstLine;
        }

        internal int LineFromClient(int line)
        {
            return line - this.clientsFirstLine;
        }

        #endregion
    }
}
