using System;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Thrown when an attempt to resolve a URI reference fails.
/// </summary>
public class SchemaRefResolutionException : Exception
{
	/// <summary>
	/// Gets the base URI.
	/// </summary>
	public Uri BaseUri { get; }
	/// <summary>
	/// Gets an anchor fragment, if applicable.
	/// </summary>
	public string? Anchor { get; }
	/// <summary>
	/// Gets whether the reference was dynamic, i.e. `$dynamicRef`.
	/// </summary>
	public bool IsDynamic { get; }
	/// <summary>
	/// Gets a JSON Pointer fragment, if applicable.
	/// </summary>
	public JsonPointer? Location { get; }

	// isDynamic true and anchor null means recursive
	internal SchemaRefResolutionException(Uri baseUri, string? anchor = null, bool isDynamic = false)
		: base($"Could not resolve {Format(baseUri, anchor, isDynamic)}")
	{
		BaseUri = baseUri;
		Anchor = anchor;
		IsDynamic = isDynamic;
	}

	internal SchemaRefResolutionException(Uri baseUri, JsonPointer location)
		: base($"Could not resolve schema '{baseUri}#{location}'")
	{
		BaseUri = baseUri;
		Location = location;
	}

	private static string Format(Uri baseUri, string? anchor, bool isDynamic)
	{
		return anchor is null
			? (isDynamic ? $"recursive reference in '{baseUri}'" : $"'{baseUri}'")
			: $"{(isDynamic ? "dynamic " : string.Empty)}anchor '{anchor}' in schema '{baseUri}'";
	}
}