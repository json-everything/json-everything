using System.Diagnostics;
using System.Text.Json;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Json.Schema;

[DebuggerDisplay("{Handler.Name} {RawValue}")]
public class KeywordData
{
	public long EvaluationOrder { get; internal set; }
	public IKeywordHandler Handler { get; internal set; }
	public JsonElement RawValue { get; internal set; }
	public JsonSchemaNode[] Subschemas { get; set; } = [];
	public object? Value { get; set; }

	internal KeywordData(){}
}