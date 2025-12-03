using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Schema.ArrayExt.Keywords;

/// <summary>
/// Handles `uniqueKeys`.
/// </summary>
public class UniqueKeysKeyword : IKeywordHandler
{
	private class MaybeJsonNodeComparer : IEqualityComparer<JsonElement?>
	{
		public static IEqualityComparer<JsonElement?> Comparer { get; } = new MaybeJsonNodeComparer();

		public bool Equals(JsonElement? x, JsonElement? y)
		{
			if (!x.HasValue) return !y.HasValue;
			if (!y.HasValue) return false;

			return JsonElementEqualityComparer.Instance.Equals(x.Value, y.Value);
		}

		public int GetHashCode(JsonElement? obj)
		{
			return !obj.HasValue ? 0 : JsonElementEqualityComparer.Instance.GetHashCode(obj.Value);
		}
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="UniqueKeysKeyword"/>.
	/// </summary>
	public static UniqueKeysKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "uniqueKeys";

	private UniqueKeysKeyword()
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
			throw new JsonSchemaException($"'{Name}' value must be an array, found {value.ValueKind}.");

		var pointers = new List<JsonPointer>();
		foreach (var x in value.EnumerateArray())
		{
			if (x.ValueKind is not JsonValueKind.String || !JsonPointer.TryParse(x.GetString(), out var pointer))
				throw new JsonSchemaException($"'{Name}' value must be an array of JSON Pointer strings.");

			pointers.Add(pointer);
		}

		return pointers;
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
		var pointers = (List<JsonPointer>)keyword.Value!;

		var collections = new List<List<JsonElement?>>();
		foreach (var item in context.Instance.EnumerateArray())
		{
			var values = pointers.Select(x => x.Evaluate(item)).ToList();
			collections.Add(values);
		}

		var matchedIndexPairs = new List<(int, int)>();
		for (int i = 0; i < collections.Count; i++)
		{
			for (int j = i + 1; j < collections.Count; j++)
			{
				var a = collections[i];
				var b = collections[j];

				if (a.SequenceEqual(b, MaybeJsonNodeComparer.Comparer))
					matchedIndexPairs.Add((i, j));
			}
		}

		if (matchedIndexPairs.Count != 0)
		{
			var pairs = string.Join(", ", matchedIndexPairs.Select(d => $"({d.Item1}, {d.Item2})"));
			return new KeywordEvaluation
			{
				IsValid = false,
				Keyword = Name,
				Error = ErrorMessages.GetUniqueItems(context.Options.Culture)
					.ReplaceToken("duplicates", pairs)
			};
		}

		return new KeywordEvaluation
		{
			IsValid = true,
			Keyword = Name,
		};
	}
}