using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Handles data references that are Relative JSON Pointers.
/// </summary>
public class RelativeJsonPointerIdentifier : IDataResourceIdentifier
{
	/// <summary>
	/// The Relative JSON Pointer target.
	/// </summary>
	public RelativeJsonPointer Target { get; }

	/// <summary>
	/// Creates a new instance of <see cref="RelativeJsonPointerIdentifier"/>.
	/// </summary>
	/// <param name="target">The target.</param>
	public RelativeJsonPointerIdentifier(RelativeJsonPointer target)
	{
		Target = target;
	}

	public bool TryResolve(KeywordEvaluation evaluation, SchemaRegistry registry, out JsonNode? value)
	{
		if (evaluation.LocalInstance == null)
		{
			value = null;
			return false;
		}
		return Target.TryEvaluate(evaluation.LocalInstance, out value);
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}