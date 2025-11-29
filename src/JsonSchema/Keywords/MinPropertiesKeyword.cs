using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `minProperties`.
/// </summary>
public class MinPropertiesKeyword : IKeywordHandler
{
	public static MinPropertiesKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "minProperties";

	protected MinPropertiesKeyword()
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
		if (context.Instance.ValueKind is not JsonValueKind.Object) return KeywordEvaluation.Ignore;

		var instance = context.Instance.GetPropertyCount();
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
			Error = ErrorMessages.GetMinProperties(context.Options.Culture)
				.ReplaceToken("received", instance)
				.ReplaceToken("limit", min)
		};
	}
}