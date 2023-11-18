using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, Value));
	}
}

/// <summary>
/// JSON converter for <see cref="ExampleKeyword"/>.
/// </summary>
public sealed class ExampleKeywordJsonConverter : JsonConverter<ExampleKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="ExampleKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override ExampleKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode>(ref reader, options);

		return new ExampleKeyword(node);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, ExampleKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ExampleKeyword.Name);
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}