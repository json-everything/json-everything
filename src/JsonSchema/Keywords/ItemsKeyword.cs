using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `items`.
/// </summary>
public class ItemsKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "items";

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

		if (context.LocalSchema.TryGetProperty("prefixItems", out var items))
			keyword.Value = items.EnumerateArray().Count();
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var prefixItemsCount = (int?)keyword.Value ?? 0;

		var subschemaEvaluations = new List<EvaluationResults>();
		var subschema = keyword.Subschemas[0];

		var evaluationPath = context.EvaluationPath.Combine(Name);
		var i = 0;
		foreach (var instance in context.Instance.EnumerateArray().Skip(prefixItemsCount))
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
