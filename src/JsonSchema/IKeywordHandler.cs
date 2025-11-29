using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Defines the contract for handling custom JSON Schema keywords, including validation, subschema construction, and
/// evaluation logic.
/// </summary>
/// <remarks>Implementations of this interface enable extensibility for JSON Schema processing by supporting
/// custom keyword behaviors. Each handler is responsible for validating keyword values, building any required
/// subschemas, and evaluating the keyword during schema validation. This interface is intended for advanced scenarios
/// where custom schema keywords are needed.</remarks>
public interface IKeywordHandler
{
	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	object? ValidateKeywordValue(JsonElement value);

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	void BuildSubschemas(KeywordData keyword, BuildContext context);

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context);
}