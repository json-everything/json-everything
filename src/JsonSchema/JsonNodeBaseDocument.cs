using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides a general <see cref="IBaseDocument"/> implementation for a <see cref="JsonNode"/>.
/// </summary>
public class JsonNodeBaseDocument : IBaseDocument
{
	private readonly JsonElement _node;
	private readonly ConcurrentDictionary<JsonPointer, JsonSchemaNode?> _foundSubschemas;

	/// <summary>
	/// Gets the base URI that applies to this schema.  This may be defined by a parent schema.
	/// </summary>
	/// <remarks>
	/// This property is initialized to a generated random value that matches `https://json-everything.net/{random}`
	/// where `random` is 10 hex characters.
	///
	/// It may change after the initial evaluation based on whether the schema contains an `$id` keyword
	/// or is a child of another schema.
	/// </remarks>
	public Uri BaseUri { get; }

	/// <summary>
	/// Creates a new <see cref="JsonNodeBaseDocument"/>.
	/// </summary>
	/// <param name="node">The JsonNode.</param>
	/// <param name="baseUri">The identifying base URI.</param>
	public JsonNodeBaseDocument(JsonElement node, Uri baseUri)
	{
		_node = node.Clone();
		_foundSubschemas = new ConcurrentDictionary<JsonPointer, JsonSchemaNode?>();

		BaseUri = baseUri;
	}

	/// <summary>
	/// Finds a schema within the document.
	/// </summary>
	/// <param name="pointer">A JSON Pointer to the location of the schema within the document.</param>
	/// <param name="context">Build context.</param>
	/// <returns>A JSON Schema, if found.</returns>
	public JsonSchemaNode? FindSubschema(JsonPointer pointer, BuildContext context)
	{
		return _foundSubschemas.GetOrAdd(pointer, _ =>
		{
			var localSchema = pointer.Evaluate(_node);
			if (localSchema is null) return null;

			var newContext = context with
			{
				LocalSchema = localSchema.Value
			};
			return JsonSchema.BuildNode(newContext);
		});
	}
}