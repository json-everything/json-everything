using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.ArrayExt.Keywords;

/// <summary>
/// Represents the `ordering` keyword.
/// </summary>
public class OrderingKeyword : IKeywordHandler
{
	private enum Ordering
	{
		Before = -1,
		Same,
		After
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="OrderingKeyword"/>.
	/// </summary>
	public static OrderingKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "ordering";

	private OrderingKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Array)
			throw new JsonSchemaException($"'{Name}' value must be an array, found {value.ValueKind}.");

		var specifiers = new List<OrderingSpecifier>();
		foreach (var x in value.EnumerateArray())
		{
			if (!x.TryGetProperty("by", out var byElement))
				throw new JsonSchemaException($"{Name} items require the 'by' property.");
			if (byElement.ValueKind != JsonValueKind.String ||
			    !JsonPointer.TryParse(byElement.GetString(), out var by))
				throw new JsonSchemaException("The 'by' property must be a valid JSON Pointer.");

			var dir = Direction.Ascending;
			x.TryGetProperty("direction", out var dirElement);
			if (dirElement.ValueKind != JsonValueKind.Undefined)
			{
				if (dirElement.ValueKind != JsonValueKind.String)
					throw new JsonSchemaException("The 'dir' property must be either 'asc' or 'desc'.");

				dir = dirElement.GetString() switch
				{
					"asc" => Direction.Ascending,
					"desc" => Direction.Descending,
					_ => throw new JsonSchemaException("The 'dir' property must be either 'asc' or 'desc'.")
				};
			}

			var ignoreCase = false;
			x.TryGetProperty("ignoreCase", out var ignoreCaseElement);
			if (ignoreCaseElement.ValueKind != JsonValueKind.Undefined)
			{
				ignoreCase = ignoreCaseElement.ValueKind switch
				{
					JsonValueKind.True => true,
					JsonValueKind.False => false,
					_ => throw new JsonSchemaException("The 'ignoreCase' property must be a boolean.")
				};
			}

			var culture = CultureInfo.InvariantCulture;
			x.TryGetProperty("culture", out var cultureElement);
			if (cultureElement.ValueKind != JsonValueKind.Undefined)
			{
				if (cultureElement.ValueKind != JsonValueKind.String)
					throw new JsonSchemaException("The 'culture' property must be either 'none' or a valid culture code.");

				if (!cultureElement.ValueEquals("none")) 
					culture = GetCulture(cultureElement.GetString()!);
			}

			specifiers.Add(new OrderingSpecifier(by, dir, culture, ignoreCase));
		}

		if (specifiers.Count == 0)
			throw new JsonSchemaException($"'{Name}' requires at least one specifier object.");

		return specifiers;
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
		var specifiers = (List<OrderingSpecifier>)keyword.Value!;

		var i = 1;
		using var enumerator = context.Instance.EnumerateArray().GetEnumerator();
		enumerator.MoveNext();
		var basisItem = enumerator.Current;
		
		while (enumerator.MoveNext())
		{
			var currentItem = enumerator.Current;

			foreach (var specifier in specifiers)
			{
				// TODO: successive specifiers only matter if the previous values are equal

				var basisValue = specifier.By.Evaluate(basisItem);
				if (basisValue is null)
				{
					return new KeywordEvaluation
					{
						Keyword = Name,
						IsValid = false,
						Error = "Item at index [[index]] does not have a value at [[pointer]]"
							.ReplaceToken("index", i - 1)
							.ReplaceToken("pointer", specifier.By, JsonSchemaArrayExtSerializerContext.Default.JsonPointer)
					};
				}

				var currentValue = specifier.By.Evaluate(currentItem);
				if (currentValue is null)
				{
					return new KeywordEvaluation
					{
						Keyword = Name,
						IsValid = false,
						Error = "Item at index [[index]] does not have a value at [[pointer]]"
							.ReplaceToken("index", i)
							.ReplaceToken("pointer", specifier.By, JsonSchemaArrayExtSerializerContext.Default.JsonPointer)
					};
				}

				var check = GetOrdering(basisValue.Value, currentValue.Value, specifier);

				if (check.Ordering == Ordering.Before) break;
				if (check.Ordering == Ordering.After)
				{
					return new KeywordEvaluation
					{
						Keyword = Name,
						IsValid = false,
						Error = "Item at index [[index]] is not in order"
							.ReplaceToken("index", i)
					};
				}
			}

			i++;
			basisItem = currentItem;
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true
		};
	}

	private static CultureInfo GetCulture(string code)
	{
		// https://stackoverflow.com/a/77408324/878701 (includes benchmark)
		try
		{
			return CultureInfo.GetCultureInfo(code);
		}
		catch (CultureNotFoundException e)
		{
			throw new JsonSchemaException($"Unrecognized culture '{code}'", e);
		}
	}

	private static (Ordering? Ordering, string? ErrorMessage) GetOrdering(JsonElement basis, JsonElement current, OrderingSpecifier specifier)
	{
		if (basis.ValueKind is JsonValueKind.String && current.ValueKind == JsonValueKind.String)
		{
			var compareOptions = specifier.IgnoreCase ? CompareOptions.OrdinalIgnoreCase : CompareOptions.Ordinal;
			var comparison = string.Compare(basis.GetString(), current.GetString(), specifier.Culture, compareOptions);
			return (AdjustForDirection(ComparisonToOrdering(comparison), specifier.Direction), null);
		}

		if (basis.ValueKind is JsonValueKind.Number && current.ValueKind == JsonValueKind.Number)
			return (AdjustForDirection(ComparisonToOrdering(JsonMath.NumberCompare(basis, current)), specifier.Direction), null);

		return (null, "Comparisons may only occur between strings or numbers and both values must be of the same type");
	}

	private static Ordering ComparisonToOrdering(int comparison) =>
		comparison switch
		{
			< 0 => Ordering.Before,
			0 => Ordering.Same,
			> 0 => Ordering.After
		};

	private static Ordering AdjustForDirection(Ordering ordering, Direction direction) =>
		direction == Direction.Descending
			? (Ordering)((int)ordering * -1)
			: ordering;
}
