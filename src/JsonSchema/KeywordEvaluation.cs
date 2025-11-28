using System;
using System.Text.Json;

namespace Json.Schema;

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