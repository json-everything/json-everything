using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;
using Json.More;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Json.Path.Expressions;

internal class LiteralValueExpressionNode : LeafValueExpressionNode
{
	private static readonly JsonSerializerOptions _options = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	private readonly JsonNode? _value;

	public LiteralValueExpressionNode(JsonNode? value)
	{
		_value = value;
	}

	public override PathValue? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return _value;
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(_value.AsJsonString(_options));
	}

	public override string ToString()
	{
		return _value.AsJsonString(_options);
	}

	public override bool Equals(object? obj) =>
		obj is LiteralValueExpressionNode other && _value.IsEquivalentTo(other._value);

	public override int GetHashCode() => _value?.GetEquivalenceHashCode() ?? 0;
}

internal class LiteralValueExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out ValueExpressionNode? expression, PathParsingOptions options)
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

		expression = new LiteralValueExpressionNode(node);
		return true;
	}
}