using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `additionalProperties`.
/// </summary>
/// <remarks>
/// This keyword specifies a schema that will be used to validate properties that are not explicitly defined
/// by `properties` or `patternProperties`.
/// </remarks>
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
	/// Gets the singleton instance of the <see cref="AdditionalPropertiesKeyword"/>.
	/// </summary>
	public static AdditionalPropertiesKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "additionalProperties";

	/// <summary>
	/// Initializes a new instance of the <see cref="AdditionalPropertiesKeyword"/> class.
	/// </summary>
	protected AdditionalPropertiesKeyword()
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

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
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