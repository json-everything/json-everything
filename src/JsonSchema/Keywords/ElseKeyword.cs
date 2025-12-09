using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `else`.
/// </summary>
/// <remarks>
/// This keyword is used in conjunction with `if` and `then`. If the `if` schema fails, then this schema is applied.
/// It is ignored if there is no `if` keyword or if the `if` keyword's subschema passes validation.
/// </remarks>
[DependsOnAnnotationsFrom<IfKeyword>]
public class ElseKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="ElseKeyword"/>.
	/// </summary>
	public static ElseKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "else";

	/// <summary>
	/// Initializes a new instance of the <see cref="ElseKeyword"/> class.
	/// </summary>
	protected ElseKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' values must be valid schemas.");

		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var ifEvaluation = context.EvaluatedKeywords?
			.SingleOrDefault(x => x.Keyword == "if");
		if (ifEvaluation?.Keyword is null || ifEvaluation.Value.IsValid) return KeywordEvaluation.Ignore;

		var subschema = keyword.Subschemas[0];
		var oneOfContext = context with
		{
			EvaluationPath = context.EvaluationPath.Combine(Name)
		};
		var result = subschema.Evaluate(oneOfContext);

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = result.IsValid,
			Details = [result]
		};
	}
}