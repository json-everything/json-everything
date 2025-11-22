using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `maxLength`.
/// </summary>
public class MaxLengthKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "maxLength";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Number)
			throw new JsonSchemaException($"'{Name}' value must be a number, found {value.ValueKind}");

		var max = value.GetInt64();
		if (max < 0)
			throw new JsonSchemaException($"'{Name}' value must be non-negative, found {max}");

		return max;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind is not JsonValueKind.String) return KeywordEvaluation.Ignore;

		var instance = context.Instance.GetString()!.Length;
		var max = (long)keyword.Value!;

		if (instance <= max)
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetMaxLength(context.Options.Culture)
				.ReplaceToken("received", instance)
				.ReplaceToken("limit", max)
		};
	}
}
