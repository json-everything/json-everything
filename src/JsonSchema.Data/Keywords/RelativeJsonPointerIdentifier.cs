using System;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Data.Keywords;

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
	/// Attempts to resolve a value from the specified JSON element.
	/// </summary>
	/// <param name="root">The root <see cref="JsonElement"/> to search for the desired value.</param>
	/// <param name="keyword">The keyword data.</param>
	/// <param name="value">When this method returns, contains the resolved <see cref="JsonElement"/> if the operation succeeds; otherwise,
	/// contains the default value.</param>
	/// <returns>true if the value was successfully resolved; otherwise, false.</returns>
	public bool TryResolve(JsonElement root, KeywordData keyword, out JsonElement value)
	{
		throw new NotImplementedException("Relative JSON Pointer support is unavailable");
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}