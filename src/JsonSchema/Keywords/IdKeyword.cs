using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$id`.
/// </summary>
/// <remarks>
/// This keyword sets the base URI for the current schema, which is used to resolve relative references.
/// </remarks>
public class IdKeyword : IKeywordHandler //, IIdKeyword
{
	private static readonly Uri _testUri = new("https://json-everything.test");

	/// <summary>
	/// Gets the singleton instance of the <see cref="IdKeyword"/>.
	/// </summary>
	public static IdKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "$id";

	/// <summary>
	/// Initializes a new instance of the <see cref="IdKeyword"/> class.
	/// </summary>
	protected IdKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || 
		    !Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of a URI");

		var testUri = new Uri(_testUri, uri);
		if (!string.IsNullOrEmpty(testUri.Fragment) && testUri.Fragment != "#")
			throw new JsonSchemaException($"'{Name}' must not contain a non-empty fragment");

		return uri;
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
