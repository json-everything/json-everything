using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class ValueFunctionExpressionNode : ValueExpressionNode
{
	public IPathFunctionDefinition Function { get; }
	public ValueExpressionNode[] Parameters { get; }

	public ValueFunctionExpressionNode(IPathFunctionDefinition function, IEnumerable<ValueExpressionNode> parameters)
	{
		Function = function;
		Parameters = parameters.ToArray();
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		var parameterValues = Parameters.Select(x =>
		{
			var result = x.Evaluate(globalParameter, localParameter);
			if (result != null) return (NodeList)result;
			return NodeList.Empty;
		});

		return Function.Evaluate(parameterValues);
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

internal class ValueFunctionExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression)
	{
		if (!FunctionExpressionParser.TryParseFunction(source, ref index, out var parameters, out var function))
		{
			expression = null;
			return false;
		}

		expression = new ValueFunctionExpressionNode(function, parameters);
		return true;
	}
}
