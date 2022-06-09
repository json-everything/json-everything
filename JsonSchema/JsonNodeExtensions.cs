using System;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema;

internal static class JsonNodeExtensions
{
	public static SchemaValueType GetSchemaValueType(this JsonNode? node)
	{
		if (node is null) return SchemaValueType.Null;
		if (node is JsonArray) return SchemaValueType.Array;
		if (node is JsonObject) return SchemaValueType.Object;
		if (node is JsonValue value)
		{
			var obj = value.GetValue<object>();
			var objType = obj.GetType();
			if (objType.IsInteger()) return SchemaValueType.Integer;
			if (objType.IsNumber()) return SchemaValueType.Number;
			if (obj is string) return SchemaValueType.String;
			if (obj is bool) return SchemaValueType.Boolean;
		}

		throw new ArgumentOutOfRangeException(nameof(node));
	}

}