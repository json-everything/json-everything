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
	/// <remarks>
	/// If this property is null while <see cref="IsDynamic"/> is true, then the failure was with a `$recursiveRef`.
	/// </remarks>
	public string? Anchor { get; }
	/// <summary>
	/// Gets whether the anchor (if one exists) is dynamic.
	/// </summary>
	/// <remarks>
	/// If this property is true while <see cref="Anchor"/> is null, then the failure was with a `$recursiveRef`.
	/// </remarks>
	public bool IsDynamic { get; }
	/// <summary>
	/// Gets a JSON PointerOld, if one exists.
	/// </summary>
	public JsonPointer_Old? Location { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="baseUri">The base URI of the reference.</param>
	/// <param name="anchor">(optional) - The anchor. Default is null.</param>
	/// <param name="isDynamic">(optional) - Whether the reference was dynamic.</param>
	public RefResolutionException(Uri baseUri, string? anchor = null, bool isDynamic = false)
		: base($"Could not resolve {Format(baseUri, anchor, isDynamic)}")
	{
		BaseUri = baseUri;
		Anchor = anchor;
		IsDynamic = isDynamic;
	}

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="baseUri">The base URI of the reference.</param>
	/// <param name="location">The JSON PointerOld location.</param>
	public RefResolutionException(Uri baseUri, JsonPointer_Old location)
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