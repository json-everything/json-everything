using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class UnevaluatedPropertiesKeywordHandler : IKeywordHandler
{
	public static UnevaluatedPropertiesKeywordHandler Instance { get; } = new();

	public string Name => "unevaluatedProperties";
	public string[]? Dependencies { get; } = ["properties", "patternProperties", "additionalProperties", "unevaluatedProperties"];

	private UnevaluatedPropertiesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		var propertiesAnnotations = evaluations.GetAllAnnotations<JsonArray>("properties");
		var patternPropertiesAnnotations = evaluations.GetAllAnnotations<JsonArray>("patternProperties");
		var additionalPropertiesAnnotations = evaluations.GetAllAnnotations<JsonArray>("additionalProperties");
		var unevaluatedPropertiesAnnotations = evaluations.GetAllAnnotations<JsonArray>("unevaluatedProperties");

		var evaluatedProperties = propertiesAnnotations
			.Concat(patternPropertiesAnnotations)
			.Concat(additionalPropertiesAnnotations)
			.Concat(unevaluatedPropertiesAnnotations)
			.SelectMany(x => x)
			.OfType<JsonValue>()
			.Select(x => x.GetString())
			.ToArray();

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