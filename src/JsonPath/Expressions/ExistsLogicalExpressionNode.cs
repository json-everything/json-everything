using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class ExistsLogicalExpressionNode : LeafLogicalExpressionNode
{
	public JsonPath Path { get; }

	public ExistsLogicalExpressionNode(JsonPath path)
	{
		Path = path;
	}

	public override void BuildString(StringBuilder builder)
	{
		Path.BuildString(builder);
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		var parameter = Path.Scope == PathScope.Global
			? globalParameter
			: localParameter;

		var result = Path.Evaluate(parameter);

		return result.Matches.Count != 0;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Path.ToString();
	}
}

internal class ExistsLogicalExpressionParser : ILogicalExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		if (!PathParser.TryParse(source, ref index, out var path, options))
		{
			expression = null;
			return false;
		}

		expression = new ExistsLogicalExpressionNode(path);
		return true;
	}
}