using System;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Represents the result of evaluating a keyword, including its validity, associated annotation, and any subschema
/// evaluation results.
/// </summary>
/// <remarks>Use this struct to capture the outcome of a keyword evaluation, such as in schema validation
/// scenarios. The struct provides information about the keyword, whether it was valid, any related annotation data,
/// subschema results, and error information if applicable. The static Ignore field can be used to represent a keyword
/// evaluation that should be excluded from further processing.</remarks>
public readonly struct KeywordEvaluation
{
	public static readonly KeywordEvaluation Ignore = new()
	{
		Keyword = Guid.NewGuid().ToString("N"),
		IsValid = true
	};

	public required string Keyword { get; init; }
	public required bool IsValid { get; init; }
	public JsonElement? Annotation { get; init; }
	public EvaluationResults[]? Details { get; init; }
	public string? Error { get; init; }
	public bool ContributesToValidation { get; init; } = true;

	public KeywordEvaluation(){}
}