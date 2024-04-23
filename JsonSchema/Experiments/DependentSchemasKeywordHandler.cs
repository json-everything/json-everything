using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class DependentSchemasKeywordHandler : IKeywordHandler
{
	public static DependentSchemasKeywordHandler Instance { get; } = new();

	public string Name => "dependentSchemas";
	public string[]? Dependencies { get; }

	private DependentSchemasKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonObject constraints)
			throw new SchemaValidationException("'dependentSchemas' keyword must contain an object", context);

		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		var children = new List<EvaluationResults>();
		var valid = true;
		foreach (var constraint in constraints)
		{
			if (!instance.ContainsKey(constraint.Key)) continue;

			var localContext = context;
			localContext.EvaluationPath = context.EvaluationPath.Combine(Name, constraint.Key);
			localContext.SchemaLocation = context.SchemaLocation.Combine(Name, constraint.Key);

			var result = localContext.Evaluate(constraint.Value);
			valid &= result.Valid;
			children.Add(result);
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Children = [.. children]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => (keywordValue as JsonObject)?.Select(x => x.Value) ?? [];
}