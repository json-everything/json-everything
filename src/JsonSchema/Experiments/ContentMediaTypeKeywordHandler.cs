using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class ContentMediaTypeKeywordHandler : IKeywordHandler
{
	public static ContentMediaTypeKeywordHandler Instance { get; } = new();

	public string Name => "contentMediaType";
	public string[]? Dependencies { get; }

	private ContentMediaTypeKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		return KeywordEvaluation.Annotate;
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}