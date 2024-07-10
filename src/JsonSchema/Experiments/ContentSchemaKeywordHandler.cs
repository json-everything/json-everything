using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class ContentSchemaKeywordHandler : IKeywordHandler
{
	public static ContentSchemaKeywordHandler Instance { get; } = new();

	public string Name => "contentSchema";
	public string[]? Dependencies { get; }

	private ContentSchemaKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		return KeywordEvaluation.Annotate;
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}