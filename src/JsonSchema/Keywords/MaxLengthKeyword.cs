using System;
using System.Globalization;
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

		var number = value.GetDouble();
		var rounded = Math.Truncate(number);
		if (number != rounded)
			throw new JsonSchemaException($"'{Name}' value must be a integer, found {value.ValueKind}");

		var max = (long)rounded;
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

		var str = context.Instance.GetString();
		var length = new StringInfo(str!).LengthInTextElements;
		var max = (long)keyword.Value!;

		if (length <= max)
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
				.ReplaceToken("received", length)
				.ReplaceToken("limit", max)
		};
	}
}
