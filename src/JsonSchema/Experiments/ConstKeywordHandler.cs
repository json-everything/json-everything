using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class ConstKeywordHandler : IKeywordHandler
{
	public static ConstKeywordHandler Instance { get; } = new();

	public string Name => "const";
	public string[]? Dependencies { get; }

	private ConstKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		return context.LocalInstance.IsEquivalentTo(keywordValue);
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}