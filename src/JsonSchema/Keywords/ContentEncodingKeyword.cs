using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `contentMediaEncoding`.
/// </summary>
public class ContentEncodingKeyword : IKeywordHandler
{
	public static ContentEncodingKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "contentEncoding";

	protected ContentEncodingKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.String))
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

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