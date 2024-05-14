using System;
using System.Text.Json.Nodes;

namespace Json.Schema;

/// <summary>
/// Represents any evaluation-time work (i.e. any work that requires the instance) for keywords.
/// </summary>
public class KeywordEvaluation
{
	private bool _evaluated;
	private bool _skipped;

	internal static KeywordEvaluation Skip { get; } = new() { _skipped = true };

	/// <summary>
	/// Gets the local instance to be evaluated.
	/// </summary>
	public JsonNode? LocalInstance { get; }
	/// <summary>
	/// Gets the local results object.
	/// </summary>
	public EvaluationResults Results { get; }

	/// <summary>
	/// Gets any sibling evaluations for this keyword.
	/// </summary>
	/// <remarks>
	/// These evaluations are sourced from the associated constraint's <see cref="KeywordConstraint.SiblingDependencies"/>.
	/// </remarks>
	public KeywordEvaluation[] SiblingEvaluations { get; internal set; } = [];
	/// <summary>
	/// Gets any child evaluations for this keyword.
	/// </summary>
	/// <remarks>
	/// These evaluations are sourced from the associated constraint's <see cref="KeywordConstraint.ChildDependencies"/>.
	///
	/// This property is publicly settable as some keywords cannot define child constraints until the instance is available.
	/// </remarks>
	public SchemaEvaluation[] ChildEvaluations { get; set; } = [];

	internal Guid Id { get; set; }
	internal KeywordConstraint Constraint { get; }

	internal KeywordEvaluation(KeywordConstraint constraint, JsonNode? localInstance, EvaluationResults results)
	{
		Constraint = constraint;
		LocalInstance = localInstance;
		Results = results;
	}
#pragma warning disable CS8618
	private KeywordEvaluation(){}
#pragma warning restore CS8618

	/// <summary>
	/// Indicates that the evaluation should be skipped and no work is to be done,
	/// e.g. `then` is skipped when `if` fails validation.
	/// </summary>
	public void MarkAsSkipped()
	{
		_skipped = true;
	}

	internal void Evaluate(EvaluationContext context)
	{
		if (_evaluated || _skipped) return;

		foreach (var evaluation in ChildEvaluations)
		{
			evaluation.Evaluate(context);
		}

		Constraint.Evaluator(this, context); // this can change _skipped

		if (!_skipped)
		{
			foreach (var evaluation in ChildEvaluations)
			{
				Results.AddNestedResult(evaluation.Results);
			}
		}

		_evaluated = true;
	}
}