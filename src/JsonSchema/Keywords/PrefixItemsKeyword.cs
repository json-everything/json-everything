using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `prefixItems`.
/// </summary>
public class PrefixItemsKeyword : IKeywordHandler
{
	public static PrefixItemsKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "prefixItems";

	protected PrefixItemsKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Array)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		if (value.EnumerateArray().Any(x => x.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
			throw new JsonSchemaException("Values must be valid schemas");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var subschemas = new List<JsonSchemaNode>();
		var index = 0;
		foreach (var definition in keyword.RawValue.EnumerateArray())
		{
			var defContext = context with
			{
				LocalSchema = definition
			};
			var node = JsonSchema.BuildNode(defContext);
			node.RelativePath = JsonPointer.Create(index);

			subschemas.Add(node);
			index++;
		}

		keyword.Subschemas = subschemas.ToArray();
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var subschemaEvaluations = new List<EvaluationResults>();
		var pairs = keyword.Subschemas.Zip(context.Instance.EnumerateArray(), (s, i) => (s, i));

		var i = 0;
		foreach (var (subschema, instance) in pairs)
		{
			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(i),
				Instance = instance,
				EvaluationPath = context.EvaluationPath.Combine(Name, i)
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
			i++;
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = (subschemaEvaluations.Count - 1).AsJsonElement()
		};
	}
}
