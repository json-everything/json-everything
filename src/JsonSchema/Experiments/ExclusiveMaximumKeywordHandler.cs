using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class ExclusiveMaximumKeywordHandler : IKeywordHandler
{
	public static ExclusiveMaximumKeywordHandler Instance { get; } = new();

	public string Name => "exclusiveMaximum";
	public string[]? Dependencies { get; }

	private ExclusiveMaximumKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'exclusiveMaximum' keyword must contain a number", context);

		var maximum = value.GetNumber();
		if (!maximum.HasValue)
			throw new SchemaValidationException("'exclusiveMaximum' keyword must contain a number", context);

		if (context.LocalInstance is not JsonValue instance) return KeywordEvaluation.Skip;
		var number = instance.GetNumber();
		if (number is null) return KeywordEvaluation.Skip;

		return maximum > number;

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}