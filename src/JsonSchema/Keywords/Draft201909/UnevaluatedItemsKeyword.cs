using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `unevaluatedItems`.
/// </summary>
/// <remarks>
/// This keyword validates items in an array that are not evaluated by `items`, `additionalItems`, or other `unevaluatedItems`
/// keywords that are found either in the local subschema directly or in subschemas of the local schema which produce a true
/// validation result.
/// </remarks>
public class UnevaluatedItemsKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="UnevaluatedItemsKeyword"/>.
	/// </summary>
	public static UnevaluatedItemsKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "unevaluatedItems";

	/// <summary>
	/// Initializes a new instance of the <see cref="UnevaluatedItemsKeyword"/> class.
	/// </summary>
	protected UnevaluatedItemsKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' value must be a valid schema, found {value.ValueKind}");

		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];
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

		var (all, lastIndex) = GetLastEvaluatedIndex(context);
		if (all)
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true,
				Annotation = JsonElementExtensions.True
			};

		var subschemaEvaluations = new List<EvaluationResults>();
		var subschema = keyword.Subschemas[0];

		var i = 0;
		foreach (var instance in context.Instance.EnumerateArray().Skip(lastIndex + 1))
		{
			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(i),
				Instance = instance,
				EvaluationPath = context.EvaluationPath.Combine(Name)
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
			i++;
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = JsonElementExtensions.True
		};
	}

	private static (bool All, int Some) GetLastEvaluatedIndex(EvaluationContext context)
	{
		if (context.EvaluatedKeywords?.Any(x => x.Keyword == "additionalItems") ?? false)
			return (true, 0);

		var index = -1;

		var itemsAnnotation = context.EvaluatedKeywords?.SingleOrDefault(x => x.Keyword == "items");
		if (itemsAnnotation?.Annotation is not null)
		{
			if (itemsAnnotation.Value.Annotation.Value.ValueKind is JsonValueKind.True)
				return (true, 0);

			index = itemsAnnotation.Value.Annotation.Value.GetInt32();
		}

		var details = context.EvaluatedKeywords?
			.SelectMany(x => x.Details ?? [])
			.Where(x => x.IsValid && x.InstanceLocation == context.InstanceLocation) ?? [];

		var all = false;
		foreach (var detail in details)
		{
			all |= GetNestedEvaluatedProperties(detail, ref index);
			if (all) break;
		}

		return (all, index);
	}

	private static bool GetNestedEvaluatedProperties(EvaluationResults results, ref int index)
	{
		if (results.Annotations is not null)
		{
			if (results.Annotations.ContainsKey("additionalItems") ||
			    results.Annotations.ContainsKey("unevaluatedItems"))
				return true;

			if (results.Annotations.TryGetValue("items", out var itemsAnnotation))
			{
				if (itemsAnnotation.ValueKind == JsonValueKind.True) return true;

				index = Math.Max(index, itemsAnnotation.GetInt32());
			}
		}

		var details = results.Details?
			.Where(x => x.IsValid && x.InstanceLocation == results.InstanceLocation) ?? [];

		var all = false;
		foreach (var detail in details)
		{
			all |= GetNestedEvaluatedProperties(detail, ref index);
			if (all) break;
		}

		return all;
	}
}