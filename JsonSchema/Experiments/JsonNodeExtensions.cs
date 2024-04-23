using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public static class JsonNodeExtensions
{
	private static readonly JsonNode _null = "null";
	private static readonly JsonNode _object = "object";
	private static readonly JsonNode _array = "array";
	private static readonly JsonNode _number = "number";
	private static readonly JsonNode _integer = "integer";
	private static readonly JsonNode _string = "string";
	private static readonly JsonNode _boolean = "boolean";

	/// <summary>
	/// Provides the JSON Schema type of a node.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <returns>The type of the node.</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static JsonNode GetSchemaValueType(this JsonNode? node)
	{
		if (node is null) return _null;
		if (node is JsonArray) return _array;
		if (node is JsonObject) return _object;
		if (node is JsonValue value)
		{
			var obj = value.GetValue<object>();
			if (obj is JsonElement element) return GetSchemaValueType(element);
			var objType = obj.GetType();
			if (objType.IsInteger()) return _integer;
			if (objType.IsNumber()) return _number;
			if (obj is string) return _string;
			if (obj is bool) return _boolean;
		}

		throw new ArgumentOutOfRangeException(nameof(node));
	}

	private static JsonNode GetSchemaValueType(JsonElement element) =>
		element.ValueKind switch
		{
			JsonValueKind.Object => _object,
			JsonValueKind.Array => _array,
			JsonValueKind.String => _string,
			JsonValueKind.Number => element.TryGetDecimal(out var d) && Math.Floor(d) == d
				? _integer
				: _number,
			JsonValueKind.True => _boolean,
			JsonValueKind.False => _boolean,
			JsonValueKind.Null => _null,
			_ => throw new ArgumentOutOfRangeException(nameof(element.ValueKind), element.ValueKind, null)
		};
}