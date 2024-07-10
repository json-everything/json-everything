using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.Schema.Experiments;

public class PatternPropertiesKeywordHandler : IKeywordHandler
{
	public static PatternPropertiesKeywordHandler Instance { get; } = new();

	public string Name => "patternProperties";
	public string[]? Dependencies { get; }

	private PatternPropertiesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (keywordValue is not JsonObject constraints)
			throw new SchemaValidationException("'patternProperties' keyword must contain an object with schema values", context);

		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		var results = new List<EvaluationResults>();
		var annotation = new HashSet<string>();
		var valid = true;

		foreach (var constraint in constraints)
		{
			foreach (var kvp in instance)
			{
				if (!Regex.IsMatch(kvp.Key, constraint.Key, RegexOptions.ECMAScript)) continue;

				var localContext = context;
				localContext.InstanceLocation = localContext.InstanceLocation.Combine(kvp.Key);
				localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name, kvp.Key);
				localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name, kvp.Key);
				localContext.LocalInstance = kvp.Value;

				var evaluation = localContext.Evaluate(constraint.Value);

				results.Add(evaluation);
				valid &= evaluation.Valid;
				annotation.Add(kvp.Key);
			}
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Annotation = annotation.Select(x => (JsonNode) x).ToJsonArray(),
			HasAnnotation = results.Count != 0,
			Children = [.. results]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => (keywordValue as JsonObject)?.Select(x => x.Value) ?? [];
}