using System.Text.Json.Nodes;

namespace Json.Schema.Experiments;

public struct KeywordEvaluation
{
	public static KeywordEvaluation Skip { get; } = new() { Valid = true };
	public static KeywordEvaluation Annotate { get; } = new() { Valid = true, HasAnnotation = true};

	public string Key { get; internal set; } = null!; // this is always set by the context
	public bool Valid { get; set; }
	public EvaluationResults[] Children { get; set; } = [];
	public bool HasAnnotation { get; set; }
	public JsonNode? Annotation { get; set; }

	public KeywordEvaluation(){}

	public static implicit operator KeywordEvaluation(bool valid) => new() { Valid = valid };
}