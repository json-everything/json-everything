using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class FunctionExpressionNode : ValueExpressionNode
{
	public string Name { get; }
	public IEnumerable<ValueExpressionNode> Parameters { get; }

	public FunctionExpressionNode(string name, IEnumerable<ValueExpressionNode> parameters)
	{
		Name = name;
		Parameters = parameters;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		throw new NotImplementedException();
	}
}

internal class FunctionExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression)
	{
		int i = index;

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
					index++;
					break;
				default:
					expression = null;
					return false;
			}
		}

		expression = new FunctionExpressionNode(name, parameters);
		return true;
	}
}