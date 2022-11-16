using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static class JsonSchemaExtensions
{
	/// <summary>
	/// Extends <see cref="JsonSchema.Evaluate"/> to take <see cref="JsonDocument"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonDocument">Instance to be evaluated.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	public static EvaluationResults Evaluate(this JsonSchema jsonSchema, JsonDocument jsonDocument, EvaluationOptions? options = null)
	{
		return jsonSchema.Evaluate(jsonDocument.RootElement, options);
	}

	/// <summary>
	/// Extends <see cref="JsonSchema.Evaluate"/> to take <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonElement">Instance to be evaluated.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	public static EvaluationResults Evaluate(this JsonSchema jsonSchema, JsonElement jsonElement, EvaluationOptions? options = null)
	{
		return jsonSchema.Evaluate(jsonElement.AsNode(), options);
	}

	/// <summary>
	/// Extends <see cref="JsonSchema.Evaluate"/> to take <see cref="JsonDocument"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonNode">Instance to be evaluated.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	[Obsolete("Use Evaluate() instead.")]
	public static EvaluationResults Validate(this JsonSchema jsonSchema, JsonNode jsonNode, EvaluationOptions? options = null)
	{
		return jsonSchema.Evaluate(jsonNode, options);
	}

	/// <summary>
	/// Extends <see cref="JsonSchema.Evaluate"/> to take <see cref="JsonDocument"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonDocument">Instance to be evaluated.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	[Obsolete("Use Evaluate() instead.")]
	public static EvaluationResults Validate(this JsonSchema jsonSchema, JsonDocument jsonDocument, EvaluationOptions? options = null)
	{
		return jsonSchema.Evaluate(jsonDocument.RootElement, options);
	}

	/// <summary>
	/// Extends <see cref="JsonSchema.Evaluate"/> to take <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonElement">Instance to be evaluated.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	[Obsolete("Use Evaluate() instead.")]
	public static EvaluationResults Validate(this JsonSchema jsonSchema, JsonElement jsonElement, EvaluationOptions? options = null)
	{
		return jsonSchema.Evaluate(jsonElement.AsNode(), options);
	}

	internal static IEnumerable<Requirement> GenerateRequirements(this JsonSchema schema)
	{
		return GenerateRequirements(schema, JsonPointer.Empty, JsonPointer.Empty);
	}

	internal static IEnumerable<Requirement> GenerateRequirements(this JsonSchema schema, JsonPointer evaluationPath, JsonPointer instanceLocation)
	{
		var requirements = new List<Requirement>();
		GenerateRequirements(schema, requirements, evaluationPath, instanceLocation);

		return requirements;
	}

	private static void GenerateRequirements(JsonSchema schema, List<Requirement> requirements, JsonPointer evaluationPath, JsonPointer instanceLocation)
	{
		if (schema.BoolValue == true)
		{
			requirements.Add(new Requirement(evaluationPath, instanceLocation,
				(_, _) => new KeywordResult
				{
					SubschemaPath = evaluationPath,
					SchemaLocation = schema.BaseUri ?? new Uri("https://json-everything.net/base"),
					InstanceLocation = instanceLocation,
					ValidationResult = true
				}));
			return;
		}

		if (schema.BoolValue == false)
		{
			requirements.Add(new Requirement(evaluationPath, instanceLocation,
				(_, _) => new KeywordResult
				{
					SubschemaPath = evaluationPath,
					Keyword = string.Empty,
					SchemaLocation = schema.BaseUri ?? new Uri("https://json-everything.net/base"),
					InstanceLocation = instanceLocation,
					ValidationResult = false,
					Error = "All values fail the false schema"
				}));
			return;
		}

		requirements.AddRange(schema.Keywords!.SelectMany(k => k.GetRequirements(evaluationPath, schema.BaseUri, instanceLocation)));
	}
}
