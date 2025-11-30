using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `properties`.
/// </summary>
/// <remarks>
/// This keyword validates properties against subschemas.
/// </remarks>
public class PropertiesKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="PropertiesKeyword"/>.
	/// </summary>
	public static PropertiesKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "properties";

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertiesKeyword"/> class.
	/// </summary>
	protected PropertiesKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		if (value.EnumerateObject().Any(x => x.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
			throw new JsonSchemaException("Values must be valid schemas");

		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var subschemas = new List<JsonSchemaNode>();
		foreach (var definition in keyword.RawValue.EnumerateObject())
		{
			var defContext = context with
			{
				LocalSchema = definition.Value,
				RelativePath = JsonPointer.Create(definition.Name)
			};
			subschemas.Add(JsonSchema.BuildNode(defContext));
		}

		keyword.Subschemas = subschemas.ToArray();
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
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = JsonSerializer.SerializeToElement(propertyNames, JsonSchemaSerializerContext.Default.HashSetString)
		};
	}
}
