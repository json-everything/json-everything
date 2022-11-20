using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
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

	// called from everything else
	public static IEnumerable<Requirement> GenerateRequirements(this JsonSchema schema, Uri baseUri, JsonPointer evaluationPath, JsonPointer instanceLocation, EvaluationOptions options)
	{
		if (schema.BoolValue == true || (!schema.BoolValue.HasValue && !schema.Keywords!.Any()))
		{
			yield return new Requirement(evaluationPath, instanceLocation,
				(_, _, _) => new KeywordResult(evaluationPath, baseUri, instanceLocation)
				{
					ValidationResult = true
				});
			yield break;
		}

		if (schema.BoolValue == false)
		{
			yield return new Requirement(evaluationPath, instanceLocation,
				(_, _, _) => new KeywordResult(evaluationPath, baseUri, instanceLocation)
				{
					ValidationResult = false,
					Error = "All values fail the false schema"
				});
			yield break;
		}

		if (schema.RequirementsGenerationActive) yield break;

		schema.RequirementsGenerationActive = true;

		// TODO: maybe get base uri from parent schema
		foreach (var requirement in schema.Keywords!.SelectMany(k => k.GetRequirements(evaluationPath, baseUri, instanceLocation, options)))
		{
			yield return requirement;
		}

		schema.RequirementsGenerationActive = false;
	}

	public static void Evaluate(this IEnumerable<Requirement> requirements, List<KeywordResult> resultsCache, Dictionary<JsonPointer, JsonNode?> instanceCatalog)
	{
		var pertinentRequirements = requirements.Join(instanceCatalog,
			r => r.InstanceLocation,
			i => i.Key,
			(r, i) => new { Requirement = r, Instance = i.Value });

		foreach (var check in pertinentRequirements)
		{
			var keywordResult = check.Requirement.Evaluate(check.Instance, resultsCache, instanceCatalog);
			if (keywordResult != null)
				resultsCache.Add(keywordResult);
		}
	}

	public static IEnumerable<KeywordResult> GetLocalResults(this IEnumerable<KeywordResult> results, JsonPointer subschemaPath, params PointerSegment[] additionalSegments)
	{
		var targetPath = subschemaPath.Combine(additionalSegments);
		return results.Where(x => x.SubschemaPath == targetPath);
	}

	public static JsonNode? GetLocalAnnotation(this List<KeywordResult> cache, JsonPointer subschemaPath, string keywordName)
	{
		var result = cache.SingleOrDefault(x => x.SubschemaPath == subschemaPath && x.Keyword == keywordName);
		return result?.Annotation;
	}
}
