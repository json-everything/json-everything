using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Schema;

/// <summary>
/// A fluent-style builder for <see cref="JsonSchema"/>.
/// </summary>
public class JsonSchemaBuilder
{
	internal readonly JsonNode Keywords = new JsonObject();

	public static JsonSchemaBuilder Empty { get; } = new();
	public static JsonSchemaBuilder True { get; } = new(true);
	public static JsonSchemaBuilder False { get; } = new(false);

	/// <summary>
	/// Initializes a new instance of the JsonSchemaBuilder class.
	/// </summary>
	/// <remarks>Use this constructor to create a new JsonSchemaBuilder for defining and constructing JSON schema
	/// objects. The builder can be configured with various schema properties and constraints before generating the final
	/// schema.</remarks>
	public JsonSchemaBuilder(){}

	private JsonSchemaBuilder(bool value)
	{
		Keywords = value;
	}

	/// <summary>
	/// Adds a new keyword with a value.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="value">The value.</param>
	public void Add(string keyword, JsonNode? value)
	{
		Keywords[keyword] = value;
	}

	/// <summary>
	/// Adds a new keyword with a nested schema.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="builder">Another builder.</param>
	public void Add(string keyword, JsonSchemaBuilder builder)
	{
		Keywords[keyword] = builder.Keywords;
	}

	/// <summary>
	/// Adds a new keyword with a nested schema.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="builders">Another builder.</param>
	public void Add(string keyword, IEnumerable<JsonSchemaBuilder> builders)
	{
		Keywords[keyword] = new JsonArray(builders.Select(x => (JsonNode?)x.Keywords).ToArray());
	}

	/// <summary>
	/// Adds a new keyword with a nested schema.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="builders">Another builder.</param>
	public void Add(string keyword, IEnumerable<(string, JsonSchemaBuilder)> builders)
	{
		Keywords[keyword] = new JsonObject(builders.ToDictionary(x => x.Item1, x => (JsonNode?)x.Item2.Keywords));
	}

	/// <summary>
	/// Adds a new keyword with a nested schema.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="builders">Another builder.</param>
	public void Add(string keyword, IEnumerable<KeyValuePair<string, JsonSchemaBuilder>> builders)
	{
		Keywords[keyword] = new JsonObject(builders.ToDictionary(x => x.Key, x => (JsonNode?)x.Value.Keywords));
	}

	/// <summary>
	/// Gets a keyword if one has been added.
	/// </summary>
	/// <typeparam name="T">the keyword type.</typeparam>
	/// <returns>The keyword, if it exists; `null` otherwise.</returns>
	public T? Get<T>()
		where T : IKeywordHandler
	{
		throw new NotImplementedException();
		//return _keywords.Values.OfType<T>().SingleOrDefault();
	}

	/// <summary>
	/// Creates a new <see cref="JsonSchema"/>.
	/// </summary>
	/// <returns>A JSON Schema that simply refers back to the root schema.</returns>
	public static JsonSchemaBuilder RefRoot()
	{
		return new JsonSchemaBuilder().Ref(new Uri("#", UriKind.RelativeOrAbsolute));
	}

	/// <summary>
	/// Creates a new <see cref="JsonSchema"/>.
	/// </summary>
	/// <returns>A JSON Schema that simply refers back to the recursive root schema.</returns>
	public static JsonSchemaBuilder RecursiveRefRoot()
	{
		return new JsonSchemaBuilder().RecursiveRef(new Uri("#", UriKind.RelativeOrAbsolute));
	}

	/// <summary>
	/// Builds the schema.
	/// </summary>
	/// <returns>A <see cref="JsonSchema"/>.</returns>
	public JsonSchema Build(BuildOptions? options = null, Uri? baseUri = null)
	{
		var root = JsonSerializer.SerializeToElement(Keywords, JsonSchemaSerializerContext.Default.JsonNode);

		return JsonSchema.Build(root, options, baseUri);
	}

	/// <summary>
	/// For convenience, implicitly calls <see cref="Build()"/>.
	/// </summary>
	/// <returns>A <see cref="JsonSchema"/>.</returns>
	public static implicit operator JsonSchema(JsonSchemaBuilder builder)
	{
		return builder.Build();
	}
}
