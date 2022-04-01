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
using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Debugger.Expressions;
using ILRuntimeDebugEngine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Serialization;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ookii.CommandLine;
//using SampleDebugAdapter.Directives;
using SysThread = System.Threading.Thread;
namespace ILRuntime.VSCode
{
    internal class ILRuntimeDebugAdapter : DebugAdapterBase
    {
        DebuggedProcessVSCode debugged;
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

            var res = new InitializeResponse()
            {
                SupportsConfigurationDoneRequest = true,
                SupportsSetVariable = true,
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
            debugged.Close();
            debugged = null;

            return new DisconnectResponse();
        }

        #endregion

        #region Launch

        class InlineVariableArguments
        {
            [JsonProperty("documentName")]
            public string DocumentName { get; set; }
            [JsonProperty("line")]
            public int Line { get; set; }
            [JsonProperty("character")]
            public int Character { get; set; }
        }

        class InlineVariableResponse : ResponseBody
        {
            [JsonProperty("variables")] 
            public List<InlineVaiable> Variables { get; set; }
        }

        class InlineVaiable
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("line")]
            public int Line { get; set; }
            [JsonProperty("endLine")]
            public int EndLine { get; set; }
            [JsonProperty("column")]
            public int Column { get; set; }
            [JsonProperty("endColumn")]
            public int EndColumn { get; set; }

