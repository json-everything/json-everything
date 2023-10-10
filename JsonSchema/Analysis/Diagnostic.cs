using Json.Pointer;

namespace Json.Schema.Analysis;

public class Diagnostic
{
	public JsonPointer Location { get; set; }
	public string Message { get; set; }
	public DiagnosticLevel Level { get; set; }
}