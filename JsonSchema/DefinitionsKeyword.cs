using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `definitions`.
/// </summary>
[SchemaPriority(long.MinValue + 1)]
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

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return KeywordConstraint.Skip;
	}
}

internal class DefinitionsKeywordJsonConverter : JsonConverter<DefinitionsKeyword>
{
	public override DefinitionsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)!;
		return new DefinitionsKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, DefinitionsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DefinitionsKeyword.Name);
		writer.WriteStartObject();
		foreach (var kvp in value.Definitions)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}