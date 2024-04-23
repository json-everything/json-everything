using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class RecursiveAnchorKeywordHandler : IKeywordHandler
{
	public static RecursiveAnchorKeywordHandler Instance { get; } = new();

	public string Name => "$recursiveAnchor";
	public string[]? Dependencies { get; }

	private RecursiveAnchorKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		return KeywordEvaluation.Skip;
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}