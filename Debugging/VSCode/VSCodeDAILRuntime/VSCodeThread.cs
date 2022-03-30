// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ILRuntime.Runtime.Debugger;
using ILRuntimeDebugEngine;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Ookii.CommandLine;

namespace ILRuntime.VSCode
{
    class VSCodeVariable
    {
        Variable variable;
        VariableInfo info;

        public Variable Variable => variable;
        public VSCodeVariable(VariableInfo info)
        {
            this.info = info;

            variable = new Variable()
            {
                Name = info.Name,
                Value = info.Value,
                Type = info.TypeName
            };
            VariablePresentationHint hint = new VariablePresentationHint();
            if (info.IsPrivate)
                hint.Visibility = VariablePresentationHint.VisibilityValue.Private;
            else if (info.IsProtected)
                hint.Visibility = VariablePresentationHint.VisibilityValue.Protected;
            else
                hint.Visibility = VariablePresentationHint.VisibilityValue.Public;
            hint.Attributes = VariablePresentationHint.AttributesValue.ReadOnly;
            if (info.Type == VariableTypes.Boolean)
                hint.Attributes |= info.Offset == 1 ? VariablePresentationHint.AttributesValue.IsBoolean | VariablePresentationHint.AttributesValue.IsTrue : VariablePresentationHint.AttributesValue.IsBoolean;
            if (info.Type >= VariableTypes.Error)
                hint.Attributes |= VariablePresentationHint.AttributesValue.FailedEvaluation;
            if (info.Type == VariableTypes.String)
                hint.Attributes |= VariablePresentationHint.AttributesValue.RawString;
            if (info.Expandable)
                variable.VariablesReference = GetHashCode();
            variable.PresentationHint = hint;

        }
    }
    class VSCodeScope
    {
        Scope scope;
        StackFrameInfo info;
        List<VSCodeVariable> variables = new List<VSCodeVariable>();

        public List<VSCodeVariable> Variables => variables;

        public Scope Scope => scope;
        public VSCodeScope(StackFrameInfo info, StackFrame frame, string name, int varCnt, int varOffset)
        {
            this.info = info;
            scope = new Scope();
            scope.Name = name;
            scope.Source = frame.Source;
            scope.Line = frame.Line;
            scope.Column = frame.Column;
            scope.EndLine = frame.EndLine;
            scope.EndColumn = frame.EndColumn;
            scope.NamedVariables = varCnt;
            scope.VariablesReference = GetHashCode();

            for(int i = 0; i < varCnt; i++)
            {
                var v = info.LocalVariables[varOffset + i];
                variables.Add(new VSCodeVariable(v));
            }
        }
    }
    class VSCodeStackFrame
    {
        StackFrameInfo info;
        VSCodeScope locals, args;
        StackFrame frame;

        public StackFrame Frame => frame;
        public VSCodeScope LocalVariables => locals;

        public VSCodeScope Arguments => args;

        public VSCodeStackFrame(StackFrameInfo info)
        {
            this.info = info;
            frame = new StackFrame()
            {
                Name = info.MethodName,
                Id = GetHashCode()
            };
            if (!string.IsNullOrEmpty(info.DocumentName))
            {
                frame.Source = new Source() { Name = Path.GetFileName(info.DocumentName), Path = info.DocumentName };
                frame.Line = info.StartLine + 1;
                frame.EndLine = info.EndLine + 1;
                frame.Column = info.StartColumn + 1;
                frame.EndColumn = info.EndColumn + 1;
            }
            

            args = new VSCodeScope(info, frame, "Arguments", info.ArgumentCount, 0);
            locals = new VSCodeScope(info, frame, "Locals", info.LocalVariables != null ? info.LocalVariables.Length - info.ArgumentCount : 0, info.ArgumentCount);
        }
    }
    class VSCodeThread : IThread
    {
        Thread thread;
        VSCodeStackFrame[] frames;
        Dictionary<int, VSCodeStackFrame> frameMapping = new Dictionary<int, VSCodeStackFrame>();
        Dictionary<int, VSCodeScope> scopeMapping = new Dictionary<int, VSCodeScope>();

        public Thread Thread => thread;

        public VSCodeStackFrame[] VSCodeStackFrames => frames;

        public StackFrameInfo[] StackFrames
        {
            set
            {
                frameMapping.Clear();
                scopeMapping.Clear();
                frames = new VSCodeStackFrame[value.Length];
                for(int i = 0; i < value.Length; i++)
                {
                    var frame = new VSCodeStackFrame(value[i]);
                    frames[i] = frame;
                    frameMapping[frame.GetHashCode()] = frame;
                    scopeMapping[frame.Arguments.GetHashCode()] = frame.Arguments;
                    scopeMapping[frame.LocalVariables.GetHashCode()] = frame.LocalVariables;
                }
            }
        }

        public VSCodeStackFrame FindFrame(int frameID)
        {
            if (frameMapping.TryGetValue(frameID, out var frame))
                return frame;
            return null;
        }

        public VSCodeScope FindScope(int scopeID)
        {
            if (scopeMapping.TryGetValue(scopeID, out var scope))
                return scope;
            return null;
        }

        public int ThreadID => thread.Id;

        public VSCodeThread(int id, string threadName)
        {
            thread = new Thread(id, threadName);
        }
    }
}
