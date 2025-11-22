using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `additionalProperties`.
/// </summary>
[DependsOnAnnotationsFrom(typeof(PropertiesKeyword))]
//[DependsOnAnnotationsFrom(typeof(PatternPropertiesKeyword))]
public class AdditionalPropertiesKeyword : IKeywordHandler
{
	private class KnownProperties
	{
		public string[] Properties { get; set; } = [];
		public Regex[] PatternProperties { get; set; } = [];
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "additionalProperties";

	public virtual object? ValidateValue(JsonElement value)
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
			knownProperties.Properties = properties.EnumerateObject().Select(x => x.Name).ToArray();

		keyword.Value = knownProperties;
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Object) return KeywordEvaluation.Ignore;

		// TODO: handle properties/patternProperties
		var knownProperties = (KnownProperties) keyword.Value!;

		var propertiesToSkip = new HashSet<string>(knownProperties.Properties);

		var subschemaEvaluations = new List<EvaluationResults>();
		var subschema = keyword.Subschemas[0];

		var evaluationPath = context.EvaluationPath.Combine(Name);
		foreach (var instance in context.Instance.EnumerateObject())
		{
			if (propertiesToSkip.Contains(instance.Name)) continue;

			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(instance.Name),
				Instance = instance.Value,
				EvaluationPath = evaluationPath.Combine(Name)
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray()
		};
	}
}