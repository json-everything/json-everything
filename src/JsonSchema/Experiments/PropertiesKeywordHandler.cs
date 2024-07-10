using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class PropertiesKeywordHandler : IKeywordHandler
{
	public static PropertiesKeywordHandler Instance { get; } = new();

	public string Name => "properties";
	public string[]? Dependencies { get; }

	private PropertiesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		if (keywordValue is not JsonObject constraints)
			throw new SchemaValidationException("'properties' keyword must contain an object with schema values", context);

		var results = new List<EvaluationResults>();
		var valid = true;
		var annotation = new JsonArray();

		foreach (var constraint in constraints)
		{
			if (!instance.TryGetValue(constraint.Key, out var value, out _)) continue;

			var localContext = context;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(constraint.Key);
			localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name, constraint.Key);
			localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name, constraint.Key);
			localContext.LocalInstance = value;

			var evaluation = localContext.Evaluate(constraint.Value);

			valid &= evaluation.Valid;
			annotation.Add((JsonNode)constraint.Key);
			results.Add(evaluation);
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Annotation = annotation,
			HasAnnotation = annotation.Count != 0,
			Children = [.. results]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => (keywordValue as JsonObject)?.Select(x => x.Value) ?? [];
}