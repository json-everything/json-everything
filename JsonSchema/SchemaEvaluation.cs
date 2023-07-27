using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

public class SchemaEvaluation
{
	public JsonNode? LocalInstance { get; }
	public JsonPointer RelativeInstanceLocation { get; internal set; }
	public EvaluationResults Results { get; }

	internal Guid Id { get; set; }
	internal KeywordEvaluation[] KeywordEvaluations { get; }

	internal SchemaEvaluation(JsonNode? localInstance, JsonPointer relativeInstanceLocation, EvaluationResults results, KeywordEvaluation[] evaluations)
	{
		LocalInstance = localInstance;
		RelativeInstanceLocation = relativeInstanceLocation;
		Results = results;
		KeywordEvaluations = evaluations;
	}

	internal void Evaluate()
	{
		foreach (var keyword in KeywordEvaluations)
		{
			keyword.Evaluate();
		}
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