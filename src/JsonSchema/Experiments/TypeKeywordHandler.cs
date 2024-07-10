using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class TypeKeywordHandler : IKeywordHandler
{
	public static TypeKeywordHandler Instance { get; } = new();

	public string Name => "type";
	public string[]? Dependencies { get; }

	private TypeKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		var instanceType = context.LocalInstance.GetSchemaValueType();
		return keywordValue switch
		{
			// TODO: keyword validation
			JsonValue value => value.IsEquivalentTo(instanceType) ||
			                   (instanceType.IsEquivalentTo("integer") && value.IsEquivalentTo("number")),
			JsonArray multipleTypes => multipleTypes.Contains(instanceType, JsonNodeEqualityComparer.Instance) ||
			                           (instanceType.IsEquivalentTo("integer") && multipleTypes.Contains("number")),
			_ => throw new SchemaValidationException("'type' keyword must contain a valid JSON Schema type or an array of valid JSON Schema types", context)
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}