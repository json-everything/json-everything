using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `minLength`.
/// </summary>
public class MinLengthKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "minLength";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Number)
			throw new JsonSchemaException($"'{Name}' value must be a number, found {value.ValueKind}");

		var min = value.GetInt64();
		if (min < 0)
			throw new JsonSchemaException($"'{Name}' value must be non-negative, found {min}");

		return min;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind is not JsonValueKind.String) return KeywordEvaluation.Ignore;

		var instance = context.Instance.GetString()!.Length;
		var min = (long)keyword.Value!;

		if (min <= instance)
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetMinLength(context.Options.Culture)
				.ReplaceToken("received", instance)
				.ReplaceToken("limit", min)
		};
	}
}