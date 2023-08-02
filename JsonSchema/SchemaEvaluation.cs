using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Represents any evaluation-time work (i.e. any work that requires the instance) for schemas.
/// </summary>
public class SchemaEvaluation
{
	private bool _hasBeenEvaluated;

	/// <summary>
	/// Gets the local instance.
	/// </summary>
	public JsonNode? LocalInstance { get; }
	/// <summary>
	/// Gets the relative instance location.  (The base location can be found in <see cref="Results"/>.
	/// </summary>
	public JsonPointer RelativeInstanceLocation { get; internal set; }
	/// <summary>
	/// Gets the local evaluation results.
	/// </summary>
	public EvaluationResults Results { get; }

	internal Guid Id { get; set; }
	internal KeywordEvaluation[] KeywordEvaluations { get; }
	internal EvaluationOptions Options { get; }

	internal SchemaEvaluation(JsonNode? localInstance, JsonPointer relativeInstanceLocation, EvaluationResults results, KeywordEvaluation[] evaluations, EvaluationOptions options)
	{
		LocalInstance = localInstance;
		RelativeInstanceLocation = relativeInstanceLocation;
		Results = results;
		KeywordEvaluations = evaluations;
		Options = options;
	}

	/// <summary>
	/// Processes the evaluation.
	/// </summary>
	/// <param name="context">The evaluation context.</param>
	public void Evaluate(EvaluationContext context)
	{
		if (_hasBeenEvaluated) return;

		foreach (var keyword in KeywordEvaluations)
		{
			keyword.Evaluate(context);
		}

		_hasBeenEvaluated = true;
	}

	internal KeywordEvaluation? FindEvaluation(Guid id)
	{
		var found = KeywordEvaluations.FirstOrDefault(x => x is not null && x.Id == id);
		if (found != null) return found;

		foreach (var evaluation in KeywordEvaluations)
		{
			foreach (var schemaEvaluation in evaluation.ChildEvaluations)
			{
				found = schemaEvaluation.FindEvaluation(id);
				if (found != null) return found;
			}
		}

		return null;
	}
}