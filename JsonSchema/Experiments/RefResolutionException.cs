using System;
using Json.Pointer;

namespace Json.Schema.Experiments;

public class RefResolutionException : Exception
{
	public Uri BaseUri { get; }
	public string? Anchor { get; }
	public bool IsDynamic { get; }
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