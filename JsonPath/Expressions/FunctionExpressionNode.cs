using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class FunctionExpressionNode : ValueExpressionNode
{
	public string Name { get; }
	public ValueExpressionNode[] Parameters { get; }

	public FunctionExpressionNode(string name, IEnumerable<ValueExpressionNode> parameters)
	{
		Name = name;
		Parameters = parameters.ToArray();
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		throw new NotImplementedException();
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Name);
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
		return $"{Name}({string.Join(',', (IEnumerable<ValueExpressionNode>)Parameters)})";
	}
}

internal class FunctionExpressionParser : IValueExpressionParser
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