using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal class LiteralExpressionNode : ValueExpressionNode
{
	public JsonNode? Value { get; }

	public LiteralExpressionNode(JsonNode? value)
	{
		Value = value ?? JsonNull.SignalNode;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Value;
	}

	public static implicit operator LiteralExpressionNode(JsonNode? value)
	{
		return new LiteralExpressionNode(value);
	}
}

internal class LiteralExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression)
	{
		if (!source.TryParseJson(ref index, out var node))
		{
			expression = null;
			return false;
		}

		expression = new LiteralExpressionNode(node);
		return true;
	}
}