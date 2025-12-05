using System;

namespace Json.Schema.Data;

internal static class GeneralExtensions
{
	public static Uri Resolve(this Uri baseUri, Uri reference)
	{
		return baseUri.IsFile && reference.OriginalString.StartsWith("#")
			// File URIs have a quirk with fragment-only references
			? new Uri(baseUri.AbsoluteUri + reference)
			// Standard URI resolution works for everything else
			: new Uri(baseUri, reference);
	}
}