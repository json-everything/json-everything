using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data.Keywords;

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
	/// Attempts to resolve a value from the specified JSON element.
	/// </summary>
	/// <param name="root">The root <see cref="JsonElement"/> to search for the desired value.</param>
	/// <param name="value">When this method returns, contains the resolved <see cref="JsonElement"/> if the operation succeeds; otherwise,
	/// contains the default value.</param>
	/// <returns>true if the value was successfully resolved; otherwise, false.</returns>
	public bool TryResolve(JsonElement root, out JsonElement value)
	{
		var result = Target.Evaluate(root);
		if (result.HasValue)
		{
			value = result.Value;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}