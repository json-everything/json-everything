using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class ItemsKeywordHandler : IKeywordHandler
{
	private readonly bool _allowArrays;

	public static ItemsKeywordHandler OnlySingle { get; } = new(false);
	public static ItemsKeywordHandler AllowArrays { get; } = new(true);

	public string Name => "items";
	public string[]? Dependencies { get; } = ["prefixItems"];

	private ItemsKeywordHandler(bool allowArrays)
	{
		_allowArrays = allowArrays;
	}

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		if (context.LocalInstance is not JsonArray instance) return KeywordEvaluation.Skip;

		if (keywordValue is JsonObject || keywordValue is JsonValue v && v.GetBool() is not null)
			return HandleSingle(keywordValue, context, evaluations, instance);

		if (!_allowArrays)
			throw new SchemaValidationException("items must be a schema", context);

		if (keywordValue is JsonArray arr)
			return HandleArray(arr, context, instance);

		throw new SchemaValidationException("items must be either a schema or an array of schemas", context);
	}

	private KeywordEvaluation HandleSingle(JsonNode keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations, JsonArray instance)
	{
		var skip = 0;
		if (evaluations.TryGetAnnotation("prefixItems", out JsonValue? prefixItemsAnnotation)) 
			skip = (int?)prefixItemsAnnotation.GetInteger() + 1 ?? 0;

		var contextTemplate = context;
		contextTemplate.EvaluationPath = context.EvaluationPath.Combine(Name);
		contextTemplate.SchemaLocation = context.SchemaLocation.Combine(Name);

		var results = new EvaluationResults[instance.Count - skip];
		bool valid = true;
		int annotation = -1;

		for (int i = 0; i < instance.Count - skip; i++)
		{
			var x = instance[skip + i];
			var localContext = contextTemplate;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(skip + i);
			localContext.LocalInstance = x;

			var evaluation = localContext.Evaluate(keywordValue);

			valid &= evaluation.Valid;
			annotation = skip + i;

			results[i] = evaluation;
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Annotation = annotation,
			HasAnnotation = annotation != -1,
			Children = results
		};
	}

	private KeywordEvaluation HandleArray(JsonArray constraints, EvaluationContext context, JsonArray instance)
	{
		var count = Math.Min(instance.Count, constraints.Count);

		var results = new EvaluationResults[count];
		bool valid = true;
		int annotation = -1;

		for (int i = 0; i < count; i++)
		{
			var item = instance[i];
			var constraint = constraints[i];

			var localContext = context;
			localContext.InstanceLocation = localContext.InstanceLocation.Combine(i);
			localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name, i);
			localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name, i);
			localContext.LocalInstance = item;

			var evaluation = localContext.Evaluate(constraint);

			valid &= evaluation.Valid;
			annotation = i;

			results[i] = evaluation;
		}

		return new KeywordEvaluation
		{
			Valid = valid,
			Annotation = annotation,
			HasAnnotation = annotation != -1,
			Children = results
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue)
	{
		if (keywordValue is JsonArray array)
		{
			foreach (var item in array)
			{
				yield return item;
			}
		}

		yield return keywordValue;
	}
}