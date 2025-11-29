using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `contentSchema`.
/// </summary>
public class ContentSchemaKeyword : IKeywordHandler
{
	public static ContentSchemaKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "contentSchema";

	protected ContentSchemaKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' value must be a valid schema, found {value.ValueKind}");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}