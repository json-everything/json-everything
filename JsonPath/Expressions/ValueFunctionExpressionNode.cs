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
		int i = index;

		source.ConsumeWhitespace(ref i);

		// parse function name
		if (!source.TryParseName(ref i, out var name))
		{
			expression = null;
			return false;
		}

		source.ConsumeWhitespace(ref i);

		// consume (
		if (source[i] != '(')
		{
			expression = null;
			return false;
		}
		i++;

		// parse list of parameters - all value expressions
		var parameters = new List<ValueExpressionNode>();
		var done = false;

		while (i < source.Length && !done)
		{
			source.ConsumeWhitespace(ref i);

			if (!ValueExpressionParser.TryParse(source, ref i, out var parameter)) break;

			parameters.Add(parameter);

			source.ConsumeWhitespace(ref i);
		
			switch (source[i])
			{
				case ')':
					done = true;
					i++;
					break;
				case ',':
					i++;
					break;
				default:
					expression = null;
					return false;
			}
		}

		if (!FunctionRepository.TryGet(name, out var function))
		{
			expression = null;
			return false;
		}

		if (function.MinArgumentCount > parameters.Count ||
		    parameters.Count > function.MaxArgumentCount)
		{
			expression = null;
			return false;
		}

		expression = new ValueFunctionExpressionNode(function, parameters);
		index = i;
		return true;
	}
}
