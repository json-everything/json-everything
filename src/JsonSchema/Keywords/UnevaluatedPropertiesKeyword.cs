using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `unevaluatedProperties`.
/// </summary>
/// <remarks>
/// This keyword validates property values in an object that are not evaluated by `properties`, `patternProperties`,
/// `additionalProperties`, or other `unevaluatedProperties` keywords that are found either in the local subschema
/// directly or in subschemas of the local schema which produce a true validation result.
/// </remarks>
[DependsOnAnnotationsFrom<PropertiesKeyword>]
[DependsOnAnnotationsFrom<PatternPropertiesKeyword>]
[DependsOnAnnotationsFrom<UnevaluatedPropertiesKeyword>]
public class UnevaluatedPropertiesKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="UnevaluatedPropertiesKeyword"/>.
	/// </summary>
	public static UnevaluatedPropertiesKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "unevaluatedProperties";

	/// <summary>
	/// Initializes a new instance of the <see cref="UnevaluatedPropertiesKeyword"/> class.
	/// </summary>
	protected UnevaluatedPropertiesKeyword()
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