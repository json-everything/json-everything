using System;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `$id`.
/// </summary>
public class IdKeyword : Json.Schema.Keywords.IdKeyword
{
	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || 
		    !Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of a URI");

		return uri;
	}
}
