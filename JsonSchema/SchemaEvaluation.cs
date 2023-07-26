using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema;

public class SchemaEvaluation
{
	public JsonNode? LocalInstance { get; }
	public JsonPointer RelativeInstanceLocation { get; internal set; }
	public EvaluationResults Results { get; }

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

		if (!KeywordEvaluations.All(x => x.Results.IsValid))
			Results.Fail();
	}
}