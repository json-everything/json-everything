using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `description`.
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
[JsonConverter(typeof(DescriptionKeywordJsonConverter))]
public class DescriptionKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "description";

	/// <summary>
	/// The description.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates a new <see cref="DescriptionKeyword"/>.
	/// </summary>
	/// <param name="value">The description.</param>
	public DescriptionKeyword(string value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, Value));
	}
}

internal class DescriptionKeywordJsonConverter : JsonConverter<DescriptionKeyword>
{
	public override DescriptionKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;

		return new DescriptionKeyword(str);
	}
	public override void Write(Utf8JsonWriter writer, DescriptionKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(DescriptionKeyword.Name, value.Value);
	}
}