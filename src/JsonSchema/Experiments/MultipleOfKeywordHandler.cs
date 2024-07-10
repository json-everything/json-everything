using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class MultipleOfKeywordHandler : IKeywordHandler
{
	public static MultipleOfKeywordHandler Instance { get; } = new();

	public string Name => "multipleOf";
	public string[]? Dependencies { get; }

	private MultipleOfKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'multipleOf' keyword must contain a number", context);

		var divisor = value.GetNumber();
		if (!divisor.HasValue)
			throw new SchemaValidationException("'multipleOf' keyword must contain a number", context);

		if (context.LocalInstance is not JsonValue instance) return KeywordEvaluation.Skip;
		var number = instance.GetNumber();
		if (number is null) return KeywordEvaluation.Skip;

		return number % divisor == 0;

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}