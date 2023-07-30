using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `readOnly`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Metadata201909Id)]
[Vocabulary(Vocabularies.Metadata202012Id)]
[Vocabulary(Vocabularies.MetadataNextId)]
[JsonConverter(typeof(ReadOnlyKeywordJsonConverter))]
public class ReadOnlyKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "readOnly";

	/// <summary>
	/// Whether the instance is read-only.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="ReadOnlyKeyword"/>.
	/// </summary>
	/// <param name="value">Whether the instance is read-only.</param>
	public ReadOnlyKeyword(bool value)
	{
		Value = value;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, Value));
	}
}

internal class ReadOnlyKeywordJsonConverter : JsonConverter<ReadOnlyKeyword>
{
	public override ReadOnlyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
			throw new JsonException("Expected boolean");

		var str = reader.GetBoolean();

		return new ReadOnlyKeyword(str);
	}
	public override void Write(Utf8JsonWriter writer, ReadOnlyKeyword value, JsonSerializerOptions options)
	{
		writer.WriteBoolean(ReadOnlyKeyword.Name, value.Value);
	}
}