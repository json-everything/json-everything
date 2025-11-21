using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$dynamicAnchor`.
/// </summary>
public partial class DynamicAnchorKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$dynamicAnchor";

#if NET7_0_OR_GREATER
	[GeneratedRegex("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
	internal static Regex AnchorPattern { get; } = GetAnchorPatternRegex();
#else
	internal static Regex AnchorPattern { get; } = new("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
#endif

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'$dynamicAnchor' value must be a string, found {value.ValueKind}");

		var anchor = value.GetString()!;
		if (!AnchorPattern.IsMatch(anchor))
			throw new JsonSchemaException($"'$dynamicAnchor' value must match '{AnchorPattern}'");

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
