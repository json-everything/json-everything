using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;
using Json.More;
using System.Text;

namespace Json.Path.Expressions;

internal class LiteralExpressionNode : ValueExpressionNode
{
	private readonly JsonNode? _value;

	public LiteralExpressionNode(JsonNode? value)
	{
		_value = value;
	}

	public override PathValue? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return _value;
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(_value.AsJsonString());
	}

	public override string ToString()
	{
		return _value.AsJsonString();
	}
}

internal class LiteralExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression, PathParsingOptions options)
	{
		if (!source.TryParseJson(ref index, out var node))
		{
			expression = null;
			return false;
		}

		if (node is not (null or JsonValue) && !options.AllowJsonConstructs)
		{
			expression = null;
			return false;
		}

		expression = new LiteralExpressionNode(node);
		return true;
	}
}