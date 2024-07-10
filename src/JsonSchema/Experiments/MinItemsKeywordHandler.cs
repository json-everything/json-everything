using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class MinItemsKeywordHandler : IKeywordHandler
{
	public static MinItemsKeywordHandler Instance { get; } = new();

	public string Name => "minItems";
	public string[]? Dependencies { get; }

	private MinItemsKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'minItems' keyword must contain a number", context);
		
		var minimum = value.GetNumber();
		if (!minimum.HasValue) 
			throw new SchemaValidationException("'minItems' keyword must contain a number", context);
		
		if (context.LocalInstance is not JsonArray instance) return KeywordEvaluation.Skip;

		return minimum <= instance.Count;

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}