using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class LiteralSegment : IContextAccessorSegment
{
	private readonly JsonNode? _literal;

	public LiteralSegment(JsonNode? literal)
	{
		_literal = literal;
	}

	public bool TryFind(JsonNode? contextValue, out JsonNode? value)
	{
		value = _literal;
		return true;
	}
}