using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class PathExpressionNode : ValueExpressionNode
{
	public JsonPath Path { get; }

	public PathExpressionNode(JsonPath path)
	{
		Path = path;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		var parameter = Path.Scope == PathScope.Global
			? globalParameter
			: localParameter;

		var result = Path.Evaluate(parameter);

		return result.Matches?.Count == 1
			? result.Matches[0].Value
			: null;
	}

	public override void BuildString(StringBuilder builder)
	{
		Path.BuildString(builder);
	}

	public static implicit operator PathExpressionNode(JsonPath value)
	{
		return new PathExpressionNode(value);
	}

	public override string ToString()
	{
		return Path.ToString();
	}
}

internal class PathExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression)
	{
		if (!PathParser.TryParse(source, ref index, out var path))
		{
			expression = null;
			return false;
		}

		expression = new PathExpressionNode(path);
		return true;
	}
}
