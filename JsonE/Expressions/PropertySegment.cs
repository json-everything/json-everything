using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class PropertySegment : IContextAccessorSegment
{
	private readonly string _name;
	private readonly bool _isBracketed;

	public PropertySegment(string name, bool isBracketed)
	{
		_name = name;
		_isBracketed = isBracketed;
	}

	public bool TryFind(JsonNode? target, out JsonNode? value)
	{
		value = null;
		return target is JsonObject obj && obj.TryGetValue(_name, out value, out _);
	}
}