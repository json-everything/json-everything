using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$dynamicRef`.
/// </summary>
/// <remarks>
/// This keyword is used to reference a schema with a dynamic anchor. The resolution of the reference
/// is dynamic, meaning it can change based on the context of the schema evaluation. The `$dynamicAnchor` that
/// will be resolved is the first occurence of the anchor being referenced found in the schema resources along
/// the evaluation path.
/// </remarks>
public partial class DynamicRefKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="DynamicRefKeyword"/>.
	/// </summary>
	public static DynamicRefKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "$dynamicRef";

#if NET7_0_OR_GREATER
	/// <summary>
	/// Gets the regular expression for validating the anchor value.
	/// </summary>
	public virtual Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^#[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	/// <summary>
	/// Gets the regular expression for validating the anchor value.
	/// </summary>
	public virtual Regex AnchorPattern { get; } = new("^#[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
#endif
	// TODO: this is the correct version - uncomment before publishing
//#if NET7_0_OR_GREATER
//	public virtual Regex AnchorPattern { get; } = GetAnchorPatternRegex();
//	[GeneratedRegex("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
//	private static partial Regex GetAnchorPatternRegex();
//#else
//	public virtual Regex AnchorPattern { get; } = new("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
//#endif

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicRefKeyword"/> class.
	/// </summary>
	protected DynamicRefKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		var anchor = value.GetString()!;
		if (!AnchorPattern.IsMatch(anchor))
			throw new JsonSchemaException($"'{Name}' value must match '{AnchorPattern}'");

		return anchor[1..];
		// TODO: this is the correct version - uncomment after updating to latest test suite
		//return anchor;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	internal virtual void TryResolve(KeywordData keyword, BuildContext context)
	{
		// nothing to do for v1 $dynamicRef
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var anchor = (string)keyword.Value!;
		var subschema = context.SchemaRegistry.GetDynamic(context.Scope, anchor);
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
