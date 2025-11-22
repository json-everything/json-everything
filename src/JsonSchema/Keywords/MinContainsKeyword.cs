using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `minContains`.
/// </summary>
public class MinContainsKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "minContains";

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