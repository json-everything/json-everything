using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class MinLengthKeywordHandler : IKeywordHandler
{
	public static MinLengthKeywordHandler Instance { get; } = new();

	public string Name => "minLength";
	public string[]? Dependencies { get; }

	private MinLengthKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'minLength' keyword must contain a number", context);
		
		var minimum = value.GetNumber();
		if (!minimum.HasValue)
			throw new SchemaValidationException("'minLength' keyword must contain a number", context);
		
		if (context.LocalInstance is not JsonValue instance) return KeywordEvaluation.Skip;
		var str = instance.GetString();
		if (str is null) return KeywordEvaluation.Skip;

		var length = new StringInfo(str).LengthInTextElements;
		return minimum <= length;

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}