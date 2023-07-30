using System;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Handles data references that are JSON Pointers.
/// </summary>
public class JsonPointerIdentifier : IDataResourceIdentifier
{
	/// <summary>
	/// The JSON Pointer target.
	/// </summary>
	public JsonPointer Target { get; }

	/// <summary>
	/// Creates a new instance of <see cref="JsonPointerIdentifier"/>.
	/// </summary>
	/// <param name="target">The target.</param>
	public JsonPointerIdentifier(JsonPointer target)
	{
		Target = target;
	}

	public bool TryResolve(KeywordEvaluation evaluation, SchemaRegistry registry, out JsonNode? value)
	{
		return Target.TryEvaluate(evaluation.LocalInstance!.Root, out value);
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}