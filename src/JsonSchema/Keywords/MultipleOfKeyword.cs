using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `multipleOf`.
/// </summary>
public class MultipleOfKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "multipleOf";

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

		if (JsonMath.Divides(context.Instance, keyword.RawValue))
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetMultipleOf(context.Options.Culture)
				.ReplaceToken("received", context.Instance)
				.ReplaceToken("divisor", keyword.RawValue)
		};
	}
}
