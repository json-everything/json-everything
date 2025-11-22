using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords.Draft06;

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
		switch (value.ValueKind)
		{
			case JsonValueKind.True:
			case JsonValueKind.False:
			case JsonValueKind.Object:
				return null;
			case JsonValueKind.Array:
				if (value.EnumerateArray().Any(x => x.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
					throw new JsonSchemaException("Values must be valid schemas");
				return null;
			default:
				throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");
		}
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		if (keyword.RawValue.ValueKind == JsonValueKind.Object)
		{
			var defContext = context with
			{
				LocalSchema = keyword.RawValue
			};

			var node = JsonSchema.BuildNode(defContext);
			keyword.Subschemas = [node];
		}
		else
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
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var subschemaEvaluations = new List<EvaluationResults>();
		if (keyword.RawValue.ValueKind == JsonValueKind.Object)
		{
			var subschema = keyword.Subschemas[0];

			var evaluationPath = context.EvaluationPath.Combine(Name);
			var i = 0;
			foreach (var instance in context.Instance.EnumerateArray())
			{
				var itemContext = context with
				{
					InstanceLocation = context.InstanceLocation.Combine(i),
					Instance = instance,
					EvaluationPath = evaluationPath
				};

				subschemaEvaluations.Add(subschema.Evaluate(itemContext));
				i++;
			}
		}
		else
		{
			var pairs = keyword.Subschemas.Zip(context.Instance.EnumerateArray(), (s, i) => (s, i));

			var i = 0;
			foreach (var (subschema,instance) in pairs)
			{
				var evaluationPath = context.EvaluationPath.Combine(i);
				var itemContext = context with
				{
					InstanceLocation = context.InstanceLocation.Combine(i),
					Instance = instance,
					EvaluationPath = evaluationPath.Combine(Name, i)
				};

				subschemaEvaluations.Add(subschema.Evaluate(itemContext));
				i++;
			}
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray()
		};
	}
}
