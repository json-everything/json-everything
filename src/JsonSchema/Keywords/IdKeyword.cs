using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$id`.
/// </summary>
public class IdKeyword : IKeywordHandler //, IIdKeyword
{
	private static readonly Uri _testUri = new("https://json-everything.test");

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$id";

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || 
		    !Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException("$id requires a string in the format of a URI");

		var testUri = new Uri(_testUri, uri);
		if (!string.IsNullOrEmpty(testUri.Fragment) && testUri.Fragment != "#")
			throw new JsonSchemaException("$id must not contain a fragment");

		return uri;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
