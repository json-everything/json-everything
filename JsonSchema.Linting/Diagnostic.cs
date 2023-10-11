using Json.Pointer;

namespace Json.Schema.Linting;

public class Diagnostic
{
	public const string MessageKeyword = "x-diagnostic-message";
	public const string TargetKeyword = "x-diagnostic-target";
	public const string LevelKeyword = "x-diagnostic-level";

	public string RuleId { get; set; }
	public JsonPointer Location { get; set; }
	public string? Target { get; set; }
	public string Message { get; set; }
	public DiagnosticLevel Level { get; set; }
}