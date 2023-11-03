using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class FunctionBooleanExpressionNode : LogicalExpressionNode
{
	public LogicalFunctionDefinition Function { get; }
	public ExpressionNode[] Parameters { get; }

	public FunctionBooleanExpressionNode(LogicalFunctionDefinition function, IEnumerable<ExpressionNode> parameters)
	{
		Function = function;
		Parameters = parameters.ToArray();
	}

	public override bool Evaluate(EvaluationContext context)
	{
		var parameterValues = Parameters.Select(x =>
		{
			return x switch
			{
				ValueExpressionNode c => (object?)c.Evaluate(context),
				BooleanResultExpressionNode b => b.Evaluate(context),
				_ => throw new ArgumentOutOfRangeException("parameter")
			};
		}).ToArray();

		return Function.Invoke(parameterValues) == true;
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Function.Name);
		builder.Append('(');

		if (Parameters.Any())
		{
			Parameters[0].BuildString(builder);
			for (int i = 1; i < Parameters.Length; i++)
			{
				builder.Append(',');
				Parameters[i].BuildString(builder);
			}
		}

		builder.Append(')');
	}

	public override string ToString()
	{
		return $"{Function.Name}({string.Join(',', (IEnumerable<ValueExpressionNode>)Parameters)})";
	}
}

internal class FunctionBooleanExpressionParser : ILogicalExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out LogicalExpressionNode? expression)
	{
		int i = index;
		if (!FunctionExpressionParser.TryParseFunction(source, ref i, out var parameters, out var function))
		{
			expression = null;
			return false;
		}

		if (function is not LogicalFunctionDefinition logicalFunc)
		{
			expression = null;
			return false;
		}

		expression = new FunctionBooleanExpressionNode(logicalFunc, parameters);
		index = i;
		return true;
	}
}
