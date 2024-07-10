using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class UnevaluatedItemsKeywordHandler : IKeywordHandler
{
	public static UnevaluatedItemsKeywordHandler Instance { get; } = new();

	public string Name => "unevaluatedItems";
	public string[]? Dependencies { get; } = ["additionalItems", "contains", "prefixItems", "items", "unevaluatedItems"];

	private UnevaluatedItemsKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonArray instance) return KeywordEvaluation.Skip;

		var indexAnnotations = evaluations.GetAllAnnotations<JsonValue>("prefixItems")
			.Concat(evaluations.GetAllAnnotations<JsonValue>("items"))
			.Concat(evaluations.GetAllAnnotations<JsonValue>("additionalItems"))
			.Concat(evaluations.GetAllAnnotations<JsonValue>("unevaluatedItems"))
			.ToArray();

		var containsIndices = evaluations.GetAllAnnotations<JsonArray>("contains")
			.SelectMany(x => x.Select(y => (y as JsonValue)?.GetInteger()))
			.Where(x => x is not null)
			.ToArray();

		var skip = indexAnnotations.Any() ? indexAnnotations.Max(x => (int?)x.GetInteger() ?? 0) + 1 : 0;

		var contextTemplate = context;
		contextTemplate.EvaluationPath = context.EvaluationPath.Combine(Name);
		contextTemplate.SchemaLocation = context.SchemaLocation.Combine(Name);

		var results = new EvaluationResults[instance.Count - skip];
		bool valid = true;
		int annotation = -1;

		for (int i = 0; i < instance.Count - skip; i++)
		{
			if (containsIndices.Contains(i + skip)) continue;

			var x = instance[skip + i];
			var localContext = contextTemplate;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(skip + i);
			localContext.LocalInstance = x;

			var evaluation = localContext.Evaluate(keywordValue);

			valid &= evaluation.Valid;
			annotation = skip + i;

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

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [keywordValue];
}