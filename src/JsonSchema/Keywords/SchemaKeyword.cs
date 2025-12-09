using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$schema`.
/// </summary>
/// <remarks>
/// This keyword specifies the dialect of JSON Schema that the schema is written in.  It will override the dialect specified
/// in the build options.
/// </remarks>
public class SchemaKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="SchemaKeyword"/>.
	/// </summary>
	public static SchemaKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "$schema";

	/// <summary>
	/// Initializes a new instance of the <see cref="SchemaKeyword"/> class.
	/// </summary>
	protected SchemaKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String ||
		    !Uri.TryCreate(value.GetString(), UriKind.Absolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of an absolute URI.");

		return uri;
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
		return KeywordEvaluation.Ignore;
	}
}
