using System;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Represents the result of evaluating a keyword, including its validity, associated annotation, and any subschema
/// evaluation results.
/// </summary>
/// <remarks>Use this struct to capture the outcome of a keyword evaluation, such as in schema validation
/// scenarios. The struct provides information about the keyword, whether it was valid, any related annotation data,
/// subschema results, and error information if applicable. The static Ignore field can be used to represent a keyword
/// evaluation that should be excluded from further processing.</remarks>
public readonly struct KeywordEvaluation
{
	/// <summary>
	/// Represents a special evaluation that indicates a keyword should be ignored during processing.
	/// </summary>
	/// <remarks>This instance is typically used to mark keywords that are intentionally excluded from validation or
	/// further analysis. The generated keyword value is unique for each application run.</remarks>
	public static readonly KeywordEvaluation Ignore = new()
	{
		Keyword = Guid.NewGuid().ToString("N"),
		IsValid = true
	};

	/// <summary>
	/// Gets the keyword that produced the evaluation.
	/// </summary>
	public required string Keyword { get; init; }

	/// <summary>
	/// Gets a value indicating whether a validation succeeded.
	/// </summary>
	/// <remarks>Some keywords do not produce a validation result.
	/// In these cases, this property will default to true.</remarks>
	public required bool IsValid { get; init; }
	
	/// <summary>
	/// Gets the a JSON annotation produced by the keyword.
	/// </summary>
	/// <remarks>The annotation can contain arbitrary metadata or additional information in JSON format. If no
	/// annotation is present, the value is <see langword="null"/>.</remarks>
	public JsonElement? Annotation { get; init; }
	
	/// <summary>
	/// Gets the collection of evaluation results produced by subschemas.
	/// </summary>
	public EvaluationResults[]? Details { get; init; }
	
	/// <summary>
	/// If validation failed, this may get an error message.
	/// </summary>
	/// <remarks>Not all keywords produce an error message when they fail validation.</remarks>
	public string? Error { get; init; }
	
	/// <summary>
	/// Gets a value indicating whether this member participates in validation.
	/// </summary>
	public bool ContributesToValidation { get; init; } = true;

	/// <summary>
	/// Initializes a new instance of the KeywordEvaluation type.
	/// </summary>
	public KeywordEvaluation(){}
}