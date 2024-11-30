using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Path;
using Json.Pointer;
using TestHelpers;

namespace Json.Schema.OpenApi.Tests;

// OpenAPI.Net would need to set up their root document class to handle this.
// This implementation is not optimal, but it seems to work.
// Having a base URI is very important.  The resolution system will break if one is not present.
// If needed, generate one.
public class OpenApiDoc : IBaseDocument
{
	private static readonly JsonPointer _componentSchemasLocation = JsonPointer.Parse("/components/schemas");
	private static readonly JsonPath _schemasQuery = JsonPath.Parse("$..schema");

	private readonly Dictionary<JsonPointer, JsonSchema> _lookup = [];

	// implements IBaseDocument
	public Uri BaseUri { get; }

	public OpenApiDoc(Uri baseUri, JsonNode definition)
	{
		BaseUri = baseUri;

		// Register this doc under the URI so that it's resolved when requested
		SchemaRegistry.Global.Register(this);

		Initialize(definition);
	}

	// implements IBaseDocument
	public JsonSchema? FindSubschema(JsonPointer pointer, EvaluationOptions options)
	{
		return _lookup.GetValueOrDefault(pointer);
	}

	private void Initialize(JsonNode definition)
	{
		// Register schemas in places we know schemas live
		if (_componentSchemasLocation.TryEvaluate(definition, out var componentSchemas))
		{
			foreach (var (name, node) in componentSchemas!.AsObject())
			{
				var location = _componentSchemasLocation.Combine(name);
				var schema = node.Deserialize<JsonSchema>(TestEnvironment.SerializerOptions)!;
				schema.BaseUri = BaseUri;
				_lookup[location] = schema;
			}
		}

		// Search the document for other places a schema might be hiding
		var otherSchemaLocations = _schemasQuery.Evaluate(definition);
		foreach (var match in otherSchemaLocations.Matches)
		{
			var location = ConvertToPointer(match.Location!);
			var schema = match.Value.Deserialize<JsonSchema>(TestEnvironment.SerializerOptions)!;
			schema.BaseUri = BaseUri;
			_lookup[location] = schema;
		}
	}

	// Paths are returned like: $['components']['schemas']
	// but we need pointers like /components/schemas to do lookups
	private static JsonPointer ConvertToPointer(JsonPath path)
	{
		return JsonPointer.Parse(path.AsJsonPointer());
	}
}