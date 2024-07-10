using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class DependentRequiredKeywordHandler : IKeywordHandler
{
	public static DependentRequiredKeywordHandler Instance { get; } = new();

	public string Name => "dependentRequired";
	public string[]? Dependencies { get; }

	private DependentRequiredKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonObject constraints)
			throw new SchemaValidationException("'dependentRequired' keyword must contain an object with string array values", context);

		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		var valid = true;
		foreach (var constraint in constraints)
		{
			if (!instance.ContainsKey(constraint.Key)) continue;

			if (constraint.Value is not JsonArray requiredArray)
					throw new SchemaValidationException("'dependencies' keyword array value must only contain strings", context);
			
			var required = requiredArray.Select(y => (y as JsonValue)?.GetString()).ToArray();
			if (required.Any(y => y is null))
				throw new SchemaValidationException("'dependencies' keyword array value must only contain strings", context);

			valid &= required.All(y => instance.ContainsKey(y!));
		}

		return valid;
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => (keywordValue as JsonObject)?.Where(x => x.Value is JsonObject).Select(x => x.Value) ?? [];
}