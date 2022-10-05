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

	/// <summary>
	/// Attempts to resolve the reference.
	/// </summary>
	/// <param name="context">The schema evaluation context.</param>
	/// <param name="value">If return is true, the value at the indicated location.</param>
	/// <returns>true if resolution is successful; false otherwise.</returns>
	public bool TryResolve(EvaluationContext context, out JsonNode? value)
	{
		return Target.TryEvaluate(context.InstanceRoot, out value);
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}