using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `format`.
/// </summary>
/// <remarks>
/// This keyword is used to validate the format of a value.  Typically, this value is a string, but may be of any JSON type.
/// </remarks>
public class FormatKeyword : Json.Schema.Keywords.FormatKeyword
{
	private readonly bool _requireFormatValidation;

	/// <summary>
	/// Gets an instance that just annotates.
	/// </summary>
	public static FormatKeyword Annotate { get; set; } = new(false);
	/// <summary>
	/// Gets an instance that validates.
	/// </summary>
	public static FormatKeyword Validate { get; set; } = new(true);

	/// <summary>
	/// Initializes a new instance of the <see cref="FormatKeyword"/> class.
	/// </summary>
	/// <param name="requireFormatValidation">Whether to require format validation.</param>
	protected FormatKeyword(bool requireFormatValidation)
	{
		_requireFormatValidation = requireFormatValidation;
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.String))
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public override void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public override KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Options.RequireFormatValidation || _requireFormatValidation)
			return base.Evaluate(keyword, context);

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}