using System;
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
	internal static JsonSchemaNode True() => new()
	{
		BaseUri = new Uri("https://json-schema.org/true"),
		Source = JsonDocument.Parse("true").RootElement
	};
	internal static JsonSchemaNode False() => new()
	{
		BaseUri = new Uri("https://json-schema.org/false"),
		Source = JsonDocument.Parse("false").RootElement
	};

	/// <summary>
	/// Gets or sets the base URI used for resolving relative paths or requests.
	/// </summary>
	/// <remarks>The base URI should be an absolute URI.</remarks>
	public required Uri BaseUri { get; init; }

	/// <summary>
	/// Gets the original JSON source data associated with this instance.
	/// </summary>
	/// <remarks>Use this property to access the raw JSON content for inspection or further processing.</remarks>
	public JsonElement Source { get; init; }

	/// <summary>
	/// Gets the collection of keyword data associated with this instance.
	/// </summary>
	public KeywordData[] Keywords { get; init; } = [];

	/// <summary>
	/// Gets the relative JSON pointer path associated with this instance.
	/// </summary>
	public JsonPointer RelativePath { get; init; }

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
		var results = new EvaluationResults(context.EvaluationPath, BaseUri, context.InstanceLocation, context.Options);
		if (Source.ValueKind == JsonValueKind.True) return results;
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
		context.EvaluatedKeywords = [];
		foreach (var keyword in Keywords.OrderBy(x => x.EvaluationOrder))
		{
			var evaluation = keyword.Handler.Evaluate(keyword, context);
			context.EvaluatedKeywords.Add(evaluation);

			results.IsValid &= evaluation.IsValid || !evaluation.ContributesToValidation;

			if (evaluation.Details is { Length: > 0 })
			{
				results.Details ??= [];
				results.Details.AddRange(evaluation.Details);
			}
			if (evaluation.Error is not null)
			{
				results.Errors ??= [];
				results.Errors[evaluation.Keyword] = evaluation.Error!;
			}
			if (evaluation.Annotation is not null)
			{
				results.Annotations ??= [];
				results.Annotations[evaluation.Keyword] = evaluation.Annotation.Value;
			}

			if (keyword.Handler is RefKeyword &&
				context.RefIgnoresSiblingKeywords)
				break;
		}

		if (!results.IsValid)
			results.Annotations?.Clear();

		if (newScope)
			context.Scope.Pop();

		return results;
	}
}