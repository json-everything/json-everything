using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `default`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Metadata201909Id)]
[Vocabulary(Vocabularies.Metadata202012Id)]
[Vocabulary(Vocabularies.MetadataNextId)]
[JsonConverter(typeof(DefaultKeywordJsonConverter))]
public class DefaultKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "default";

	/// <summary>
	/// The value to use as the default.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="DefaultKeyword"/>.
	/// </summary>
	/// <param name="value">The value to use as the default.</param>
	public DefaultKeyword(JsonNode? value)
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
		return KeywordConstraint.SimpleAnnotation(Name, Value);
	}
}

/// <summary>
/// JSON converter for <see cref="DefaultKeyword"/>.
/// </summary>
public sealed class DefaultKeywordJsonConverter : JsonConverter<DefaultKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="DefaultKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override DefaultKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.JsonNode);

		return new DefaultKeyword(node);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DefaultKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}