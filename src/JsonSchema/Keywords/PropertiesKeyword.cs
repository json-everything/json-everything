using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `properties`.
/// </summary>
//[SchemaKeyword(Name)]
//[SchemaSpecVersion(SpecVersion.Draft6)]
//[SchemaSpecVersion(SpecVersion.Draft7)]
//[SchemaSpecVersion(SpecVersion.Draft201909)]
//[SchemaSpecVersion(SpecVersion.Draft202012)]
//[SchemaSpecVersion(SpecVersion.DraftNext)]
//[Vocabulary(Vocabularies.Applicator201909Id)]
//[Vocabulary(Vocabularies.Applicator202012Id)]
//[Vocabulary(Vocabularies.ApplicatorNextId)]
public class PropertiesKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "properties";

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'properties' value must be an object, found {value.ValueKind}");

		if (value.EnumerateObject().Any(x => x.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
			throw new JsonSchemaException("Values must be valid schemas");

		return null;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
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

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var subschemaEvaluations = new List<EvaluationResults>();

		foreach (var subschema in keyword.Subschemas)
		{
			var instance = subschema.RelativePath.Evaluate(context.Instance);
			if (!instance.HasValue) continue;

			var propContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(subschema.RelativePath),
				Instance = instance.Value,
				EvaluationPath = context.EvaluationPath.Combine(Name, subschema.RelativePath[0].ToString())
			};

			subschemaEvaluations.Add(subschema.Evaluate(propContext));
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray()
		};
	}
}
