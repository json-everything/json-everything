using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `default`.
/// </summary>
public class DefaultKeyword : IKeywordHandler
{
	public static DefaultKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "default";

	protected DefaultKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}