using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `pattern`.
/// </summary>
public class PatternKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "pattern";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a number, found {value.ValueKind}");

		var regex = new Regex(value.GetString()!, RegexOptions.ECMAScript | RegexOptions.Compiled);

		return regex;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind is not JsonValueKind.String) return KeywordEvaluation.Ignore;

		var regex = (Regex)keyword.Value!;
		var str = context.Instance.GetString()!;

		if (regex.IsMatch(str))
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetPattern(context.Options.Culture)
				.ReplaceToken("pattern", regex.ToString())
		};
	}
}
