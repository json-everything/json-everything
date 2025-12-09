using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Schema.Data;

/// <summary>
/// Provides a fluent interface for <see cref="JsonSchemaBuilder"/>.
/// </summary>
public static class JsonSchemaBuilderExtensions
{
	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, string> data)
	{
		builder.Add("data", new JsonObject(data.ToDictionary(x => x.Key, x => (JsonNode?)x.Value)));
		return builder;
	}

	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, params (string name, string reference)[] data)
	{
		builder.Add("data", new JsonObject(data.ToDictionary(x => x.name, x => (JsonNode?)x.reference)));
		return builder;
	}

	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder OptionalData(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, string> data)
	{
		builder.Add("optionalData", new JsonObject(data.ToDictionary(x => x.Key, x => (JsonNode?)x.Value)));
		return builder;
	}

	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder OptionalData(this JsonSchemaBuilder builder, params (string name, string reference)[] data)
	{
		builder.Add("optionalData", new JsonObject(data.ToDictionary(x => x.name, x => (JsonNode?)x.reference)));
		return builder;
	}
}