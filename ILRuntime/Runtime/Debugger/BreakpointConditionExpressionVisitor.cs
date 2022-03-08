using ILRuntime.CLR.Method;
using ILRuntime.Reflection;
using ILRuntime.Runtime.Intepreter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ILRuntime.Runtime.Debugger
{
    internal class BreakpointConditionExpressionVisitor : CSharpSyntaxVisitor<TempComputeResult>
    {
        public DebugService DebugService { get; private set; }
        public ILIntepreter Intepreter { get; private set; }
        public IDictionary<string, VariableInfo> LocalVariables { get; private set; }
        private int tempVariableIndex = 0;
        private static Dictionary<string, string> dic_UnaryOperator_MethodName = new Dictionary<string, string>();
        private static Dictionary<string, string> dic_BinaryOperator_MethodName = new Dictionary<string, string>();

        static BreakpointConditionExpressionVisitor()
        {
            dic_UnaryOperator_MethodName["+"] = "op_UnaryPlus";
            dic_UnaryOperator_MethodName["-"] = "op_UnaryNegation";
            dic_UnaryOperator_MethodName["!"] = "op_LogicalNot";
            dic_UnaryOperator_MethodName["~"] = "op_OnesComplement";
            dic_UnaryOperator_MethodName["++"] = "op_Increment";
            dic_UnaryOperator_MethodName["--"] = "op_Decrement";
            dic_UnaryOperator_MethodName["true"] = "op_True";
            dic_UnaryOperator_MethodName["false"] = "op_False";

            dic_BinaryOperator_MethodName["+"] = "op_Addition";
            dic_BinaryOperator_MethodName["-"] = "op_Subtraction";
            dic_BinaryOperator_MethodName["*"] = "op_Multiply";
            dic_BinaryOperator_MethodName["/"] = "op_Division";
            dic_BinaryOperator_MethodName["%"] = "op_Modulus";
            dic_BinaryOperator_MethodName["&"] = "op_BitwiseAnd";
            dic_BinaryOperator_MethodName["|"] = "op_BitwiseOr";
            dic_BinaryOperator_MethodName["^"] = "op_ExclusiveOr";
            dic_BinaryOperator_MethodName["<<"] = "op_LeftShift";
            dic_BinaryOperator_MethodName[">>"] = "op_RightShift";
            dic_BinaryOperator_MethodName["<"] = "op_LessThan";
            dic_BinaryOperator_MethodName[">"] = "op_GreaterThan";
            dic_BinaryOperator_MethodName["<="] = "op_LessThanOrEqual";
            dic_BinaryOperator_MethodName[">="] = "op_GreaterThanOrEqual";
            dic_BinaryOperator_MethodName["=="] = "op_Equality";
            dic_BinaryOperator_MethodName["!="] = "op_Inequality";
        }

        public BreakpointConditionExpressionVisitor(DebugService debugService, ILIntepreter intp, VariableInfo[] localVariables)
        {
            DebugService = debugService;
            Intepreter = intp;
            LocalVariables = localVariables.ToDictionary(i => i.Name);
        }

        private TempComputeResult CreateComputeResult(object value, Type type)
        {
            return new TempComputeResult("tempVariable" + (++tempVariableIndex), value, type);
        }

        public override TempComputeResult DefaultVisit(SyntaxNode node)
        {
            throw new NotSupportedException("Unknown Expression Type:" + node.GetType().Name);
        }

        public override TempComputeResult VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            var value = node.Token.Value;
            return CreateComputeResult(value, (value == null) ? null : value.GetType());
        }

        public override TempComputeResult VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            PassVariableReference(node, node.Expression);
            return Visit(node.Expression);
        }

        public override TempComputeResult VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var operatorText = node.OperatorToken.Text;
            if (operatorText == "&&") // TODO:用户定义的条件逻辑运算符
            {

            }
            else if (operatorText == "||")
            {

            }
            else
            {
                var left = Visit(node.Left);
                var right = Visit(node.Right);
                if (left.Type == null && right.Type == null)
                    return CreateComputeResult(null, null);

                string methodName;
                if (dic_BinaryOperator_MethodName.TryGetValue(operatorText, out methodName))
                {
                    var leftType = left.Type;
                    var rightType = right.Type;
                    var bindingFlags = BindingFlags.Public | BindingFlags.Static;
                    var overloadOperatorMethod = DebugService.GetMethod(leftType, methodName, bindingFlags, leftType, rightType);
                    if (overloadOperatorMethod == null)
                        overloadOperatorMethod = DebugService.GetMethod(rightType, methodName, bindingFlags, leftType, rightType);
                    if (overloadOperatorMethod != null) // 有运算符重载
                    {
                        var result = overloadOperatorMethod.Invoke(null, new object[2] { left.Value, right.Value });
                        return CreateComputeResult(result, overloadOperatorMethod.ReturnType);
                    }
                    else // 数学运算, 依赖动态编译lambda表达式，只能JIT
                    {
                        try
                        {
                            var func = new DynamicExpresso.Interpreter().Parse(string.Format("x{0}y", operatorText), new DynamicExpresso.Parameter("x", UnWrapper(leftType)), new DynamicExpresso.Parameter("y", UnWrapper(rightType)));
                            var result = func.Invoke(left.Value, right.Value);
                            return CreateComputeResult(result, result == null ? null : result.GetType());
                        }
                        catch
                        {
                            throw new Exception(string.Format("Fail to calculate '{0}'", node.ToString()));
                        }
                    }
                }
                else
                    throw new NotSupportedException("Unknown Binary Operator:" + operatorText);
            }

            return null;
        }

        private static Type UnWrapper(Type type)
        {
            if (type is ILRuntimeWrapperType)
                return (type as ILRuntimeWrapperType).RealType;
            return type;
        }

        public override TempComputeResult VisitThisExpression(ThisExpressionSyntax node)
        {
            VariableReferenceTuple tuple;
            if (dic_Expression_Variable.TryGetValue(node, out tuple))
                return ResolveVariable(tuple.Bottom);

            ILMethod currentMethod;
            var v = DebugService.GetThis(Intepreter, out currentMethod);
            return CreateComputeResult(v, currentMethod.DeclearingType.ReflectionType);
        }

        public override TempComputeResult VisitIdentifierName(IdentifierNameSyntax node)
        {
            var identifierName = node.ToString();
            var tuple = GetOrCreateVariableReference(node, identifierName);
            if (node.Parent is InvocationExpressionSyntax)
                HandleInvocationExpressionSyntax(tuple, node.Parent as InvocationExpressionSyntax);
            else
            {
                var top = tuple.Top;
                VariableInfo localVariable;
                if (LocalVariables.TryGetValue(top.Name, out localVariable))
                {
                    top.Type = VariableTypes.Normal;
                    top.Address = localVariable.Address;
                    top.ValueType = localVariable.ValueObjType;
                }
                else
                    top.Type = VariableTypes.FieldReference;
            }
            
            return ResolveVariable(tuple.Bottom);
        }

        private TempComputeResult ResolveVariable(VariableReference variableReference)
        {
            object variableValue;
            var variableInfo = DebugService.ResolveVariable(Intepreter.GetHashCode(), variableReference, out variableValue);
            HandleResolveVariableError(variableInfo);
            return CreateComputeResult(variableValue, variableInfo.ValueObjType);
        }

        public override TempComputeResult VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            PassVariableReference(node, node.Expression);
            return Visit(node.Expression);
        }

        public override TempComputeResult VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var tuple = GetOrCreateVariableReference(node, node.Name.ToString());
            if (node.Parent is InvocationExpressionSyntax)
                HandleInvocationExpressionSyntax(tuple, node.Parent as InvocationExpressionSyntax);
            else
                tuple.Top.Type = VariableTypes.FieldReference;
            dic_Expression_Variable.Add(node.Expression, tuple);
            return Visit(node.Expression);
        }

        private void HandleInvocationExpressionSyntax(VariableReferenceTuple tuple, InvocationExpressionSyntax invocationExpressionSyntax)
        {
            tuple.Top.Type = VariableTypes.Invocation;
            var variableReferenceList = new List<VariableReference>();
            foreach (var argument in invocationExpressionSyntax.ArgumentList.Arguments)
            {
                var argumentResult = Visit(argument.Expression);
                variableReferenceList.Add(new VariableReference
                {
                    Type = VariableTypes.Value,
                    Value = argumentResult.Value,
                    ValueType = argumentResult.Type,
                });
            }
            tuple.Top.Parameters = variableReferenceList.ToArray();
        }

        private void HandleResolveVariableError(VariableInfo variableInfo)
        {
            if (variableInfo.Type == VariableTypes.Null)
                throw new NullReferenceException();
            if (variableInfo.Type == VariableTypes.Error || variableInfo.Type == VariableTypes.NotFound)
                throw new Exception(variableInfo.Value);
        }

        private Dictionary<ExpressionSyntax, VariableReferenceTuple> dic_Expression_Variable = new Dictionary<ExpressionSyntax, VariableReferenceTuple>();
        private VariableReferenceTuple GetOrCreateVariableReference(ExpressionSyntax expressionSyntax, string name, Func<bool> createParentCondition = null)
        {
            VariableReferenceTuple tuple;
            if (!dic_Expression_Variable.TryGetValue(expressionSyntax, out tuple))
            {
                var variableReference = new VariableReference { Name = name };
                tuple = new VariableReferenceTuple { Bottom = variableReference, Top = variableReference };
            }
            else
            {
                dic_Expression_Variable.Remove(expressionSyntax);
                if (createParentCondition == null || createParentCondition())
                {
                    tuple.Top.Parent = new VariableReference { Name = name };
                    tuple.Top = tuple.Top.Parent;
                }
            }
            return tuple;
        }

        private void PassVariableReference(ExpressionSyntax from, ExpressionSyntax to)
        {
            VariableReferenceTuple tuple;
            if (dic_Expression_Variable.TryGetValue(from, out tuple))
            {
                dic_Expression_Variable.Remove(from);
                dic_Expression_Variable.Add(to, tuple);
            }
        }
    }

    internal class VariableReferenceTuple
    {
        public VariableReference Bottom { get; set; }
        public VariableReference Top { get; set; }
    }

    internal class TempComputeResult
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type Type { get; set; }

        public TempComputeResult(string name, object value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }
    }
}