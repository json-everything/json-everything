using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class DependenciesKeywordHandler : IKeywordHandler
{
	public static DependenciesKeywordHandler Instance { get; } = new();

	public string Name => "dependencies";
	public string[]? Dependencies { get; }

	private DependenciesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonObject constraints)
			throw new SchemaValidationException("'dependencies' keyword must contain an object", context);

		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		var children = new List<EvaluationResults>();
		var valid = true;
		foreach (var constraint in constraints)
		{
			if (!instance.ContainsKey(constraint.Key)) continue;

			if (constraint.Value is JsonArray requiredArray)
			{
				var required = requiredArray.Select(y => (y as JsonValue)?.GetString()).ToArray();
				if (required.Any(y => y is null))
					throw new SchemaValidationException("'dependencies' keyword array value must only contain strings", context);
				
				valid &= required.All(y => instance.ContainsKey(y!));
				continue;
			}

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

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}