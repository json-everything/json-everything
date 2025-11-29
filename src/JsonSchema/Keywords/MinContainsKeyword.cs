using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `minContains`.
/// </summary>
public class MinContainsKeyword : IKeywordHandler
{
	public static MinContainsKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "minContains";

	protected MinContainsKeyword()
	{
	}

	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Number)
			throw new JsonSchemaException($"'{Name}' value must be a number, found {value.ValueKind}");

		var number = value.GetDouble();
		var rounded = Math.Truncate(number);
		if (number != rounded)
			throw new JsonSchemaException($"'{Name}' value must be a integer, found {value.ValueKind}");

		var min = (long)rounded;
		if (min < 0)
			throw new JsonSchemaException($"'{Name}' value must be non-negative, found {min}");

		return null;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}