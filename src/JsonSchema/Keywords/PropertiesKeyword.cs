using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `properties`.
/// </summary>
public class PropertiesKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "properties";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		if (value.EnumerateObject().Any(x => x.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
			throw new JsonSchemaException("Values must be valid schemas");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var subschemas = new List<JsonSchemaNode>();
		foreach (var definition in keyword.RawValue.EnumerateObject())
		{
			var defContext = context with
			{
				LocalSchema = definition.Value
			};
			var node = JsonSchema.BuildNode(defContext);
			node.RelativePath = JsonPointer.Create(definition.Name);
			subschemas.Add(node);
		}

		keyword.Subschemas = subschemas.ToArray();
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Object) return KeywordEvaluation.Ignore;

		var subschemaEvaluations = new List<EvaluationResults>();
		var propertyNames = new HashSet<string>();

		foreach (var subschema in keyword.Subschemas)
		{
			var instance = subschema.RelativePath.Evaluate(context.Instance);
			if (!instance.HasValue) continue;

			var propertyName = subschema.RelativePath[0].ToString();
			propertyNames.Add(propertyName);

			var propContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(subschema.RelativePath),
				Instance = instance.Value,
				EvaluationPath = context.EvaluationPath.Combine(Name, propertyName)
			};

			subschemaEvaluations.Add(subschema.Evaluate(propContext));
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = JsonSerializer.SerializeToElement(propertyNames, JsonSchemaSerializerContext.Default.HashSetString)
		};
	}
}
