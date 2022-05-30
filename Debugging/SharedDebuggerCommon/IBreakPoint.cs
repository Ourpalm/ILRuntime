using ILRuntime.Runtime.Debugger.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Linq;

namespace ILRuntimeDebugEngine
{
    internal abstract class IBreakPoint
    {
        public int EndLine { get; protected set; }
        public int EndColumn { get; protected set; }

        public string DocumentName { get; protected set; }
        public abstract string ConditionExpression { get;  }
        public int StartLine { get; protected set; }
        public int StartColumn { get; protected set; }
        public abstract bool IsBound { get; }
        public abstract bool TryBind();
        public abstract void Bound(BindBreakpointResults res);

        protected CSBindBreakpoint CreateBindRequest(bool enabled, BreakpointConditionStyle style, string condition)
        {
            using (var stream = File.OpenRead(DocumentName))
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(stream), path: DocumentName);
                TextLine textLine = syntaxTree.GetText().Lines[StartLine];
                Location location = syntaxTree.GetLocation(textLine.Span);
                SyntaxTree sourceTree = location.SourceTree;
                SyntaxNode node = location.SourceTree.GetRoot().FindNode(location.SourceSpan, true, true);

                bool isLambda = GetParentMethod<LambdaExpressionSyntax>(node.Parent) != null;
                BaseMethodDeclarationSyntax method = GetParentMethod<MethodDeclarationSyntax>(node.Parent);
                string methodName = null;
                if (method != null)
                    methodName = ((MethodDeclarationSyntax)method).Identifier.Text;
                else
                {
                    method = GetParentMethod<ConstructorDeclarationSyntax>(node.Parent);
                    if (method != null)
                    {
                        bool isStatic = false;
                        foreach (var i in method.Modifiers)
                        {
                            if (i.Text == "static")
                                isStatic = true;
                        }
                        if (isStatic)
                            methodName = ".cctor";
                        else
                            methodName = ".ctor";
                    }
                }

                string className = GetClassName(method);

                //var ns = GetParentMethod<NamespaceDeclarationSyntax>(method);
                //string nsname = ns != null ? ns.Name.ToString() : null;

                //string name = ns != null ? string.Format("{0}.{1}", nsname, className) : className;
                var nameSpaceStack = new Stack<string>();
                var usingSyntaxList = new List<UsingDirectiveSyntax>(syntaxTree.GetCompilationUnitRoot().Usings);
                GetCurrentNameSpaceDeclaration(node.Parent, nameSpaceStack, usingSyntaxList);

                var bindRequest = new CSBindBreakpoint();
                bindRequest.BreakpointHashCode = this.GetHashCode();
                bindRequest.IsLambda = isLambda;
                bindRequest.NamespaceName = string.Join(".", nameSpaceStack);
                bindRequest.TypeName = className;
                bindRequest.MethodName = methodName;
                bindRequest.StartLine = StartLine;
                bindRequest.EndLine = EndLine;
                bindRequest.Enabled = enabled;
                bindRequest.Condition = new BreakpointCondition();
                bindRequest.Condition.Style = style;
                bindRequest.Condition.Expression = condition;
                bindRequest.UsingInfos = usingSyntaxList.Select(n => new UsingInfo
                {
                    Alias = n.Alias != null ? n.Alias.Name.ToString() : "",
                    Name = n.Name.ToString(),
                }).ToArray();

                return bindRequest;
            }
        }



        protected string GetClassName(BaseMethodDeclarationSyntax method)
        {
            ClassDeclarationSyntax cur = GetParentMethod<ClassDeclarationSyntax>(method);
            string clsName = null;
            while (cur != null)
            {
                if (clsName == null)
                    clsName = cur.Identifier.Text;
                else
                    clsName = string.Format("{0}/{1}", cur.Identifier.Text, clsName);
                cur = GetParentMethod<ClassDeclarationSyntax>(cur.Parent);
            }

            return clsName;
        }
        protected T GetParentMethod<T>(SyntaxNode node) where T : SyntaxNode
        {
            if (node == null)
                return null;

            if (node is T)
                return node as T;
            return GetParentMethod<T>(node.Parent);
        }

        protected void GetCurrentNameSpaceDeclaration(SyntaxNode node, Stack<string> namespaceList, List<UsingDirectiveSyntax> usingSyntaxList)
        {
            if (node == null)
                return;

            if (node is NamespaceDeclarationSyntax)
            {
                var namespaceDeclarationSyntax = node as NamespaceDeclarationSyntax;
                namespaceList.Push(namespaceDeclarationSyntax.Name.ToString());
                usingSyntaxList.AddRange(namespaceDeclarationSyntax.Usings);
            }
            GetCurrentNameSpaceDeclaration(node.Parent, namespaceList, usingSyntaxList);
        }
    }
}
