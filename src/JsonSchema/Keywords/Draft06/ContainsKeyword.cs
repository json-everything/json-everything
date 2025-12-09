using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `contains`.
/// </summary>
/// <remarks>
/// This keyword is used to validate that an array contains at least one item that matches the subschema.
/// </remarks>
public class ContainsKeyword : Json.Schema.Keywords.ContainsKeyword
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="ContainsKeyword"/>.
	/// </summary>
	public new static ContainsKeyword Instance { get; set; } = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="ContainsKeyword"/> class.
	/// </summary>
	protected ContainsKeyword()
	{
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public override void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
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
