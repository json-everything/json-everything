using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `dependentSchemas`.
/// </summary>
/// <remarks>
/// This keyword specifies that if a property is present in an object, then the instance (the full object, not the property's
/// value) must be valid against the corresponding subschema.
/// </remarks>
public class DependentSchemasKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="DependentSchemasKeyword"/>.
	/// </summary>
	public static DependentSchemasKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "dependentSchemas";

	/// <summary>
	/// Initializes a new instance of the <see cref="DependentSchemasKeyword"/> class.
	/// </summary>
	protected DependentSchemasKeyword()
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
		var propertyNames = new HashSet<string>();
		foreach (var definition in keyword.RawValue.EnumerateObject())
		{
			var defContext = context with
			{
				LocalSchema = definition.Value,
				RelativePath = JsonPointer.Create(definition.Name)
			};
			subschemas.Add(JsonSchema.BuildNode(defContext));
			propertyNames.Add(definition.Name);
		}

		keyword.Subschemas = subschemas.ToArray();
		keyword.Value = propertyNames;
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
