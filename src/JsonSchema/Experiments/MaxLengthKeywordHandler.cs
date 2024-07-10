using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class MaxLengthKeywordHandler : IKeywordHandler
{
	public static MaxLengthKeywordHandler Instance { get; } = new();

	public string Name => "maxLength";
	public string[]? Dependencies { get; }

	private MaxLengthKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'maxLength' keyword must contain a number", context);
		
		var maximum = value.GetNumber();
		if (!maximum.HasValue)
			throw new SchemaValidationException("'maxLength' keyword must contain a number", context);
		
		if (context.LocalInstance is not JsonValue instance) return KeywordEvaluation.Skip;
		var str = instance.GetString();
		if (str is null) return KeywordEvaluation.Skip;

		var length = new StringInfo(str).LengthInTextElements;
		return maximum >= length;

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}