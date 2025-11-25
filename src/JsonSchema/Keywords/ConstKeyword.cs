using System;
using System.Text.Json;
using Json.More;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `const`.
/// </summary>
public class ConstKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "const";

	public object? ValidateKeywordValue(JsonElement value)
	{
		return null;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (!keyword.RawValue.IsEquivalentTo(context.Instance))
		{
			return new KeywordEvaluation
			{
				IsValid = false,
				Keyword = Name,
				Error = ErrorMessages.GetConst(context.Options.Culture)
					.ReplaceToken("value", keyword.RawValue.ToJsonString())
			};
		}

		return new KeywordEvaluation
		{
			IsValid = true,
			Keyword = Name
		};
	}
}
