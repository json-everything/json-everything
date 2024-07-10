using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class MinPropertiesKeywordHandler : IKeywordHandler
{
	public static MinPropertiesKeywordHandler Instance { get; } = new();

	public string Name => "minProperties";
	public string[]? Dependencies { get; }

	private MinPropertiesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'minProperties' keyword must contain a number", context);
		
		var minimum = value.GetNumber();
		if (!minimum.HasValue)
			throw new SchemaValidationException("'minProperties' keyword must contain a number", context);
		
		if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

		return minimum <= instance.Count;

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}