using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$dynamicAnchor`.
/// </summary>
/// <remarks>
/// This keyword is used to create a named dynamic anchor at the current schema location.
/// This anchor can be used with `$dynamicRef` to reference this schema.  The `$dynamicAnchor` that will be resolved
/// is the first occurence of the anchor being referenced found in the schema resources along the evaluation path.
/// </remarks>
public partial class DynamicAnchorKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="DynamicAnchorKeyword"/>.
	/// </summary>
	public static DynamicAnchorKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "$dynamicAnchor";

#if NET7_0_OR_GREATER
	/// <summary>
	/// Gets the regular expression for validating the anchor value.
	/// </summary>
	public virtual Regex AnchorPattern { get; } = GetAnchorPatternRegex();
	[GeneratedRegex("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled)]
	private static partial Regex GetAnchorPatternRegex();
#else
	/// <summary>
	/// Gets the regular expression for validating the anchor value.
	/// </summary>
	public virtual Regex AnchorPattern { get; } = new("^[A-Za-z_][-A-Za-z0-9._]*$", RegexOptions.Compiled);
#endif

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicAnchorKeyword"/> class.
	/// </summary>
	protected DynamicAnchorKeyword()
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
			throw new JsonSchemaException($"'{Name}' value '{anchor}' must match '{AnchorPattern}'");

		return anchor;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{

	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
