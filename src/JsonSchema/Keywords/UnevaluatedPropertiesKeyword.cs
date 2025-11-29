using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `unevaluatedProperties`.
/// </summary>
[DependsOnAnnotationsFrom<PropertiesKeyword>]
[DependsOnAnnotationsFrom<PatternPropertiesKeyword>]
[DependsOnAnnotationsFrom<UnevaluatedPropertiesKeyword>]
public class UnevaluatedPropertiesKeyword : IKeywordHandler
{
	public static UnevaluatedPropertiesKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "unevaluatedProperties";

	protected UnevaluatedPropertiesKeyword()
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
		if (context.Instance.ValueKind != JsonValueKind.Object) return KeywordEvaluation.Ignore;

		var (all, evaluatedProperties) = GetEvaluatedProperties(context);
		if (all)
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true,
				Annotation = JsonElementExtensions.True
			};

		var subschemaEvaluations = new List<EvaluationResults>();
		var subschema = keyword.Subschemas[0];

		var evaluationPath = context.EvaluationPath.Combine(Name);
		foreach (var instance in context.Instance.EnumerateObject())
		{
			if (evaluatedProperties!.Contains(instance.Name)) continue;

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
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = JsonElementExtensions.True
		};
	}

	private static (bool All, HashSet<string>? Some) GetEvaluatedProperties(EvaluationContext context)
	{
		if (context.EvaluatedKeywords?.Any(x => x.Keyword == "additionalProperties") ?? false)
			return (true, null);

		var properties = new HashSet<string>();
		var propertiesKeys = context.EvaluatedKeywords?
			.SingleOrDefault(x => x.Keyword == "properties")
			.Annotation?.EnumerateArray().Select(p => p.GetString()!) ?? [];
		var patternPropertiesKeys = context.EvaluatedKeywords?
			.SingleOrDefault(x => x.Keyword == "patternProperties")
			.Annotation?.EnumerateArray().Select(p => p.GetString()!) ?? [];

		foreach (var property in propertiesKeys.Union(patternPropertiesKeys))
		{
			properties.Add(property);
		}

		var details = context.EvaluatedKeywords?
			.SelectMany(x => x.Details ?? [])
			.Where(x => x.IsValid && x.InstanceLocation == context.InstanceLocation) ?? [];

		var all = false;
		foreach (var detail in details)
		{
			all |= GetNestedEvaluatedProperties(detail, properties);
			if (all) break;
		}

		return (all, properties);
	}

	private static bool GetNestedEvaluatedProperties(EvaluationResults results, HashSet<string> properties)
	{
		if (results.Annotations is not null)
		{
			if (results.Annotations.ContainsKey("additionalProperties") ||
			    results.Annotations.ContainsKey("unevaluatedProperties"))
				return true;
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
		}

		var details = results.Details?
			.Where(x => x.IsValid && x.InstanceLocation == results.InstanceLocation) ?? [];

		var all = false;
		foreach (var detail in details)
		{
			all |= GetNestedEvaluatedProperties(detail, properties);
			if (all) break;
		}

		return all;
	}
}