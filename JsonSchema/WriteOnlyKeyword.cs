using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `writeOnly`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Metadata201909Id)]
[Vocabulary(Vocabularies.Metadata202012Id)]
[Vocabulary(Vocabularies.MetadataNextId)]
[JsonConverter(typeof(WriteOnlyKeywordJsonConverter))]
public class WriteOnlyKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "writeOnly";

	/// <summary>
	/// Whether the instance is read-only.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="WriteOnlyKeyword"/>.
	/// </summary>
	/// <param name="value">Whether the instance is read-only.</param>
	public WriteOnlyKeyword(bool value)
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

internal class WriteOnlyKeywordJsonConverter : JsonConverter<WriteOnlyKeyword>
{
	public override WriteOnlyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
			throw new JsonException("Expected boolean");

		var str = reader.GetBoolean();

		return new WriteOnlyKeyword(str);
	}
	public override void Write(Utf8JsonWriter writer, WriteOnlyKeyword value, JsonSerializerOptions options)
	{
		writer.WriteBoolean(WriteOnlyKeyword.Name, value.Value);
	}
}