            public static InlineVaiable FromIdentifier(SyntaxToken identifier)
            {
                var span = identifier.SyntaxTree.GetLineSpan(identifier.Span);
                InlineVaiable res = new InlineVaiable()
                {
                    Name = identifier.Text,
                    Line = span.StartLinePosition.Line,
                    Column = span.StartLinePosition.Character,
                    EndLine = span.EndLinePosition.Line,
                    EndColumn = span.EndLinePosition.Character
                };
                return res;
            }
        }
        class InlineVairaleRequest : DebugRequest<InlineVariableArguments>
        {
            public InlineVairaleRequest() : base("inlineVariables")
            {
            }
        }

        void HandleInlineVariable(IRequestResponder<InlineVariableArguments> handler)
        {
            List<InlineVaiable> res = new List<InlineVaiable>();
            var DocumentName = handler.Arguments.DocumentName;
            try
            {
                if (File.Exists(DocumentName))
                {
                    using (var stream = File.OpenRead(DocumentName))
                    {
                        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(stream), path: DocumentName);
                        TextLine textLine = syntaxTree.GetText().Lines[handler.Arguments.Line];
                        Location location = syntaxTree.GetLocation(textLine.Span);
                        SyntaxTree sourceTree = location.SourceTree;
                        SyntaxNode node = location.SourceTree.GetRoot().FindNode(location.SourceSpan, true, true);

                        bool isLambda = GetParentMethod<LambdaExpressionSyntax>(node.Parent) != null;
                        BaseMethodDeclarationSyntax method = GetParentMethod<MethodDeclarationSyntax>(node.Parent);
                        if (method == null)
                        {
                            method = GetParentMethod<ConstructorDeclarationSyntax>(node.Parent);
                        }

                        foreach (var i in method.ParameterList.Parameters)
                        {
                            var span = syntaxTree.GetLineSpan(i.Identifier.Span);
                            if (span.StartLinePosition.Line > handler.Arguments.Line)
                                continue;
                            res.Add(InlineVaiable.FromIdentifier(i.Identifier));
                        }

                        foreach(var i in method.Body.Statements)
                        {
                            if(i is LocalDeclarationStatementSyntax local)
                            {
                                foreach(var j in local.Declaration.Variables)
                                {
                                    var span = syntaxTree.GetLineSpan(j.Identifier.Span);
                                    if (span.StartLinePosition.Line > handler.Arguments.Line)
                                        continue;
                                    res.Add(InlineVaiable.FromIdentifier(j.Identifier));
                                }
                            }
                            if(i is ForStatementSyntax fore)
                            {
                                foreach (var j in fore.Declaration.Variables)
                                {
                                    var span = syntaxTree.GetLineSpan(j.Identifier.Span);
                                    if (span.StartLinePosition.Line > handler.Arguments.Line)
                                        continue;

                                    res.Add(InlineVaiable.FromIdentifier(j.Identifier));
                                }
                            }
                            if(i is ForEachStatementSyntax fore2)
                            {
                                
                            }
                        }
                    }
                }
                handler.SetResponse(new InlineVariableResponse()
                {
                    Variables = res
                });
            }
            catch (Exception ex)
            {
                handler.SetError(new ProtocolException(ex.ToString()));
            }
        }
        protected T GetParentMethod<T>(SyntaxNode node) where T : SyntaxNode
        {
            if (node == null)
                return null;

            if (node is T)
                return node as T;
            return GetParentMethod<T>(node.Parent);
        }

        protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments)
        {
            Protocol.RegisterRequestType<InlineVairaleRequest, InlineVariableArguments>(HandleInlineVariable);
            this.Protocol.SendEvent(new InitializedEvent());
            Protocol.SendEvent(new OutputEvent("Launching..."));
            string address = arguments.ConfigurationProperties.GetValueAsString("address");
            if (String.IsNullOrEmpty(address))
            {
                throw new ProtocolException("Launch failed because launch configuration did not specify 'address'.");
            }

            string[] token = address.Split(':');
            
            if (token.Length < 2)
            {
                throw new ProtocolException($"Launch failed because 'address' is invalid({address}).");
            }
            string host = token[0];
            int port;
            if(!int.TryParse(token[1], out port))
            {
                throw new ProtocolException($"Launch failed because 'address' is invalid({address}).");
            }
            
            this.stopAtEntry = arguments.ConfigurationProperties.GetValueAsBool("stopAtEntry") ?? false;
            this.hyperStepSpeed = arguments.ConfigurationProperties.GetValueAsInt("hyperStepSpeed") ?? 0;
            
            debugged = new DebuggedProcessVSCode(this, host, port);
            while (debugged.Connecting)
            {
                System.Threading.Thread.Sleep(10);
            }

            if (debugged.Connected)
            {
                if (debugged.CheckDebugServerVersion())
                {
                    debugged.OnDisconnected = OnDisconnected;
                    return new LaunchResponse();
                }
                else
                {
                    debugged.Close();
                    throw new ProtocolException(String.Format("ILRuntime Debugger version mismatch\n Expected version:{0}\n Actual version:{1}", DebuggerServer.Version, debugged.RemoteDebugVersion));
                }
            }
            else
            {
                debugged = null;
                throw new ProtocolException("Cannot connect to ILRuntime");                
            }
        }

        void OnDisconnected()
        {
            this.Protocol.SendEvent(new ExitedEvent());
            this.Protocol.SendEvent(new TerminatedEvent());
        }

        #endregion

        #region Continue/Stepping

        protected override ConfigurationDoneResponse HandleConfigurationDoneRequest(ConfigurationDoneArguments arguments)
        {
            if (this.stopAtEntry)
            {
                // Clear the event so we'll break at startup
                //this.RequestStop(StoppedEvent.ReasonValue.Step);
            }

            return new ConfigurationDoneResponse();
        }

        protected override ContinueResponse HandleContinueRequest(ContinueArguments arguments)
        {
            debugged.SendExecute(arguments.ThreadId);
            //this.Continue(step: false);
            return new ContinueResponse()
            {
                AllThreadsContinued = true
            };
        }

        protected override StepInResponse HandleStepInRequest(StepInArguments arguments)
        {
            debugged.SendStep(arguments.ThreadId, StepTypes.Into);
            return new StepInResponse();
        }

        protected override StepOutResponse HandleStepOutRequest(StepOutArguments arguments)
        {
            //this.Continue(step: true);
            debugged.SendStep(arguments.ThreadId, StepTypes.Out);

            return new StepOutResponse();
        }

        protected override NextResponse HandleNextRequest(NextArguments arguments)
        {
            debugged.SendStep(arguments.ThreadId, StepTypes.Over);
            //this.Continue(step: true);
            return new NextResponse();
        }

        protected override PauseResponse HandlePauseRequest(PauseArguments arguments)
        {
            throw new ProtocolException("Not supported");
            //this.RequestStop(StoppedEvent.ReasonValue.Pause);
            //return new PauseResponse();
        }

        #endregion

        #region Debug Thread

        public void SendOutput(string message, bool isError = false)
        {
            string outputText = !String.IsNullOrEmpty(message) ? message.Trim() : String.Empty;

            this.Protocol.SendEvent(new OutputEvent()
            {
                Output = ($"{outputText}{Environment.NewLine}"),
                Category = isError ? OutputEvent.CategoryValue.Stderr : OutputEvent.CategoryValue.Stdout
            });
        }

        #endregion

        #region Breakpoints
       
        protected override SetBreakpointsResponse HandleSetBreakpointsRequest(SetBreakpointsArguments arguments)
        {
            List<Breakpoint> result = new List<Breakpoint>();
            HashSet<VSCodeBreakPoint> validBPs = new HashSet<VSCodeBreakPoint>();
            foreach(var i in arguments.Breakpoints)
            {
                try
                {
                    VSCodeBreakPoint bp = debugged.FindBreakpoint(arguments.Source.Path, i.Line);
                    if (bp != null)
                    {
                        if(bp.ConditionExpression != i.Condition)
                        {
                            debugged.SendSetBreakpointCondition(bp.GetHashCode(),
                                string.IsNullOrEmpty(i.Condition) ? Runtime.Debugger.Protocol.BreakpointConditionStyle.None : Runtime.Debugger.Protocol.BreakpointConditionStyle.WhenTrue,
                                i.Condition);
                        }
                    }
                    else
                    {
                        bp = new VSCodeBreakPoint(debugged, arguments.Source, i.Line, i.Column.GetValueOrDefault(), i.Condition);
                    }
                    validBPs.Add(bp);

                    if (!bp.IsBound && bp.TryBind())
                    {
                        debugged.AddPendingBreakpoint(bp);
                        result.Add(bp.BreakPoint);
                    }
                    
                }
                catch (Exception ex)
                {
                    SendOutput(ex.ToString());
                }
            }

            debugged.UpdateBreakpoints(arguments.Source.Path, validBPs);
            return new SetBreakpointsResponse(result);
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
            ThreadsResponse res = new ThreadsResponse();
            foreach(var i in debugged.Threads)
            {
                VSCodeThread t = (VSCodeThread)i.Value;
                res.Threads.Add(t.Thread);
            }
            return res;
        }

        protected override ScopesResponse HandleScopesRequest(ScopesArguments arguments)
        {
            foreach (var t in debugged.Threads)
            {
                var thread = t.Value as VSCodeThread;
                var frame = thread.FindFrame(arguments.FrameId);
                if (frame != null)
                {
                    ScopesResponse res = new ScopesResponse();
                    res.Scopes = new List<Scope>() { frame.LocalVariables.Scope, frame.Arguments.Scope };
                    return res;
                }
            }
            return new ScopesResponse();
        }

        protected override StackTraceResponse HandleStackTraceRequest(StackTraceArguments arguments)
        {
            List<StackFrame> result = new List<StackFrame>();
            if(debugged.Threads.TryGetValue(arguments.ThreadId, out IThread t))
            {
                var thread = t as VSCodeThread;
                foreach(var i in thread.VSCodeStackFrames)
                {
                    result.Add(i.Frame);
                }
            }

            return new StackTraceResponse(result);
        }

        protected override VariablesResponse HandleVariablesRequest(VariablesArguments arguments)

        {
            foreach (var t in debugged.Threads)
            {
                var thread = t.Value as VSCodeThread;
                var scope = thread.FindScope(arguments.VariablesReference);
                if (scope != null)
                {
                    List<Variable> res = new List<Variable>();
                    foreach(var i in scope.Variables)
                    {
                        res.Add(i.Variable);
                    }
                    return new VariablesResponse(res);
                }
                var variable = thread.FindVariable(arguments.VariablesReference);
                if (variable != null)
                {
                    var children = variable.EnumChildren(arguments.Timeout.GetValueOrDefault());
                    List<Variable> res = new List<Variable>();
                    foreach (var i in children)
                    {
                        res.Add(i.Variable);
                    }

                    return new VariablesResponse(res);
                }
            }
            
            return new VariablesResponse();
        }

        protected override SetVariableResponse HandleSetVariableRequest(SetVariableArguments arguments)
        {
            throw new ProtocolException("Not Implemented");
        }

        protected override EvaluateResponse HandleEvaluateRequest(EvaluateArguments arguments)
        {
            Parser parser = new Parser(arguments.Expression);
            EvalExpression exp = null;
            try
            {
                exp = parser.Parse();
            }
            catch (Exception ex)
            {
                throw new ProtocolException(ex.ToString());
            }
            VSCodeStackFrame frame;
            foreach (var t in debugged.Threads)
            {
                var thread = t.Value as VSCodeThread;
                frame = thread.FindFrame(arguments.FrameId.GetValueOrDefault());
                if (frame != null)
                {
                    var prop = debugged.Resolve(frame, exp, (uint)Math.Max(arguments.Timeout.GetValueOrDefault(), 5000)) as VSCodeVariable;
                    if (prop.Info.Expandable)
                    {
                        frame.RegisterVariable(prop);
                    }
                    return new EvaluateResponse()
                    {
                        Result = prop.Info.Value,
                        Type = prop.Info.TypeName,
                        VariablesReference = prop.Info.Expandable ? prop.GetHashCode() : 0
                    };
                }
            }
            throw new ProtocolException($"Evaluation failed:{arguments.Expression}");
        }

        protected override SetExpressionResponse HandleSetExpressionRequest(SetExpressionArguments arguments)
        {
            throw new ProtocolException("Not Implemented");
        }
        #endregion

        #region Modules

        protected override ModulesResponse HandleModulesRequest(ModulesArguments arguments)
        {
            throw new ProtocolException("Not Implemented");
        }

        #endregion

        #region Source Code Requests

        protected override SourceResponse HandleSourceRequest(SourceArguments arguments)
        {
            throw new ProtocolException("Not Implemented");
        }

        #endregion

        #region Exceptions

        protected override ExceptionInfoResponse HandleExceptionInfoRequest(ExceptionInfoArguments arguments)
        {
            throw new ProtocolException("Not Implemented");
        }

        protected override SetExceptionBreakpointsResponse HandleSetExceptionBreakpointsRequest(SetExceptionBreakpointsArguments arguments)
        {
            throw new ProtocolException("Not Implemented");
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
