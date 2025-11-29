using System;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `$vocabulary`.
/// </summary>
/// <remarks>
/// This keyword is used in the meta-schema to declare which vocabularies a schema uses.
/// </remarks>
public class VocabularyKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="VocabularyKeyword"/>.
	/// </summary>
	public static VocabularyKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "$vocabulary";

	/// <summary>
	/// Initializes a new instance of the <see cref="VocabularyKeyword"/> class.
	/// </summary>
	protected VocabularyKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' requires a object.");

		foreach (var entry in value.EnumerateObject())
		{
			var uri = entry.Name;
			var required = entry.Value;

			if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
				throw new JsonSchemaException($"'{Name}' keys must be absolute URIs.");
			if (required.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
				throw new JsonSchemaException($"'{Name}' values must be booleans.");
		}

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
		return KeywordEvaluation.Ignore;
	}
}
