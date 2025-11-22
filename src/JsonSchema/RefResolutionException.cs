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
	/// Gets the type of anchor, if one exists.
	/// </summary>
	public AnchorType? AnchorType { get; }
	/// <summary>
	/// Gets a JSON Pointer, if one exists.
	/// </summary>
	public JsonPointer? Location { get; }

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="baseUri">The base URI of the reference.</param>
	/// <param name="anchor">(optional) - The anchor. Default is null.</param>
	/// <param name="anchorType">(optional) - The type of anchor, if present.</param>
	public RefResolutionException(Uri baseUri, string? anchor = null, AnchorType? anchorType = null)
		: base($"Could not resolve {Format(baseUri, anchor, anchorType)}")
	{
		BaseUri = baseUri;
		Anchor = anchor;
		AnchorType = anchor is null
			? null
			: anchorType ?? Schema.AnchorType.Static;
	}

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="baseUri">The base URI of the reference.</param>
	/// <param name="location">The JSON Pointer location.</param>
	public RefResolutionException(Uri baseUri, JsonPointer location)
		: base($"Could not resolve schema '{baseUri}#{location}'")
	{
		BaseUri = baseUri;
		Location = location;
	}

	private static string Format(Uri baseUri, string? anchor, AnchorType? anchorType)
	{
		if (anchor is null) return $"'{baseUri}'";

		switch (anchorType)
		{
			case Schema.AnchorType.Static:
				return $"anchor '{anchor}' in schema '{baseUri}'";
			case Schema.AnchorType.Dynamic:
				return $"dynamic anchor '{anchor}' in schema '{baseUri}'";
			case Schema.AnchorType.Recursive:
				return $"recursive anchor in schema '{baseUri}'";
			default:
				throw new ArgumentOutOfRangeException(nameof(anchorType), "Anchor type must be specified if anchor is present.");
		}
	}
}

public enum AnchorType
{
	Static,
	Dynamic,
	Recursive
}