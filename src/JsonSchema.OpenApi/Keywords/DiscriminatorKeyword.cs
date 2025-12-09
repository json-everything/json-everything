using System.Text.Json;

namespace Json.Schema.OpenApi.Keywords;

/// <summary>
/// Handles `example`.
/// </summary>
public class DiscriminatorKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="DiscriminatorKeyword"/>.
	/// </summary>
	public static DiscriminatorKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "discriminator";

	private DiscriminatorKeyword()
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

		if (!value.TryGetProperty("propertyName", out var propertyNameElement))
			throw new JsonSchemaException("'discriminator' requires a 'propertyName' property");

		if (propertyNameElement.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException("'propertyName' must be a string");

		if (value.TryGetProperty("mapping", out var mappingElement))
		{
			if (mappingElement.ValueKind != JsonValueKind.Object)
				throw new JsonSchemaException("'mapping' must be an object");

			foreach (var prop in mappingElement.EnumerateObject())
			{
				if (prop.Value.ValueKind != JsonValueKind.String)
					throw new JsonSchemaException("'mapping' values must be strings");
			}
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