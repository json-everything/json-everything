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

	// only called from JsonSchema
	internal static IEnumerable<Requirement> GenerateRequirements(this JsonSchema schema, EvaluationOptions options)
	{
		return GenerateRequirements(schema, options.DefaultBaseUri, JsonPointer.Empty, JsonPointer.Empty);
	}

	// called from everything else
	public static IEnumerable<Requirement> GenerateRequirements(this JsonSchema schema, Uri baseUri, JsonPointer evaluationPath, JsonPointer instanceLocation)
	{
		if (schema.BoolValue == true)
		{
			yield return new Requirement(evaluationPath, instanceLocation,
				(_, _) => new KeywordResult
				{
					SubschemaPath = evaluationPath,
					SchemaLocation = evaluationPath.Resolve(baseUri),
					InstanceLocation = instanceLocation,
					ValidationResult = true
				});
			yield break;
		}

		if (schema.BoolValue == false)
		{
			yield return new Requirement(evaluationPath, instanceLocation,
				(_, _) => new KeywordResult
				{
					SubschemaPath = evaluationPath,
					SchemaLocation = evaluationPath.Resolve(baseUri),
					Keyword = string.Empty,
					InstanceLocation = instanceLocation,
					ValidationResult = false,
					Error = "All values fail the false schema"
				});
			yield break;
		}

		CheckForNewBaseUri(schema, baseUri);

		foreach (var requirement in schema.Keywords!.SelectMany(k => k.GetRequirements(evaluationPath, schema.BaseUri!, instanceLocation)))
		{
			yield return requirement;
		}
	}

	private static void CheckForNewBaseUri(JsonSchema schema, Uri currentBaseUri)
	{
		var idKeyword = schema.Keywords!.OfType<IdKeyword>().FirstOrDefault();

		schema.BaseUri = idKeyword == null
			? currentBaseUri
			// TODO: resolve this against the current base URI
			: idKeyword.Id;
	}
}
