using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$anchor`.
/// </summary>
public partial class AnchorKeyword : IKeywordHandler
{
	public static AnchorKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$anchor";

#if NET7_0_OR_GREATER
	public virtual Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	public virtual Regex AnchorPattern { get; } = new("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
#endif

	protected AnchorKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		var anchor = value.GetString()!;
		if (!AnchorPattern.IsMatch(anchor))
			throw new JsonSchemaException($"'{Name}' value must match '{AnchorPattern}'");

		return anchor;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
