using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public interface IKeywordHandler
{
	public string Name { get; }
	public string[]? Dependencies { get; }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations);
	public IEnumerable<JsonNode?> GetSubschemas(JsonNode? keywordValue);
}