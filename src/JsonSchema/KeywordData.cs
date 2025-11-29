using System.Diagnostics;
using System.Text.Json;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Json.Schema;

/// <summary>
/// Represents the data and metadata associated with a JSON Schema keyword, including its handler, raw value, evaluated
/// value, and any associated subschemas.
/// </summary>
/// <remarks>Instances of this class are used during JSON Schema processing to track keyword-specific
/// information. The properties provide access to both the raw JSON representation and the processed value, as well as
/// any subschemas relevant to the keyword.</remarks>
[DebuggerDisplay("{Handler.Name} {RawValue}")]
public class KeywordData
{
	/// <summary>
	/// Gets the keyword handler used to process keyword-related operations.
	/// </summary>
	public IKeywordHandler Handler { get; internal set; }

	/// <summary>
	/// Gets the underlying JSON value represented by this instance.
	/// </summary>
	/// <remarks>This property provides direct access to the raw JSON data as a <see
	/// cref="System.Text.Json.JsonElement"/>. It can be used to inspect or process the value without additional parsing or
	/// conversion.</remarks>
	public JsonElement RawValue { get; internal set; }

	/// <summary>
	/// Gets or sets the collection of subschema nodes.
	/// </summary>
	/// <remarks>Each element in the array represents a subschema that is part of the overall schema definition.</remarks>
	public JsonSchemaNode[] Subschemas { get; set; } = [];

	/// <summary>
	/// Gets or sets a value.
	/// </summary>
	/// <remarks>The value may or may not represent the value of the keyword.  It is primarily useful for pre-processing
	/// values and communicating them between the <see cref="IKeywordHandler"/> methods.</remarks>
	public object? Value { get; set; }

	internal long EvaluationOrder { get; set; }

	internal KeywordData(){}
}