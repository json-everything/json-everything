using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `additionalProperties`.
/// </summary>
[DependsOnAnnotationsFrom(typeof(PropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(PatternPropertiesKeyword))]
public class AdditionalPropertiesKeyword : IKeywordHandler
{
	private class KnownProperties
	{
		public HashSet<string> Properties { get; set; } = [];
		public Regex[] PatternProperties { get; set; } = [];
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "additionalProperties";

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

		var knownProperties = new KnownProperties();

		if (context.LocalSchema.TryGetProperty("properties", out var properties))
			knownProperties.Properties =
			[
				..properties
					.EnumerateObject()
					.Select(x => x.Name)
			];

		if (context.LocalSchema.TryGetProperty("patternProperties", out var patternProperties))
			knownProperties.PatternProperties = patternProperties
				.EnumerateObject()
				.Select(x => new Regex(x.Name, RegexOptions.ECMAScript | RegexOptions.Compiled))
				.ToArray();

		keyword.Value = knownProperties;
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Object) return KeywordEvaluation.Ignore;

		var knownProperties = (KnownProperties) keyword.Value!;

		var subschemaEvaluations = new List<EvaluationResults>();
		var propertyNames = new HashSet<string>();
		var subschema = keyword.Subschemas[0];

		var evaluationPath = context.EvaluationPath.Combine(Name);
		foreach (var instance in context.Instance.EnumerateObject())
		{
			if (knownProperties.Properties.Contains(instance.Name)) continue;
			if (knownProperties.PatternProperties.Any(x => x.IsMatch(instance.Name))) continue;

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
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = JsonElementExtensions.True
		};
	}
}