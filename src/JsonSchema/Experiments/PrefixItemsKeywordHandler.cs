using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class PrefixItemsKeywordHandler : IKeywordHandler
{
	public static PrefixItemsKeywordHandler Instance { get; } = new();

	public string Name => "prefixItems";
	public string[]? Dependencies { get; }

	private PrefixItemsKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonArray instance) return KeywordEvaluation.Skip;

		if (keywordValue is not JsonArray constraints)
			throw new SchemaValidationException("'prefixItems' keyword must contain an object with schema values", context);

		var count = Math.Min(instance.Count, constraints.Count);

		var results = new EvaluationResults[count];
		bool valid = true;
		int annotation = -1;

		for (int i = 0; i < count; i++)
		{
			var item = instance[i];
			var constraint = constraints[i];

			var localContext = context;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(i);
			localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name, i);
			localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name, i);
			localContext.LocalInstance = item;

			var evaluation = localContext.Evaluate(constraint);

			valid &= evaluation.Valid;
			annotation = i;

			results[i] = evaluation;
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Annotation = annotation,
			HasAnnotation = annotation != -1,
			Children = results
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => keywordValue as JsonArray ?? [];
}