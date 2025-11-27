using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles unrecognized keywords.
/// </summary>
public class AnnotationKeyword : IKeywordHandler
{
	public static AnnotationKeyword Instance { get; } = new();

	/// <summary>
	/// The name or key of the keyword.
	/// </summary>
	public string Name { get; } = Guid.NewGuid().ToString("N");

	public object? ValidateKeywordValue(JsonElement value)
	{
		return null;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return new KeywordEvaluation
		{
			Keyword = (string) keyword.Value!,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}