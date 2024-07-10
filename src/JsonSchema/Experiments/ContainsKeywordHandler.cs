using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class ContainsKeywordHandler : IKeywordHandler
{
	public static ContainsKeywordHandler Instance { get; } = new();

	public string Name => "contains";
	public string[]? Dependencies { get; } = ["minContains", "maxContains"];

	private ContainsKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonArray instance) return KeywordEvaluation.Skip;

		var minContains = 1;
		if (evaluations.TryGetAnnotation("minContains", out JsonValue? minContainsAnnotation)) 
			minContains = (int?)minContainsAnnotation.GetInteger() ?? 1;
		var maxContains = int.MaxValue;
		if (evaluations.TryGetAnnotation("maxContains", out JsonValue? maxContainsAnnotation))
			maxContains = (int?)maxContainsAnnotation.GetInteger() ?? int.MaxValue;

		var contextTemplate = context;
		contextTemplate.EvaluationPath = context.EvaluationPath.Combine(Name);
		contextTemplate.SchemaLocation = context.SchemaLocation.Combine(Name);

		var results = new EvaluationResults[instance.Count];
		var validCount = 0;
		var annotation = new JsonArray();

		for (int i = 0; i < instance.Count; i++)
		{
			var x = instance[i];
			var localContext = contextTemplate;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(i);
			localContext.LocalInstance = x;

			var evaluation = localContext.Evaluate(keywordValue);

			if (evaluation.Valid)
			{
				validCount++;
				annotation.Add((JsonNode)i);
			}

			results[i] = evaluation;
		}
		
		return new KeywordEvaluation
		{
			Valid = minContains <= validCount && validCount <= maxContains,
			Annotation = annotation,
			HasAnnotation = annotation.Count != 0,
			Children = results
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [keywordValue];
}