using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$id`.
/// </summary>
public class IdKeyword : IKeywordHandler //, IIdKeyword
{
	private static readonly Uri _testUri = new("https://json-everything.test");

	public static IdKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$id";

	protected IdKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || 
		    !Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of a URI");

		var testUri = new Uri(_testUri, uri);
		if (!string.IsNullOrEmpty(testUri.Fragment) && testUri.Fragment != "#")
			throw new JsonSchemaException($"'{Name}' must not contain a non-empty fragment");

		return uri;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
