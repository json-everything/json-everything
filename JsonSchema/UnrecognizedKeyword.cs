using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles unrecognized keywords.
/// </summary>
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[Vocabulary(Vocabularies.Core201909Id)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[Vocabulary(Vocabularies.Core202012Id)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[JsonConverter(typeof(UnrecognizedKeywordJsonConverter))]
public class UnrecognizedKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The name or key of the keyword.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The value of the keyword.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="UnrecognizedKeyword"/>.
	/// </summary>
	/// <param name="name">The name of the keyword.</param>
	/// <param name="value">The value of the keyword.</param>
	public UnrecognizedKeyword(string name, JsonNode? value)
	{
		Name = name;
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
/// JSON converter for <see cref="UnrecognizedKeyword"/>.
/// </summary>
public sealed class UnrecognizedKeywordJsonConverter : JsonConverter<UnrecognizedKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="UnrecognizedKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override UnrecognizedKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Unrecognized keywords should be handled manually during JsonSchema deserialization.");
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, UnrecognizedKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Value, options);
	}
}