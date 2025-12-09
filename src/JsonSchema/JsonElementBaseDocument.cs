using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides a general <see cref="IBaseDocument"/> implementation for a <see cref="JsonNode"/>.
/// </summary>
public class JsonElementBaseDocument : IBaseDocument
{
	private readonly JsonElement _root;
	private readonly ConcurrentDictionary<JsonPointer, JsonSchemaNode?> _foundSubschemas;

	/// <summary>
	/// Gets the base URI that applies to this schema.  This may be defined by a parent schema.
	/// </summary>
	public Uri BaseUri { get; }

	/// <summary>
	/// Creates a new <see cref="JsonElementBaseDocument"/>.
	/// </summary>
	/// <param name="node">The JsonNode.</param>
	/// <param name="baseUri">The identifying base URI.</param>
	public JsonElementBaseDocument(JsonElement node, Uri baseUri)
	{
		_root = node.Clone();
		_foundSubschemas = new ConcurrentDictionary<JsonPointer, JsonSchemaNode?>();

		BaseUri = baseUri;
	}

	/// <summary>
	/// Finds a schema within the document.
	/// </summary>
	/// <param name="pointer">A JSON Pointer to the location of the schema within the document.</param>
	/// <param name="context">Build context.</param>
	/// <returns>A JSON Schema, if found.</returns>
#pragma warning disable CS0618 // Type or member is obsolete
	public JsonSchemaNode? FindSubschema(JsonPointer pointer, BuildContext context)
	{
		return _foundSubschemas.GetOrAdd(pointer, _ =>
		{
			var localSchema = pointer.Evaluate(_root);
			if (localSchema is null) return null;

			var newContext = context with
			{
				LocalSchema = localSchema.Value,
				BaseUri = BaseUri,
				PathFromResourceRoot = pointer
			};
			var node = JsonSchema.BuildNode(newContext);
			node.PathFromResourceRoot = pointer;

			return node;
		});
#pragma warning restore CS0618 // Type or member is obsolete
	}
}