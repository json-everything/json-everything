using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Path;
using Json.Pointer;

namespace Json.Schema.OpenApi.Tests;

// OpenAPI.Net would need to set up their root document class to handle this.
// This implementation is not optimal, but it seems to work.
// Having a base URI is very important.  The resolution system will break if one is not present.
// If needed, generate one.
public class OpenApiDoc : IBaseDocument
{
	private readonly JsonElement _definition;
	private readonly Dictionary<JsonPointer, JsonSchemaNode> _lookup = [];

	// implements IBaseDocument
	public Uri BaseUri { get; }

	public OpenApiDoc(Uri baseUri, JsonElement definition)
	{
		_definition = definition;
		BaseUri = baseUri;

		// Register this doc under the URI so that it's resolved when requested
		SchemaRegistry.Global.Register(this);
	}

	// implements IBaseDocument
	public JsonSchemaNode? FindSubschema(JsonPointer pointer, BuildContext context)
	{
		var subschema = _lookup.GetValueOrDefault(pointer);
		if (subschema is null)
		{
			var target = pointer.Evaluate(_definition);
			if (target is null) return null;

#pragma warning disable CS0618 // Type or member is obsolete
			var newContext = context with
			{
				LocalSchema = target.Value,
				BaseUri = BaseUri,
				PathFromResourceRoot = pointer
			};
			subschema = JsonSchema.BuildNode(newContext);
			subschema.PathFromResourceRoot = pointer;
#pragma warning restore CS0618 // Type or member is obsolete

			_lookup[pointer] = subschema;
		}

		return subschema;
	}

	// Paths are returned like: $['components']['schemas']
	// but we need pointers like /components/schemas to do lookups
	private static JsonPointer ConvertToPointer(JsonPath path) => 
		JsonPointer.Parse(path.AsJsonPointer());

	private static JsonElement ConvertToElement(JsonNode? node) =>
		JsonSerializer.SerializeToElement(node, TestSerializationContext.Default.JsonNode!);
}