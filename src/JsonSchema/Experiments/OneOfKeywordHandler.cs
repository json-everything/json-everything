using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class OneOfKeywordHandler : IKeywordHandler
{
	public static OneOfKeywordHandler Instance { get; } = new();

	public string Name => "oneOf";
	public string[]? Dependencies { get; }

	private OneOfKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (keywordValue is not JsonArray constraints)
			throw new SchemaValidationException("'oneOf' keyword must contain an array of schemas", context);

		var results = new EvaluationResults[constraints.Count];
		int validCount = 0;

		for (int i = 0; i < constraints.Count; i++)
		{
			var localContext = context;
			localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name, i);
			localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name, i);

			var evaluation = localContext.Evaluate(constraints[i]);

			if (evaluation.Valid) validCount++;

			results[i] = evaluation;
		}

		return new KeywordEvaluation
		{
			Valid = validCount == 1,
			Children = [.. results]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => keywordValue as JsonArray ?? [];
}