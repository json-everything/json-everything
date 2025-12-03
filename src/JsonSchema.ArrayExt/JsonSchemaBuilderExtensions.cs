using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Provides a fluent interface for <see cref="JsonSchemaBuilder"/>.
/// </summary>
public static class JsonSchemaBuilderExtensions
{
	/// <summary>
	/// Adds a `uniqueKeys` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="keys">The collection of pointers to the keys which should be unique within the array.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder UniqueKeys(this JsonSchemaBuilder builder, params IEnumerable<JsonPointer> keys)
	{
		builder.Add("uniqueKeys", new JsonArray(keys.Select(x => (JsonNode?)x.ToString()).ToArray()));
		return builder;
	}

	/// <summary>
	/// Adds a `uniqueKeys` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="keys">The collection of pointers to the keys which should be unique within the array.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder UniqueKeys(this JsonSchemaBuilder builder, params IEnumerable<string> keys)
	{
		builder.Add("uniqueKeys", new JsonArray(keys.Select(x => (JsonNode?)x).ToArray()));
		return builder;
	}

	/// <summary>
	/// Adds an `ordering` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="specifiers">The collection of ordering specifiers.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Ordering(this JsonSchemaBuilder builder, params IEnumerable<OrderingSpecifier> specifiers)
	{
		builder.Add("ordering", JsonSerializer.SerializeToNode(specifiers.ToArray(), JsonSchemaArrayExtSerializerContext.Default.OrderingSpecifierArray));
		return builder;
	}
}