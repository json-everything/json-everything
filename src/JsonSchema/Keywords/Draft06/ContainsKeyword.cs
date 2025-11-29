using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `contains`.
/// </summary>
public class ContainsKeyword : Json.Schema.Keywords.ContainsKeyword
{
	public new static ContainsKeyword Instance { get; set; } = new();

	protected ContainsKeyword()
	{
	}

	public override void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];
	}

	public override KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

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

		var found = subschemaEvaluations.Any(x => x.IsValid);
		string? error = null;
		if (found)
		{
			error = ErrorMessages.GetContainsTooFew(context.Options.Culture)
				.ReplaceToken("received", found)
				.ReplaceToken("minimum", 1);
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = found,
			Details = subschemaEvaluations.ToArray(),
			Error = error
		};
	}
}
