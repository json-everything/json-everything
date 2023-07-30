using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.OpenApi;

/// <summary>
/// Handles `example`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[Vocabulary(Vocabularies.OpenApiId)]
[JsonConverter(typeof(ExampleKeywordJsonConverter))]
public class ExampleKeyword : IJsonSchemaKeyword
{
	internal const string Name = "example";

	/// <summary>
	/// The example value.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="ExampleKeyword"/>.
	/// </summary>
	/// <param name="value">The example value.</param>
	public ExampleKeyword(JsonNode? value)
	{
		Value = value;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, Value));
	}
}

internal class ExampleKeywordJsonConverter : JsonConverter<ExampleKeyword>
{
	public override ExampleKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		return new ExampleKeyword(node);
	}
	public override void Write(Utf8JsonWriter writer, ExampleKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ExampleKeyword.Name);
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}