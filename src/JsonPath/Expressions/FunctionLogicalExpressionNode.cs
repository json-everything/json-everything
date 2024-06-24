using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class FunctionLogicalExpressionNode : LogicalExpressionNode
{
	public LogicalFunctionDefinition Function { get; }
	public ExpressionNode[] Parameters { get; }

	public FunctionLogicalExpressionNode(LogicalFunctionDefinition function, IEnumerable<ExpressionNode> parameters)
	{
		Function = function;
		Parameters = parameters.ToArray();
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		var parameterValues = Parameters.Select(x =>
		{
			return x switch
			{
				ValueExpressionNode c => (object?)c.Evaluate(globalParameter, localParameter),
				LogicalExpressionNode b => b.Evaluate(globalParameter, localParameter),
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
		return $"{Function.Name}({string.Join(",", (IEnumerable<ValueExpressionNode>)Parameters)})";
	}
}

internal class FunctionLogicalExpressionParser : ILogicalExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		int i = index;
		if (!FunctionExpressionParser.TryParseFunction(source, ref i, out var parameters, out var function, options))
		{
			expression = null;
			return false;
		}

		if (function is not LogicalFunctionDefinition logicalFunc)
		{
			expression = null;
			return false;
		}

		expression = new FunctionLogicalExpressionNode(logicalFunc, parameters);
		index = i;
		return true;
	}
}
