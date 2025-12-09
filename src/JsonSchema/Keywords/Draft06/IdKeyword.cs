using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `id`.
/// </summary>
/// <remarks>
/// This keyword is used to set the base URI for a schema or to identify a location-independent identifier
/// for a subschema.
/// </remarks>
public partial class IdKeyword : Json.Schema.Keywords.IdKeyword
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="IdKeyword"/>.
	/// </summary>
	public new static IdKeyword Instance { get; } = new();

#if NET7_0_OR_GREATER
	/// <summary>
	/// The pattern for valid anchor identifiers.
	/// </summary>
	public Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	/// <summary>
	/// The pattern for valid anchor identifiers.
	/// </summary>
	public Regex AnchorPattern { get; } = new("^[A-Za-z][-A-Za-z0-9.:_]*$", RegexOptions.Compiled);
#endif

	/// <summary>
	/// Initializes a new instance of the <see cref="IdKeyword"/> class.
	/// </summary>
	protected IdKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || 
		    !Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of a URI");

		return uri;
	}
}
