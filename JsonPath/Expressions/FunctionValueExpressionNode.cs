using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class FunctionValueExpressionNode : ValueExpressionNode
{
	public ValueFunctionDefinition Function { get; }
	public ExpressionNode[] Parameters { get; }

	public FunctionValueExpressionNode(ValueFunctionDefinition function, IEnumerable<ExpressionNode> parameters)
	{
		Function = function;
		Parameters = parameters.ToArray();
	}

	public override PathValue? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		var parameterValues = Parameters.Select(x =>
		{
			return x switch
			{
				ValueExpressionNode c => (object?)c.Evaluate(globalParameter, localParameter),
				BooleanResultExpressionNode b => b.Evaluate(globalParameter, localParameter),
				_ => throw new ArgumentOutOfRangeException("parameter")
			};
		}).ToArray();

		return Function.Invoke(parameterValues);
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

internal class FunctionValueExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression, PathParsingOptions options)
	{
		int i = index;
		if (!FunctionExpressionParser.TryParseFunction(source, ref i, out var parameters, out var function, options))
		{
			expression = null;
			return false;
		}

		if (function is not ValueFunctionDefinition valueFunction)
		{
			expression = null;
			return false;
		}

		index = i;
		expression = new FunctionValueExpressionNode(valueFunction, parameters);
		return true;
	}
}
