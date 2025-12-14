using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Json.Pointer;
using Json.Schema.Keywords;

namespace Json.Schema;

/// <summary>
/// Represents a subschema.
/// </summary>
/// <remarks>A JsonSchemaNode provides the context and data necessary for evaluating a subschema
/// against an instance. It supports evaluation of schema logic, including handling of boolean schemas and keyword-based
/// validation.</remarks>
[DebuggerDisplay("{BaseUri}")]
public class JsonSchemaNode
{
	private const string _unknownKeywordsAnnotation = "$unknownKeywords";

	private static readonly Uri _trueBaseUri = new("https://json-schema.org/true");
	private static readonly JsonElement _trueElement = JsonDocument.Parse("true").RootElement;
	internal static JsonSchemaNode True() => new()
	{
		BaseUri = _trueBaseUri,
		Source = _trueElement
	};
	private static readonly Uri _falseBaseUri = new("https://json-schema.org/false");
	private static readonly JsonElement _falseElement = JsonDocument.Parse("false").RootElement;
	internal static JsonSchemaNode False() => new()
	{
		BaseUri = _falseBaseUri,
		Source = _falseElement
	};

	/// <summary>
	/// Gets or sets the base URI used for resolving relative paths or requests.
	/// </summary>
	/// <remarks>The base URI should be an absolute URI.</remarks>
	public required Uri BaseUri { get; init; }

	/// <summary>
	/// Gets the original JSON source data this subschema.
	/// </summary>
	/// <remarks>Use this property to access the raw JSON content for inspection or further processing.</remarks>
	public JsonElement Source { get; init; }

	/// <summary>
	/// Gets the collection of keyword data this subschema.
	/// </summary>
	public KeywordData[] Keywords { get; init; } = [];

	/// <summary>
	/// Gets a JSON Pointer to this subschema from the parent subschema, excluding the intermediate keyword.
	/// </summary>
	public JsonPointer RelativePath { get; internal set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	[Obsolete("This is only for advanced usage.")]
	public JsonPointer PathFromResourceRoot { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

	internal JsonSchemaNode()
	{
	}

	/// <summary>
	/// Evaluates the schema against the provided context and returns the results of the evaluation.
	/// </summary>
	/// <remarks>If the schema is a boolean schema, the evaluation result is determined immediately. For schemas
	/// with keywords, each keyword is evaluated in order, and the overall validity is determined by their contributions.
	/// The method manages evaluation scope and tracks evaluated keywords, errors, and annotations as
	/// appropriate.</remarks>
	/// <param name="context">The evaluation context containing the instance data, schema location, and evaluation options to use during schema
	/// evaluation. Cannot be null.</param>
	/// <returns>An EvaluationResults object containing the outcome of the schema evaluation, including validity, errors,
	/// annotations, and details. The results reflect the evaluation of all applicable keywords in the schema.</returns>
	public EvaluationResults Evaluate(EvaluationContext context)
	{
		var baseUri = (BaseUri == _trueBaseUri || BaseUri == _falseBaseUri)
			? context.Scope.LocalScope
			: BaseUri;

		context.CanOptimize &= Keywords.All(x =>
			x.Handler is not (UnevaluatedItemsKeyword or
				UnevaluatedPropertiesKeyword or
				Schema.Keywords.Draft201909.UnevaluatedItemsKeyword));

#pragma warning disable CS0618 // Type or member is obsolete
		var results = context.CanOptimize
			? new EvaluationResults()
			: new EvaluationResults(context.EvaluationPath, new Uri(baseUri, $"#{PathFromResourceRoot}"), context.InstanceLocation, context.Options);
#pragma warning restore CS0618 // Type or member is obsolete
		if (Source.ValueKind == JsonValueKind.True)
		{
			return results;
		}
		if (Source.ValueKind == JsonValueKind.False)
		{
			results.IsValid = false;
			results.Errors = new() { [""] = ErrorMessages.FalseSchema };
			return results;
		}

		var newScope = !Equals(BaseUri, context.Scope.LocalScope);
		if (newScope)
			context.Scope.Push(BaseUri);

		results.IsValid = true;
		context.EvaluatedKeywords = new KeywordEvaluation[Keywords.Length];
		int i = 0;
		foreach (var keyword in Keywords.OrderBy(x => x.EvaluationOrder))
		{
			var evaluation = keyword.Handler.Evaluate(keyword, context);
			context.EvaluatedKeywords[i] = evaluation;

			results.IsValid &= evaluation.IsValid || !evaluation.ContributesToValidation;

			if (!context.CanOptimize)
			{
				if (evaluation.Details is { Length: > 0 })
				{
					results.Details ??= [];
					foreach (var detail in evaluation.Details)
					{
						detail.Parent = results;
						results.Details.Add(detail);
					}
				}
				if (evaluation.Error is not null)
				{
					results.Errors ??= [];
					results.Errors[evaluation.Keyword] = evaluation.Error!;
				}
				if (evaluation.Annotation is not null &&
				    (context.Options.IgnoredAnnotations == null ||
				     !context.Options.IgnoredAnnotations.Contains(keyword.Handler.GetType())))
				{
					results.Annotations ??= [];
					results.Annotations[evaluation.Keyword] = evaluation.Annotation.Value;
				}
			}

			i++;

			if (keyword.Handler is RefKeyword && context.RefIgnoresSiblingKeywords) break;
		}

		if (!context.CanOptimize)
		{
			if (!results.IsValid && !context.Options.PreserveDroppedAnnotations)
				results.Annotations?.Clear();

			if (context.Options.AddAnnotationForUnknownKeywords)
			{
				var unknownKeywords = Keywords
					.Where(x => x.Handler is AnnotationKeyword)
					.Select(x => (string)x.Value!)
					.ToArray();
				if (unknownKeywords.Length != 0)
				{
					results.Annotations ??= [];
					results.Annotations[_unknownKeywordsAnnotation] = JsonSerializer.SerializeToElement(unknownKeywords, JsonSchemaSerializerContext.Default.StringArray);
				}
			}
		}

		if (newScope)
			context.Scope.Pop();

		return results;
	}
}
