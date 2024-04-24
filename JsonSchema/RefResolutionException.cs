using System;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Thrown when a reference cannot be resolved.
/// </summary>
public class RefResolutionException : JsonSchemaException
{
	/// <summary>
	/// Gets the base URI of the reference.
	/// </summary>
	public Uri BaseUri { get; }
	/// <summary>
	/// Gets an anchor, if one exists.
	/// </summary>
	public string? Anchor { get; }
	/// <summary>
	/// Gets whether the anchor (if one exists) is dynamic.
	/// </summary>
	public bool IsDynamic { get; }
	/// <summary>
	/// Gets a JSON Pointer, if one exists.
	/// </summary>
	public JsonPointer? Location { get; }

	// isDynamic true and anchor null means recursive
	public RefResolutionException(Uri baseUri, string? anchor = null, bool isDynamic = false)
		: base($"Could not resolve {Format(baseUri, anchor, isDynamic)}")
	{
		BaseUri = baseUri;
		Anchor = anchor;
		IsDynamic = isDynamic;
	}

	public RefResolutionException(Uri baseUri, JsonPointer location)
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