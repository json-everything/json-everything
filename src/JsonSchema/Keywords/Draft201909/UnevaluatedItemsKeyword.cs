using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `unevaluatedItems`.
/// </summary>
public class UnevaluatedItemsKeyword : IKeywordHandler
{
	public static UnevaluatedItemsKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "unevaluatedItems";

	protected UnevaluatedItemsKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' value must be a valid schema, found {value.ValueKind}");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];
	}

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