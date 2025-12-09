using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `multipleOf`.
/// </summary>
/// <remarks>
/// This keyword specifies that a numeric instance must be a multiple of the value of the keyword.
/// </remarks>
public class MultipleOfKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="MultipleOfKeyword"/>.
	/// </summary>
	public static MultipleOfKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "multipleOf";

	/// <summary>
	/// Initializes a new instance of the <see cref="MultipleOfKeyword"/> class.
	/// </summary>
	protected MultipleOfKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Number)
			throw new JsonSchemaException($"'{Name}' value must be a number, found {value.ValueKind}");

		return null;
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
		if (context.Instance.ValueKind is not JsonValueKind.Number) return KeywordEvaluation.Ignore;

		if (JsonMath.Divides(context.Instance, keyword.RawValue))
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetMultipleOf(context.Options.Culture)
				.ReplaceToken("received", context.Instance)
				.ReplaceToken("divisor", keyword.RawValue)
		};
	}
}
