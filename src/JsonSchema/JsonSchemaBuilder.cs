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
	private readonly BuildOptions? _buildOptions;
	internal readonly JsonNode Keywords = new JsonObject();

	/// <summary>
	/// Gets an instance of <see cref="JsonSchemaBuilder"/> that represents an empty JSON schema.
	/// </summary>
	/// <remarks>Use this property to obtain a schema with no defined constraints or properties. This can be useful
	/// as a starting point for building more complex schemas or when a schema is required but no validation is
	/// needed.</remarks>
	public static JsonSchemaBuilder Empty { get; } = new();

	/// <summary>
	/// Gets a schema builder that creates a schema which always validates as <see langword="true"/>.
	/// </summary>
	/// <remarks>Use this property to obtain a schema that unconditionally accepts any JSON value. This is
	/// equivalent to a schema defined as <c>true</c> in JSON Schema specifications.</remarks>
	public static JsonSchemaBuilder True { get; } = new(true);

	/// <summary>
	/// Gets a schema builder that always evaluates to false, representing a schema that does not match any JSON value.
	/// </summary>
	/// <remarks>Use this property to create a schema that explicitly rejects all input. This can be useful for
	/// disabling or invalidating parts of a schema in compositional scenarios.</remarks>
	public static JsonSchemaBuilder False { get; } = new(false);

	/// <summary>
	/// Initializes a new instance of the JsonSchemaBuilder class.
	/// </summary>
	/// <remarks>Use this constructor to create a new JsonSchemaBuilder for defining and constructing JSON schema
	/// objects. The builder can be configured with various schema properties and constraints before generating the final
	/// schema.</remarks>
	public JsonSchemaBuilder(BuildOptions? buildOptions = null)
	{
		_buildOptions = buildOptions;
	}

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
		Keywords[keyword] = builder.Keywords.DeepClone();
	}

	/// <summary>
	/// Adds a new keyword with a nested schema.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="builders">Another builder.</param>
	public void Add(string keyword, IEnumerable<JsonSchemaBuilder> builders)
	{
		Keywords[keyword] = new JsonArray(builders.Select(x => (JsonNode?)x.Keywords.DeepClone()).ToArray());
	}

	/// <summary>
	/// Adds a new keyword with a nested schema.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="builders">Another builder.</param>
	public void Add(string keyword, IEnumerable<(string, JsonSchemaBuilder)> builders)
	{
		Keywords[keyword] = new JsonObject(builders.ToDictionary(x => x.Item1, x => (JsonNode?)x.Item2.Keywords.DeepClone()));
	}

	/// <summary>
	/// Adds a new keyword with a nested schema.
	/// </summary>
	/// <param name="keyword">The keyword to add.</param>
	/// <param name="builders">Another builder.</param>
	public void Add(string keyword, IEnumerable<KeyValuePair<string, JsonSchemaBuilder>> builders)
	{
		Keywords[keyword] = new JsonObject(builders.ToDictionary(x => x.Key, x => (JsonNode?)x.Value.Keywords.DeepClone()));
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
	/// Builds a JSON schema from the configured keywords and returns a corresponding <see cref="JsonSchema"/> instance.
	/// </summary>
	/// <param name="options">(Optional) Build options that control schema generation behavior.  Overrides build options
	/// passed into the constructor.</param>
	/// <param name="baseUri">(Optional) A base URI to associate with the generated schema. If specified, it is used to resolve relative
	/// references within the schema.</param>
	/// <returns>A <see cref="JsonSchema"/> instance representing the constructed schema based on the provided options and base URI.</returns>
	public JsonSchema Build(BuildOptions? options = null, Uri? baseUri = null)
	{
		var root = JsonSerializer.SerializeToElement(Keywords, JsonSchemaSerializerContext.Default.JsonNode);

		return JsonSchema.Build(root, options ?? _buildOptions, baseUri);
	}

	/// <summary>
	/// For convenience, implicitly calls <see cref="Build(BuildOptions, Uri)"/>.
	/// </summary>
	/// <returns>A <see cref="JsonSchema"/>.</returns>
	public static implicit operator JsonSchema(JsonSchemaBuilder builder)
	{
		return builder.Build();
	}

	/// <summary>
	/// Defines an implicit conversion from a Boolean value to a JsonSchemaBuilder representing either the 'true' or
	/// 'false' JSON Schema.
	/// </summary>
	/// <param name="schema">A Boolean value indicating whether to create a schema that always validates (<see langword="true"/>) or never
	/// validates (<see langword="false"/>).</param>
	public static implicit operator JsonSchemaBuilder(bool schema) => schema ? True : False;
}
