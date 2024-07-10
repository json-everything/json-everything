using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public class PropertyNamesKeywordHandler : IKeywordHandler
{
	public static PropertyNamesKeywordHandler Instance { get; } = new();

	public string Name => "propertyNames";
	public string[]? Dependencies { get; }

	private PropertyNamesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		var contextTemplate = context;
		contextTemplate.EvaluationPath = context.EvaluationPath.Combine(Name);
		contextTemplate.SchemaLocation = context.SchemaLocation.Combine(Name);

		var results = new EvaluationResults[instance.Count];
		var valid = true;
		var i = 0;

		foreach (var kvp in instance)
		{
			var localContext = contextTemplate;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(kvp.Key);
			localContext.LocalInstance = kvp.Key;

			var evaluation = localContext.Evaluate(keywordValue);

			valid &= evaluation.Valid;
			results[i] = evaluation;
			i++;
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Children = results
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [keywordValue];
}