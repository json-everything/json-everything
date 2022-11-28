﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using JetBrains.Annotations;
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

	public static IEnumerable<Requirement> GenerateRequirements(this JsonSchema schema, DynamicScope scope, JsonPointer evaluationPath, JsonPointer instanceLocation, EvaluationOptions options)
	{
		return GenerateRequirementsUnordered(schema, scope, evaluationPath, instanceLocation, options)
			.OrderByDescending(x => x.SubschemaPath.Segments.Length)
			.ThenBy(x => x.Priority);
	}

	private static IEnumerable<Requirement> GenerateRequirementsUnordered(JsonSchema schema, DynamicScope scope, JsonPointer evaluationPath, JsonPointer instanceLocation, EvaluationOptions options)
	{
		if (schema.BoolValue == true || (!schema.BoolValue.HasValue && !schema.Keywords!.Any()))
		{
			yield return new Requirement(evaluationPath, instanceLocation,
				(_, _, _) => new KeywordResult(evaluationPath, scope.LocalScope, instanceLocation)
				{
					ValidationResult = true
				});
			yield break;
		}

		if (schema.BoolValue == false)
		{
			yield return new Requirement(evaluationPath, instanceLocation,
				(_, _, _) => new KeywordResult(evaluationPath, scope.LocalScope, instanceLocation)
				{
					ValidationResult = false,
					Error = "All values fail the false schema"
				});
			yield break;
		}

		if (schema.GeneratingRequirements.Contains(instanceLocation)) yield break;

		schema.GeneratingRequirements.Add(instanceLocation);

		if (schema.BaseUri != scope.LocalScope)
			scope = scope.Append(schema.BaseUri!);

		var keywords = options.FilterKeywords(schema.Keywords!);
		var requirements = keywords.SelectMany(k => k.GetRequirements(evaluationPath, scope, instanceLocation, options));
		foreach (var requirement in requirements)
		{
			yield return requirement;
		}

		schema.GeneratingRequirements.Remove(instanceLocation);
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static IEnumerable<T> Debug<T>(this IEnumerable<T> items)
	{
#if DEBUG
		return items.ToList();
#else
		return items;
#endif
	}
}

public class DynamicScope : IEnumerable<Uri>
{
	private readonly Uri[] _scope;

	public Uri LocalScope => _scope[_scope.Length - 1];

	internal DynamicScope(Uri initialScope)
	{
		_scope = new[] { initialScope };
	}

	private DynamicScope(IEnumerable<Uri> scope)
	{
		_scope = scope.ToArray();
	}

	public DynamicScope Append(Uri localScope)
	{
		return new DynamicScope(_scope.Append(localScope));
	}

	public IEnumerator<Uri> GetEnumerator()
	{
		return ((IEnumerable<Uri>)_scope).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
