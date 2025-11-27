using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `$id`.
/// </summary>
public partial class IdKeyword : Json.Schema.Keywords.IdKeyword
{
#if NET7_0_OR_GREATER
	public Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	public Regex AnchorPattern { get; } = new("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled);
#endif

	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || 
		    !Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of a URI");

		return uri;
	}
}
