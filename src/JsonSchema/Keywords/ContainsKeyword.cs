using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `contains`.
/// </summary>
// TODO: for property erroring, we should make this dependent upon min/max
public class ContainsKeyword : IKeywordHandler
{
	private struct ContainsLimits
	{
		public int Min { get; init; }
		public int? Max { get; init; }
	}

	public static ContainsKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "contains";

	protected ContainsKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' value must be a valid schema, found {value.ValueKind}");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];

		var limits = new ContainsLimits
		{
			Min = context.LocalSchema.TryGetProperty("minContains", out var minContains) ? (int) Math.Truncate(minContains.GetDouble()) : 1,
			Max = context.LocalSchema.TryGetProperty("maxContains", out var maxContains) ? (int)Math.Truncate(maxContains.GetDouble()) : null
		};
		keyword.Value = limits;
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var limits = (ContainsLimits)keyword.Value!;

		var subschemaEvaluations = new List<EvaluationResults>();
		var subschema = keyword.Subschemas[0];

		var i = 0;
		foreach (var instance in context.Instance.EnumerateArray())
		{
			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(i),
				Instance = instance,
				EvaluationPath = context.EvaluationPath.Combine(Name)
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
			i++;
		}

		var found = subschemaEvaluations
			.Select((x, j) => (x.IsValid, j))
			.Where(x => x.IsValid)
			.Select(x => x.j)
			.ToArray();
		var valid = true;
		string? error = null;
		if (limits.Min > found.Length)
		{
			valid = false;
			error = ErrorMessages.GetContainsTooFew(context.Options.Culture)
				.ReplaceToken("received", found.Length)
				.ReplaceToken("minimum", limits.Min);
		}
		else if (found.Length > limits.Max)
		{
			valid = false;
			error = ErrorMessages.GetContainsTooMany(context.Options.Culture)
				.ReplaceToken("received", found.Length)
				.ReplaceToken("maximum", limits.Max.Value);
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = valid,
			Details = subschemaEvaluations.ToArray(),
			Error = error,
			Annotation = JsonSerializer.SerializeToElement(found, JsonSchemaSerializerContext.Default.Int32Array)
		};
	}
}
