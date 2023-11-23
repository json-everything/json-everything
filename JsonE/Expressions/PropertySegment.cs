using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class PropertySegment : IContextAccessorSegment
{
	private readonly bool _isBracketed;

	public string Name { get; }

	public PropertySegment(string name, bool isBracketed)
	{
		Name = name;
		_isBracketed = isBracketed;
	}

	public bool TryFind(JsonNode? contextValue, out JsonNode? value)
	{
		return TryFind(Name, _isBracketed, contextValue, out value);
	}

	public static bool TryFind(string name, bool isBracketed, JsonNode? contextValue, out JsonNode? value)
	{
		value = null;
		if (contextValue is JsonObject obj) return obj.TryGetValue(name, out value, out _);

		if (!isBracketed) throw new InterpreterException("infix: . expects objects");

		throw new InterpreterException("should only use integers to access arrays or strings");
	}
}