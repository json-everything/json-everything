using System.Text.Json.Nodes;

namespace Json.Schema;

public class KeywordEvaluation
{
	private bool _evaluated;

	internal static KeywordEvaluation Skip { get; } = new() { _evaluated = true };

	public JsonNode? LocalInstance { get; }
	public EvaluationResults Results { get; }

	public KeywordEvaluation[] SiblingEvaluations { get; set; }
	public SchemaEvaluation[] ChildEvaluations { get; set; }

	internal KeywordConstraint Constraint { get; }

	internal KeywordEvaluation(KeywordConstraint constraint, JsonNode? localInstance, EvaluationResults results)
	{
		Constraint = constraint;
		LocalInstance = localInstance;
		Results = results;
	}
	private KeywordEvaluation(){}

	internal void Evaluate()
	{
		if (_evaluated) return;

		foreach (var evaluation in ChildEvaluations)
		{
			evaluation.Evaluate();
		}

		Constraint.Evaluator(this);

		_evaluated = true;
	}
}