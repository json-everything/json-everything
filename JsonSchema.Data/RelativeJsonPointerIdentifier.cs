using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

	/// <summary>
	/// Attempts to resolve the reference.
	/// </summary>
	/// <param name="context">The schema evaluation context.</param>
	/// <returns>true if resolution is successful; false otherwise.</returns>
	public Task<(bool, JsonNode?)> TryResolve(EvaluationContext context)
	{
		if (context.LocalInstance == null)
			return Task.FromResult<(bool, JsonNode?)>((false, null));

		var success = Target.TryEvaluate(context.LocalInstance, out var value);
		return Task.FromResult((success, value));
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}