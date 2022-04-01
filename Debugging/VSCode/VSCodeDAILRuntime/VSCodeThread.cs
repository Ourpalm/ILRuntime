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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ILRuntime.VSCode
{
    class VSCodeVariable : IProperty
    {
        Variable variable;
        VariableInfo info;
        VSCodeStackFrame frame;
        public Variable Variable => variable;

        public IProperty Parent { get; set; }

        public VariableInfo Info => info;
        public VariableReference[] Parameters { get; set; }

        public VSCodeVariable(VSCodeStackFrame frame, VariableInfo info)
        {
            this.info = info;
            this.frame = frame;

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
            variable.EvaluateName = info.Name;
            variable.PresentationHint = hint;

        }

        public VSCodeVariable[] EnumChildren(int dwTimeout)
        {
            var thread = frame.Thread;
            var c = thread.Engine.EnumChildren(GetVariableReference(), thread.ThreadID, frame.Index, (uint)Math.Max(dwTimeout, 5000));
            var children = new VSCodeVariable[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                var vi = c[i];
                VSCodeVariable v = new VSCodeVariable(frame, vi);
                if (vi.Type == VariableTypes.IndexAccess)
                    v.Parameters = new VariableReference[] { VariableReference.GetInteger(vi.Offset) };
                v.Parent = this;
                frame.RegisterVariable(v);
                children[i] = v;
            }

            return children;
        }

        public VariableReference GetVariableReference()
        {
            if (info != null)
            {
                VariableReference res = new VariableReference();
                res.Address = info.Address;
                res.Name = info.Name;
                res.Type = info.Type;
                res.Offset = info.Offset;
                if (Parent != null)
                    res.Parent = Parent.GetVariableReference();
                res.Parameters = Parameters;

                return res;
            }
            else
                return null;
        }
    }
    class VSCodeScope
    {
        Scope scope;
        VSCodeStackFrame info;
        List<VSCodeVariable> variables = new List<VSCodeVariable>();
        Dictionary<int, VSCodeVariable> variableMapping = new Dictionary<int, VSCodeVariable>();
        public List<VSCodeVariable> Variables => variables;

        public Scope Scope => scope;
        public VSCodeScope(VSCodeStackFrame info, StackFrame frame, Scope.PresentationHintValue name, int varCnt, int varOffset, FileLinePositionSpan span)
        {
            this.info = info;
            scope = new Scope();
            scope.Name = name.ToString();
            scope.PresentationHint = name;
            scope.Source = frame.Source;
            if (span.IsValid)
            {
                scope.Line = span.StartLinePosition.Line + 1;
                scope.Column = span.StartLinePosition.Character + 1;
                scope.EndLine = span.EndLinePosition.Line + 1;
                scope.EndColumn = span.EndLinePosition.Character + 1;
            }
            else
            {
                scope.Line = frame.Line;
                scope.Column = frame.Column;
                scope.EndLine = frame.EndLine;
                scope.EndColumn = frame.EndColumn;
            }
            scope.NamedVariables = varCnt;
            scope.VariablesReference = GetHashCode();

            for(int i = 0; i < varCnt; i++)
            {
                var v = info.Info.LocalVariables[varOffset + i];
                var variable = new VSCodeVariable(info, v);
                variables.Add(variable);
                if (v.Expandable)
                    variableMapping[variable.GetHashCode()] = variable;
            }
        }

        public VSCodeVariable FindVariable(int id)
        {
            if (variableMapping.TryGetValue(id, out var v))
                return v;
            return null;
        }
    }
    class VSCodeStackFrame: IStackFrame
    {
        StackFrameInfo info;
        VSCodeScope locals, args;
        StackFrame frame;
        VSCodeThread thread;
        Dictionary<int, VSCodeVariable> variableMapping = new Dictionary<int, VSCodeVariable>();
        Dictionary<string, IProperty> propertyMapping = new Dictionary<string, IProperty>();

        public StackFrame Frame => frame;
        public VSCodeScope LocalVariables => locals;

        public VSCodeScope Arguments => args;

        public StackFrameInfo Info => info;

        public VSCodeThread Thread => thread;

        public int Index { get; private set; }

        public int ThreadID => thread.ThreadID;

        public VSCodeStackFrame(StackFrameInfo info, VSCodeThread thread, int index)
        {
            this.info = info;
            this.thread = thread;
            this.Index = index;
            frame = new StackFrame()
            {
                Name = info.MethodName,
                Id = GetHashCode()
            };
            FileLinePositionSpan span = default;
            if (!string.IsNullOrEmpty(info.DocumentName))
            {
                frame.Source = new Source() { Name = Path.GetFileName(info.DocumentName), Path = info.DocumentName };
                frame.Line = info.StartLine + 1;
                frame.EndLine = info.EndLine + 1;
                frame.Column = info.StartColumn + 1;
                frame.EndColumn = info.EndColumn + 1;

                if (File.Exists(info.DocumentName))
                {
                    using (var stream = File.OpenRead(info.DocumentName))
                    {
                        try
                        {
                            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(stream), path: info.DocumentName);
                            TextLine textLine = syntaxTree.GetText().Lines[info.StartLine];
                            Location location = syntaxTree.GetLocation(textLine.Span);
                            SyntaxTree sourceTree = location.SourceTree;
                            SyntaxNode node = location.SourceTree.GetRoot().FindNode(location.SourceSpan, true, true);

                            bool isLambda = GetParentMethod<LambdaExpressionSyntax>(node.Parent) != null;
                            BaseMethodDeclarationSyntax method = GetParentMethod<MethodDeclarationSyntax>(node.Parent);
                            if (method == null)
                            {
                                method = GetParentMethod<ConstructorDeclarationSyntax>(node.Parent);
                            }
                            if (method != null)
                                span = syntaxTree.GetLineSpan(method.FullSpan);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            args = new VSCodeScope(this, frame, Scope.PresentationHintValue.Arguments, info.ArgumentCount, 0, span);
            locals = new VSCodeScope(this, frame, Scope.PresentationHintValue.Locals, info.LocalVariables != null ? info.LocalVariables.Length - info.ArgumentCount : 0, info.ArgumentCount, span);

            foreach (var i in args.Variables)
                propertyMapping[i.Variable.Name] = i;
            foreach (var i in locals.Variables)
                propertyMapping[i.Variable.Name] = i;
        }
        protected T GetParentMethod<T>(SyntaxNode node) where T : SyntaxNode
        {
            if (node == null)
                return null;

            if (node is T)
                return node as T;
            return GetParentMethod<T>(node.Parent);
        }

        public void RegisterVariable(VSCodeVariable variable)
        {
            variableMapping[variable.GetHashCode()] = variable;
        }

        public VSCodeVariable FindVariable(int id)
        {
            var res = Arguments.FindVariable(id);
            if (res == null)
                res = LocalVariables.FindVariable(id);
            if (res == null)
                variableMapping.TryGetValue(id, out res);

            return res;
        }
        public IProperty GetPropertyByName(string name)
        {
            if (propertyMapping.TryGetValue(name, out var p))
                return p;
            return null;
        }
    }
    class VSCodeThread : IThread
    {
        DebuggedProcessVSCode engine;
        Thread thread;
        VSCodeStackFrame[] frames;
        Dictionary<int, VSCodeStackFrame> frameMapping = new Dictionary<int, VSCodeStackFrame>();
        Dictionary<int, VSCodeScope> scopeMapping = new Dictionary<int, VSCodeScope>();

        public Thread Thread => thread;

        public VSCodeStackFrame[] VSCodeStackFrames => frames;

        public DebuggedProcessVSCode Engine => engine;

        public StackFrameInfo[] StackFrames
        {
            set
            {
                frameMapping.Clear();
                scopeMapping.Clear();
                frames = new VSCodeStackFrame[value.Length];
                for(int i = 0; i < value.Length; i++)
                {
                    var frame = new VSCodeStackFrame(value[i], this, i);
                    frames[i] = frame;
                    frameMapping[frame.GetHashCode()] = frame;
                    scopeMapping[frame.Arguments.GetHashCode()] = frame.Arguments;
                    scopeMapping[frame.LocalVariables.GetHashCode()] = frame.LocalVariables;
                }
                if (frames.Length > 0)
                {
                    thread.Name = frames[0].Info.MethodName;
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

        public VSCodeVariable FindVariable(int variableID)
        {
            foreach (var i in frames)
            {
                var v = i.FindVariable(variableID);
                if (v != null)
                    return v;
            }
            return null;
        }

        public int ThreadID => thread.Id;

        public VSCodeThread(DebuggedProcessVSCode engine, int id, string threadName)
        {
            this.engine = engine;
            thread = new Thread(id, threadName);
        }
    }
}
