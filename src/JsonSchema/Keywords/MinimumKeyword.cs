using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `minimum`.
/// </summary>
public class MinimumKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "minimum";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Number)
			throw new JsonSchemaException($"'{Name}' value must be a number, found {value.ValueKind}");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind is not JsonValueKind.Number) return KeywordEvaluation.Ignore;

		var comparison = JsonMath.NumberCompare(context.Instance, keyword.RawValue);

		if (comparison >= 0)
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetMinimum(context.Options.Culture)
				.ReplaceToken("received", context.Instance)
				.ReplaceToken("limit", keyword.RawValue)
		};
	}
}