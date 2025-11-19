using System;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Represents an identifiable or locatable base document that may be or contain a JSON Schema.
/// </summary>
public interface IBaseDocument
{
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
	Uri BaseUri { get; }

	/// <summary>
	/// Finds a schema within the document.
	/// </summary>
	/// <param name="pointer">A JSON Pointer to the location of the schema within the document.</param>
	/// <param name="options">Evaluation options.  This is needed for internal processing.</param>
	/// <returns>A JSON Schema, if found.</returns>
	JsonSchemaNode? FindSubschema(JsonPointer pointer, BuildOptions options);
}