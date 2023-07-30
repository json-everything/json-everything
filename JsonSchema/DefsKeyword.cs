using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `$defs`.
/// </summary>
[SchemaPriority(long.MinValue + 1)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(DefsKeywordJsonConverter))]
public class DefsKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$defs";

	/// <summary>
	/// The collection of schema definitions.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Definitions { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Definitions;

	/// <summary>
	/// Creates a new <see cref="DefsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schema definitions.</param>
	public DefsKeyword(IReadOnlyDictionary<string, JsonSchema> values)
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

internal class DefsKeywordJsonConverter : JsonConverter<DefsKeyword>
{
	public override DefsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)!;
		return new DefsKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, DefsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DefsKeyword.Name);
		writer.WriteStartObject();
		foreach (var kvp in value.Definitions)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, options);
		}
		writer.WriteEndObject();
	}
}