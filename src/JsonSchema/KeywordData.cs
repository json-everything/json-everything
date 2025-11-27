using System.Text.Json;

namespace Json.Schema;

public class KeywordData
{
	public required long EvaluationOrder { get; set; }
	public required IKeywordHandler Handler { get; set; }
	public required JsonElement RawValue { get; set; }
	public JsonSchemaNode[] Subschemas { get; set; } = [];
	public object? Value { get; set; }
}