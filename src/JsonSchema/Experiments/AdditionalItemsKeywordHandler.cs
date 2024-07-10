using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class AdditionalItemsKeywordHandler : IKeywordHandler
{
	public static AdditionalItemsKeywordHandler Instance { get; } = new();

	public string Name => "additionalItems";
	public string[]? Dependencies { get; } = ["items"];

	private AdditionalItemsKeywordHandler(){}

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonArray instance) return KeywordEvaluation.Skip;

		int skip;
		if (evaluations.TryGetAnnotation("items", out JsonValue? itemsAnnotation))
			skip = (int?)itemsAnnotation.GetInteger() + 1 ?? 0;
		else
			return KeywordEvaluation.Skip;

		var contextTemplate = context;
		contextTemplate.EvaluationPath = context.EvaluationPath.Combine(Name);
		contextTemplate.SchemaLocation = context.SchemaLocation.Combine(Name);

		var results = new EvaluationResults[instance.Count - skip];
		bool valid = true;
		int annotation = -1;

		for (int i = 0; i < instance.Count - skip; i++)
		{
			var x = instance[skip + i];
			var localContext = contextTemplate;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(skip + i);
			localContext.LocalInstance = x;

			var evaluation = localContext.Evaluate(keywordValue);

			valid &= evaluation.Valid;
			annotation = skip + i;

			results[i] = evaluation;
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Annotation = annotation,
			HasAnnotation = annotation != -1,
			Children = results
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [keywordValue];
}