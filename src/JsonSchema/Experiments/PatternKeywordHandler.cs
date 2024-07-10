using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.Schema.Experiments;

public class PatternKeywordHandler : IKeywordHandler
{
	public static PatternKeywordHandler Instance { get; } = new();

	public string Name => "pattern";
	public string[]? Dependencies { get; }

	private PatternKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is not JsonValue value)
			throw new SchemaValidationException("'maxLength' keyword must contain a string", context);
		
		var pattern = value.GetString();
		if (pattern is null)
			throw new SchemaValidationException("'maxLength' keyword must contain a string", context);
		
		if (context.LocalInstance is not JsonValue instance) return KeywordEvaluation.Skip;
		var str = instance.GetString();
		if (str is null) return KeywordEvaluation.Skip;

		return Regex.IsMatch(str, pattern);

	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}