using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles 'propertyDependencies'.
/// </summary>
public class PropertyDependenciesKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "propertyDependencies";

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		foreach (var property in value.EnumerateObject())
		{
			if (property.Value.ValueKind != JsonValueKind.Object)
				throw new JsonSchemaException($"'{Name}' values must be objects with keys representing property values, found {value.ValueKind} under property {property.Name}");

			if (property.Value.EnumerateObject().Any(x => x.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
				throw new JsonSchemaException($"Values inside property values of {Name} must be valid schemas");
		}

		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var subschemas = new List<JsonSchemaNode>();
		foreach (var property in keyword.RawValue.EnumerateObject())
		{
			foreach (var value in property.Value.EnumerateObject())
			{
				var defContext = context with
				{
					LocalSchema = value.Value,
					RelativePath = JsonPointer.Create(property.Name, value.Name)
				};
				subschemas.Add(JsonSchema.BuildNode(defContext));
			}
		}

		keyword.Subschemas = subschemas.ToArray();
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Object) return KeywordEvaluation.Ignore;

		var propertyNames = (HashSet<string>)keyword.Value!;
		var subschemaEvaluations = new List<EvaluationResults>();

		foreach (var property in context.Instance.EnumerateObject())
		{
			if (!propertyNames.Contains(property.Name)) continue;

			var schemaIndex = Array.FindIndex(keyword.Subschemas, s => s.RelativePath[0].ToString() == property.Name);
			if (schemaIndex == -1) continue;

			var propContext = context with
			{
				EvaluationPath = context.EvaluationPath.Combine(Name, property.Name)
			};

			subschemaEvaluations.Add(keyword.Subschemas[schemaIndex].Evaluate(propContext));
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray()
		};
	}
}