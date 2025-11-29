using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$schema`.
/// </summary>
public class SchemaKeyword : IKeywordHandler
{
	public static SchemaKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$schema";

	protected SchemaKeyword()
	{
	}

	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String ||
		    !Uri.TryCreate(value.GetString(), UriKind.Absolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of an absolute URI.");

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
