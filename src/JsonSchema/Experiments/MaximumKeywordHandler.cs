using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class MaximumKeywordHandler : IKeywordHandler
{
	public static MaximumKeywordHandler Instance { get; } = new();

	public string Name => "maximum";
	public string[]? Dependencies { get; }

	private MaximumKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'maximum' keyword must contain a number", context);

		var maximum = value.GetNumber();
		if (!maximum.HasValue)
			throw new SchemaValidationException("'maximum' keyword must contain a number", context);

		if (context.LocalInstance is not JsonValue instance) return KeywordEvaluation.Skip;
		var number = instance.GetNumber();
		if (number is null) return KeywordEvaluation.Skip;

		return maximum >= number;

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}