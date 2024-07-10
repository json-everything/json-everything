using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class ElseKeywordHandler : IKeywordHandler
{
	public static ElseKeywordHandler Instance { get; } = new();

	public string Name => "else";
	public string[]? Dependencies { get; } = ["if"];

	private ElseKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		var ifEvaluation = evaluations.FirstOrDefault(x => x.Key == "if");
		if (ifEvaluation.Key is null || ifEvaluation.Children[0].Valid) return KeywordEvaluation.Skip;

		var localContext = context;
		localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name);
		localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name);

		var result = localContext.Evaluate(keywordValue);

		return new KeywordEvaluation
		{
			Valid = result.Valid,
			Children = [result]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [keywordValue];
}