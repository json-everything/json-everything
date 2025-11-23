using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `format`.
/// </summary>
public class FormatKeyword : Json.Schema.Keywords.FormatKeyword
{
	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.String))
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		return null;
	}

	public override void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public override KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}