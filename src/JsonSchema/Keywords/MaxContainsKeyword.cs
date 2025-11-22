using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `maxContains`.
/// </summary>
public class MaxContainsKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "maxContains";

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Number) || !value.TryGetInt32(out _))
			throw new JsonSchemaException($"'{Name}' value must be an integer, found {value.ValueKind}");
		
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
