using System;
using System.Text.Json;

namespace Json.Schema.OpenApi.Keywords;

/// <summary>
/// Handles `example`.
/// </summary>
public class ExternalDocsKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="ExternalDocsKeyword"/>.
	/// </summary>
	public static ExternalDocsKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "externalDocs";

	private ExternalDocsKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		if (!value.TryGetProperty("url", out var urlElement))
			throw new JsonSchemaException("'externalDocs' requires a 'url' property");

		if (urlElement.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException("'url' must be a string");

		if (value.TryGetProperty("description", out var descriptionElement))
		{
			if (descriptionElement.ValueKind != JsonValueKind.String)
				throw new JsonSchemaException("'description' must be a string");
		}

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
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}