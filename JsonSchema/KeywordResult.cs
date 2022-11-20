using System;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

public class KeywordResult
{
	public JsonPointer SubschemaPath { get; }
	public string Keyword { get; }
	public Uri SchemaLocation { get; }
	public JsonPointer InstanceLocation { get; }

	public bool? ValidationResult { get; set; }
	/// <summary>
	/// Gets or sets the annotation result from the keyword.
	/// </summary>
	/// <remarks>
	/// To distinguish between an annotation value of `null` and not having an annotation,
	/// use <see cref="JsonNull.SignalNode"/> for a JSON null annotation value.
	/// </remarks>
	public JsonNode? Annotation { get; set; }
	public string? Error { get; set; }

	public KeywordResult(string keyword, JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation)
	{
		Keyword = keyword;
		SubschemaPath = subschemaPath;
		SchemaLocation = subschemaPath.Resolve(baseUri);
		InstanceLocation = instanceLocation;
	}

	internal KeywordResult(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation)
		: this(string.Empty, subschemaPath, baseUri, instanceLocation)
	{
	}
}