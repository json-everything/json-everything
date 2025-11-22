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

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "contains";

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
			Min = context.LocalSchema.TryGetProperty("minContains", out var minContains) ? minContains.GetInt32() : 1,
			Max = context.LocalSchema.TryGetProperty("maxContains", out var maxContains) ? maxContains.GetInt32() : null
		};
		keyword.Value = limits;
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var limits = (ContainsLimits)keyword.Value!;

		var subschemaEvaluations = new List<EvaluationResults>();
		var subschema = keyword.Subschemas[0];

		var evaluationPath = context.EvaluationPath.Combine(Name);
		var i = 0;
		foreach (var instance in context.Instance.EnumerateArray())
		{
			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(i),
				Instance = instance,
				EvaluationPath = evaluationPath.Combine(Name)
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
			i++;
		}

		var found = subschemaEvaluations.Count(x => x.IsValid);
		var valid = true;
		string? error = null;
		if (limits.Min > found)
		{
			valid = false;
			error = ErrorMessages.GetContainsTooFew(context.Options.Culture)
				.ReplaceToken("received", found)
				.ReplaceToken("minimum", limits.Min);
		}
		else if (found > limits.Max)
		{
			valid = false;
			error = ErrorMessages.GetContainsTooMany(context.Options.Culture)
				.ReplaceToken("received", found)
				.ReplaceToken("maximum", limits.Max.Value);
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = valid,
			Details = subschemaEvaluations.ToArray(),
			Error = error
		};
	}
}
