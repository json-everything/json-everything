using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `unevaluatedProperties`.
/// </summary>
[DependsOnAnnotationsFrom<PropertiesKeyword>]
[DependsOnAnnotationsFrom<PatternPropertiesKeyword>]
[DependsOnAnnotationsFrom<UnevaluatedPropertiesKeyword>]
public class UnevaluatedPropertiesKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "unevaluatedProperties";

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
		if (context.Instance.ValueKind != JsonValueKind.Object) return KeywordEvaluation.Ignore;

		var evaluatedProperties = GetEvaluatedProperties(context);

		var subschemaEvaluations = new List<EvaluationResults>();
		var propertyNames = new HashSet<string>();
		var subschema = keyword.Subschemas[0];

		var evaluationPath = context.EvaluationPath.Combine(Name);
		foreach (var instance in context.Instance.EnumerateObject())
		{
			if (evaluatedProperties.Contains(instance.Name)) continue;

			propertyNames.Add(instance.Name);

			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(instance.Name),
				Instance = instance.Value,
				EvaluationPath = evaluationPath
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = JsonSerializer.SerializeToElement(propertyNames, JsonSchemaSerializerContext.Default.HashSetString)
		};
	}

	private static HashSet<string> GetEvaluatedProperties(EvaluationContext context)
	{
		var properties = new HashSet<string>();

		var propertiesKeys = context.EvaluatedKeywords?
			.Where(x => x.Keyword == "properties")
			.SelectMany(x => x.Annotation?.EnumerateArray().Select(p => p.GetString()!) ?? []) ?? [];
		var patternPropertiesKeys = context.EvaluatedKeywords?
			.Where(x => x.Keyword == "patternProperties")
			.SelectMany(x => x.Annotation?.EnumerateArray().Select(p => p.GetString()!) ?? []) ?? [];
		var unevaluatedPropertiesKeys = context.EvaluatedKeywords?
			.Where(x => x.Keyword == "unevaluatedProperties")
			.SelectMany(x => x.Annotation?.EnumerateArray().Select(p => p.GetString()!) ?? []) ?? [];

		foreach (var property in propertiesKeys.Union(patternPropertiesKeys).Union(unevaluatedPropertiesKeys))
		{
			properties.Add(property);
		}

		var details = context.EvaluatedKeywords?
			.SelectMany(x => x.Details ?? [])
			.Where(x => x.IsValid && x.InstanceLocation == context.InstanceLocation) ?? [];

		foreach (var detail in details)
		{
			GetNestedEvaluatedProperties(detail, properties);
		}

		return properties;
	}

	private static void GetNestedEvaluatedProperties(EvaluationResults results, HashSet<string> properties)
	{
		if (results.Annotations is not null)
		{
			if (results.Annotations.TryGetValue("properties", out var propertiesAnnotation))
			{
				var propertiesKeys = propertiesAnnotation.EnumerateArray().Select(x => x.GetString()!);
				foreach (var key in propertiesKeys)
				{
					properties.Add(key);
				}
			}
			if (results.Annotations.TryGetValue("patternProperties", out var patternPropertiesAnnotation))
			{
				var patternPropertiesKeys = patternPropertiesAnnotation.EnumerateArray().Select(x => x.GetString()!);
				foreach (var key in patternPropertiesKeys)
				{
					properties.Add(key);
				}
			}
			if (results.Annotations.TryGetValue("unevaluatedProperties", out var unevaluatedPropertiesAnnotation))
			{
				var unevaluatedPropertiesKeys = unevaluatedPropertiesAnnotation.EnumerateArray().Select(x => x.GetString()!);
				foreach (var key in unevaluatedPropertiesKeys)
				{
					properties.Add(key);
				}
			}
		}

		var details = results.Details?
			.Where(x => x.IsValid && x.InstanceLocation == results.InstanceLocation) ?? [];

		foreach (var detail in details)
		{
			GetNestedEvaluatedProperties(detail, properties);
		}
	}
}