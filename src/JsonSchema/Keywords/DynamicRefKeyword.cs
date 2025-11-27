using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$dynamicRef`.
/// </summary>
public partial class DynamicRefKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$dynamicRef";

#if NET7_0_OR_GREATER
	public virtual Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^#[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	public virtual Regex AnchorPattern { get; } = new("^#[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
#endif
	// TODO: this is the correct version - uncomment after updating to latest test suite
//#if NET7_0_OR_GREATER
//	public virtual Regex AnchorPattern { get; } = GetAnchorPatternRegex();
//	[GeneratedRegex("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
//	private static partial Regex GetAnchorPatternRegex();
//#else
//	public virtual Regex AnchorPattern { get; } = new("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
//#endif

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		// TODO: may need options passed in to allow #
		var anchor = value.GetString()!;
		if (!AnchorPattern.IsMatch(anchor))
			throw new JsonSchemaException($"'{Name}' value must match '{AnchorPattern}'");

		return anchor[1..];
		// TODO: this is the correct version - uncomment after updating to latest test suite
		//return anchor;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	internal virtual void TryResolve(KeywordData keyword, BuildContext context)
	{
		// nothing to do for v1 $dynamicRef
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var anchor = (string)keyword.Value!;
		var subschema = context.BuildOptions.SchemaRegistry.GetDynamic(context.Scope, anchor);
		if (subschema is null)
			throw new RefResolutionException(context.Scope.LocalScope, anchor, AnchorType.Dynamic);

		var refContext = context with
		{
			EvaluationPath = context.EvaluationPath.Combine(Name)
		};

		var subschemaEvaluation = subschema.Evaluate(refContext);
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluation.IsValid,
			Details = [subschemaEvaluation]
		};
	}
}
