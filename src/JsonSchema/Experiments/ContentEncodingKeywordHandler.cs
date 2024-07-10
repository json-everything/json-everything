using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class ContentEncodingKeywordHandler : IKeywordHandler
{
	public static ContentEncodingKeywordHandler Instance { get; } = new();

	public string Name => "contentEncoding";
	public string[]? Dependencies { get; }

	private ContentEncodingKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		return KeywordEvaluation.Annotate;
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}