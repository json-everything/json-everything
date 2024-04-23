using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class AnyOfKeywordHandler : IKeywordHandler
{
	public static AnyOfKeywordHandler Instance { get; } = new();

	public string Name => "anyOf";
	public string[]? Dependencies { get; }

	private AnyOfKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (keywordValue is not JsonArray constraints)
			throw new SchemaValidationException("'anyOf' keyword must contain an array of schemas", context);

		var results = new EvaluationResults[constraints.Count];
		bool valid = constraints.Count == 0;

		for (int i = 0; i < constraints.Count; i++)
		{
			var localContext = context;
			localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name, i);
			localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name, i);

			var evaluation = localContext.Evaluate(constraints[i]);

			valid |= evaluation.Valid;

			results[i] = evaluation;
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Children = [.. results]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => keywordValue as JsonArray ?? [];
}