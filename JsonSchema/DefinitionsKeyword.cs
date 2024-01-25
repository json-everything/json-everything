using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `definitions`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[JsonConverter(typeof(DefinitionsKeywordJsonConverter))]
public class DefinitionsKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "definitions";

	/// <summary>
	/// The collection of schema definitions.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Definitions { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Definitions;

	/// <summary>
	/// Creates a new <see cref="DefinitionsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schema definitions.</param>
	public DefinitionsKeyword(IReadOnlyDictionary<string, JsonSchema> values)
	{
		Definitions = values ?? throw new ArgumentNullException(nameof(values));
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
		return KeywordConstraint.Skip;
	}
}

/// <summary>
/// JSON converter for <see cref="DefinitionsKeyword"/>.
/// </summary>
public sealed class DefinitionsKeywordJsonConverter : JsonConverter<DefinitionsKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="DefinitionsKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override DefinitionsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.DictionaryStringJsonSchema)!;
		return new DefinitionsKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DefinitionsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var kvp in value.Definitions)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}