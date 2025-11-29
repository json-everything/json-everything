using System;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `minItems`.
/// </summary>
public class MinItemsKeyword : IKeywordHandler
{
	public static MinItemsKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "minItems";

	protected MinItemsKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Number)
			throw new JsonSchemaException($"'{Name}' value must be a number, found {value.ValueKind}");

		var number = value.GetDouble();
		var rounded = Math.Truncate(number);
		if (number != rounded)
			throw new JsonSchemaException($"'{Name}' value must be a integer, found {value.ValueKind}");

		var min = (long)rounded;
		if (min < 0)
			throw new JsonSchemaException($"'{Name}' value must be non-negative, found {min}");

		return min;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind is not JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var instance = context.Instance.EnumerateArray().Count();
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
			Error = ErrorMessages.GetMinItems(context.Options.Culture)
				.ReplaceToken("received", instance)
				.ReplaceToken("limit", min)
		};
	}
}