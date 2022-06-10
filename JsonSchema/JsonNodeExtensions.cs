using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema;

public static class JsonNodeExtensions
{
	public static SchemaValueType GetSchemaValueType(this JsonNode? node)
	{
		if (node is null) return SchemaValueType.Null;
		if (node is JsonArray) return SchemaValueType.Array;
		if (node is JsonObject) return SchemaValueType.Object;
		if (node is JsonValue value)
		{
			var obj = value.GetValue<object>();
			if (obj is JsonElement element) return GetSchemaValueType(element);
			var objType = obj.GetType();
			if (objType.IsInteger()) return SchemaValueType.Integer;
			if (objType.IsNumber()) return SchemaValueType.Number;
			if (obj is string) return SchemaValueType.String;
			if (obj is bool) return SchemaValueType.Boolean;
		}

		throw new ArgumentOutOfRangeException(nameof(node));
	}

	private static SchemaValueType GetSchemaValueType(JsonElement element) =>
		element.ValueKind switch
		{
			JsonValueKind.Object => SchemaValueType.Object,
			JsonValueKind.Array => SchemaValueType.Array,
			JsonValueKind.String => SchemaValueType.String,
			JsonValueKind.Number => element.TryGetInt64(out _)
				? SchemaValueType.Integer
				: SchemaValueType.Number,
			JsonValueKind.True => SchemaValueType.Boolean,
			JsonValueKind.False => SchemaValueType.Boolean,
			JsonValueKind.Null => SchemaValueType.Null,
			_ => throw new ArgumentOutOfRangeException(nameof(element.ValueKind), element.ValueKind, null)
		};
}