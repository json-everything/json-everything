using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class ReadOnlyKeywordHandler : IKeywordHandler
{
	public static ReadOnlyKeywordHandler Instance { get; } = new();

	public string Name => "readOnly";
	public string[]? Dependencies { get; }

	private ReadOnlyKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		return KeywordEvaluation.Annotate;
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}