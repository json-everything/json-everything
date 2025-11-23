using System;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Provides some schema-related functionality for <see cref="JsonElement"/>.
/// </summary>
public static class JsonElementExtensions
{
	/// <summary>
	/// Provides the JSON Schema type for a value.
	/// </summary>
	/// <param name="element">The node.</param>
	/// <returns>The <see cref="SchemaValueType"/> of the value.</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static SchemaValueType GetSchemaValueType(this JsonElement element) =>
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