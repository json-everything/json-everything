using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$anchor`.
/// </summary>
public partial class AnchorKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$anchor";

#if NET7_0_OR_GREATER
	[GeneratedRegex("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
	protected virtual Regex AnchorPattern { get; } = GetAnchorPatternRegex();
#else
	protected virtual Regex AnchorPattern { get; } = new("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
#endif

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'$anchor' value must be a string, found {value.ValueKind}");

		var anchor = value.GetString()!;
		if (!AnchorPattern.IsMatch(anchor))
			throw new JsonSchemaException($"'$anchor' value must match '{AnchorPattern}'");

		return anchor;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
