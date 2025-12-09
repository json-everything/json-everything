using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `enum`.
/// </summary>
/// <remarks>
/// This keyword specifies that the instance must be equal to one of the values in the keyword's array.
/// </remarks>
public class EnumKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="EnumKeyword"/>.
	/// </summary>
	public static EnumKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "enum";

	/// <summary>
	/// Initializes a new instance of the <see cref="EnumKeyword"/> class.
	/// </summary>
	protected EnumKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Array)
			throw new JsonSchemaException($"'{Name}' value must be an array, found {value.ValueKind}");

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
		if (!keyword.RawValue.EnumerateArray().Any(x => x.IsEquivalentTo(context.Instance)))
		{
			return new KeywordEvaluation
			{
				IsValid = false,
				Keyword = Name,
				Error = ErrorMessages.GetEnum(context.Options.Culture)
					.ReplaceToken("received", context.Instance, JsonSchemaSerializerContext.Default.JsonElement)
					.ReplaceToken("values", keyword.RawValue!, JsonSchemaSerializerContext.Default.JsonElement)
			};
		}

		return new KeywordEvaluation
		{
			IsValid = true,
			Keyword = Name
		};
	}
}
