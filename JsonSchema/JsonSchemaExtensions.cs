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
			requirements.Add(new Requirement(evaluationPath, schema.BaseUri, instanceLocation,
				(_, _) => new KeywordResult
			{
				EvaluationPath = evaluationPath,
				SchemaLocation = schema.BaseUri ?? new Uri("https://json-everything.net/base"),
				InstanceLocation = instanceLocation,
				ValidationResult = true
			}));
			return;
		}

		if (schema.BoolValue == false)
		{
			requirements.Add(new Requirement(evaluationPath, schema.BaseUri, instanceLocation,
				(_, _) => new KeywordResult
			{
				EvaluationPath = evaluationPath,
				SchemaLocation = schema.BaseUri ?? new Uri("https://json-everything.net/base"),
				InstanceLocation = instanceLocation,
				ValidationResult = false,
				Message = "All values fail the false schema"
			}));
			return;
		}

		requirements.AddRange(schema.Keywords!.SelectMany(k => k.GetRequirements(evaluationPath.Combine(k.Keyword()), schema.BaseUri, instanceLocation)));
	}
}

public static class PointerExtensions
{
	public static JsonPointer GetSibling(this JsonPointer pointer, PointerSegment newSegment)
	{
		return JsonPointer.Create(pointer.Segments.Take(pointer.Segments.Length - 1).Append(newSegment), false);
	}
}

public class Requirement
{
	public string Keyword { get; }
	public int Priority { get; }
	public JsonPointer EvaluationPath { get; }
	public Uri SchemaLocation { get; }
	public JsonPointer InstanceLocation { get; }

	public Func<JsonNode?, List<KeywordResult>, KeywordResult> Evaluate { get; }

	public Requirement(JsonPointer evaluationPath, Uri baseUri, JsonPointer instanceLocation, Func<JsonNode?, List<KeywordResult>, KeywordResult> evaluate)
	{
		// TODO: schema location is schema's base uri + evaluation path after final $ref
		EvaluationPath = evaluationPath;
		InstanceLocation = instanceLocation;
		Evaluate = evaluate;
	}
}

public class KeywordResult
{
	public JsonPointer EvaluationPath { get; set; }
	public Uri SchemaLocation { get; set; }
	public JsonPointer InstanceLocation { get; set; }

	// use JsonNull for null
	public JsonNode? Annotation { get; set; }
	public bool ValidationResult { get; set; }
	public string Message { get; set; }
}