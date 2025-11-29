using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `additionalItems`.
/// </summary>
public class AdditionalItemsKeyword : IKeywordHandler
{
	public static AdditionalItemsKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "additionalItems";

	private AdditionalItemsKeyword()
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
		if (context.LocalSchema.TryGetProperty("items", out var items) &&
		    items.ValueKind != JsonValueKind.Array) return;

		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Value = items.ValueKind == JsonValueKind.Undefined
			? null
			: items.EnumerateArray().Count(); // how is there a .GetPropertyCount(), but not a .GetItemCount()?
		keyword.Subschemas = [node];
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var itemsCount = (int?)keyword.Value;
		if (itemsCount is null) return KeywordEvaluation.Ignore;

		var subschemaEvaluations = new List<EvaluationResults>();
		var subschema = keyword.Subschemas[0];

		var evaluationPath = context.EvaluationPath.Combine(Name);
		var i = itemsCount.Value;
		foreach (var instance in context.Instance.EnumerateArray().Skip(itemsCount.Value))
		{
			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(i),
				Instance = instance,
				EvaluationPath = evaluationPath.Combine(Name)
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
}
