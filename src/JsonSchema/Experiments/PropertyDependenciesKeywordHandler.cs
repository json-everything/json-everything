using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class PropertyDependenciesKeywordHandler : IKeywordHandler
{
	public static PropertyDependenciesKeywordHandler Instance { get; } = new();

	public string Name => "propertyDependencies";
	public string[]? Dependencies { get; }

	private PropertyDependenciesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonObject constraints)
			throw new SchemaValidationException("'propertyDependencies' keyword must contain an object", context);

		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		var results = new List<EvaluationResults>();
		var valid = true;

		foreach (var constraint in constraints)
		{
			if (constraint.Value is not JsonObject options)
				throw new SchemaValidationException("'propertyDependencies' keyword must contain object values", context);

			if (!instance.TryGetValue(constraint.Key, out var value, out _)) continue;

			var str = (value as JsonValue)?.GetString();
			if (str is null) continue;

			if (!options.TryGetValue(str, out var option, out _)) continue;

			var localContext = context;
			localContext.EvaluationPath = context.EvaluationPath.Combine(Name, constraint.Key, str);
			localContext.SchemaLocation = context.SchemaLocation.Combine(Name, constraint.Key, str);

			var evaluation = localContext.Evaluate(option);

			valid &= evaluation.Valid;
			results.Add(evaluation);
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Children = [.. results]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue)
	{
		if (keywordValue is not JsonObject a) yield break;

		foreach (var kvp in a)
		{
			if (kvp.Value is not JsonObject b) continue;
			foreach (var inner in b)
			{
				yield return inner.Value;
			}
		}
	}
}