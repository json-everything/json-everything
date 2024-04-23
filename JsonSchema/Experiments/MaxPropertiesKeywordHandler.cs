using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class MaxPropertiesKeywordHandler : IKeywordHandler
{
	public static MaxPropertiesKeywordHandler Instance { get; } = new();

	public string Name => "maxProperties";
	public string[]? Dependencies { get; }

	private MaxPropertiesKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		if (keywordValue is JsonValue value)
		{
			var maximum = value.GetNumber();
			if (maximum.HasValue)
			{
				if (context.LocalInstance is not JsonObject instance) return KeywordEvaluation.Skip;

				return maximum >= instance.Count;
			}
		}

		throw new ArgumentException("'maxProperties' keyword must contain a number");
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}