using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `writeOnly`.
/// </summary>
public class WriteOnlyKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "writeOnly";

	public virtual object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' value must be a boolean, found {value.ValueKind}");

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