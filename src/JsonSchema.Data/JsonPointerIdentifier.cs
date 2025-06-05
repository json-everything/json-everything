using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Handles data references that are JSON Pointers.
/// </summary>
public class JsonPointerIdentifier : IDataResourceIdentifier
{
	/// <summary>
	/// The JSON PointerOld target.
	/// </summary>
	public JsonPointer_Old Target { get; }

	/// <summary>
	/// Creates a new instance of <see cref="JsonPointerIdentifier"/>.
	/// </summary>
	/// <param name="target">The target.</param>
	public JsonPointerIdentifier(JsonPointer_Old target)
	{
		Target = target;
	}

	/// <summary>
	/// Resolves a resource.
	/// </summary>
	/// <param name="evaluation">The evaluation being process.  This will help identify.</param>
	/// <param name="registry">The schema registry.</param>
	/// <param name="value">The value, if <paramref name="evaluation"/> was resolvable.</param>
	/// <returns>True if resolution was successful; false otherwise.</returns>
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