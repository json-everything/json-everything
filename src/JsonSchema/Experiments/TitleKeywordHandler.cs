using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class TitleKeywordHandler : IKeywordHandler
{
	public static TitleKeywordHandler Instance { get; } = new();

	public string Name => "title";
	public string[]? Dependencies { get; }

	private TitleKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		return KeywordEvaluation.Annotate;
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}