using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `deprecated`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Metadata201909Id)]
[Vocabulary(Vocabularies.Metadata202012Id)]
[Vocabulary(Vocabularies.MetadataNextId)]
[JsonConverter(typeof(DeprecatedKeywordJsonConverter))]
public class DeprecatedKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "deprecated";

	/// <summary>
	/// Whether the schema is deprecated.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="DeprecatedKeyword"/>.
	/// </summary>
	/// <param name="value">Whether the schema is deprecated.</param>
	public DeprecatedKeyword(bool value)
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

internal class DeprecatedKeywordJsonConverter : JsonConverter<DeprecatedKeyword>
{
	public override DeprecatedKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
			throw new JsonException("Expected boolean");

		var value = reader.GetBoolean();

		return new DeprecatedKeyword(value);
	}
	public override void Write(Utf8JsonWriter writer, DeprecatedKeyword value, JsonSerializerOptions options)
	{
		writer.WriteBoolean(DeprecatedKeyword.Name, value.Value);
	}
}