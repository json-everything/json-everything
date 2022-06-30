using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Provides some schema-related functionality for <see cref="JsonNode"/>.
/// </summary>
public static class JsonNodeExtensions
{
	/// <summary>
	/// Provides the JSON Schema type of a node.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <returns>The <see cref="SchemaValueType"/> of the node.</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
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

	/// <summary>
	/// Verifies that a <see cref="JsonObject"/> doesn't have any duplicate keys and can
	/// therefore be processed.
	/// </summary>
	/// <param name="obj">The object.</param>
	/// <param name="context">The validation context to log errors.</param>
	/// <returns>true if the the object can be processed; false otherwise.</returns>
	/// <remarks>See https://github.com/dotnet/runtime/issues/70604 for more information.</remarks>
	public static bool VerifyJsonObject(this JsonObject obj, ValidationContext context)
	{
		if (!obj.TryGetValue("_", out _, out var e) && e != null)
		{
			context.Log(() => "This object has a duplicate key and cannot be processed.");
			context.LocalResult.Fail("__instance_error", "This object has a duplicate key and cannot be processed.");
			return false;
		}

		return true;
	}
}