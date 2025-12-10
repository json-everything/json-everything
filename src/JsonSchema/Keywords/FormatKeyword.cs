using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `format`.
/// </summary>
/// <remarks>
/// This keyword validates that a string instance conforms to a predefined format.  Custom formats may be used
/// however they must be registered with <see cref="Formats"/>.  Unknown formats will cause a schema to fail the build step.
/// </remarks>
public class FormatKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="FormatKeyword"/>.
	/// </summary>
	public static FormatKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "format";

	/// <summary>
	/// Initializes a new instance of the <see cref="FormatKeyword"/> class.
	/// </summary>
	protected FormatKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
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
		var formatName = keyword.RawValue.GetString()!;
		var format = context.Options.FormatRegistry.Get(formatName);

		var valid = format.Validate(context.Instance, out var error);
		if (!valid)
			error ??= ErrorMessages.GetFormat(context.Options.Culture).ReplaceToken("format", formatName);

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = valid,
			Annotation = keyword.RawValue,
			Error = error
		};
	}
}