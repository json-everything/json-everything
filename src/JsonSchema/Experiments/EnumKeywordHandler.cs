using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class EnumKeywordHandler : IKeywordHandler
{
	public static EnumKeywordHandler Instance { get; } = new();

	public string Name => "enum";
	public string[]? Dependencies { get; }

	private EnumKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonArray { Count: > 0 } value)
			throw new SchemaValidationException("'enum' keyword must contain an array", context);

		return value.Contains(context.LocalInstance, JsonNodeEqualityComparer.Instance);

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}