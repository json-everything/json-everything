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
	private readonly JsonNode _node;
	private readonly ConcurrentDictionary<JsonPointer_Old, JsonSchema?> _foundSubschemas;

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
	public JsonNodeBaseDocument(JsonNode node, Uri baseUri)
	{
		_node = node;
		_foundSubschemas = new ConcurrentDictionary<JsonPointer_Old, JsonSchema?>();

		BaseUri = baseUri;
	}

	/// <summary>
	/// Finds a schema within the document.
	/// </summary>
	/// <param name="pointerOld">A JSON PointerOld to the location of the schema within the document.</param>
	/// <param name="options">Evaluation options.  This is needed for internal processing.</param>
	/// <returns>A JSON Schema, if found.</returns>
	public JsonSchema? FindSubschema(JsonPointer_Old pointerOld, EvaluationOptions options)
	{
		return _foundSubschemas.GetOrAdd(pointerOld, jsonPointer =>
		{
			if (!jsonPointer.TryEvaluate(_node, out var location)) return null;

			var schema = location.Deserialize(JsonSchemaSerializerContext.Default.JsonSchema);
			if (schema != null)
			{
				schema.BaseUri = BaseUri;
				options.SchemaRegistry.Initialize(BaseUri, schema);
			}

			return schema;
		});
	}
}