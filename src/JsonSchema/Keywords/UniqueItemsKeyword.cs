using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `uniqueItems`.
/// </summary>
/// <remarks>
/// This keyword validates that all items in an array are unique.
/// </remarks>
public class UniqueItemsKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="UniqueItemsKeyword"/>.
	/// </summary>
	public static UniqueItemsKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "uniqueItems";

	/// <summary>
	/// Initializes a new instance of the <see cref="UniqueItemsKeyword"/> class.
	/// </summary>
	protected UniqueItemsKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		return value.ValueKind switch
		{
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			_ => throw new JsonSchemaException($"'{Name}' value must be a boolean, found {value.ValueKind}")
		};
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
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;
		if (keyword.RawValue.ValueKind == JsonValueKind.False) return KeywordEvaluation.Ignore;

		var valid = true;
		var items = context.Instance.EnumerateArray().ToArray();
		List<string>? matches = null;
		for (int i = 0; i < items.Length; i++)
		{
			for (int j = i + 1; j < items.Length; j++)
			{
				if (items[i].IsEquivalentTo(items[j]))
				{
					valid = false;
					matches ??= [];
					matches.Add($"({i},{j})");
				}
			}
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = valid,
			Error = valid
				? null
				: ErrorMessages.GetUniqueItems(context.Options.Culture)
					.ReplaceToken("duplicates", matches!.ToArray())
		};
	}
}
