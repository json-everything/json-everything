using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `exclusiveMaximum`.
/// </summary>
public class ExclusiveMaximumKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "exclusiveMaximum";

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

		// TODO: number handling
		var instance = context.Instance.GetDouble();
		var max = keyword.RawValue.GetDouble();

		if (instance < max)
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetExclusiveMaximum(context.Options.Culture)
				.ReplaceToken("received", instance)
				.ReplaceToken("limit", max)
		};
	}
}
