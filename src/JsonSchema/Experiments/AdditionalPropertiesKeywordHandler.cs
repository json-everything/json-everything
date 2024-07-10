using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class AdditionalPropertiesKeywordHandler : IKeywordHandler
{
	public static AdditionalPropertiesKeywordHandler Instance { get; } = new();

	public string Name => "additionalProperties";
	public string[]? Dependencies { get; } = ["properties", "patternProperties"];

	private AdditionalPropertiesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		if (!evaluations.TryGetAnnotation("properties", out JsonArray? propertiesAnnotation))
			propertiesAnnotation = [];
		if (!evaluations.TryGetAnnotation("patternProperties", out JsonArray? patternPropertiesAnnotation))
			patternPropertiesAnnotation = [];

		var evaluatedProperties = new HashSet<string>(propertiesAnnotation
			.Concat(patternPropertiesAnnotation)
			.Select(x => ((JsonValue)x!).GetString()!));

		var properties = instance.Where(x => !evaluatedProperties.Contains(x.Key)).ToArray();

		var contextTemplate = context;
		contextTemplate.EvaluationPath = context.EvaluationPath.Combine(Name);
		contextTemplate.SchemaLocation = context.SchemaLocation.Combine(Name);

		var results = new EvaluationResults[properties.Length];
		bool valid = true;

		int i = 0;
		foreach (var x in properties)
		{
			var localContext = contextTemplate;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(x.Key);
			localContext.LocalInstance = x.Value;

			var evaluation = localContext.Evaluate(keywordValue);

			valid &= evaluation.Valid;
			results[i] = evaluation;
			i++;
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Annotation = properties.Select(x => (JsonNode)x.Key).ToJsonArray(),
			HasAnnotation = properties.Length != 0,
			Children = results
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [keywordValue];
}