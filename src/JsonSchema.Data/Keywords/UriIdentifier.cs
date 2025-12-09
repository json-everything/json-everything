using System;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Data.Keywords;

/// <summary>
/// Handles data references that are URIs.
/// </summary>
public class UriIdentifier : IDataResourceIdentifier
{
	private static readonly Uri _base = new("https://json-everything.lib");

	/// <summary>
	/// The URI target.
	/// </summary>
	public Uri Target { get; }

	/// <summary>
	/// Creates a new instance of <see cref="UriIdentifier"/>.
	/// </summary>
	/// <param name="target">The target.</param>
	public UriIdentifier(Uri target)
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
		value = default;
		var fragment = _base.Resolve(Target).Fragment;
		if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			value = pointerFragment.Evaluate(root) ?? default;
		}
		else if (!string.IsNullOrWhiteSpace(fragment))
		{
			value = default;
			return false;
		}

		return value.ValueKind != JsonValueKind.Undefined;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}
