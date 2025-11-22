using System;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `maxProperties`.
/// </summary>
public class MaxPropertiesKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "maxProperties";

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
		if (context.Instance.ValueKind is not JsonValueKind.Object) return KeywordEvaluation.Ignore;

#if NET8_0
		var instance = context.Instance.EnumerateObject().Count();
#else
		var instance = context.Instance.GetPropertyCount();
#endif
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
			Error = ErrorMessages.GetMaxProperties(context.Options.Culture)
				.ReplaceToken("received", instance)
				.ReplaceToken("limit", max)
		};
	}
}
