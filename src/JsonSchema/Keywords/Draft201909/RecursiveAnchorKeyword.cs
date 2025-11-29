using System;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `$recursiveAnchor`.
/// </summary>
public class RecursiveAnchorKeyword : IKeywordHandler
{
	public static RecursiveAnchorKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$recursiveAnchor";

	protected RecursiveAnchorKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		return value.ValueKind switch
		{
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			_ => throw new JsonSchemaException($"'{Name}' value must be a boolean, found {value.ValueKind}")
		};
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}