using System;
using System.Linq;
using System.Text.Json;
using Json.Pointer;
using Json.Schema.Keywords;

namespace Json.Schema;

public class JsonSchemaNode
{
	public static JsonSchemaNode True() => new()
	{
		BaseUri = new Uri("https://json-schema.org/true"),
		Source = JsonDocument.Parse("true").RootElement
	};
	public static JsonSchemaNode False() => new()
	{
		BaseUri = new Uri("https://json-schema.org/false"),
		Source = JsonDocument.Parse("false").RootElement
	};

	public required Uri BaseUri { get; set; }
	public JsonElement Source { get; set; }
	public KeywordData[] Keywords { get; init; } = [];
	public JsonPointer RelativePath { get; set; }

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
				context.BuildOptions.KeywordRegistry.RefIgnoresSiblingKeywords)
				break;
		}

		if (!results.IsValid)
			results.Annotations?.Clear();

		if (newScope)
			context.Scope.Pop();

		return results;
	}
}