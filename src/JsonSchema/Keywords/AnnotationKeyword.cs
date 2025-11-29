using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles unrecognized keywords.
/// </summary>
/// <remarks>
/// This keyword handler is used for any keyword that is not recognized by the vocabulary.
/// It will always pass validation and will add an annotation with the keyword's value.
/// </remarks>
public class AnnotationKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="AnnotationKeyword"/>.
	/// </summary>
	public static AnnotationKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	/// <remarks>
	/// For this keyword, this value is not used.  The keyword name is determined during schema building.
	/// </remarks>
	public string Name { get; } = Guid.NewGuid().ToString("N");

	/// <summary>
	/// Initializes a new instance of the <see cref="AnnotationKeyword"/> class.
	/// </summary>
	protected AnnotationKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public object? ValidateKeywordValue(JsonElement value)
	{
		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return new KeywordEvaluation
		{
			Keyword = (string) keyword.Value!,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}